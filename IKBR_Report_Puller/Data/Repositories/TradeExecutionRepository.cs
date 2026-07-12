using Microsoft.Data.SqlClient;
using PikUpStix.TraderView.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using IKBR_Report_Puller.Domain;
namespace IKBR_Report_Puller.Data.Repositories
{
    /// <summary>
    /// Repository for TradeExecution-related database operations
    /// </summary>
    public class TradeExecutionRepository : BaseRepository, ITradeExecutionRepository
    {
        private readonly IPositionRepository _positionRepository;
        private readonly IInstrumentRepository _instrumentRepository;

        public TradeExecutionRepository(string connectionString, IPositionRepository positionRepository, IInstrumentRepository instrumentRepository) : base(connectionString)
        {
            _positionRepository = positionRepository;
            _instrumentRepository = instrumentRepository;
        }

        /// <summary>
        /// Inserts or updates trade executions from a report
        /// </summary>
        public void UpsertTradeExecutions(List<TradeExecution> trades)
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

                        if (!exists)
                        {
                            try
                            {
                                // Get InstrumentId for the trade's symbol
                                int? instrumentId = _instrumentRepository.GetInstrumentIdFromConId(trade.Conid);
                                if (!instrumentId.HasValue || instrumentId.Value == 0)
                                {
                                    throw new InvalidOperationException($"Instrument not found for symbol {trade.Symbol} with Conid {trade.Conid}. Instruments must be upserted before trade executions.");
                                }
                                trade.InstrumentId = instrumentId.Value;

                                // Check for open position for the trade's symbol and instrument (within the same transaction)
                                var openPosition = _positionRepository.GetOpenPosition(connection, transaction, trade.Symbol, instrumentId.Value);

                                // If no open position exists, create a new position and get its ID
                                // If Open Position exists, trade.PositionId = openPosition.Id
                                if (openPosition == null)
                                {
                                    trade.PositionId = _positionRepository.CreatePosition(connection, transaction, instrumentId.Value, trade.Symbol, trade.TradeDate);
                                }
                                else
                                {
                                    trade.PositionId = openPosition.Id;
                                }

                                // Add trade to TradeExecutions table with the correct PositionId
                                InsertTrade(connection, transaction, trade);

                                // Check if latest trade execution closes out the position (i.e., if the sum of quantities for that position is zero)
                                decimal totalQuantity = GetTotalQuantityForPosition(connection, transaction, trade.PositionId);

                                // If position is closed, update the Positions table to mark it as closed and set the close date
                                if (totalQuantity == 0)
                                {
                                    _positionRepository.ClosePosition(connection, transaction, trade.PositionId, trade.TradeDate);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error inserting trade with ibExecID {ibExecID}: {ex.Message}");
                            }
                        }
                    }
                    transaction.Commit();
                }

