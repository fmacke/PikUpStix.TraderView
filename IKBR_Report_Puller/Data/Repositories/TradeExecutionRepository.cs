using IKBR_Report_Puller.Domain;
using IKBR_Report_Puller.Interfaces;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace IKBR_Report_Puller.Data.Repositories
{
    /// <summary>
    /// Repository for Trade-related database operations
    /// </summary>
    public class TradeExecutionRepository : BaseRepository, ITradeExecutionRepository
    {
        private readonly IInstrumentRepository _instrumentRepository;

        public TradeExecutionRepository(string connectionString, IInstrumentRepository instrumentRepository) : base(connectionString)
        {
            _instrumentRepository = instrumentRepository;
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
                            var tradeExec = GetTradeExecutionsByIbExecID(connection, transaction, ibExecID, out var existingTrade);
                            trade.InstrumentId = existingTrade.InstrumentId;
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
                    "SELECT ibOrderID, symbol, tradeDate, quantity, tradePrice, openCloseIndicator, instrumentid, currency, conid, ibExecID, IBCommission, IBCommissionCurrency " +
                    "FROM [dbo].[TradeExecutions] " +
                    "ORDER BY ibOrderID, tradeDate ASC, dateTime ASC", connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tradeExecutions.Add(new TradeExecution
                            {
                                IbOrderID = reader.GetInt64("ibOrderID"),
                                Symbol = reader.GetString("symbol"),
                                TradeDate = reader.GetDateTime("tradeDate"),
                                Quantity = reader.GetDecimal("quantity"),
                                AveragePrice = reader.GetDecimal("tradePrice"),
                                InstrumentId = reader.GetInt32("instrumentid"),
                                Currency = reader.GetString("currency"),
                                SecurityId = reader.GetString("conid"),
                                IbExecID = reader.GetString("ibExecID"),
                                IBCommission = reader.GetDecimal("ibCommission"),
                                IBCommissionCurrency = reader.GetString("ibCommissionCurrency"),
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

        private void UpdateTradeIfIncomplete(SqlConnection connection, SqlTransaction transaction, Trade trade, string ibExecID)
        {
            try
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating trade with ibExecID {ibExecID}: {ex.Message}"); Console.WriteLine(ex.Message);
            }
        }

        private void UpdateTrade(SqlConnection connection, SqlTransaction transaction, Trade trade)
        {
            const string updateQuery = @"
                UPDATE dbo.TradeExecutions
                SET InstrumentId = @instrumentId, symbol = @symbol, securityID = @securityID, tradeID = @tradeID, dateTime = @dateTime,
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

            var parameters = TradeParameterBuilder.GetTradeParameters(trade);
            

            ExecuteCommand(connection, transaction, updateQuery, parameters);
        }

        private void InsertTrade(SqlConnection connection, SqlTransaction transaction, Trade trade)
        {
            const string insertQuery = @"
                INSERT INTO [dbo].[TradeExecutions]
                ([InstrumentId], [symbol], [securityID], [tradeID], [dateTime], [tradeDate], [quantity], [tradePrice], [ibCommission],
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
                (@instrumentId, @symbol, @securityID, @tradeID, @dateTime, @tradeDate, @quantity, @tradePrice, @ibCommission,
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

            var parameters = TradeParameterBuilder.GetTradeParameters(trade);

            ExecuteCommand(connection, transaction, insertQuery, parameters);
        }

        private void UpdateTodayExecution(SqlConnection connection, SqlTransaction transaction, TradeConfirm tradeConfirm, string execID)
        {
            const string updateQuery = @"
                UPDATE dbo.TradeExecutions 
                SET symbol = @symbol, tradeDate = @tradeDate, quantity = @quantity, tradePrice = @tradePrice,
                    currency = @currency, conid = @conid, instrumentId = @instrumentId 
                WHERE ibexecID = @execID";

            var parameters = new Dictionary<string, object>
            {
                { "@execID", execID },
                { "@symbol", tradeConfirm.Symbol },
                { "@tradeDate", tradeConfirm.TradeDate },
                { "@quantity", tradeConfirm.Quantity },
                { "@tradePrice", tradeConfirm.Price },
                { "@currency", tradeConfirm.Currency },
                { "@conid", tradeConfirm.ConId },
                { "@instrumentId", tradeConfirm.InstrumentID }
            };

            ExecuteCommand(connection, transaction, updateQuery, parameters);
        }

        private void InsertTodayExecution(SqlConnection connection, SqlTransaction transaction, TradeConfirm tradeConfirm, string execID)
        {
            const string insertQuery = @"
                INSERT INTO dbo.TradeExecutions (ibOrderID, ibexecID, symbol, tradeDate, quantity, tradePrice, currency, conid, instrumentId) 
                VALUES (@ibexecID, @symbol, @tradeDate, @quantity, @tradePrice, @currency, @conid, @instrumentId)";

            var parameters = new Dictionary<string, object>
            {
                { "@ibOrderID", tradeConfirm.IbOrderID.ToString() },
                { "@ibexecID", execID },
                { "@symbol", tradeConfirm.Symbol },
                { "@tradeDate", tradeConfirm.TradeDate },
                { "@quantity", tradeConfirm.Quantity },
                { "@tradePrice", tradeConfirm.Price },
                { "@currency", tradeConfirm.Currency },
                { "@conid", tradeConfirm.ConId },
                { "@instrumentId", tradeConfirm.InstrumentID }
            };

            ExecuteCommand(connection, transaction, insertQuery, parameters);
        }

        /// <summary>
        /// Gets trade execution data by ibExecID and returns the existing trade information
        /// </summary>
        private TradeExecution GetTradeExecutionsByIbExecID(SqlConnection connection, SqlTransaction transaction, string ibExecID, out Trade existingTrade)
        {
            existingTrade = new Trade();
            TradeExecution tradeExecution = null;

            using (var cmd = new SqlCommand(
                @"SELECT InstrumentId, symbol, tradeDate, quantity, tradePrice, currency, conid, ibOrderID, 
                         securityID, tradeID, dateTime 
                  FROM dbo.TradeExecutions 
                  WHERE ibExecID = @ibExecID", 
                connection, transaction))
            {
                cmd.Parameters.AddWithValue("@ibExecID", ibExecID);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // Populate the existing trade with instrumentId and other key fields
                        existingTrade.InstrumentId = reader.IsDBNull(reader.GetOrdinal("InstrumentId")) 
                            ? 0 
                            : reader.GetInt32(reader.GetOrdinal("InstrumentId"));

                        existingTrade.IbExecID = ibExecID;
                        existingTrade.Symbol = reader.IsDBNull(reader.GetOrdinal("symbol")) 
                            ? null 
                            : reader.GetString(reader.GetOrdinal("symbol"));

                        existingTrade.Conid = reader.IsDBNull(reader.GetOrdinal("conid")) 
                            ? null 
                            : reader.GetString(reader.GetOrdinal("conid"));

                        // Populate trade execution for return
                        tradeExecution = new TradeExecution
                        {
                            InstrumentId = existingTrade.InstrumentId,
                            Symbol = existingTrade.Symbol,
                            SecurityId = existingTrade.Conid,
                            TradeDate = reader.IsDBNull(reader.GetOrdinal("tradeDate")) 
                                ? DateTime.MinValue 
                                : reader.GetDateTime(reader.GetOrdinal("tradeDate")),
                            Quantity = reader.IsDBNull(reader.GetOrdinal("quantity")) 
                                ? 0 
                                : reader.GetDecimal(reader.GetOrdinal("quantity")),
                            AveragePrice = reader.IsDBNull(reader.GetOrdinal("tradePrice")) 
                                ? 0 
                                : reader.GetDecimal(reader.GetOrdinal("tradePrice")),
                            Currency = reader.IsDBNull(reader.GetOrdinal("currency")) 
                                ? null 
                                : reader.GetString(reader.GetOrdinal("currency")),
                            IbOrderID = reader.IsDBNull(reader.GetOrdinal("ibOrderID")) 
                                ? 0 
                                : reader.GetInt64(reader.GetOrdinal("ibOrderID"))
                        };
                    }
                }
            }

            return tradeExecution;
        }

        /// <summary>
        /// Gets aggregated trade summary by order ID
        /// Tracks the position from opening through closing executions
        /// </summary>
        public TradeSummary? GetTradeSummaryByOrderId(long orderId)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                // First, get all executions for this order and subsequent related executions
                var query = @"
                    WITH TradeChain AS (
                        -- Get the opening trade details
                        SELECT 
                            ibOrderID,
                            InstrumentId,
                            symbol,
                            tradeDate,
                            dateTime,
                            quantity,
                            tradePrice,
                            buySell,
                            fifoPnlRealized,
                            SUM(quantity) OVER (PARTITION BY symbol, InstrumentId ORDER BY tradeDate, dateTime) as RunningQuantity
                        FROM TradeExecutions
                        WHERE symbol = (SELECT TOP 1 symbol FROM TradeExecutions WHERE ibOrderID = @OrderId)
                            AND InstrumentId = (SELECT TOP 1 InstrumentId FROM TradeExecutions WHERE ibOrderID = @OrderId)
                            AND tradeDate >= (SELECT MIN(tradeDate) FROM TradeExecutions WHERE ibOrderID = @OrderId)
                    ),
                    PositionLifecycle AS (
                        SELECT *,
                            ROW_NUMBER() OVER (ORDER BY tradeDate, dateTime) as RowNum,
                            CASE WHEN RunningQuantity = 0 THEN 1 ELSE 0 END as IsClosed
                        FROM TradeChain
                        WHERE tradeDate >= (SELECT MIN(tradeDate) FROM TradeExecutions WHERE ibOrderID = @OrderId)
                    )
                    SELECT 
                        @OrderId as Id,
                        InstrumentId,
                        symbol as Symbol,
                        MIN(tradeDate) as EntryDate,
                        MAX(CASE WHEN IsClosed = 1 THEN tradeDate END) as ExitDate,
                        CASE 
                            WHEN SUM(CASE WHEN buySell = 'BUY' THEN ABS(quantity) ELSE 0 END) > 
                                 SUM(CASE WHEN buySell = 'SELL' THEN ABS(quantity) ELSE 0 END) THEN 'BUY'
                            ELSE 'SELL'
                        END as BuySell,
                        AVG(CASE WHEN quantity > 0 THEN tradePrice ELSE NULL END) as AvgEntryPrice,
                        AVG(CASE WHEN quantity < 0 THEN tradePrice ELSE NULL END) as AvgExitPrice,
                        MAX(ABS(RunningQuantity)) as TotalQuantity,
                        SUM(ISNULL(fifoPnlRealized, 0)) as TotalPnl
                    FROM PositionLifecycle
                    WHERE RowNum <= (SELECT MIN(RowNum) FROM PositionLifecycle WHERE IsClosed = 1)
                    GROUP BY InstrumentId, symbol";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@OrderId", orderId);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return new TradeSummary
                    {
                        Id = reader.GetInt64("Id"),
                        InstrumentId = reader.GetInt32("InstrumentId"),
                        Symbol = reader.GetString("Symbol"),
                        EntryDate = reader.GetDateTime("EntryDate"),
                        ExitDate = reader.IsDBNull(reader.GetOrdinal("ExitDate")) ? reader.GetDateTime("EntryDate") : reader.GetDateTime("ExitDate"),
                        EntryPrice = reader.IsDBNull(reader.GetOrdinal("AvgEntryPrice")) ? 0 : reader.GetDecimal("AvgEntryPrice"),
                        ExitPrice = reader.IsDBNull(reader.GetOrdinal("AvgExitPrice")) ? 0 : reader.GetDecimal("AvgExitPrice"),
                        Quantity = reader.GetDecimal("TotalQuantity"),
                        Pnl = reader.GetDecimal("TotalPnl"),
                        BuySell = reader.GetString("BuySell")
                    };
                }

                return null;
            });
        }

        /// <summary>
        /// Gets aggregated trade summary by the closing order ID
        /// </summary>
        public TradeSummary? GetTradeSummaryByCloseOrderId(long closeOrderId)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                // Find the close order in trade executions and trace back to find all related executions
                var query = @"
                    WITH CloseExecution AS (
                        SELECT 
                            symbol,
                            InstrumentId,
                            tradeDate,
                            quantity
                        FROM TradeExecutions
                        WHERE ibOrderID = @CloseOrderId
                    ),
                    TradeChain AS (
                        -- Get all executions for the same symbol/instrument leading up to and including the close
                        SELECT 
                            te.ibOrderID,
                            te.InstrumentId,
                            te.symbol,
                            te.tradeDate,
                            te.dateTime,
                            te.quantity,
                            te.tradePrice,
                            te.buySell,
                            te.fifoPnlRealized,
                            SUM(te.quantity) OVER (PARTITION BY te.symbol, te.InstrumentId ORDER BY te.tradeDate, te.dateTime) as RunningQuantity
                        FROM TradeExecutions te
                        INNER JOIN CloseExecution ce ON te.symbol = ce.symbol AND te.InstrumentId = ce.InstrumentId
                        WHERE te.tradeDate <= ce.tradeDate
                    ),
                    PositionLifecycle AS (
                        SELECT *,
                            ROW_NUMBER() OVER (ORDER BY tradeDate, dateTime) as RowNum,
                            CASE WHEN RunningQuantity = 0 THEN 1 ELSE 0 END as IsClosed,
                            CASE WHEN ibOrderID = @CloseOrderId THEN 1 ELSE 0 END as IsCloseOrder
                        FROM TradeChain
                    )
                    SELECT 
                        @CloseOrderId as Id,
                        InstrumentId,
                        symbol as Symbol,
                        MIN(tradeDate) as EntryDate,
                        MAX(CASE WHEN IsCloseOrder = 1 THEN tradeDate END) as ExitDate,
                        CASE 
                            WHEN SUM(CASE WHEN buySell = 'BUY' THEN ABS(quantity) ELSE 0 END) > 
                                 SUM(CASE WHEN buySell = 'SELL' THEN ABS(quantity) ELSE 0 END) THEN 'BUY'
                            ELSE 'SELL'
                        END as BuySell,
                        AVG(CASE WHEN quantity > 0 THEN tradePrice ELSE NULL END) as AvgEntryPrice,
                        AVG(CASE WHEN quantity < 0 THEN tradePrice ELSE NULL END) as AvgExitPrice,
                        MAX(ABS(RunningQuantity)) as TotalQuantity,
                        SUM(ISNULL(fifoPnlRealized, 0)) as TotalPnl
                    FROM PositionLifecycle
                    WHERE RowNum <= (SELECT MAX(RowNum) FROM PositionLifecycle WHERE IsCloseOrder = 1)
                    GROUP BY InstrumentId, symbol";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CloseOrderId", closeOrderId);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return new TradeSummary
                    {
                        Id = reader.GetInt64("Id"),
                        InstrumentId = reader.GetInt32("InstrumentId"),
                        Symbol = reader.GetString("Symbol"),
                        EntryDate = reader.GetDateTime("EntryDate"),
                        ExitDate = reader.IsDBNull(reader.GetOrdinal("ExitDate")) ? reader.GetDateTime("EntryDate") : reader.GetDateTime("ExitDate"),
                        EntryPrice = reader.IsDBNull(reader.GetOrdinal("AvgEntryPrice")) ? 0 : reader.GetDecimal("AvgEntryPrice"),
                        ExitPrice = reader.IsDBNull(reader.GetOrdinal("AvgExitPrice")) ? 0 : reader.GetDecimal("AvgExitPrice"),
                        Quantity = reader.GetDecimal("TotalQuantity"),
                        Pnl = reader.GetDecimal("TotalPnl"),
                        BuySell = reader.GetString("BuySell")
                    };
                }

                return null;
            });
        }
    }
}

