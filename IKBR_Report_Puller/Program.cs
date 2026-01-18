using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

class Program
{
    /// <summary>
    /// This console application fetches a financial report from the Interactive Brokers (IBKR) Flex Query API,
    /// saves the report locally as an XML file, and then inserts the trade and open position data from the report
    /// into a SQL Server database.
    /// </summary>
    static async Task Main(string[] args)
    {
        var host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                if (context.HostingEnvironment.IsDevelopment())
                {
                    config.AddUserSecrets<Program>();
                }
            })
            .Build();

        var config = host.Services.GetRequiredService<IConfiguration>();

        // --- Configuration ---
        // API credentials and identifiers for the IBKR Flex Query are now read from user secrets
        var token = config["IBKR:Token"];
        var queryId = config["IBKR:QueryId"];
        var baseUrl = config["IBKR:BaseUrl"];
        
        // Local file path to save the downloaded report
        var outputFilePath = config["IBKR:OutputFilePath"];
        
        // Retry logic parameters for polling the report generation status
        const int maxRetries = 10; // The maximum number of times to retry fetching the report
        const int delayInSeconds = 15; // The delay between each retry attempt
        
        // Database connection details are now read from user secrets
        var dbUser = config["Database:User"];
        var dbPassword = config["Database:Password"];
        var dbHost = config["Database:Host"];
        var dbName = config["Database:DbName"];
        string connectionString = $"Server={dbHost};Database={dbName};User ID={dbUser};Password={dbPassword};TrustServerCertificate=True;";

        // The initial URL to request the report generation
        string requestUrl = $"{baseUrl}?t={token}&q={queryId}&v=3";

        using HttpClient client = new HttpClient();
        
        Console.WriteLine("Pinging Flex Query API to request report...");

        try
        {
            HttpResponseMessage initialResponse = await client.GetAsync(requestUrl);
            initialResponse.EnsureSuccessStatusCode();
            string initialResponseBody = await initialResponse.Content.ReadAsStringAsync();
            
            Console.WriteLine("Initial API Response:");
            Console.WriteLine(initialResponseBody);

            XDocument initialXml = XDocument.Parse(initialResponseBody);
            var responseElement = initialXml.Element("FlexStatementResponse");
            string status = responseElement?.Element("Status")?.Value;
            string referenceCode = responseElement?.Element("ReferenceCode")?.Value;
            string statementUrl = responseElement?.Element("Url")?.Value;

            if (status == "Success" && !string.IsNullOrEmpty(referenceCode) && !string.IsNullOrEmpty(statementUrl))
            {
                Console.WriteLine($"Report requested successfully. Reference code: {referenceCode}");
                
                for (int i = 0; i < maxRetries; i++)
                {
                    Console.WriteLine($"Attempt {i + 1} of {maxRetries}: Fetching the full report in {delayInSeconds} seconds...");
                    await Task.Delay(TimeSpan.FromSeconds(delayInSeconds));

                    string getStatementUrl = $"{statementUrl}?t={token}&q={referenceCode}&v=3";
                    HttpResponseMessage reportResponse = await client.GetAsync(getStatementUrl);
                    reportResponse.EnsureSuccessStatusCode();
                    string reportBody = await reportResponse.Content.ReadAsStringAsync();

                    XDocument reportXml;
                    try
                    {
                        reportXml = XDocument.Parse(reportBody);
                    }
                    catch (Exception)
                    {
                        // If parsing fails, it's likely not the XML we expect.
                        Console.WriteLine("Received non-XML response while waiting for the report. Retrying...");
                        continue;
                    }

                    var flexStatementResponse = reportXml.Element("FlexStatementResponse");
                    if (flexStatementResponse != null && flexStatementResponse.Element("ErrorCode")?.Value == "1019")
                    {
                        Console.WriteLine("Report generation in progress. Will try again.");
                        continue;
                    }

                    if (reportXml.Element("FlexQueryResponse") != null)
                    {
                        Console.WriteLine("Full report received.");
                        reportXml.Save(outputFilePath);
                        Console.WriteLine($"Successfully saved report to {outputFilePath}");
                        
                        InsertTradesIntoDatabase(reportXml, connectionString);
                        InsertOpenPositionsIntoDatabase(reportXml, connectionString);

                        return; // Exit after success
                    }
                    
                    Console.WriteLine("Received an unexpected response format. Retrying...");
                }
                
                Console.WriteLine("Failed to retrieve the report after multiple retries.");
            }
            else
            {
                Console.WriteLine("Failed to request report. The initial response did not indicate success or was missing data.");
                string errorCode = responseElement?.Element("ErrorCode")?.Value;
                string errorMessage = responseElement?.Element("ErrorMessage")?.Value;
                if(!string.IsNullOrEmpty(errorCode) || !string.IsNullOrEmpty(errorMessage))
                {
                    Console.WriteLine($"Error Code: {errorCode}");
                    Console.WriteLine($"Error Message: {errorMessage}");
                }
            }
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("\nException Caught!");
            Console.WriteLine("Message :{0} ", e.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nAn unexpected error occurred: {ex.Message}");
        }
    }

    private static void InsertOpenPositionsIntoDatabase(XDocument reportXml, string connectionString)
    {
        try
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            Console.WriteLine("Successfully connected to the database for Open Positions.");

            var flexStatement = reportXml.Descendants("FlexStatement").FirstOrDefault();
            if (flexStatement == null)
            {
                Console.WriteLine("No FlexStatement found in the report. Skipping Open Positions insert.");
                return;
            }

            string whenGeneratedStr = flexStatement.Attribute("whenGenerated")?.Value;
            string accountId = flexStatement.Attribute("accountId")?.Value;

            if (string.IsNullOrEmpty(whenGeneratedStr) || string.IsNullOrEmpty(accountId))
            {
                Console.WriteLine("whenGenerated or accountId attribute is missing from FlexStatement. Skipping Open Positions insert.");
                return;
            }

            DateTime whenGenerated = DateTime.ParseExact(whenGeneratedStr, "yyyyMMdd;HHmmss", CultureInfo.InvariantCulture);

            var openPositions = reportXml.Descendants("OpenPosition").ToList();
            if (!openPositions.Any())
            {
                Console.WriteLine("No open positions found in the report.");
                return;
            }

            using (var transaction = connection.BeginTransaction())
            {
                // Delete existing snapshot for this account and timestamp
                using (SqlCommand deleteCmd = new SqlCommand("DELETE FROM [dbo].[OpenPositions] WHERE [accountId] = @accountId AND [whenGenerated] = @whenGenerated", connection, transaction))
                {
                    deleteCmd.Parameters.AddWithValue("@accountId", accountId);
                    deleteCmd.Parameters.AddWithValue("@whenGenerated", whenGenerated);
                    int rowsDeleted = deleteCmd.ExecuteNonQuery();
                    Console.WriteLine($"Deleted {rowsDeleted} old open position(s) for account {accountId} and timestamp {whenGenerated}.");
                }

                int newPositionsCount = 0;
                foreach (var position in openPositions)
                {
                    newPositionsCount++;
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO [dbo].[OpenPositions] ([whenGenerated], [accountId], [acctAlias], [model], [currency], [fxRateToBase], [assetCategory], [subCategory], [symbol], [description], [conid], [securityID], [securityIDType], [cusip], [isin], [figi], [listingExchange], [underlyingConid], [underlyingSymbol], [underlyingSecurityID], [underlyingListingExchange], [issuer], [issuerCountryCode], [multiplier], [strike], [expiry], [putCall], [principalAdjustFactor], [reportDate], [position], [markPrice], [positionValue], [openPrice], [costBasisPrice], [costBasisMoney], [percentOfNAV], [fifoPnlUnrealized], [side], [levelOfDetail], [openDateTime], [holdingPeriodDateTime], [vestingDate], [code], [originatingOrderID], [originatingTransactionID], [accruedInt], [serialNumber], [deliveryType], [commodityType], [fineness], [weight]) VALUES (@whenGenerated, @accountId, @acctAlias, @model, @currency, @fxRateToBase, @assetCategory, @subCategory, @symbol, @description, @conid, @securityID, @securityIDType, @cusip, @isin, @figi, @listingExchange, @underlyingConid, @underlyingSymbol, @underlyingSecurityID, @underlyingListingExchange, @issuer, @issuerCountryCode, @multiplier, @strike, @expiry, @putCall, @principalAdjustFactor, @reportDate, @position, @markPrice, @positionValue, @openPrice, @costBasisPrice, @costBasisMoney, @percentOfNAV, @fifoPnlUnrealized, @side, @levelOfDetail, @openDateTime, @holdingPeriodDateTime, @vestingDate, @code, @originatingOrderID, @originatingTransactionID, @accruedInt, @serialNumber, @deliveryType, @commodityType, @fineness, @weight)", connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@whenGenerated", whenGenerated);
                        AddParameter(cmd, "@accountId", position.Attribute("accountId")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@acctAlias", position.Attribute("acctAlias")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@model", position.Attribute("model")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@currency", position.Attribute("currency")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@fxRateToBase", position.Attribute("fxRateToBase")?.Value, SqlDbType.Decimal);
                        AddParameter(cmd, "@assetCategory", position.Attribute("assetCategory")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@subCategory", position.Attribute("subCategory")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@symbol", position.Attribute("symbol")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@description", position.Attribute("description")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@conid", position.Attribute("conid")?.Value, SqlDbType.BigInt);
                        AddParameter(cmd, "@securityID", position.Attribute("securityID")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@securityIDType", position.Attribute("securityIDType")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@cusip", position.Attribute("cusip")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@isin", position.Attribute("isin")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@figi", position.Attribute("figi")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@listingExchange", position.Attribute("listingExchange")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@underlyingConid", position.Attribute("underlyingConid")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@underlyingSymbol", position.Attribute("underlyingSymbol")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@underlyingSecurityID", position.Attribute("underlyingSecurityID")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@underlyingListingExchange", position.Attribute("underlyingListingExchange")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@issuer", position.Attribute("issuer")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@issuerCountryCode", position.Attribute("issuerCountryCode")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@multiplier", position.Attribute("multiplier")?.Value, SqlDbType.Int);
                        AddParameter(cmd, "@strike", position.Attribute("strike")?.Value, SqlDbType.Decimal);
                        AddParameter(cmd, "@expiry", position.Attribute("expiry")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@putCall", position.Attribute("putCall")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@principalAdjustFactor", position.Attribute("principalAdjustFactor")?.Value, SqlDbType.Decimal);
                        AddParameter(cmd, "@reportDate", position.Attribute("reportDate")?.Value, SqlDbType.Date);
                        AddParameter(cmd, "@position", position.Attribute("position")?.Value, SqlDbType.Decimal);
                        AddParameter(cmd, "@markPrice", position.Attribute("markPrice")?.Value, SqlDbType.Decimal);
                        AddParameter(cmd, "@positionValue", position.Attribute("positionValue")?.Value, SqlDbType.Decimal);
                        AddParameter(cmd, "@openPrice", position.Attribute("openPrice")?.Value, SqlDbType.Decimal);
                        AddParameter(cmd, "@costBasisPrice", position.Attribute("costBasisPrice")?.Value, SqlDbType.Decimal);
                        AddParameter(cmd, "@costBasisMoney", position.Attribute("costBasisMoney")?.Value, SqlDbType.Decimal);
                        AddParameter(cmd, "@percentOfNAV", position.Attribute("percentOfNAV")?.Value, SqlDbType.Decimal);
                        AddParameter(cmd, "@fifoPnlUnrealized", position.Attribute("fifoPnlUnrealized")?.Value, SqlDbType.Decimal);
                        AddParameter(cmd, "@side", position.Attribute("side")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@levelOfDetail", position.Attribute("levelOfDetail")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@openDateTime", position.Attribute("openDateTime")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@holdingPeriodDateTime", position.Attribute("holdingPeriodDateTime")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@vestingDate", position.Attribute("vestingDate")?.Value, SqlDbType.Date);
                        AddParameter(cmd, "@code", position.Attribute("code")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@originatingOrderID", position.Attribute("originatingOrderID")?.Value, SqlDbType.BigInt);
                        AddParameter(cmd, "@originatingTransactionID", position.Attribute("originatingTransactionID")?.Value, SqlDbType.BigInt);
                        AddParameter(cmd, "@accruedInt", position.Attribute("accruedInt")?.Value, SqlDbType.Decimal);
                        AddParameter(cmd, "@serialNumber", position.Attribute("serialNumber")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@deliveryType", position.Attribute("deliveryType")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@commodityType", position.Attribute("commodityType")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@fineness", position.Attribute("fineness")?.Value, SqlDbType.Decimal);
                        AddParameter(cmd, "@weight", position.Attribute("weight")?.Value, SqlDbType.Decimal);

                        cmd.ExecuteNonQuery();
                    }
                }
                transaction.Commit();
                Console.WriteLine($"Successfully inserted {newPositionsCount} new open positions into the database.");
            }
        }
        catch (SqlException e)
        {
            Console.WriteLine($"\nDatabase error while inserting Open Positions: {e.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nAn error occurred during the Open Positions database operation: {ex.Message}");
        }
    }

    private static void InsertTradesIntoDatabase(XDocument reportXml, string connectionString)
    {
        try
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            Console.WriteLine("Successfully connected to the database.");

            var existingIbExecIDs = new HashSet<string>();
            using (SqlCommand cmd = new SqlCommand("SELECT ibExecID FROM dbo.Trades", connection))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        existingIbExecIDs.Add(reader.GetString(0));
                    }
                }
            }
            Console.WriteLine($"Found {existingIbExecIDs.Count} existing trades in the database.");

            var trades = reportXml.Descendants("Trade").ToList();
            int newTradesCount = 0;

            foreach (var trade in trades)
            {
                string ibExecID = trade.Attribute("ibExecID")?.Value;
                if (string.IsNullOrEmpty(ibExecID) || existingIbExecIDs.Contains(ibExecID))
                {
                    continue; // Skip if ibExecID is missing or already exists
                }

                newTradesCount++;
                using (SqlCommand cmd = new SqlCommand("INSERT INTO [dbo].[Trades] ([symbol], [securityID], [tradeID], [dateTime], [tradeDate], [quantity], [tradePrice], [ibCommission], [ibCommissionCurrency], [closePrice], [cost], [fifoPnlRealized], [buySell], [transactionID], [ibExecID], [brokerageOrderID], [exchOrderId], [extExecID], [orderType], [traderID], [currency], [description], [conid], [taxes], [assetCategory], [expiry], [transactionType], [exchange], [proceeds], [netCash], [mtmPnl], [origTradePrice], [origTradeDate], [origTradeID], [origOrderID], [origTransactionID], [ibOrderID], [openDateTime], [initialInvestment], [accountId], [acctAlias], [model], [fxRateToBase], [subCategory], [securityIDType], [cusip], [isin], [figi], [listingExchange], [underlyingConid], [underlyingSymbol], [underlyingSecurityID], [underlyingListingExchange], [issuer], [issuerCountryCode], [multiplier], [relatedTradeID], [strike], [reportDate], [putCall], [principalAdjustFactor], [settleDateTarget], [tradeMoney], [openCloseIndicator], [notes], [clearingFirmID], [relatedTransactionID], [rtn], [orderReference], [volatilityOrderLink], [orderTime], [holdingPeriodDateTime], [whenRealized], [whenReopened], [levelOfDetail], [changeInPrice], [changeInQuantity], [isAPIOrder], [accruedInt], [positionActionID], [serialNumber], [deliveryType], [commodityType], [fineness], [weight]) VALUES (@symbol, @securityID, @tradeID, @dateTime, @tradeDate, @quantity, @tradePrice, @ibCommission, @ibCommissionCurrency, @closePrice, @cost, @fifoPnlRealized, @buySell, @transactionID, @ibExecID, @brokerageOrderID, @exchOrderId, @extExecID, @orderType, @traderID, @currency, @description, @conid, @taxes, @assetCategory, @expiry, @transactionType, @exchange, @proceeds, @netCash, @mtmPnl, @origTradePrice, @origTradeDate, @origTradeID, @origOrderID, @origTransactionID, @ibOrderID, @openDateTime, @initialInvestment, @accountId, @acctAlias, @model, @fxRateToBase, @subCategory, @securityIDType, @cusip, @isin, @figi, @listingExchange, @underlyingConid, @underlyingSymbol, @underlyingSecurityID, @underlyingListingExchange, @issuer, @issuerCountryCode, @multiplier, @relatedTradeID, @strike, @reportDate, @putCall, @principalAdjustFactor, @settleDateTarget, @tradeMoney, @openCloseIndicator, @notes, @clearingFirmID, @relatedTransactionID, @rtn, @orderReference, @volatilityOrderLink, @orderTime, @holdingPeriodDateTime, @whenRealized, @whenReopened, @levelOfDetail, @changeInPrice, @changeInQuantity, @isAPIOrder, @accruedInt, @positionActionID, @serialNumber, @deliveryType, @commodityType, @fineness, @weight)", connection))
                {
                    AddParameter(cmd, "@symbol", trade.Attribute("symbol")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@securityID", trade.Attribute("securityID")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@tradeID", trade.Attribute("tradeID")?.Value, SqlDbType.BigInt);
                    AddParameter(cmd, "@dateTime", trade.Attribute("dateTime")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@tradeDate", trade.Attribute("tradeDate")?.Value, SqlDbType.Date);
                    AddParameter(cmd, "@quantity", trade.Attribute("quantity")?.Value, SqlDbType.Decimal);
                    AddParameter(cmd, "@tradePrice", trade.Attribute("tradePrice")?.Value, SqlDbType.Decimal);
                    AddParameter(cmd, "@ibCommission", trade.Attribute("ibCommission")?.Value, SqlDbType.Decimal);
                    AddParameter(cmd, "@ibCommissionCurrency", trade.Attribute("ibCommissionCurrency")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@closePrice", trade.Attribute("closePrice")?.Value, SqlDbType.Decimal);
                    AddParameter(cmd, "@cost", trade.Attribute("cost")?.Value, SqlDbType.Decimal);
                    AddParameter(cmd, "@fifoPnlRealized", trade.Attribute("fifoPnlRealized")?.Value, SqlDbType.Decimal);
                    AddParameter(cmd, "@buySell", trade.Attribute("buySell")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@transactionID", trade.Attribute("transactionID")?.Value, SqlDbType.BigInt);
                    AddParameter(cmd, "@ibExecID", ibExecID, SqlDbType.VarChar);
                    AddParameter(cmd, "@brokerageOrderID", trade.Attribute("brokerageOrderID")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@exchOrderId", trade.Attribute("exchOrderId")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@extExecID", trade.Attribute("extExecID")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@orderType", trade.Attribute("orderType")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@traderID", trade.Attribute("traderID")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@currency", trade.Attribute("currency")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@description", trade.Attribute("description")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@conid", trade.Attribute("conid")?.Value, SqlDbType.BigInt);
                    AddParameter(cmd, "@taxes", trade.Attribute("taxes")?.Value, SqlDbType.Decimal);
                    AddParameter(cmd, "@assetCategory", trade.Attribute("assetCategory")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@expiry", trade.Attribute("expiry")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@transactionType", trade.Attribute("transactionType")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@exchange", trade.Attribute("exchange")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@proceeds", trade.Attribute("proceeds")?.Value, SqlDbType.Decimal);
                    AddParameter(cmd, "@netCash", trade.Attribute("netCash")?.Value, SqlDbType.Decimal);
                    AddParameter(cmd, "@mtmPnl", trade.Attribute("mtmPnl")?.Value, SqlDbType.Decimal);
                    AddParameter(cmd, "@origTradePrice", trade.Attribute("origTradePrice")?.Value, SqlDbType.Decimal);
                    AddParameter(cmd, "@origTradeDate", trade.Attribute("origTradeDate")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@origTradeID", trade.Attribute("origTradeID")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@origOrderID", trade.Attribute("origOrderID")?.Value, SqlDbType.BigInt);
                    AddParameter(cmd, "@origTransactionID", trade.Attribute("origTransactionID")?.Value, SqlDbType.BigInt);
                    AddParameter(cmd, "@ibOrderID", trade.Attribute("ibOrderID")?.Value, SqlDbType.BigInt);
                    AddParameter(cmd, "@openDateTime", trade.Attribute("openDateTime")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@initialInvestment", trade.Attribute("initialInvestment")?.Value, SqlDbType.Decimal);
                    AddParameter(cmd, "@accountId", trade.Attribute("accountId")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@acctAlias", trade.Attribute("acctAlias")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@model", trade.Attribute("model")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@fxRateToBase", trade.Attribute("fxRateToBase")?.Value, SqlDbType.Decimal);
                    AddParameter(cmd, "@subCategory", trade.Attribute("subCategory")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@securityIDType", trade.Attribute("securityIDType")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@cusip", trade.Attribute("cusip")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@isin", trade.Attribute("isin")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@figi", trade.Attribute("figi")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@listingExchange", trade.Attribute("listingExchange")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@underlyingConid", trade.Attribute("underlyingConid")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@underlyingSymbol", trade.Attribute("underlyingSymbol")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@underlyingSecurityID", trade.Attribute("underlyingSecurityID")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@underlyingListingExchange", trade.Attribute("underlyingListingExchange")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@issuer", trade.Attribute("issuer")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@issuerCountryCode", trade.Attribute("issuerCountryCode")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@multiplier", trade.Attribute("multiplier")?.Value, SqlDbType.Int);
                    AddParameter(cmd, "@relatedTradeID", trade.Attribute("relatedTradeID")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@strike", trade.Attribute("strike")?.Value, SqlDbType.Decimal);
                    AddParameter(cmd, "@reportDate", trade.Attribute("reportDate")?.Value, SqlDbType.Date);
                    AddParameter(cmd, "@putCall", trade.Attribute("putCall")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@principalAdjustFactor", trade.Attribute("principalAdjustFactor")?.Value, SqlDbType.Decimal);
                    AddParameter(cmd, "@settleDateTarget", trade.Attribute("settleDateTarget")?.Value, SqlDbType.Date);
                    AddParameter(cmd, "@tradeMoney", trade.Attribute("tradeMoney")?.Value, SqlDbType.Decimal);
                    AddParameter(cmd, "@openCloseIndicator", trade.Attribute("openCloseIndicator")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@notes", trade.Attribute("notes")?.Value, SqlDbType.Text);
                    AddParameter(cmd, "@clearingFirmID", trade.Attribute("clearingFirmID")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@relatedTransactionID", trade.Attribute("relatedTransactionID")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@rtn", trade.Attribute("rtn")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@orderReference", trade.Attribute("orderReference")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@volatilityOrderLink", trade.Attribute("volatilityOrderLink")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@orderTime", trade.Attribute("orderTime")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@holdingPeriodDateTime", trade.Attribute("holdingPeriodDateTime")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@whenRealized", trade.Attribute("whenRealized")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@whenReopened", trade.Attribute("whenReopened")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@levelOfDetail", trade.Attribute("levelOfDetail")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@changeInPrice", trade.Attribute("changeInPrice")?.Value, SqlDbType.Decimal);
                    AddParameter(cmd, "@changeInQuantity", trade.Attribute("changeInQuantity")?.Value, SqlDbType.Decimal);
                    AddParameter(cmd, "@isAPIOrder", trade.Attribute("isAPIOrder")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@accruedInt", trade.Attribute("accruedInt")?.Value, SqlDbType.Decimal);
                    AddParameter(cmd, "@positionActionID", trade.Attribute("positionActionID")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@serialNumber", trade.Attribute("serialNumber")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@deliveryType", trade.Attribute("deliveryType")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@commodityType", trade.Attribute("commodityType")?.Value, SqlDbType.VarChar);
                    AddParameter(cmd, "@fineness", trade.Attribute("fineness")?.Value, SqlDbType.Decimal);
                    AddParameter(cmd, "@weight", trade.Attribute("weight")?.Value, SqlDbType.Decimal);

                    cmd.ExecuteNonQuery();
                }
            }
            Console.WriteLine($"Successfully inserted {newTradesCount} new trades into the database.");
        }
        catch (SqlException e)
        {
            Console.WriteLine($"\nDatabase error: {e.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nAn error occurred during the database operation: {ex.Message}");
        }
    }

    private static void AddParameter(SqlCommand cmd, string paramName, string value, SqlDbType dbType)
    {
        var param = cmd.Parameters.Add(paramName, dbType);
        if (string.IsNullOrEmpty(value))
        {
            param.Value = DBNull.Value;
        }
        else
        {
            if (dbType == SqlDbType.Decimal)
            {
                param.Value = decimal.Parse(value, CultureInfo.InvariantCulture);
            }
            else if (dbType == SqlDbType.Date)
            {
                param.Value = DateTime.ParseExact(value, "yyyyMMdd", CultureInfo.InvariantCulture);
            }
            else if (dbType == SqlDbType.DateTime)
            {
                param.Value = DateTime.ParseExact(value, "yyyyMMdd;HHmmss", CultureInfo.InvariantCulture);
            }
            else
            {
                param.Value = value;
            }
        }
    }
}
