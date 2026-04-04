using System;
using System.Collections.Generic;
using System.Linq;
using IKBR_Report_Puller.Domain;
using Microsoft.Data.SqlClient;

namespace IKBR_Report_Puller.Data.Repositories
{
    /// <summary>
    /// Repository for Trade-related database operations
    /// </summary>
    public class TradeRepository : BaseRepository
    {
        public TradeRepository(string connectionString) : base(connectionString)
        {
        }

        /// <summary>
        /// Inserts or updates trade executions from a report
        /// </summary>
        public void UpsertTradeExecutions(List<Trade> trades)
        {
            if (trades == null || !trades.Any())
            {
                Console.WriteLine("No trades to insert.");
                return;
            }

            ExecuteDatabaseOperation(connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    foreach (var trade in trades)
                    {
                        string ibExecID = trade.IbExecID;
                        if (string.IsNullOrEmpty(ibExecID))
                        {
                            continue;
                        }

                        bool exists = RecordExists(connection, transaction,
                            "SELECT COUNT(*) FROM dbo.TradeExecutions WHERE ibExecID = @ibExecID",
                            new Dictionary<string, object> { { "@ibExecID", ibExecID } });

                        if (exists)
                        {
                            UpdateTradeIfIncomplete(connection, transaction, trade, ibExecID);
                        }
                        else
                        {
                            InsertTrade(connection, transaction, trade);
                        }
                    }
                    transaction.Commit();
                }

                Console.WriteLine($"Successfully processed {trades.Count} trades.");
            });
        }

        /// <summary>
        /// Gets all trade executions ordered by order ID and date
        /// </summary>
        public List<TradeExecution> GetTradeExecutions()
        {
            return ExecuteDatabaseOperation(connection =>
            {
                var tradeExecutions = new List<TradeExecution>();

                using (var cmd = new SqlCommand(
                    "SELECT ibOrderID, symbol, tradeDate, quantity, tradePrice, openCloseIndicator " +
                    "FROM [dbo].[TradeExecutions] " +
                    "ORDER BY ibOrderID, tradeDate ASC, dateTime ASC", connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tradeExecutions.Add(new TradeExecution
                            {
                                IbOrderID = reader.GetInt64(0),
                                Symbol = reader.GetString(1),
                                TradeDate = reader.GetDateTime(2),
                                Quantity = reader.GetDecimal(3),
                                AveragePrice = reader.GetDecimal(4)
                            });
                        }
                    }
                }

                return tradeExecutions;
            });
        }

        /// <summary>
        /// Inserts or updates today's trade confirmations
        /// </summary>
        public void UpsertTodayExecutions(List<TradeConfirm> tradeConfirms)
        {
            if (tradeConfirms == null || !tradeConfirms.Any())
            {
                Console.WriteLine("No trade confirmations to process.");
                return;
            }

            ExecuteDatabaseOperation(connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    foreach (var tradeConfirm in tradeConfirms)
                    {
                        string execID = tradeConfirm.ExecID;
                        if (string.IsNullOrEmpty(execID))
                        {
                            Console.WriteLine("Trade confirmation missing execID. Skipping.");
                            continue;
                        }

                        bool exists = RecordExists(connection, transaction,
                            "SELECT COUNT(*) FROM dbo.TradeExecutions WHERE ibexecID = @execID",
                            new Dictionary<string, object> { { "@execID", execID } });

                        if (exists)
                        {
                            UpdateTodayExecution(connection, transaction, tradeConfirm, execID);
                        }
                        else
                        {
                            InsertTodayExecution(connection, transaction, tradeConfirm, execID);
                        }
                    }

                    transaction.Commit();
                    Console.WriteLine("Successfully processed today's trade confirmations.");
                }
            });
        }

        #region Private Helper Methods

        private void UpdateTradeIfIncomplete(SqlConnection connection, SqlTransaction transaction, Trade trade, string ibExecID)
        {
            using (var selectCmd = new SqlCommand(
                "SELECT securityID, tradeID, dateTime FROM dbo.TradeExecutions WHERE ibExecID = @ibExecID",
                connection, transaction))
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
                            reader.Close();
                            UpdateTrade(connection, transaction, trade);
                        }
                    }
                }
            }
        }

        private void UpdateTrade(SqlConnection connection, SqlTransaction transaction, Trade trade)
        {
            const string updateQuery = @"
                UPDATE dbo.TradeExecutions
                SET symbol = @symbol, securityID = @securityID, tradeID = @tradeID, dateTime = @dateTime,
                    tradeDate = @tradeDate, quantity = @quantity, tradePrice = @tradePrice, ibCommission = @ibCommission,
                    ibCommissionCurrency = @ibCommissionCurrency, closePrice = @closePrice, cost = @cost,
                    fifoPnlRealized = @fifoPnlRealized, buySell = @buySell, transactionID = @transactionID,
                    brokerageOrderID = @brokerageOrderID, exchOrderId = @exchOrderId, extExecID = @extExecID,
                    orderType = @orderType, traderID = @traderID, currency = @currency, description = @description,
                    conid = @conid, taxes = @taxes, assetCategory = @assetCategory, expiry = @expiry,
                    transactionType = @transactionType, exchange = @exchange, proceeds = @proceeds, netCash = @netCash,
                    mtmPnl = @mtmPnl, origTradePrice = @origTradePrice, origTradeDate = @origTradeDate,
                    origTradeID = @origTradeID, origOrderID = @origOrderID, origTransactionID = @origTransactionID,
                    ibOrderID = @ibOrderID, openDateTime = @openDateTime, initialInvestment = @initialInvestment,
                    accountId = @accountId, acctAlias = @acctAlias, model = @model, fxRateToBase = @fxRateToBase,
                    subCategory = @subCategory, securityIDType = @securityIDType, cusip = @cusip, isin = @isin,
                    figi = @figi, listingExchange = @listingExchange, underlyingConid = @underlyingConid,
                    underlyingSymbol = @underlyingSymbol, underlyingSecurityID = @underlyingSecurityID,
                    underlyingListingExchange = @underlyingListingExchange, issuer = @issuer,
                    issuerCountryCode = @issuerCountryCode, multiplier = @multiplier, relatedTradeID = @relatedTradeID,
                    strike = @strike, reportDate = @reportDate, putCall = @putCall,
                    principalAdjustFactor = @principalAdjustFactor, settleDateTarget = @settleDateTarget,
                    tradeMoney = @tradeMoney, openCloseIndicator = @openCloseIndicator, notes = @notes,
                    clearingFirmID = @clearingFirmID, relatedTransactionID = @relatedTransactionID, rtn = @rtn,
                    orderReference = @orderReference, volatilityOrderLink = @volatilityOrderLink, orderTime = @orderTime,
                    holdingPeriodDateTime = @holdingPeriodDateTime, whenRealized = @whenRealized,
                    whenReopened = @whenReopened, levelOfDetail = @levelOfDetail, changeInPrice = @changeInPrice,
                    changeInQuantity = @changeInQuantity, isAPIOrder = @isAPIOrder, accruedInt = @accruedInt,
                    positionActionID = @positionActionID, serialNumber = @serialNumber, deliveryType = @deliveryType,
                    commodityType = @commodityType, fineness = @fineness, weight = @weight
                WHERE ibExecID = @ibExecID";

            ExecuteCommand(connection, transaction, updateQuery, TradeParameterBuilder.GetTradeParameters(trade));
        }

        private void InsertTrade(SqlConnection connection, SqlTransaction transaction, Trade trade)
        {
            const string insertQuery = @"
                INSERT INTO [dbo].[TradeExecutions]
                ([symbol], [securityID], [tradeID], [dateTime], [tradeDate], [quantity], [tradePrice], [ibCommission],
                 [ibCommissionCurrency], [closePrice], [cost], [fifoPnlRealized], [buySell], [transactionID], [ibExecID],
                 [brokerageOrderID], [exchOrderId], [extExecID], [orderType], [traderID], [currency], [description],
                 [conid], [taxes], [assetCategory], [expiry], [transactionType], [exchange], [proceeds], [netCash],
                 [mtmPnl], [origTradePrice], [origTradeDate], [origTradeID], [origOrderID], [origTransactionID],
                 [ibOrderID], [openDateTime], [initialInvestment], [accountId], [acctAlias], [model], [fxRateToBase],
                 [subCategory], [securityIDType], [cusip], [isin], [figi], [listingExchange], [underlyingConid],
                 [underlyingSymbol], [underlyingSecurityID], [underlyingListingExchange], [issuer], [issuerCountryCode],
                 [multiplier], [relatedTradeID], [strike], [reportDate], [putCall], [principalAdjustFactor],
                 [settleDateTarget], [tradeMoney], [openCloseIndicator], [notes], [clearingFirmID], [relatedTransactionID],
                 [rtn], [orderReference], [volatilityOrderLink], [orderTime], [holdingPeriodDateTime], [whenRealized],
                 [whenReopened], [levelOfDetail], [changeInPrice], [changeInQuantity], [isAPIOrder], [accruedInt],
                 [positionActionID], [serialNumber], [deliveryType], [commodityType], [fineness], [weight])
                VALUES
                (@symbol, @securityID, @tradeID, @dateTime, @tradeDate, @quantity, @tradePrice, @ibCommission,
                 @ibCommissionCurrency, @closePrice, @cost, @fifoPnlRealized, @buySell, @transactionID, @ibExecID,
                 @brokerageOrderID, @exchOrderId, @extExecID, @orderType, @traderID, @currency, @description,
                 @conid, @taxes, @assetCategory, @expiry, @transactionType, @exchange, @proceeds, @netCash,
                 @mtmPnl, @origTradePrice, @origTradeDate, @origTradeID, @origOrderID, @origTransactionID,
                 @ibOrderID, @openDateTime, @initialInvestment, @accountId, @acctAlias, @model, @fxRateToBase,
                 @subCategory, @securityIDType, @cusip, @isin, @figi, @listingExchange, @underlyingConid,
                 @underlyingSymbol, @underlyingSecurityID, @underlyingListingExchange, @issuer, @issuerCountryCode,
                 @multiplier, @relatedTradeID, @strike, @reportDate, @putCall, @principalAdjustFactor,
                 @settleDateTarget, @tradeMoney, @openCloseIndicator, @notes, @clearingFirmID, @relatedTransactionID,
                 @rtn, @orderReference, @volatilityOrderLink, @orderTime, @holdingPeriodDateTime, @whenRealized,
                 @whenReopened, @levelOfDetail, @changeInPrice, @changeInQuantity, @isAPIOrder, @accruedInt,
                 @positionActionID, @serialNumber, @deliveryType, @commodityType, @fineness, @weight)";

            ExecuteCommand(connection, transaction, insertQuery, TradeParameterBuilder.GetTradeParameters(trade));
        }

        private void UpdateTodayExecution(SqlConnection connection, SqlTransaction transaction, TradeConfirm tradeConfirm, string execID)
        {
            const string updateQuery = @"
                UPDATE dbo.TradeExecutions 
                SET symbol = @symbol, tradeDate = @tradeDate, quantity = @quantity, tradePrice = @tradePrice 
                WHERE execID = @execID";

            var parameters = new Dictionary<string, object>
            {
                { "@execID", execID },
                { "@symbol", tradeConfirm.Symbol },
                { "@tradeDate", tradeConfirm.TradeDate },
                { "@quantity", tradeConfirm.Quantity },
                { "@tradePrice", tradeConfirm.Price }
            };

            ExecuteCommand(connection, transaction, updateQuery, parameters);
        }

        private void InsertTodayExecution(SqlConnection connection, SqlTransaction transaction, TradeConfirm tradeConfirm, string execID)
        {
            const string insertQuery = @"
                INSERT INTO dbo.TradeExecutions (execID, symbol, tradeDate, quantity, tradePrice) 
                VALUES (@execID, @symbol, @tradeDate, @quantity, @tradePrice)";

            var parameters = new Dictionary<string, object>
            {
                { "@execID", execID },
                { "@symbol", tradeConfirm.Symbol },
                { "@tradeDate", tradeConfirm.TradeDate },
                { "@quantity", tradeConfirm.Quantity },
                { "@tradePrice", tradeConfirm.Price }
            };

            ExecuteCommand(connection, transaction, insertQuery, parameters);
        }

        #endregion
    }
}
