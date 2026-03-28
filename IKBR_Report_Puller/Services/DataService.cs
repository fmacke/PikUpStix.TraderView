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

        public void InsertOpenPositions(XDocument reportXml)
        {
            ExecuteDatabaseOperation(connection =>
            {
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
                        ExecuteInsertCommand(connection, transaction, "INSERT INTO [dbo].[OpenPositions] ([whenGenerated], [accountId], [acctAlias], [model], [currency], [fxRateToBase], [assetCategory], [subCategory], [symbol], [description], [conid], [securityID], [securityIDType], [cusip], [isin], [figi], [listingExchange], [underlyingConid], [underlyingSymbol], [underlyingSecurityID], [underlyingListingExchange], [issuer], [issuerCountryCode], [multiplier], [strike], [expiry], [putCall], [principalAdjustFactor], [reportDate], [position], [markPrice], [positionValue], [openPrice], [costBasisPrice], [costBasisMoney], [percentOfNAV], [fifoPnlUnrealized], [side], [levelOfDetail], [openDateTime], [holdingPeriodDateTime], [vestingDate], [code], [originatingOrderID], [originatingTransactionID], [accruedInt], [serialNumber], [deliveryType], [commodityType], [fineness], [weight]) VALUES (@whenGenerated, @accountId, @acctAlias, @model, @currency, @fxRateToBase, @assetCategory, @subCategory, @symbol, @description, @conid, @securityID, @securityIDType, @cusip, @isin, @figi, @listingExchange, @underlyingConid, @underlyingSymbol, @underlyingSecurityID, @underlyingListingExchange, @issuer, @issuerCountryCode, @multiplier, @strike, @expiry, @putCall, @principalAdjustFactor, @reportDate, @position, @markPrice, @positionValue, @openPrice, @costBasisPrice, @costBasisMoney, @percentOfNAV, @fifoPnlUnrealized, @side, @levelOfDetail, @openDateTime, @holdingPeriodDateTime, @vestingDate, @code, @originatingOrderID, @originatingTransactionID, @accruedInt, @serialNumber, @deliveryType, @commodityType, @fineness, @weight)",
                            new Dictionary<string, object>
                            {
                                { "@whenGenerated", whenGenerated },
                                { "@accountId", position.Attribute("accountId")?.Value },
                                { "@acctAlias", position.Attribute("acctAlias")?.Value },
                                { "@model", position.Attribute("model")?.Value },
                                { "@currency", position.Attribute("currency")?.Value },
                                { "@fxRateToBase", ConvertToDecimal(position.Attribute("fxRateToBase")?.Value) },
                                { "@assetCategory", position.Attribute("assetCategory")?.Value },
                                { "@subCategory", position.Attribute("subCategory")?.Value },
                                { "@symbol", position.Attribute("symbol")?.Value },
                                { "@description", position.Attribute("description")?.Value },
                                { "@conid", ConvertToLong(position.Attribute("conid")?.Value) },
                                { "@securityID", position.Attribute("securityID")?.Value },
                                { "@securityIDType", position.Attribute("securityIDType")?.Value },
                                { "@cusip", position.Attribute("cusip")?.Value },
                                { "@isin", position.Attribute("isin")?.Value },
                                { "@figi", position.Attribute("figi")?.Value },
                                { "@listingExchange", position.Attribute("listingExchange")?.Value },
                                { "@underlyingConid", position.Attribute("underlyingConid")?.Value },
                                { "@underlyingSymbol", position.Attribute("underlyingSymbol")?.Value },
                                { "@underlyingSecurityID", position.Attribute("underlyingSecurityID")?.Value },
                                { "@underlyingListingExchange", position.Attribute("underlyingListingExchange")?.Value },
                                { "@issuer", position.Attribute("issuer")?.Value },
                                { "@issuerCountryCode", position.Attribute("issuerCountryCode")?.Value },
                                { "@multiplier", ConvertToInt(position.Attribute("multiplier")?.Value) },
                                { "@strike", ConvertToDecimal(position.Attribute("strike")?.Value) },
                                { "@expiry", position.Attribute("expiry")?.Value },
                                { "@putCall", position.Attribute("putCall")?.Value },
                                { "@principalAdjustFactor", ConvertToDecimal(position.Attribute("principalAdjustFactor")?.Value) },
                                { "@reportDate", ConvertToDate(position.Attribute("reportDate")?.Value) },
                                { "@position", ConvertToDecimal(position.Attribute("position")?.Value) },
                                { "@markPrice", ConvertToDecimal(position.Attribute("markPrice")?.Value) },
                                { "@positionValue", ConvertToDecimal(position.Attribute("positionValue")?.Value) },
                                { "@openPrice", ConvertToDecimal(position.Attribute("openPrice")?.Value) },
                                { "@costBasisPrice", ConvertToDecimal(position.Attribute("costBasisPrice")?.Value) },
                                { "@costBasisMoney", ConvertToDecimal(position.Attribute("costBasisMoney")?.Value) },
                                { "@percentOfNAV", ConvertToDecimal(position.Attribute("percentOfNAV")?.Value) },
                                { "@fifoPnlUnrealized", ConvertToDecimal(position.Attribute("fifoPnlUnrealized")?.Value) },
                                { "@side", position.Attribute("side")?.Value },
                                { "@levelOfDetail", position.Attribute("levelOfDetail")?.Value },
                                { "@openDateTime", position.Attribute("openDateTime")?.Value },
                                { "@holdingPeriodDateTime", position.Attribute("holdingPeriodDateTime")?.Value },
                                { "@vestingDate", ConvertToDate(position.Attribute("vestingDate")?.Value) },
                                { "@code", position.Attribute("code")?.Value },
                                { "@originatingOrderID", ConvertToLong(position.Attribute("originatingOrderID")?.Value) },
                                { "@originatingTransactionID", ConvertToLong(position.Attribute("originatingTransactionID")?.Value) },
                                { "@accruedInt", ConvertToDecimal(position.Attribute("accruedInt")?.Value) },
                                { "@serialNumber", position.Attribute("serialNumber")?.Value },
                                { "@deliveryType", position.Attribute("deliveryType")?.Value },
                                { "@commodityType", position.Attribute("commodityType")?.Value },
                                { "@fineness", ConvertToDecimal(position.Attribute("fineness")?.Value) },
                                { "@weight", ConvertToDecimal(position.Attribute("weight")?.Value) }
                            });
                    }
                    transaction.Commit();
                    Console.WriteLine($"Successfully inserted {newPositionsCount} new open positions into the database.");
                }
            });
        }

        public void InsertTradeExecutions(XDocument reportXml)
        {
            ExecuteDatabaseOperation(connection =>
            {
                var trades = reportXml.Descendants("Trade").ToList();
                if (!trades.Any())
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
                    foreach (var trade in trades)
                    {
                        string ibExecID = trade.Attribute("ibExecID")?.Value;
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
                                                using (var updateCmd = new SqlCommand(@"
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
                                    WHERE ibExecID = @ibExecID", connection, transaction))
                                                {
                                                    updateCmd.Parameters.AddWithValue("@ibExecID", ibExecID);
                                                    updateCmd.Parameters.AddWithValue("@symbol", trade.Attribute("symbol")?.Value);
                                                    updateCmd.Parameters.AddWithValue("@securityID", trade.Attribute("securityID")?.Value);
                                                    updateCmd.Parameters.AddWithValue("@tradeID", ConvertToLong(trade.Attribute("tradeID")?.Value));
                                                    updateCmd.Parameters.AddWithValue("@dateTime", trade.Attribute("dateTime")?.Value);
                                                    updateCmd.Parameters.AddWithValue("@tradeDate", trade.Attribute("tradeDate")?.Value);
                                                    updateCmd.Parameters.AddWithValue("@quantity", ConvertToDecimal(trade.Attribute("quantity")?.Value));
                                                    updateCmd.Parameters.AddWithValue("@tradePrice", ConvertToDecimal(trade.Attribute("tradePrice")?.Value));
                                                    updateCmd.Parameters.AddWithValue("@ibCommission", ConvertToDecimal(trade.Attribute("ibCommission")?.Value));
                                                    updateCmd.Parameters.AddWithValue("@ibCommissionCurrency", trade.Attribute("ibCommissionCurrency")?.Value);
                                                    updateCmd.Parameters.AddWithValue("@closePrice", ConvertToDecimal(trade.Attribute("closePrice")?.Value));
                                                    updateCmd.Parameters.AddWithValue("@cost", ConvertToDecimal(trade.Attribute("cost")?.Value));
                                                    updateCmd.Parameters.AddWithValue("@fifoPnlRealized", ConvertToDecimal(trade.Attribute("fifoPnlRealized")?.Value));
                                                    updateCmd.Parameters.AddWithValue("@buySell", trade.Attribute("buySell")?.Value);
                                                    updateCmd.Parameters.AddWithValue("@transactionID", ConvertToLong(trade.Attribute("transactionID")?.Value));
                                                    updateCmd.Parameters.AddWithValue("@brokerageOrderID", trade.Attribute("brokerageOrderID")?.Value);
                                                    updateCmd.Parameters.AddWithValue("@exchOrderId", trade.Attribute("exchOrderId")?.Value);
                                                    updateCmd.Parameters.AddWithValue("@extExecID", trade.Attribute("extExecID")?.Value);
                                                    updateCmd.Parameters.AddWithValue("@orderType", trade.Attribute("orderType")?.Value);
                                                    updateCmd.Parameters.AddWithValue("@traderID", trade.Attribute("traderID")?.Value);
                                                    updateCmd.Parameters.AddWithValue("@currency", trade.Attribute("currency")?.Value);
                                                    updateCmd.Parameters.AddWithValue("@description", trade.Attribute("description")?.Value );
                                                    updateCmd.Parameters.AddWithValue("@conid", ConvertToLong(trade.Attribute("conid")?.Value));
                                                    updateCmd.Parameters.AddWithValue("@taxes", ConvertToDecimal(trade.Attribute("taxes")?.Value));
                                                    updateCmd.Parameters.AddWithValue("@assetCategory", trade.Attribute("assetCategory")?.Value);
                                                    updateCmd.Parameters.AddWithValue("@expiry", trade.Attribute("expiry")?.Value);
                                                    updateCmd.Parameters.AddWithValue("@transactionType", trade.Attribute("transactionType")?.Value);
                                                    updateCmd.Parameters.AddWithValue("@exchange", trade.Attribute("exchange")?.Value);
                                                    updateCmd.Parameters.AddWithValue("@proceeds", ConvertToDecimal(trade.Attribute("proceeds")?.Value));
                                                    updateCmd.Parameters.AddWithValue("@netCash", ConvertToDecimal(trade.Attribute("netCash")?.Value));
                                                    updateCmd.Parameters.AddWithValue("@mtmPnl", ConvertToDecimal(trade.Attribute("mtmPnl")?.Value));
                                                    updateCmd.Parameters.AddWithValue("@origTradePrice", ConvertToDecimal(trade.Attribute("origTradePrice")?.Value));
                                                    updateCmd.Parameters.AddWithValue("@origTradeDate", trade.Attribute("origTradeDate")?.Value);
                                                    updateCmd.Parameters.AddWithValue("@origTradeID", trade.Attribute("origTradeID")?.Value);
                                                    updateCmd.Parameters.AddWithValue("@origOrderID", ConvertToLong(trade.Attribute("origOrderID")?.Value));
                                                    updateCmd.Parameters.AddWithValue("@origTransactionID", ConvertToLong(trade.Attribute("origTransactionID")?.Value));
                                                    updateCmd.Parameters.AddWithValue("@ibOrderID", ConvertToLong(trade.Attribute("ibOrderID")?.Value));
                                                    updateCmd.Parameters.AddWithValue("@openDateTime", trade.Attribute("openDateTime")?.Value);
                                                    updateCmd.Parameters.AddWithValue("@initialInvestment", ConvertToDecimal(trade.Attribute("initialInvestment")?.Value));
                                                    updateCmd.Parameters.AddWithValue("@accountId", trade.Attribute("accountId")?.Value);
                                                    updateCmd.Parameters.AddWithValue("@acctAlias", trade.Attribute("acctAlias")?.Value);
                                                    updateCmd.Parameters.AddWithValue("@model", trade.Attribute("model")?.Value);
                                                    updateCmd.Parameters.AddWithValue("@fxRateToBase", ConvertToDecimal(trade.Attribute("fxRateToBase")?.Value));
                                                    updateCmd.Parameters.AddWithValue("@subCategory", trade.Attribute("subCategory")?.Value);
                                                    updateCmd.Parameters.AddWithValue("@securityIDType", trade.Attribute("securityIDType")?.Value);
                                                    updateCmd.Parameters.AddWithValue("@cusip", trade.Attribute("cusip")?.Value);
                                                    updateCmd.Parameters.AddWithValue("@isin", trade.Attribute("isin")?.Value);
                                                    updateCmd.Parameters.AddWithValue("@figi", trade.Attribute("figi")?.Value);
                                                    updateCmd.Parameters.AddWithValue("@listingExchange", trade.Attribute("listingExchange")?.Value);
                                                    updateCmd.Parameters.AddWithValue("@underlyingConid", trade.Attribute("underlyingConid")?.Value);
                                                    updateCmd.Parameters.AddWithValue("@underlyingSymbol", trade.Attribute("underlyingSymbol")?.Value);
                                                    updateCmd.Parameters.AddWithValue("@underlyingSecurityID", trade.Attribute("underlyingSecurityID")?.Value);
                                                    updateCmd.Parameters.AddWithValue("@underlyingListingExchange", trade.Attribute("underlyingListingExchange")?.Value);
                                                    updateCmd.Parameters.AddWithValue("@issuer", trade.Attribute("issuer")?.Value);
                                                    updateCmd.Parameters.AddWithValue("@issuerCountryCode", trade.Attribute("issuerCountryCode")?.Value);
                                                    updateCmd.Parameters.AddWithValue("@multiplier", ConvertToInt(trade.Attribute("multiplier")?.Value));
                                                    updateCmd.Parameters.AddWithValue("@relatedTradeID", trade.Attribute("relatedTradeID")?.Value );
                                                    updateCmd.Parameters.AddWithValue("@strike", ConvertToDecimal(trade.Attribute("strike")?.Value));
                                                    updateCmd.Parameters.AddWithValue("@reportDate", trade.Attribute("reportDate")?.Value);
                                                    updateCmd.Parameters.AddWithValue("@putCall", trade.Attribute("putCall")?.Value);
                                                    updateCmd.Parameters.AddWithValue("@principalAdjustFactor", ConvertToDecimal(trade.Attribute("principalAdjustFactor")?.Value));
                                                    updateCmd.Parameters.AddWithValue("@settleDateTarget", trade.Attribute("settleDateTarget")?.Value);
                                                    updateCmd.Parameters.AddWithValue("@tradeMoney", ConvertToDecimal(trade.Attribute("tradeMoney")?.Value));
                                                    updateCmd.Parameters.AddWithValue("@openCloseIndicator", trade.Attribute("openCloseIndicator")?.Value);
                                                    updateCmd.Parameters.AddWithValue("@notes", trade.Attribute("notes")?.Value);
                                                    updateCmd.Parameters.AddWithValue("@clearingFirmID", trade.Attribute("clearingFirmID")?.Value);
                                                    updateCmd.Parameters.AddWithValue("@relatedTransactionID", trade.Attribute("relatedTransactionID")?.Value);
                                                    updateCmd.Parameters.AddWithValue("@rtn", trade.Attribute("rtn")?.Value);
                                                    updateCmd.Parameters.AddWithValue("@orderReference", trade.Attribute("orderReference")?.Value);
                                                    updateCmd.Parameters.AddWithValue("@volatilityOrderLink", trade.Attribute("volatilityOrderLink")?.Value );
                                                    updateCmd.Parameters.AddWithValue("@orderTime", trade.Attribute("orderTime")?.Value);
                                                    updateCmd.Parameters.AddWithValue("@holdingPeriodDateTime", trade.Attribute("holdingPeriodDateTime")?.Value );
                                                    updateCmd.Parameters.AddWithValue("@whenRealized", trade.Attribute("whenRealized")?.Value );
                                                    updateCmd.Parameters.AddWithValue("@whenReopened", trade.Attribute("whenReopened")?.Value );
                                                    updateCmd.Parameters.AddWithValue("@levelOfDetail", trade.Attribute("levelOfDetail")?.Value );
                                                    updateCmd.Parameters.AddWithValue("@changeInPrice", ConvertToDecimal(trade.Attribute("changeInPrice")?.Value));
                                                    updateCmd.Parameters.AddWithValue("@changeInQuantity", ConvertToDecimal(trade.Attribute("changeInQuantity")?.Value));
                                                    updateCmd.Parameters.AddWithValue("@isAPIOrder", trade.Attribute("isAPIOrder")?.Value );
                                                    updateCmd.Parameters.AddWithValue("@accruedInt", ConvertToDecimal(trade.Attribute("accruedInt")?.Value));
                                                    updateCmd.Parameters.AddWithValue("@positionActionID", trade.Attribute("positionActionID")?.Value );
                                                    updateCmd.Parameters.AddWithValue("@serialNumber", trade.Attribute("serialNumber")?.Value );
                                                    updateCmd.Parameters.AddWithValue("@deliveryType", trade.Attribute("deliveryType")?.Value );
                                                    updateCmd.Parameters.AddWithValue("@commodityType", trade.Attribute("commodityType")?.Value );
                                                    updateCmd.Parameters.AddWithValue("@fineness", ConvertToDecimal(trade.Attribute("fineness")?.Value));
                                                    updateCmd.Parameters.AddWithValue("@weight", ConvertToDecimal(trade.Attribute("weight")?.Value));
                                                    updateCmd.ExecuteNonQuery();
                                                }
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
                                    new Dictionary<string, object>
                                    {
                                        { "@symbol", trade.Attribute("symbol")?.Value },
                                        { "@securityID", trade.Attribute("securityID")?.Value },
                                        { "@tradeID", ConvertToLong(trade.Attribute("tradeID")?.Value) },
                                        { "@dateTime", trade.Attribute("dateTime")?.Value },
                                        { "@tradeDate", trade.Attribute("tradeDate")?.Value },
                                        { "@quantity", ConvertToDecimal(trade.Attribute("quantity")?.Value) },
                                        { "@tradePrice", ConvertToDecimal(trade.Attribute("tradePrice")?.Value) },
                                        { "@ibCommission", ConvertToDecimal(trade.Attribute("ibCommission")?.Value) },
                                        { "@ibCommissionCurrency", trade.Attribute("ibCommissionCurrency")?.Value },
                                        { "@closePrice", ConvertToDecimal(trade.Attribute("closePrice")?.Value) },
                                        { "@cost", ConvertToDecimal(trade.Attribute("cost")?.Value) },
                                        { "@fifoPnlRealized", ConvertToDecimal(trade.Attribute("fifoPnlRealized")?.Value) },
                                        { "@buySell", trade.Attribute("buySell")?.Value },
                                        { "@transactionID", ConvertToLong(trade.Attribute("transactionID")?.Value) },
                                        { "@ibExecID", trade.Attribute("ibExecID")?.Value },
                                        { "@brokerageOrderID", trade.Attribute("brokerageOrderID")?.Value },
                                        { "@exchOrderId", trade.Attribute("exchOrderId")?.Value },
                                        { "@extExecID", trade.Attribute("extExecID")?.Value },
                                        { "@orderType", trade.Attribute("orderType")?.Value },
                                        { "@traderID", trade.Attribute("traderID")?.Value },
                                        { "@currency", trade.Attribute("currency")?.Value },
                                        { "@description", trade.Attribute("description")?.Value },
                                        { "@conid", ConvertToLong(trade.Attribute("conid")?.Value) },
                                        { "@taxes", ConvertToDecimal(trade.Attribute("taxes")?.Value) },
                                        { "@assetCategory", trade.Attribute("assetCategory")?.Value },
                                        { "@expiry", trade.Attribute("expiry")?.Value },
                                        { "@transactionType", trade.Attribute("transactionType")?.Value },
                                        { "@exchange", trade.Attribute("exchange")?.Value },
                                        { "@proceeds", ConvertToDecimal(trade.Attribute("proceeds")?.Value) },
                                        { "@netCash", ConvertToDecimal(trade.Attribute("netCash")?.Value) },
                                        { "@mtmPnl", ConvertToDecimal(trade.Attribute("mtmPnl")?.Value) },
                                        { "@origTradePrice", ConvertToDecimal(trade.Attribute("origTradePrice")?.Value) },
                                        { "@origTradeDate", trade.Attribute("origTradeDate")?.Value },
                                        { "@origTradeID", trade.Attribute("origTradeID")?.Value },
                                        { "@origOrderID", ConvertToLong(trade.Attribute("origOrderID")?.Value) },
                                        { "@origTransactionID", ConvertToLong(trade.Attribute("origTransactionID")?.Value) },
                                        { "@ibOrderID", ConvertToLong(trade.Attribute("ibOrderID")?.Value) },
                                        { "@openDateTime", trade.Attribute("openDateTime")?.Value },
                                        { "@initialInvestment", ConvertToDecimal(trade.Attribute("initialInvestment")?.Value) },
                                        { "@accountId", trade.Attribute("accountId")?.Value },
                                        { "@acctAlias", trade.Attribute("acctAlias")?.Value },
                                        { "@model", trade.Attribute("model")?.Value },
                                        { "@fxRateToBase", ConvertToDecimal(trade.Attribute("fxRateToBase")?.Value) },
                                        { "@subCategory", trade.Attribute("subCategory")?.Value },
                                        { "@securityIDType", trade.Attribute("securityIDType")?.Value },
                                        { "@cusip", trade.Attribute("cusip")?.Value },
                                        { "@isin", trade.Attribute("isin")?.Value },
                                        { "@figi", trade.Attribute("figi")?.Value },
                                        { "@listingExchange", trade.Attribute("listingExchange")?.Value },
                                        { "@underlyingConid", trade.Attribute("underlyingConid")?.Value },
                                        { "@underlyingSymbol", trade.Attribute("underlyingSymbol")?.Value },
                                        { "@underlyingSecurityID", trade.Attribute("underlyingSecurityID")?.Value },
                                        { "@underlyingListingExchange", trade.Attribute("underlyingListingExchange")?.Value },
                                        { "@issuer", trade.Attribute("issuer")?.Value },
                                        { "@issuerCountryCode", trade.Attribute("issuerCountryCode")?.Value },
                                        { "@multiplier", ConvertToInt(trade.Attribute("multiplier")?.Value) },
                                        { "@relatedTradeID", trade.Attribute("relatedTradeID")?.Value },
                                        { "@strike", ConvertToDecimal(trade.Attribute("strike")?.Value) },
                                        { "@reportDate", trade.Attribute("reportDate")?.Value },
                                        { "@putCall", trade.Attribute("putCall")?.Value },
                                        { "@principalAdjustFactor", ConvertToDecimal(trade.Attribute("principalAdjustFactor")?.Value) },
                                        { "@settleDateTarget", trade.Attribute("settleDateTarget")?.Value },
                                        { "@tradeMoney", ConvertToDecimal(trade.Attribute("tradeMoney")?.Value) },
                                        { "@openCloseIndicator", trade.Attribute("openCloseIndicator")?.Value },
                                        { "@notes", trade.Attribute("notes")?.Value },
                                        { "@clearingFirmID", trade.Attribute("clearingFirmID")?.Value },
                                        { "@relatedTransactionID", trade.Attribute("relatedTransactionID")?.Value },
                                        { "@rtn", trade.Attribute("rtn")?.Value },
                                        { "@orderReference", trade.Attribute("orderReference")?.Value },
                                        { "@volatilityOrderLink", trade.Attribute("volatilityOrderLink")?.Value },
                                        { "@orderTime", trade.Attribute("orderTime")?.Value },
                                        { "@holdingPeriodDateTime", trade.Attribute("holdingPeriodDateTime")?.Value },
                                        { "@whenRealized", trade.Attribute("whenRealized")?.Value },
                                        { "@whenReopened", trade.Attribute("whenReopened")?.Value },
                                        { "@levelOfDetail", trade.Attribute("levelOfDetail")?.Value },
                                        { "@changeInPrice", ConvertToDecimal(trade.Attribute("changeInPrice")?.Value) },
                                        { "@changeInQuantity", ConvertToDecimal(trade.Attribute("changeInQuantity")?.Value) },
                                        { "@isAPIOrder", trade.Attribute("isAPIOrder")?.Value },
                                        { "@accruedInt", ConvertToDecimal(trade.Attribute("accruedInt")?.Value) },
                                        { "@positionActionID", trade.Attribute("positionActionID")?.Value },
                                        { "@serialNumber", trade.Attribute("serialNumber")?.Value },
                                        { "@deliveryType", trade.Attribute("deliveryType")?.Value },
                                        { "@commodityType", trade.Attribute("commodityType")?.Value },
                                        { "@fineness", ConvertToDecimal(trade.Attribute("fineness")?.Value) },
                                        { "@weight", ConvertToDecimal(trade.Attribute("weight")?.Value) }
                                    });
                            }
                        }
                    }
                    transaction.Commit();
                }

                Console.WriteLine($"Successfully inserted {trades.Count} trades into the database.");
            });
        }

        public void InsertTodayExecutions(XDocument reportXml)
        {
            ExecuteDatabaseOperation(connection =>
            {
                var tradeConfirms = reportXml.Descendants("TradeConfirm").ToList();
                if (!tradeConfirms.Any())
                {
                    Console.WriteLine("No trade confirmations found in the report.");
                    return;
                }

                using (var transaction = connection.BeginTransaction())
                {
                    foreach (var tradeConfirm in tradeConfirms)
                    {
                        string execID = tradeConfirm.Attribute("execID")?.Value;
                        if (string.IsNullOrEmpty(execID))
                        {
                            Console.WriteLine("Trade confirmation missing execID. Skipping.");
                            continue;
                        }

                        // Check if the execID already exists in the database
                        using (var checkCmd = new SqlCommand("SELECT COUNT(*) FROM dbo.TradeExecutions WHERE execID = @execID", connection, transaction))
                        {
                            checkCmd.Parameters.AddWithValue("@execID", execID);
                            int count = (int)checkCmd.ExecuteScalar();

                            if (count > 0)
                            {
                                // Update existing row
                                using (var updateCmd = new SqlCommand("UPDATE dbo.TradeExecutions SET symbol = @symbol, tradeDate = @tradeDate, quantity = @quantity, tradePrice = @tradePrice WHERE execID = @execID", connection, transaction))
                                {
                                    updateCmd.Parameters.AddWithValue("@execID", execID);
                                    updateCmd.Parameters.AddWithValue("@symbol", tradeConfirm.Attribute("symbol")?.Value);
                                    updateCmd.Parameters.AddWithValue("@tradeDate", tradeConfirm.Attribute("tradeDate")?.Value);
                                    updateCmd.Parameters.AddWithValue("@quantity", ConvertToDecimal(tradeConfirm.Attribute("quantity")?.Value));
                                    updateCmd.Parameters.AddWithValue("@tradePrice", ConvertToDecimal(tradeConfirm.Attribute("price")?.Value));
                                    updateCmd.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                // Insert new row
                                using (var insertCmd = new SqlCommand("INSERT INTO dbo.TradeExecutions (execID, symbol, tradeDate, quantity, tradePrice) VALUES (@execID, @symbol, @tradeDate, @quantity, @tradePrice)", connection, transaction))
                                {
                                    insertCmd.Parameters.AddWithValue("@execID", execID);
                                    insertCmd.Parameters.AddWithValue("@symbol", tradeConfirm.Attribute("symbol")?.Value);
                                    insertCmd.Parameters.AddWithValue("@tradeDate", tradeConfirm.Attribute("tradeDate")?.Value);
                                    insertCmd.Parameters.AddWithValue("@quantity", ConvertToDecimal(tradeConfirm.Attribute("quantity")?.Value));
                                    insertCmd.Parameters.AddWithValue("@tradePrice", ConvertToDecimal(tradeConfirm.Attribute("price")?.Value));
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

        public List<(string securityID, string listingExchange, string symbol)> GetOpenPositionInstrumentNames(XDocument xmlReport)
        {
            // Extract 'securityID', 'listingExchange', and 'symbol' attributes from OpenPosition elements in the XML report
            var instrumentDetails = xmlReport.Descendants("OpenPosition")
                                      .Select(op => (
                                          securityID: op.Attribute("securityID")?.Value,
                                          listingExchange: op.Attribute("listingExchange")?.Value,
                                          symbol: op.Attribute("symbol")?.Value
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