                Console.WriteLine($"Successfully processed {trades.Count} trades.");
            });
        }

        /// <summary>
        /// Gets the total quantity for a position by summing all trade executions
        /// </summary>
        private decimal GetTotalQuantityForPosition(SqlConnection connection, SqlTransaction transaction, int positionId)
        {
            const string query = @"
                SELECT ISNULL(SUM(quantity), 0) as TotalQuantity
                FROM [dbo].[TradeExecutions]
                WHERE PositionID = @positionId";

            var parameters = new Dictionary<string, object>
            {
                { "@positionId", positionId }
            };

            return ExecuteScalar<decimal>(connection, transaction, query, parameters);
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
                    "SELECT te.PositionID, te.ibOrderID, te.symbol, CONVERT(varchar(8), TRY_CAST(te.tradeDate AS datetime), 112) AS tradeDate, te.quantity, te.tradePrice, te.openCloseIndicator, p.InstrumentId, te.currency, te.conid, te.ibExecID, te.IBCommission, te.IBCommissionCurrency " +
                    "FROM [dbo].[TradeExecutions] te " +
                    "INNER JOIN [dbo].[Positions] p ON te.PositionID = p.Id " +
                    "ORDER BY te.ibOrderID, te.tradeDate ASC, te.dateTime ASC", connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var dd = reader.GetString("tradeDate");
                            tradeExecutions.Add(new TradeExecution
                            {
                                PositionId = reader.GetInt32("PositionID"),
                                IbOrderID = reader.GetInt64("ibOrderID"),
                                Symbol = reader.GetString("symbol"),
                                TradeDate = DateTime.ParseExact(reader.GetString("tradeDate"), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture),
                                Quantity = reader.GetDecimal("quantity"),
                                TradePrice = reader.GetDecimal("tradePrice"),
                                InstrumentId = reader.GetInt32("InstrumentId"),
                                Currency = reader.GetString("currency"),
                                Conid = reader.GetString("conid"),
                                IbExecID = reader.GetString("ibExecID"),
                                IbCommission = reader.GetDecimal("ibCommission"),
                                IbCommissionCurrency = reader.GetString("ibCommissionCurrency")
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
        public void UpsertTodayExecutions(List<TradeExecution> tradeConfirms)
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
                        string execID = tradeConfirm.IbExecID;
                        if (string.IsNullOrEmpty(execID))
                        {
                            Console.WriteLine("TradeExecution confirmation missing execID. Skipping.");
                            continue;
                        }

                        bool exists = RecordExists(connection, transaction,
                            "SELECT COUNT(*) FROM dbo.TradeExecutions WHERE ibExecID = @execID",
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

        private void UpdateTradeIfIncomplete(SqlConnection connection, SqlTransaction transaction, TradeExecution trade, string ibExecID)
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
                Console.WriteLine($"Error updating trade with ibExecID {ibExecID}: {ex.Message}");
            }
        }

        private void UpdateTrade(SqlConnection connection, SqlTransaction transaction, TradeExecution trade)
        {
            // CHANGED: Removed InstrumentId from assignment, converted PositionId to PositionID matching your schema diagram
            const string updateQuery = @"
                UPDATE dbo.TradeExecutions
                SET PositionID = @positionId, symbol = @symbol, securityID = @securityID, tradeID = @tradeID, dateTime = @dateTime,
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

        private void InsertTrade(SqlConnection connection, SqlTransaction transaction, TradeExecution trade)
        {
            const string insertQuery = @"
                INSERT INTO [dbo].[TradeExecutions]
                ([PositionID], [symbol], [securityID], [tradeID], [dateTime], [tradeDate], [quantity], [tradePrice], [ibCommission],
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
                (@positionId, @symbol, @securityID, @tradeID, @dateTime, @tradeDate, @quantity, @tradePrice, @ibCommission,
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
            try
            {
                var parameters = TradeParameterBuilder.GetTradeParameters(trade);
                ExecuteCommand(connection, transaction, insertQuery, parameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting trade with ibExecID {trade.IbExecID}: {ex.Message}");
            }
        }

        private void UpdateTodayExecution(SqlConnection connection, SqlTransaction transaction, TradeExecution tradeConfirm, string execID)
        {
            const string updateQuery = @"
                UPDATE dbo.TradeExecutions 
                SET symbol = @symbol, tradeDate = @tradeDate, quantity = @quantity, tradePrice = @tradePrice,
                    currency = @currency, conid = @conid 
                WHERE ibexecID = @execID";

            var parameters = new Dictionary<string, object>
            {
                { "@execID", execID },
                { "@symbol", tradeConfirm.Symbol },
                { "@tradeDate", tradeConfirm.TradeDate },
                { "@quantity", tradeConfirm.Quantity },
                { "@tradePrice", tradeConfirm.TradePrice },
                { "@currency", tradeConfirm.Currency },
                { "@conid", tradeConfirm.Conid }
            };

            ExecuteCommand(connection, transaction, updateQuery, parameters);
        }

        private void InsertTodayExecution(SqlConnection connection, SqlTransaction transaction, TradeExecution tradeConfirm, string execID)
        {
            const string insertQuery = @"
                INSERT INTO dbo.TradeExecutions (PositionID, ibOrderID, ibexecID, symbol, tradeDate, quantity, tradePrice, currency, conid) 
                VALUES (@positionId, @ibOrderID, @ibexecID, @symbol, @tradeDate, @quantity, @tradePrice, @currency, @conid)";

            var parameters = new Dictionary<string, object>
            {
                { "@positionId", tradeConfirm.PositionId },
                { "@ibOrderID", tradeConfirm.IbOrderID.ToString() },
                { "@ibexecID", execID },
                { "@symbol", tradeConfirm.Symbol },
                { "@tradeDate", tradeConfirm.TradeDate },
                { "@quantity", tradeConfirm.Quantity },
                { "@tradePrice", tradeConfirm.TradePrice },
                { "@currency", tradeConfirm.Currency }, 
                { "@conid", tradeConfirm.Conid }
            };
            try
            {
                ExecuteCommand(connection, transaction, insertQuery, parameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting trade confirmation with ibExecID {execID}: {ex.Message}");
            }
        }
        

        /// <summary>
        /// Gets aggregated trade summary by order ID
        /// Tracks the position from opening through closing executions
        /// </summary>
        public TradeSummary? GetTradeSummaryByOrderId(long orderId)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                // Core CTE update logic injected cleanly here (incorporating previous refactoring context)
                var query = @"
                    WITH TradeChain AS (
                            SELECT 
                                te.ibOrderID,
                                p.InstrumentId,
                                te.PositionID,
                                te.symbol,
                                te.tradeDate,
                                te.dateTime,
                                te.quantity,
                                te.tradePrice,
                                te.buySell,
                                te.fifoPnlRealized,
                                SUM(te.quantity) OVER (PARTITION BY te.symbol, p.InstrumentId ORDER BY te.tradeDate, te.dateTime) as RunningQuantity
                            FROM [TradingBE].[dbo].[TradeExecutions] te
                            INNER JOIN [TradingBE].[dbo].[Positions] p ON te.PositionID = p.Id
                            WHERE te.symbol = (SELECT TOP 1 symbol FROM [TradingBE].[dbo].[TradeExecutions] WHERE ibOrderID = @OrderId)
                                AND p.InstrumentId = (
                                    SELECT TOP 1 p2.InstrumentId 
                                    FROM [TradingBE].[dbo].[TradeExecutions] te2
                                    INNER JOIN [TradingBE].[dbo].[Positions] p2 ON te2.PositionID = p2.Id
                                    WHERE te2.ibOrderID = @OrderId
                                )
                                AND te.tradeDate >= (SELECT MIN(tradeDate) FROM [TradingBE].[dbo].[TradeExecutions] WHERE ibOrderID = @OrderId)
                        ),
                        PositionLifecycle AS (
                            SELECT *,
                                ROW_NUMBER() OVER (ORDER BY tradeDate, dateTime) as RowNum,
                                CASE WHEN RunningQuantity = 0 THEN 1 ELSE 0 END as IsClosed
                            FROM TradeChain
                            WHERE tradeDate >= (SELECT MIN(tradeDate) FROM TradeChain WHERE ibOrderID = @OrderId)
                        )
                        SELECT 
                            @OrderId as Id,
                            InstrumentId,
                            PositionID as PositionId,
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
                        WHERE RowNum <= ISNULL((SELECT MIN(RowNum) FROM PositionLifecycle WHERE IsClosed = 1), (SELECT MAX(RowNum) FROM PositionLifecycle))
                        GROUP BY InstrumentId, PositionID, symbol";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@OrderId", orderId);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return new TradeSummary
                    {
                        Id = reader.GetInt64("Id"),
                        InstrumentId = reader.GetInt32("InstrumentId"),
                        PositionId = reader.GetInt32("PositionId"),
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
                // CHANGED: Restructured query chain to incorporate JOIN on Positions across CloseExecution and TradeChain CTE layers
                var query = @"
                    WITH CloseExecution AS (
                        SELECT 
                            te.symbol,
                            p.InstrumentId,
                            CONVERT(varchar(8), TRY_CAST(te.tradeDate AS datetime), 112) AS tradeDate,
                            te.quantity
                        FROM TradeExecutions te
                        INNER JOIN Positions p ON te.PositionID = p.Id
                        WHERE te.ibOrderID = @CloseOrderId
                    ),
                    TradeChain AS (
                        SELECT 
                            te.ibOrderID,
                            p.InstrumentId,
                            te.PositionID,
                            te.symbol,
                            te.tradeDate,
                            te.dateTime,
                            te.quantity,
                            te.tradePrice,
                            te.buySell,
                            te.fifoPnlRealized,
                            SUM(te.quantity) OVER (PARTITION BY te.symbol, p.InstrumentId ORDER BY te.tradeDate, te.dateTime) as RunningQuantity
                        FROM TradeExecutions te
                        INNER JOIN Positions p ON te.PositionID = p.Id
                        INNER JOIN CloseExecution ce ON te.symbol = ce.symbol AND p.InstrumentId = ce.InstrumentId
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
                        PositionID as PositionId,
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
                    GROUP BY InstrumentId, PositionID, symbol";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CloseOrderId", closeOrderId);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return new TradeSummary
                    {
                        Id = reader.GetInt64(reader.GetOrdinal("Id")),
                        InstrumentId = reader.GetInt32(reader.GetOrdinal("InstrumentId")),
                        PositionId = reader.GetInt32(reader.GetOrdinal("PositionId")),
                        Symbol = reader.GetString(reader.GetOrdinal("Symbol")),
                        EntryDate = DateTime.ParseExact(reader.GetString(reader.GetOrdinal("EntryDate")), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture),
                        ExitDate = reader.IsDBNull(reader.GetOrdinal("ExitDate"))
                            ? DateTime.ParseExact(reader.GetString(reader.GetOrdinal("EntryDate")), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture)
                            : DateTime.ParseExact(reader.GetString(reader.GetOrdinal("ExitDate")), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture),
                        EntryPrice = reader.IsDBNull(reader.GetOrdinal("AvgEntryPrice")) ? 0 : reader.GetDecimal(reader.GetOrdinal("AvgEntryPrice")),
                        ExitPrice = reader.IsDBNull(reader.GetOrdinal("AvgExitPrice")) ? 0 : reader.GetDecimal(reader.GetOrdinal("AvgExitPrice")),
                        Quantity = reader.GetDecimal(reader.GetOrdinal("TotalQuantity")),
                        Pnl = reader.GetDecimal(reader.GetOrdinal("TotalPnl")),
                        BuySell = reader.GetString(reader.GetOrdinal("BuySell"))
                    };
                }

                return null;
            });
        }

        /// <summary>
        /// Gets trade executions for a specific ConId and AccountId, ordered by trade date and time
        /// </summary>
        public List<(DateTime TradeDate, decimal Quantity, string OpenCloseIndicator)> GetTradeExecutionsByConIdAndAccount(long? conid, string accountId)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                var trades = new List<(DateTime TradeDate, decimal Quantity, string OpenCloseIndicator)>();

                const string query = @"
                    SELECT CONVERT(varchar(8), TRY_CAST(tradeDate AS datetime), 112) AS tradeDate, 
                           quantity, 
                           openCloseIndicator 
                    FROM [dbo].[TradeExecutions] 
                    WHERE [conid] = @conid 
                      AND [accountId] = @accountId 
                    ORDER BY tradeDate ASC, dateTime ASC";

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@conid", conid ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@accountId", accountId ?? (object)DBNull.Value);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            trades.Add((
                                DateTime.ParseExact(reader.GetString("tradeDate"), "yyyyMMdd", CultureInfo.InvariantCulture),
                                reader.GetDecimal("quantity"),
                                reader.IsDBNull(reader.GetOrdinal("openCloseIndicator")) ? string.Empty : reader.GetString("openCloseIndicator")
                            ));
                        }
                    }
                }

                return trades;
            });
        }
    }
}