using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using IKBR_Report_Puller.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace IKBR_Report_Puller.Services
{
    public class DataService : IDataService
    {
        public string ConnectionString { get; }

        public DataService(IConfiguration config)
        {
            var dbUser = config["Database:User"];
            var dbPassword = config["Database:Password"];
            var dbHost = config["Database:Host"];
            var dbName = config["Database:DbName"];
            ConnectionString = $"Server={dbHost};Database={dbName};User ID={dbUser};Password={dbPassword};TrustServerCertificate=True;";
        }

        public void InsertOpenPositions(XDocument reportXml)
        {
            try
            {
                using SqlConnection connection = new SqlConnection(ConnectionString);
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

        public void InsertTodayExecutions(XDocument reportXml)
        {
            try
            {
                using SqlConnection connection = new SqlConnection(ConnectionString);
                connection.Open();
                Console.WriteLine("Successfully connected to the database for Today's Executions.");

                var existingIbExecIDs = new HashSet<string>();
                using (SqlCommand cmd = new SqlCommand("SELECT ibExecID FROM dbo.TradeExecutions", connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            existingIbExecIDs.Add(reader.GetString(0));
                        }
                    }
                }
                Console.WriteLine($"Found {existingIbExecIDs.Count} existing trades executions in the database.");

                var trades = reportXml.Descendants("TradeConfirm")
                                    .Where(t => t.Attribute("levelOfDetail")?.Value == "EXECUTION")
                                    .ToList();
                int newTradesCount = 0;

                foreach (var trade in trades)
                {
                    string ibExecID = trade.Attribute("execID")?.Value;
                    if (string.IsNullOrEmpty(ibExecID) || existingIbExecIDs.Contains(ibExecID))
                    {
                        continue; // Skip if ibExecID is missing or already exists
                    }

                    newTradesCount++;
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO [dbo].[TradeExecutions] ([symbol], [securityID], [tradeID], [dateTime], [tradeDate], [quantity], [tradePrice], [ibCommission], [ibCommissionCurrency], [closePrice], [cost], [fifoPnlRealized], [buySell], [transactionID], [ibExecID], [brokerageOrderID], [exchOrderId], [extExecID], [orderType], [traderID], [currency], [description], [conid], [taxes], [assetCategory], [expiry], [transactionType], [exchange], [proceeds], [netCash], [mtmPnl], [origTradePrice], [origTradeDate], [origTradeID], [origOrderID], [origTransactionID], [ibOrderID], [openDateTime], [initialInvestment], [accountId], [acctAlias], [model], [fxRateToBase], [subCategory], [securityIDType], [cusip], [isin], [figi], [listingExchange], [underlyingConid], [underlyingSymbol], [underlyingSecurityID], [underlyingListingExchange], [issuer], [issuerCountryCode], [multiplier], [relatedTradeID], [strike], [reportDate], [putCall], [principalAdjustFactor], [settleDateTarget], [tradeMoney], [openCloseIndicator], [notes], [clearingFirmID], [relatedTransactionID], [rtn], [orderReference], [volatilityOrderLink], [orderTime], [holdingPeriodDateTime], [whenRealized], [whenReopened], [levelOfDetail], [changeInPrice], [changeInQuantity], [isAPIOrder], [accruedInt], [positionActionID], [serialNumber], [deliveryType], [commodityType], [fineness], [weight]) VALUES (@symbol, @securityID, @tradeID, @dateTime, @tradeDate, @quantity, @tradePrice, @ibCommission, @ibCommissionCurrency, @closePrice, @cost, @fifoPnlRealized, @buySell, @transactionID, @ibExecID, @brokerageOrderID, @exchOrderId, @extExecID, @orderType, @traderID, @currency, @description, @conid, @taxes, @assetCategory, @expiry, @transactionType, @exchange, @proceeds, @netCash, @mtmPnl, @origTradePrice, @origTradeDate, @origTradeID, @origOrderID, @origTransactionID, @ibOrderID, @openDateTime, @initialInvestment, @accountId, @acctAlias, @model, @fxRateToBase, @subCategory, @securityIDType, @cusip, @isin, @figi, @listingExchange, @underlyingConid, @underlyingSymbol, @underlyingSecurityID, @underlyingListingExchange, @issuer, @issuerCountryCode, @multiplier, @relatedTradeID, @strike, @reportDate, @putCall, @principalAdjustFactor, @settleDateTarget, @tradeMoney, @openCloseIndicator, @notes, @clearingFirmID, @relatedTransactionID, @rtn, @orderReference, @volatilityOrderLink, @orderTime, @holdingPeriodDateTime, @whenRealized, @whenReopened, @levelOfDetail, @changeInPrice, @changeInQuantity, @isAPIOrder, @accruedInt, @positionActionID, @serialNumber, @deliveryType, @commodityType, @fineness, @weight)", connection))
                    {
                        AddParameter(cmd, "@symbol", trade.Attribute("symbol")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@securityID", trade.Attribute("securityID")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@tradeID", trade.Attribute("tradeID")?.Value, SqlDbType.BigInt);
                        AddParameter(cmd, "@dateTime", trade.Attribute("dateTime")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@tradeDate", trade.Attribute("tradeDate")?.Value, SqlDbType.Date);
                        AddParameter(cmd, "@quantity", trade.Attribute("quantity")?.Value, SqlDbType.Decimal);
                        AddParameter(cmd, "@tradePrice", trade.Attribute("price")?.Value, SqlDbType.Decimal);
                        AddParameter(cmd, "@ibCommission", trade.Attribute("commission")?.Value, SqlDbType.Decimal);
                        AddParameter(cmd, "@ibCommissionCurrency", trade.Attribute("commissionCurrency")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@closePrice", null, SqlDbType.Decimal); // Not in TradeConfirm
                        AddParameter(cmd, "@cost", null, SqlDbType.Decimal); // Not in TradeConfirm
                        AddParameter(cmd, "@fifoPnlRealized", null, SqlDbType.Decimal); // Not in TradeConfirm
                        AddParameter(cmd, "@buySell", trade.Attribute("buySell")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@transactionID", null, SqlDbType.BigInt); // Not in TradeConfirm
                        AddParameter(cmd, "@ibExecID", ibExecID, SqlDbType.VarChar);
                        AddParameter(cmd, "@brokerageOrderID", trade.Attribute("brokerageOrderID")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@exchOrderId", null, SqlDbType.VarChar); // Not in TradeConfirm
                        AddParameter(cmd, "@extExecID", trade.Attribute("extExecID")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@orderType", trade.Attribute("orderType")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@traderID", trade.Attribute("traderID")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@currency", trade.Attribute("currency")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@description", trade.Attribute("description")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@conid", trade.Attribute("conid")?.Value, SqlDbType.BigInt);
                        AddParameter(cmd, "@taxes", trade.Attribute("tax")?.Value, SqlDbType.Decimal);
                        AddParameter(cmd, "@assetCategory", trade.Attribute("assetCategory")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@expiry", trade.Attribute("expiry")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@transactionType", trade.Attribute("transactionType")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@exchange", trade.Attribute("exchange")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@proceeds", trade.Attribute("proceeds")?.Value, SqlDbType.Decimal);
                        AddParameter(cmd, "@netCash", trade.Attribute("netCash")?.Value, SqlDbType.Decimal);
                        AddParameter(cmd, "@mtmPnl", null, SqlDbType.Decimal); // Not in TradeConfirm
                        AddParameter(cmd, "@origTradePrice", trade.Attribute("origTradePrice")?.Value, SqlDbType.Decimal);
                        AddParameter(cmd, "@origTradeDate", trade.Attribute("origTradeDate")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@origTradeID", trade.Attribute("origTradeID")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@origOrderID", null, SqlDbType.BigInt); // Not in TradeConfirm
                        AddParameter(cmd, "@origTransactionID", null, SqlDbType.BigInt); // Not in TradeConfirm
                        AddParameter(cmd, "@ibOrderID", trade.Attribute("orderID")?.Value, SqlDbType.BigInt);
                        AddParameter(cmd, "@openDateTime", null, SqlDbType.VarChar); // Not in TradeConfirm
                        AddParameter(cmd, "@initialInvestment", null, SqlDbType.Decimal); // Not in TradeConfirm
                        AddParameter(cmd, "@accountId", trade.Attribute("accountId")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@acctAlias", trade.Attribute("acctAlias")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@model", trade.Attribute("model")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@fxRateToBase", null, SqlDbType.Decimal); // Not in TradeConfirm
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
                        AddParameter(cmd, "@relatedTradeID", null, SqlDbType.VarChar); // Not in TradeConfirm
                        AddParameter(cmd, "@strike", trade.Attribute("strike")?.Value, SqlDbType.Decimal);
                        AddParameter(cmd, "@reportDate", trade.Attribute("reportDate")?.Value, SqlDbType.Date);
                        AddParameter(cmd, "@putCall", trade.Attribute("putCall")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@principalAdjustFactor", trade.Attribute("principalAdjustFactor")?.Value, SqlDbType.Decimal);
                        AddParameter(cmd, "@settleDateTarget", trade.Attribute("settleDate")?.Value, SqlDbType.Date);
                        AddParameter(cmd, "@tradeMoney", trade.Attribute("amount")?.Value, SqlDbType.Decimal);
                        
                        string code = trade.Attribute("code")?.Value;
                        string openCloseIndicator = "";
                        string notes = "";
                        if (!string.IsNullOrEmpty(code))
                        {
                            foreach (var part in code.Split(';'))
                            {
                                if (part.Trim().Equals("O", StringComparison.OrdinalIgnoreCase))
                                {
                                    openCloseIndicator += "O";
                                }
                                else if (part.Trim().Equals("C", StringComparison.OrdinalIgnoreCase))
                                {
                                    openCloseIndicator += "C";
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(notes))
                                    {
                                        notes += ";";
                                    }
                                    notes += part.Trim();
                                }
                            }
                        }
                        AddParameter(cmd, "@openCloseIndicator", openCloseIndicator, SqlDbType.VarChar);

                        AddParameter(cmd, "@notes", string.IsNullOrEmpty(notes) ? null : notes, SqlDbType.Text); // Not in TradeConfirm
                        AddParameter(cmd, "@clearingFirmID", trade.Attribute("clearingFirmID")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@relatedTransactionID", null, SqlDbType.VarChar); // Not in TradeConfirm
                        AddParameter(cmd, "@rtn", null, SqlDbType.VarChar); // Not in TradeConfirm
                        AddParameter(cmd, "@orderReference", trade.Attribute("orderReference")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@volatilityOrderLink", trade.Attribute("volatilityOrderLink")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@orderTime", trade.Attribute("orderTime")?.Value, SqlDbType.VarChar);
                        AddParameter(cmd, "@holdingPeriodDateTime", null, SqlDbType.VarChar); // Not in TradeConfirm
                        AddParameter(cmd, "@whenRealized", null, SqlDbType.VarChar); // Not in TradeConfirm
                        AddParameter(cmd, "@whenReopened", null, SqlDbType.VarChar); // Not in TradeConfirm
                        AddParameter(cmd, "@levelOfDetail", trade.Attribute("levelOfDetail")?.Value + "_TODAY", SqlDbType.VarChar);
                        AddParameter(cmd, "@changeInPrice", null, SqlDbType.Decimal); // Not in TradeConfirm
                        AddParameter(cmd, "@changeInQuantity", null, SqlDbType.Decimal); // Not in TradeConfirm
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
                Console.WriteLine($"Successfully inserted {newTradesCount} new trades executions from Today's Executions into the database.");
            }
            catch (SqlException e)
            {
                Console.WriteLine($"\nDatabase error while inserting Today's Executions: {e.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nAn error occurred during the Today's Executions database operation: {ex.Message}");
            }
        }

        public void InsertTradeExecutions(XDocument reportXml)
        {
            try
            {
                using SqlConnection connection = new SqlConnection(ConnectionString);
                connection.Open();
                Console.WriteLine("Successfully connected to the database.");

                var existingTrades = new Dictionary<string, string>();
                using (SqlCommand cmd = new SqlCommand("SELECT ibExecID, levelOfDetail FROM dbo.TradeExecutions", connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (!reader.IsDBNull(0))
                            {
                                existingTrades.Add(reader.GetString(0), reader.IsDBNull(1) ? null : reader.GetString(1));
                            }
                        }
                    }
                }
                Console.WriteLine($"Found {existingTrades.Count} existing trades executions in the database.");

                var trades = reportXml.Descendants("Trade").ToList();
                int newTradesCount = 0;
                int updatedTradesCount = 0;

                foreach (var trade in trades)
                {
                    string ibExecID = trade.Attribute("ibExecID")?.Value;
                    if (string.IsNullOrEmpty(ibExecID))
                    {
                        continue; // Skip if ibExecID is missing
                    }

                    if (existingTrades.TryGetValue(ibExecID, out var levelOfDetail))
                    {
                        if (levelOfDetail == "EXECUTION_TODAY")
                        {
                            // UPDATE logic
                            using (SqlCommand cmd = new SqlCommand("UPDATE [dbo].[TradeExecutions] SET [symbol] = @symbol, [securityID] = @securityID, [tradeID] = @tradeID, [dateTime] = @dateTime, [tradeDate] = @tradeDate, [quantity] = @quantity, [tradePrice] = @tradePrice, [ibCommission] = @ibCommission, [ibCommissionCurrency] = @ibCommissionCurrency, [closePrice] = @closePrice, [cost] = @cost, [fifoPnlRealized] = @fifoPnlRealized, [buySell] = @buySell, [transactionID] = @transactionID, [brokerageOrderID] = @brokerageOrderID, [exchOrderId] = @exchOrderId, [extExecID] = @extExecID, [orderType] = @orderType, [traderID] = @traderID, [currency] = @currency, [description] = @description, [conid] = @conid, [taxes] = @taxes, [assetCategory] = @assetCategory, [expiry] = @expiry, [transactionType] = @transactionType, [exchange] = @exchange, [proceeds] = @proceeds, [netCash] = @netCash, [mtmPnl] = @mtmPnl, [origTradePrice] = @origTradePrice, [origTradeDate] = @origTradeDate, [origTradeID] = @origTradeID, [origOrderID] = @origOrderID, [origTransactionID] = @origTransactionID, [ibOrderID] = @ibOrderID, [openDateTime] = @openDateTime, [initialInvestment] = @initialInvestment, [accountId] = @accountId, [acctAlias] = @acctAlias, [model] = @model, [fxRateToBase] = @fxRateToBase, [subCategory] = @subCategory, [securityIDType] = @securityIDType, [cusip] = @cusip, [isin] = @isin, [figi] = @figi, [listingExchange] = @listingExchange, [underlyingConid] = @underlyingConid, [underlyingSymbol] = @underlyingSymbol, [underlyingSecurityID] = @underlyingSecurityID, [underlyingListingExchange] = @underlyingListingExchange, [issuer] = @issuer, [issuerCountryCode] = @issuerCountryCode, [multiplier] = @multiplier, [relatedTradeID] = @relatedTradeID, [strike] = @strike, [reportDate] = @reportDate, [putCall] = @putCall, [principalAdjustFactor] = @principalAdjustFactor, [settleDateTarget] = @settleDateTarget, [tradeMoney] = @tradeMoney, [openCloseIndicator] = @openCloseIndicator, [notes] = @notes, [clearingFirmID] = @clearingFirmID, [relatedTransactionID] = @relatedTransactionID, [rtn] = @rtn, [orderReference] = @orderReference, [volatilityOrderLink] = @volatilityOrderLink, [orderTime] = @orderTime, [holdingPeriodDateTime] = @holdingPeriodDateTime, [whenRealized] = @whenRealized, [whenReopened] = @whenReopened, [levelOfDetail] = @levelOfDetail, [changeInPrice] = @changeInPrice, [changeInQuantity] = @changeInQuantity, [isAPIOrder] = @isAPIOrder, [accruedInt] = @accruedInt, [positionActionID] = @positionActionID, [serialNumber] = @serialNumber, [deliveryType] = @deliveryType, [commodityType] = @commodityType, [fineness] = @fineness, [weight] = @weight WHERE [ibExecID] = @ibExecID", connection))
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
                                AddParameter(cmd, "@ibExecID", ibExecID, SqlDbType.VarChar);
                                cmd.ExecuteNonQuery();
                                updatedTradesCount++;
                            }
                        }
                        continue;
                    }

                    newTradesCount++;
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO [dbo].[TradeExecutions] ([symbol], [securityID], [tradeID], [dateTime], [tradeDate], [quantity], [tradePrice], [ibCommission], [ibCommissionCurrency], [closePrice], [cost], [fifoPnlRealized], [buySell], [transactionID], [ibExecID], [brokerageOrderID], [exchOrderId], [extExecID], [orderType], [traderID], [currency], [description], [conid], [taxes], [assetCategory], [expiry], [transactionType], [exchange], [proceeds], [netCash], [mtmPnl], [origTradePrice], [origTradeDate], [origTradeID], [origOrderID], [origTransactionID], [ibOrderID], [openDateTime], [initialInvestment], [accountId], [acctAlias], [model], [fxRateToBase], [subCategory], [securityIDType], [cusip], [isin], [figi], [listingExchange], [underlyingConid], [underlyingSymbol], [underlyingSecurityID], [underlyingListingExchange], [issuer], [issuerCountryCode], [multiplier], [relatedTradeID], [strike], [reportDate], [putCall], [principalAdjustFactor], [settleDateTarget], [tradeMoney], [openCloseIndicator], [notes], [clearingFirmID], [relatedTransactionID], [rtn], [orderReference], [volatilityOrderLink], [orderTime], [holdingPeriodDateTime], [whenRealized], [whenReopened], [levelOfDetail], [changeInPrice], [changeInQuantity], [isAPIOrder], [accruedInt], [positionActionID], [serialNumber], [deliveryType], [commodityType], [fineness], [weight]) VALUES (@symbol, @securityID, @tradeID, @dateTime, @tradeDate, @quantity, @tradePrice, @ibCommission, @ibCommissionCurrency, @closePrice, @cost, @fifoPnlRealized, @buySell, @transactionID, @ibExecID, @brokerageOrderID, @exchOrderId, @extExecID, @orderType, @traderID, @currency, @description, @conid, @taxes, @assetCategory, @expiry, @transactionType, @exchange, @proceeds, @netCash, @mtmPnl, @origTradePrice, @origTradeDate, @origTradeID, @origOrderID, @origTransactionID, @ibOrderID, @openDateTime, @initialInvestment, @accountId, @acctAlias, @model, @fxRateToBase, @subCategory, @securityIDType, @cusip, @isin, @figi, @listingExchange, @underlyingConid, @underlyingSymbol, @underlyingSecurityID, @underlyingListingExchange, @issuer, @issuerCountryCode, @multiplier, @relatedTradeID, @strike, @reportDate, @putCall, @principalAdjustFactor, @settleDateTarget, @tradeMoney, @openCloseIndicator, @notes, @clearingFirmID, @relatedTransactionID, @rtn, @orderReference, @volatilityOrderLink, @orderTime, @holdingPeriodDateTime, @whenRealized, @whenReopened, @levelOfDetail, @changeInPrice, @changeInQuantity, @isAPIOrder, @accruedInt, @positionActionID, @serialNumber, @deliveryType, @commodityType, @fineness, @weight)", connection))
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
                if (updatedTradesCount > 0)
                {
                    Console.WriteLine($"Successfully updated {updatedTradesCount} trades executions in the database.");
                }
                Console.WriteLine($"Successfully inserted {newTradesCount} new trades executions into the database.");
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

        public void UpsertTimeSeriesData(string instrumentName, string provider, string dataName, string dataSource, string format, string frequency, string currency, DateTime date, double openPrice, double closePrice, double lowPrice, double highPrice, double volume)
        {
            using SqlConnection connection = new SqlConnection(ConnectionString);
            connection.Open();

            // Check if the instrument exists
            string instrumentQuery = "SELECT Id FROM Instruments WHERE InstrumentName = @InstrumentName";
            int? instrumentId;

            using (SqlCommand cmd = new SqlCommand(instrumentQuery, connection))
            {
                cmd.Parameters.AddWithValue("@InstrumentName", instrumentName);
                instrumentId = cmd.ExecuteScalar() as int?;
            }

            // Insert the instrument if it doesn't exist
            if (!instrumentId.HasValue)
            {
                string insertInstrumentQuery = @"INSERT INTO Instruments (InstrumentName, Provider, DataName, DataSource, Format, Frequency, Currency)
                                                OUTPUT INSERTED.Id
                                                VALUES (@InstrumentName, @Provider, @DataName, @DataSource, @Format, @Frequency, @Currency)";

                using (SqlCommand cmd = new SqlCommand(insertInstrumentQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@InstrumentName", instrumentName);
                    cmd.Parameters.AddWithValue("@Provider", provider);
                    cmd.Parameters.AddWithValue("@DataName", dataName);
                    cmd.Parameters.AddWithValue("@DataSource", dataSource);
                    cmd.Parameters.AddWithValue("@Format", format);
                    cmd.Parameters.AddWithValue("@Frequency", frequency);
                    cmd.Parameters.AddWithValue("@Currency", currency);

                    instrumentId = (int)cmd.ExecuteScalar();
                }
            }

            // Check if the historical data exists
            string historicalDataQuery = "SELECT COUNT(*) FROM HistoricalData WHERE InstrumentId = @InstrumentId AND Date = @Date";
            int count;

            using (SqlCommand cmd = new SqlCommand(historicalDataQuery, connection))
            {
                cmd.Parameters.AddWithValue("@InstrumentId", instrumentId);
                cmd.Parameters.AddWithValue("@Date", date);
                count = (int)cmd.ExecuteScalar();
            }

            // Insert the historical data if it doesn't exist
            if (count == 0)
            {
                string insertHistoricalDataQuery = @"INSERT INTO HistoricalData (Date, OpenPrice, ClosePrice, LowPrice, HighPrice, Volume, InstrumentId)
                                                    VALUES (@Date, @OpenPrice, @ClosePrice, @LowPrice, @HighPrice, @Volume, @InstrumentId)";

                using (SqlCommand cmd = new SqlCommand(insertHistoricalDataQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@Date", date);
                    cmd.Parameters.AddWithValue("@OpenPrice", openPrice);
                    cmd.Parameters.AddWithValue("@ClosePrice", closePrice);
                    cmd.Parameters.AddWithValue("@LowPrice", lowPrice);
                    cmd.Parameters.AddWithValue("@HighPrice", highPrice);
                    cmd.Parameters.AddWithValue("@Volume", volume);
                    cmd.Parameters.AddWithValue("@InstrumentId", instrumentId);

                    cmd.ExecuteNonQuery();
                }
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
}
