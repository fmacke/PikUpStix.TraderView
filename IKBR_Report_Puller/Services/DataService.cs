using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using IKBR_Report_Puller.Domain;
using IKBR_Report_Puller.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace IKBR_Report_Puller.Services
{
    public class DataService : IDataService
    {
        private readonly string _connectionString;

        public DataService(IConfiguration config)
        {
            var dbUser = config["Database:User"];
            var dbPassword = config["Database:Password"];
            var dbHost = config["Database:Host"];
            var dbName = config["Database:DbName"];
            _connectionString = $"Server={dbHost};Database={dbName};User ID={dbUser};Password={dbPassword};TrustServerCertificate=True;";
        }

        public void InsertOpenPositions(IKBRReport report)
        {
            ExecuteDatabaseOperation(connection =>
            {
                if (report == null || string.IsNullOrEmpty(report.AccountId))
                {
                    Console.WriteLine("Report or accountId is missing. Skipping Open Positions insert.");
                    return;
                }

                if (!report.OpenPositions.Any())
                {
                    Console.WriteLine("No open positions found in the report.");
                    return;
                }

                using (var transaction = connection.BeginTransaction())
                {
                    int newPositionsCount = 0;
                    foreach (var position in report.OpenPositions)
                    {
                        newPositionsCount++;
                        ExecuteInsertCommand(connection, transaction, "INSERT INTO [dbo].[OpenPositions] ([whenGenerated], [accountId], [acctAlias], [model], [currency], [fxRateToBase], [assetCategory], [subCategory], [symbol], [description], [conid], [securityID], [securityIDType], [cusip], [isin], [figi], [listingExchange], [underlyingConid], [underlyingSymbol], [underlyingSecurityID], [underlyingListingExchange], [issuer], [issuerCountryCode], [multiplier], [strike], [expiry], [putCall], [principalAdjustFactor], [reportDate], [position], [markPrice], [positionValue], [openPrice], [costBasisPrice], [costBasisMoney], [percentOfNAV], [fifoPnlUnrealized], [side], [levelOfDetail], [openDateTime], [holdingPeriodDateTime], [vestingDate], [code], [originatingOrderID], [originatingTransactionID], [accruedInt], [serialNumber], [deliveryType], [commodityType], [fineness], [weight]) VALUES (@whenGenerated, @accountId, @acctAlias, @model, @currency, @fxRateToBase, @assetCategory, @subCategory, @symbol, @description, @conid, @securityID, @securityIDType, @cusip, @isin, @figi, @listingExchange, @underlyingConid, @underlyingSymbol, @underlyingSecurityID, @underlyingListingExchange, @issuer, @issuerCountryCode, @multiplier, @strike, @expiry, @putCall, @principalAdjustFactor, @reportDate, @position, @markPrice, @positionValue, @openPrice, @costBasisPrice, @costBasisMoney, @percentOfNAV, @fifoPnlUnrealized, @side, @levelOfDetail, @openDateTime, @holdingPeriodDateTime, @vestingDate, @code, @originatingOrderID, @originatingTransactionID, @accruedInt, @serialNumber, @deliveryType, @commodityType, @fineness, @weight)",
                            new Dictionary<string, object>
                            {
                                { "@whenGenerated", report.WhenGenerated },
                                { "@accountId", position.AccountId },
                                { "@acctAlias", position.AcctAlias },
                                { "@model", position.Model },
                                { "@currency", position.Currency },
                                { "@fxRateToBase", position.FxRateToBase },
                                { "@assetCategory", position.AssetCategory },
                                { "@subCategory", position.SubCategory },
                                { "@symbol", position.Symbol },
                                { "@description", position.Description },
                                { "@conid", position.Conid },
                                { "@securityID", position.SecurityID },
                                { "@securityIDType", position.SecurityIDType },
                                { "@cusip", position.Cusip },
                                { "@isin", position.Isin },
                                { "@figi", position.Figi },
                                { "@listingExchange", position.ListingExchange },
                                { "@underlyingConid", position.UnderlyingConid },
                                { "@underlyingSymbol", position.UnderlyingSymbol },
                                { "@underlyingSecurityID", position.UnderlyingSecurityID },
                                { "@underlyingListingExchange", position.UnderlyingListingExchange },
                                { "@issuer", position.Issuer },
                                { "@issuerCountryCode", position.IssuerCountryCode },
                                { "@multiplier", position.Multiplier },
                                { "@strike", position.Strike },
                                { "@expiry", position.Expiry },
                                { "@putCall", position.PutCall },
                                { "@principalAdjustFactor", position.PrincipalAdjustFactor },
                                { "@reportDate", position.ReportDate },
                                { "@position", position.Position },
                                { "@markPrice", position.MarkPrice },
                                { "@positionValue", position.PositionValue },
                                { "@openPrice", position.OpenPrice },
                                { "@costBasisPrice", position.CostBasisPrice },
                                { "@costBasisMoney", position.CostBasisMoney },
                                { "@percentOfNAV", position.PercentOfNAV },
                                { "@fifoPnlUnrealized", position.FifoPnlUnrealized },
                                { "@side", position.Side },
                                { "@levelOfDetail", position.LevelOfDetail },
                                { "@openDateTime", position.OpenDateTime },
                                { "@holdingPeriodDateTime", position.HoldingPeriodDateTime },
                                { "@vestingDate", position.VestingDate },
                                { "@code", position.Code },
                                { "@originatingOrderID", position.OriginatingOrderID },
                                { "@originatingTransactionID", position.OriginatingTransactionID },
                                { "@accruedInt", position.AccruedInt },
                                { "@serialNumber", position.SerialNumber },
                                { "@deliveryType", position.DeliveryType },
                                { "@commodityType", position.CommodityType },
                                { "@fineness", position.Fineness },
                                { "@weight", position.Weight }
                            });
                    }
                    transaction.Commit();
                    Console.WriteLine($"Successfully inserted {newPositionsCount} new open positions into the database.");
                }
            });
        }

        public void InsertTradeExecutions(IKBRReport report)
        {
            ExecuteDatabaseOperation(connection =>
            {
                if (report == null || !report.Trades.Any())
                {
                    Console.WriteLine("No trades found in the report.");
                    return;
                }

                var existingTrades = new HashSet<string>();
                using (SqlCommand cmd = new SqlCommand("SELECT ibExecID FROM dbo.TradeExecutions", connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        existingTrades.Add(reader.GetString(0));
                    }
                }

                using (var transaction = connection.BeginTransaction())
                {
                    foreach (var trade in report.Trades)
                    {
                        string ibExecID = trade.IbExecID;
                        if (string.IsNullOrEmpty(ibExecID))
                        {
                            continue;
                        }

                        // Check if the execID already exists in the database
                        using (var checkCmd = new SqlCommand("SELECT COUNT(*) FROM dbo.TradeExecutions WHERE ibExecID = @ibExecID", connection, transaction))
                        {
                            checkCmd.Parameters.AddWithValue("@ibExecID", ibExecID);
                            int count = (int)checkCmd.ExecuteScalar();

                            if (count > 0)
                            {
                                // Check if securityID, tradeID, and dateTime are null
                                using (var selectCmd = new SqlCommand("SELECT securityID, tradeID, dateTime FROM dbo.TradeExecutions WHERE ibExecID = @ibExecID", connection, transaction))
                                {
                                    selectCmd.Parameters.AddWithValue("@ibExecID", ibExecID);
                                    using (var reader = selectCmd.ExecuteReader())
                                    {
                                        if (reader.Read())
                                        {
                                            var securityID = reader["securityID"] as string;
                                            var tradeID = reader["tradeID"] as long?;
                                            var dateTime = reader["dateTime"] as string;

                                            if (string.IsNullOrEmpty(securityID) && tradeID == null && string.IsNullOrEmpty(dateTime))
                                            {
                                                // Update existing row
                                                reader.Close();
                                                ExecuteInsertCommand(connection, transaction, @"
                                    UPDATE dbo.TradeExecutions
                                    SET symbol = @symbol,
                                        securityID = @securityID,
                                        tradeID = @tradeID,
                                        dateTime = @dateTime,
                                        tradeDate = @tradeDate,
                                        quantity = @quantity,
                                        tradePrice = @tradePrice,
                                        ibCommission = @ibCommission,
                                        ibCommissionCurrency = @ibCommissionCurrency,
                                        closePrice = @closePrice,
                                        cost = @cost,
                                        fifoPnlRealized = @fifoPnlRealized,
                                        buySell = @buySell,
                                        transactionID = @transactionID,
                                        brokerageOrderID = @brokerageOrderID,
                                        exchOrderId = @exchOrderId,
                                        extExecID = @extExecID,
                                        orderType = @orderType,
                                        traderID = @traderID,
                                        currency = @currency,
                                        description = @description,
                                        conid = @conid,
                                        taxes = @taxes,
                                        assetCategory = @assetCategory,
                                        expiry = @expiry,
                                        transactionType = @transactionType,
                                        exchange = @exchange,
                                        proceeds = @proceeds,
                                        netCash = @netCash,
                                        mtmPnl = @mtmPnl,
                                        origTradePrice = @origTradePrice,
                                        origTradeDate = @origTradeDate,
                                        origTradeID = @origTradeID,
                                        origOrderID = @origOrderID,
                                        origTransactionID = @origTransactionID,
                                        ibOrderID = @ibOrderID,
                                        openDateTime = @openDateTime,
                                        initialInvestment = @initialInvestment,
                                        accountId = @accountId,
                                        acctAlias = @acctAlias,
                                        model = @model,
                                        fxRateToBase = @fxRateToBase,
                                        subCategory = @subCategory,
                                        securityIDType = @securityIDType,
                                        cusip = @cusip,
                                        isin = @isin,
                                        figi = @figi,
                                        listingExchange = @listingExchange,
                                        underlyingConid = @underlyingConid,
                                        underlyingSymbol = @underlyingSymbol,
                                        underlyingSecurityID = @underlyingSecurityID,
                                        underlyingListingExchange = @underlyingListingExchange,
                                        issuer = @issuer,
                                        issuerCountryCode = @issuerCountryCode,
                                        multiplier = @multiplier,
                                        relatedTradeID = @relatedTradeID,
                                        strike = @strike,
                                        reportDate = @reportDate,
                                        putCall = @putCall,
                                        principalAdjustFactor = @principalAdjustFactor,
                                        settleDateTarget = @settleDateTarget,
                                        tradeMoney = @tradeMoney,
                                        openCloseIndicator = @openCloseIndicator,
                                        notes = @notes,
                                        clearingFirmID = @clearingFirmID,
                                        relatedTransactionID = @relatedTransactionID,
                                        rtn = @rtn,
                                        orderReference = @orderReference,
                                        volatilityOrderLink = @volatilityOrderLink,
                                        orderTime = @orderTime,
                                        holdingPeriodDateTime = @holdingPeriodDateTime,
                                        whenRealized = @whenRealized,
                                        whenReopened = @whenReopened,
                                        levelOfDetail = @levelOfDetail,
                                        changeInPrice = @changeInPrice,
                                        changeInQuantity = @changeInQuantity,
                                        isAPIOrder = @isAPIOrder,
                                        accruedInt = @accruedInt,
                                        positionActionID = @positionActionID,
                                        serialNumber = @serialNumber,
                                        deliveryType = @deliveryType,
                                        commodityType = @commodityType,
                                        fineness = @fineness,
                                        weight = @weight
                                    WHERE ibExecID = @ibExecID", GetTradeParameters(trade));
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // Insert new row
                                ExecuteInsertCommand(connection, transaction, @"INSERT INTO [dbo].[TradeExecutions]
                                    ([symbol], [securityID], [tradeID], [dateTime], [tradeDate], [quantity], [tradePrice], [ibCommission], [ibCommissionCurrency],
                                     [closePrice], [cost], [fifoPnlRealized], [buySell], [transactionID], [ibExecID], [brokerageOrderID], [exchOrderId], [extExecID],
                                     [orderType], [traderID], [currency], [description], [conid], [taxes], [assetCategory], [expiry], [transactionType], [exchange],
                                     [proceeds], [netCash], [mtmPnl], [origTradePrice], [origTradeDate], [origTradeID], [origOrderID], [origTransactionID], [ibOrderID],
                                     [openDateTime], [initialInvestment], [accountId], [acctAlias], [model], [fxRateToBase], [subCategory], [securityIDType], [cusip],
                                     [isin], [figi], [listingExchange], [underlyingConid], [underlyingSymbol], [underlyingSecurityID], [underlyingListingExchange],
                                     [issuer], [issuerCountryCode], [multiplier], [relatedTradeID], [strike], [reportDate], [putCall], [principalAdjustFactor],
                                     [settleDateTarget], [tradeMoney], [openCloseIndicator], [notes], [clearingFirmID], [relatedTransactionID], [rtn], [orderReference],
                                     [volatilityOrderLink], [orderTime], [holdingPeriodDateTime], [whenRealized], [whenReopened], [levelOfDetail], [changeInPrice],
                                     [changeInQuantity], [isAPIOrder], [accruedInt], [positionActionID], [serialNumber], [deliveryType], [commodityType], [fineness], [weight])
                                    VALUES
                                    (@symbol, @securityID, @tradeID, @dateTime, @tradeDate, @quantity, @tradePrice, @ibCommission, @ibCommissionCurrency,
                                     @closePrice, @cost, @fifoPnlRealized, @buySell, @transactionID, @ibExecID, @brokerageOrderID, @exchOrderId, @extExecID,
                                     @orderType, @traderID, @currency, @description, @conid, @taxes, @assetCategory, @expiry, @transactionType, @exchange,
                                     @proceeds, @netCash, @mtmPnl, @origTradePrice, @origTradeDate, @origTradeID, @origOrderID, @origTransactionID, @ibOrderID,
                                     @openDateTime, @initialInvestment, @accountId, @acctAlias, @model, @fxRateToBase, @subCategory, @securityIDType, @cusip,
                                     @isin, @figi, @listingExchange, @underlyingConid, @underlyingSymbol, @underlyingSecurityID, @underlyingListingExchange,
                                     @issuer, @issuerCountryCode, @multiplier, @relatedTradeID, @strike, @reportDate, @putCall, @principalAdjustFactor,
                                     @settleDateTarget, @tradeMoney, @openCloseIndicator, @notes, @clearingFirmID, @relatedTransactionID, @rtn, @orderReference,
                                     @volatilityOrderLink, @orderTime, @holdingPeriodDateTime, @whenRealized, @whenReopened, @levelOfDetail, @changeInPrice,
                                     @changeInQuantity, @isAPIOrder, @accruedInt, @positionActionID, @serialNumber, @deliveryType, @commodityType, @fineness, @weight)",
                                    GetTradeParameters(trade));
                            }
                        }
                    }
                    transaction.Commit();
                }

                Console.WriteLine($"Successfully inserted {report.Trades.Count} trades into the database.");
            });
        }

        private Dictionary<string, object> GetTradeParameters(Trade trade)
        {
            return new Dictionary<string, object>
            {
                { "@symbol", trade.Symbol },
                { "@securityID", trade.SecurityID },
                { "@tradeID", trade.TradeID },
                { "@dateTime", trade.DateTime },
                { "@tradeDate", trade.TradeDate },
                { "@quantity", trade.Quantity },
                { "@tradePrice", trade.TradePrice },
                { "@ibCommission", trade.IbCommission },
                { "@ibCommissionCurrency", trade.IbCommissionCurrency },
                { "@closePrice", trade.ClosePrice },
                { "@cost", trade.Cost },
                { "@fifoPnlRealized", trade.FifoPnlRealized },
                { "@buySell", trade.BuySell },
                { "@transactionID", trade.TransactionID },
                { "@ibExecID", trade.IbExecID },
                { "@brokerageOrderID", trade.BrokerageOrderID },
                { "@exchOrderId", trade.ExchOrderId },
                { "@extExecID", trade.ExtExecID },
                { "@orderType", trade.OrderType },
                { "@traderID", trade.TraderID },
                { "@currency", trade.Currency },
                { "@description", trade.Description },
                { "@conid", trade.Conid },
                { "@taxes", trade.Taxes },
                { "@assetCategory", trade.AssetCategory },
                { "@expiry", trade.Expiry },
                { "@transactionType", trade.TransactionType },
                { "@exchange", trade.Exchange },
                { "@proceeds", trade.Proceeds },
                { "@netCash", trade.NetCash },
                { "@mtmPnl", trade.MtmPnl },
                { "@origTradePrice", trade.OrigTradePrice },
                { "@origTradeDate", trade.OrigTradeDate },
                { "@origTradeID", trade.OrigTradeID },
                { "@origOrderID", trade.OrigOrderID },
                { "@origTransactionID", trade.OrigTransactionID },
                { "@ibOrderID", trade.IbOrderID },
                { "@openDateTime", trade.OpenDateTime },
                { "@initialInvestment", trade.InitialInvestment },
                { "@accountId", trade.AccountId },
                { "@acctAlias", trade.AcctAlias },
                { "@model", trade.Model },
                { "@fxRateToBase", trade.FxRateToBase },
                { "@subCategory", trade.SubCategory },
                { "@securityIDType", trade.SecurityIDType },
                { "@cusip", trade.Cusip },
                { "@isin", trade.Isin },
                { "@figi", trade.Figi },
                { "@listingExchange", trade.ListingExchange },
                { "@underlyingConid", trade.UnderlyingConid },
                { "@underlyingSymbol", trade.UnderlyingSymbol },
                { "@underlyingSecurityID", trade.UnderlyingSecurityID },
                { "@underlyingListingExchange", trade.UnderlyingListingExchange },
                { "@issuer", trade.Issuer },
                { "@issuerCountryCode", trade.IssuerCountryCode },
                { "@multiplier", trade.Multiplier },
                { "@relatedTradeID", trade.RelatedTradeID },
                { "@strike", trade.Strike },
                { "@reportDate", trade.ReportDate },
                { "@putCall", trade.PutCall },
                { "@principalAdjustFactor", trade.PrincipalAdjustFactor },
                { "@settleDateTarget", trade.SettleDateTarget },
                { "@tradeMoney", trade.TradeMoney },
                { "@openCloseIndicator", trade.OpenCloseIndicator },
                { "@notes", trade.Notes },
                { "@clearingFirmID", trade.ClearingFirmID },
                { "@relatedTransactionID", trade.RelatedTransactionID },
                { "@rtn", trade.Rtn },
                { "@orderReference", trade.OrderReference },
                { "@volatilityOrderLink", trade.VolatilityOrderLink },
                { "@orderTime", trade.OrderTime },
                { "@holdingPeriodDateTime", trade.HoldingPeriodDateTime },
                { "@whenRealized", trade.WhenRealized },
                { "@whenReopened", trade.WhenReopened },
                { "@levelOfDetail", trade.LevelOfDetail },
                { "@changeInPrice", trade.ChangeInPrice },
                { "@changeInQuantity", trade.ChangeInQuantity },
                { "@isAPIOrder", trade.IsAPIOrder },
                { "@accruedInt", trade.AccruedInt },
                { "@positionActionID", trade.PositionActionID },
                { "@serialNumber", trade.SerialNumber },
                { "@deliveryType", trade.DeliveryType },
                { "@commodityType", trade.CommodityType },
                { "@fineness", trade.Fineness },
                { "@weight", trade.Weight }
            };
        }

        public void InsertTodayExecutions(IKBRReport report)
        {
            ExecuteDatabaseOperation(connection =>
            {
                if (report == null || !report.TradeConfirms.Any())
                {
                    Console.WriteLine("No trade confirmations found in the report.");
                    return;
                }

                using (var transaction = connection.BeginTransaction())
                {
                    foreach (var tradeConfirm in report.TradeConfirms)
                    {
                        string execID = tradeConfirm.ExecID;
                        if (string.IsNullOrEmpty(execID))
                        {
                            Console.WriteLine("Trade confirmation missing execID. Skipping.");
                            continue;
                        }

                        // Check if the execID already exists in the database
                        using (var checkCmd = new SqlCommand("SELECT COUNT(*) FROM dbo.TradeExecutions WHERE ibexecID = @execID", connection, transaction))
                        {
                            checkCmd.Parameters.AddWithValue("@execID", execID);
                            int count = (int)checkCmd.ExecuteScalar();

                            if (count > 0)
                            {
                                // Update existing row
                                using (var updateCmd = new SqlCommand("UPDATE dbo.TradeExecutions SET symbol = @symbol, tradeDate = @tradeDate, quantity = @quantity, tradePrice = @tradePrice WHERE execID = @execID", connection, transaction))
                                {
                                    updateCmd.Parameters.AddWithValue("@execID", execID);
                                    updateCmd.Parameters.AddWithValue("@symbol", tradeConfirm.Symbol);
                                    updateCmd.Parameters.AddWithValue("@tradeDate", tradeConfirm.TradeDate);
                                    updateCmd.Parameters.AddWithValue("@quantity", tradeConfirm.Quantity);
                                    updateCmd.Parameters.AddWithValue("@tradePrice", tradeConfirm.Price);
                                    updateCmd.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                // Insert new row
                                using (var insertCmd = new SqlCommand("INSERT INTO dbo.TradeExecutions (execID, symbol, tradeDate, quantity, tradePrice) VALUES (@execID, @symbol, @tradeDate, @quantity, @tradePrice)", connection, transaction))
                                {
                                    insertCmd.Parameters.AddWithValue("@execID", execID);
                                    insertCmd.Parameters.AddWithValue("@symbol", tradeConfirm.Symbol);
                                    insertCmd.Parameters.AddWithValue("@tradeDate", tradeConfirm.TradeDate);
                                    insertCmd.Parameters.AddWithValue("@quantity", tradeConfirm.Quantity);
                                    insertCmd.Parameters.AddWithValue("@tradePrice", tradeConfirm.Price);
                                    insertCmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }

                    transaction.Commit();
                    Console.WriteLine("Successfully processed today's trade confirmations.");
                }
            });
        }

        public void UpsertTimeSeriesData(string instrumentName, string listingExchange, string securityIdentifier, string provider, string dataName, string dataSource, string format, string frequency, string currency, DateTime date, double openPrice, double closePrice, double lowPrice, double highPrice, double volume)
        {
            ExecuteDatabaseOperation(connection =>
            {
                // Check if the instrument already exists in the Instruments table
                using (var checkCmd = new SqlCommand("SELECT COUNT(*) FROM dbo.Instruments WHERE SecurityId = @securityId AND Frequency = @frequency AND Provider = @provider", connection))
                {
                    checkCmd.Parameters.AddWithValue("@securityId", securityIdentifier);
                    checkCmd.Parameters.AddWithValue("@frequency", frequency);
                    checkCmd.Parameters.AddWithValue("@provider", provider);

                    int count = (int)checkCmd.ExecuteScalar();

                    if (count == 0)
                    {
                        // Insert new instrument if it does not exist
                        using (var insertCmd = new SqlCommand("INSERT INTO dbo.Instruments (InstrumentName, Provider, DataName, DataSource, Format, Frequency, ContractUnit, ContractUnitType, PriceQuotation, MinimumPriceFluctuation, Currency, ListingExchange, SecurityId) VALUES (@instrumentName, @provider, @dataName, @dataSource, @format, @frequency, @contractUnit, @contractUnitType, @priceQuotation, @minimumPriceFluctuation, @currency, @listingExchange, @securityId)", connection))
                        {
                            insertCmd.Parameters.AddWithValue("@instrumentName", instrumentName);
                            insertCmd.Parameters.AddWithValue("@provider", provider);
                            insertCmd.Parameters.AddWithValue("@dataName", dataName);
                            insertCmd.Parameters.AddWithValue("@dataSource", dataSource);
                            insertCmd.Parameters.AddWithValue("@format", format);
                            insertCmd.Parameters.AddWithValue("@frequency", frequency);
                            insertCmd.Parameters.AddWithValue("@contractUnit", DBNull.Value); // Assuming ContractUnit is not provided
                            insertCmd.Parameters.AddWithValue("@contractUnitType", DBNull.Value); // Assuming ContractUnitType is not provided
                            insertCmd.Parameters.AddWithValue("@priceQuotation", DBNull.Value); // Assuming PriceQuotation is not provided
                            insertCmd.Parameters.AddWithValue("@minimumPriceFluctuation", DBNull.Value); // Assuming MinimumPriceFluctuation is not provided
                            insertCmd.Parameters.AddWithValue("@currency", currency);
                            insertCmd.Parameters.AddWithValue("@listingExchange", listingExchange);
                            insertCmd.Parameters.AddWithValue("@securityId", securityIdentifier);

                            insertCmd.ExecuteNonQuery();
                        }
                    }
                }

                // Additional logic for upserting time series data can be added here
            });
        }

        public string ConnectionString => _connectionString;

        public List<(string securityID, string listingExchange, string symbol)> GetOpenPositionInstrumentNames(IKBRReport report)
        {
            if (report == null || !report.OpenPositions.Any())
            {
                return new List<(string securityID, string listingExchange, string symbol)>();
            }

            // Extract 'securityID', 'listingExchange', and 'symbol' from OpenPosition objects
            var instrumentDetails = report.OpenPositions
                                      .Select(op => (
                                          securityID: op.SecurityID,
                                          listingExchange: op.ListingExchange,
                                          symbol: op.Symbol
                                      ))
                                      .Where(details => !string.IsNullOrEmpty(details.securityID) &&
                                                        !string.IsNullOrEmpty(details.listingExchange) &&
                                                        !string.IsNullOrEmpty(details.symbol))
                                      .Distinct()
                                      .ToList();

            return instrumentDetails;
        }

        public List<TradeExecution> GetTradeExecutions()
        {
            var tradeExecutions = new List<TradeExecution>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new SqlCommand("SELECT ibOrderID, symbol, tradeDate, quantity, tradePrice, openCloseIndicator FROM [dbo].[TradeExecutions] ORDER BY ibOrderID, tradeDate ASC, dateTime ASC", connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tradeExecutions.Add(new TradeExecution
                            {
                                IbOrderID = reader.GetInt64(0), // Updated to GetInt64 for BIGINT
                                Symbol = reader.GetString(1),
                                TradeDate = reader.GetDateTime(2),
                                Quantity = reader.GetDecimal(3),
                                AveragePrice = reader.GetDecimal(4)
                            });
                        }
                    }
                }
            }

            return tradeExecutions;
        }

        public void InsertChartData(string instrumentId, List<Bar> bars)
        {
            if (bars == null || !bars.Any())
            {
                Console.WriteLine("No bars data to insert.");
                return;
            }

            if (!int.TryParse(instrumentId, out int instrumentIdInt))
            {
                Console.WriteLine($"Invalid instrument ID: {instrumentId}");
                return;
            }

            ExecuteDatabaseOperation(connection =>
            {
                var existingDates = new HashSet<DateTime>();
                using (var cmd = new SqlCommand("SELECT [Date] FROM dbo.HistoricalData WHERE InstrumentId = @instrumentId", connection))
                {
                    cmd.Parameters.AddWithValue("@instrumentId", instrumentIdInt);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            existingDates.Add(reader.GetDateTime(0));
                        }
                    }
                }

                var newBars = bars.Where(bar => !existingDates.Contains(bar.Date)).ToList();

                if (!newBars.Any())
                {
                    Console.WriteLine($"All chart data already exists for instrument {instrumentId}.");
                    return;
                }

                using (var transaction = connection.BeginTransaction())
                {
                    foreach (var bar in newBars)
                    {
                        ExecuteInsertCommand(connection, transaction,
                            @"INSERT INTO [dbo].[HistoricalData]
                            ([Date], [OpenPrice], [ClosePrice], [LowPrice], [HighPrice], [Volume], [Settle], [OpenInterest], [InstrumentId])
                            VALUES (@date, @openPrice, @closePrice, @lowPrice, @highPrice, @volume, @settle, @openInterest, @instrumentId)",
                            new Dictionary<string, object>
                            {
                                { "@date", bar.Date },
                                { "@openPrice", bar.OpenPrice },
                                { "@closePrice", bar.ClosePrice },
                                { "@lowPrice", bar.LowPrice },
                                { "@highPrice", bar.HighPrice },
                                { "@volume", bar.Volume },
                                { "@settle", bar.Settle },
                                { "@openInterest", bar.OpenInterest },
                                { "@instrumentId", instrumentIdInt }
                            });
                    }
                    transaction.Commit();
                    Console.WriteLine($"Successfully inserted {newBars.Count} new chart data records for instrument {instrumentId}.");
                }
            });
        }

        private void ExecuteDatabaseOperation(Action<SqlConnection> operation)
        {
            try
            {
                using SqlConnection connection = new SqlConnection(_connectionString);
                connection.Open();
                operation(connection);
            }
            catch (SqlException e)
            {
                Console.WriteLine($"\nDatabase error: {e.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nAn error occurred: {ex.Message}");
            }
        }

        private void ExecuteInsertCommand(SqlConnection connection, SqlTransaction transaction, string query, Dictionary<string, object> parameters)
        {
            using SqlCommand cmd = new SqlCommand(query, connection, transaction);
            foreach (var param in parameters)
            {
                cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
            }
            cmd.ExecuteNonQuery();
        }

        private decimal? ConvertToDecimal(string value)
        {
            if (decimal.TryParse(value, out var result))
            {
                return result;
            }
            return null;
        }

        private long? ConvertToLong(string value)
        {
            if (long.TryParse(value, out var result))
            {
                return result;
            }
            return null;
        }

        private int? ConvertToInt(string value)
        {
            if (int.TryParse(value, out var result))
            {
                return result;
            }
            return null;
        }

        private DateTime? ConvertToDate(string value)
        {
            if (DateTime.TryParseExact(value, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            {
                return result;
            }
            return null;
        }
    }
}
