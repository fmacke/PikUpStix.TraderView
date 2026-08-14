using IKBR_Report_Puller.Data;
using IKBR_Report_Puller.Domain;
using Microsoft.Data.SqlClient;
using PikUpStix.TraderView.Domain;
using PikUpStix.TraderView.Interfaces;
using System.Data;
using PikUpStix.TraderView.Data.Scripts.DataComms.TradeExecutions.Changes;

namespace PikUpStix.TraderView.Data.Repositories
{
    /// <summary>
    /// Repository for TradeExecution-related database operations
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
        void ITradeExecutionRepository.UpsertTradeExecutions(List<TradeExecution> trades)
        {
            if (trades == null || !trades.Any())
            {
                Console.WriteLine("No trades to insert.");
                return;
            }

            foreach (var trade in trades)
            {
                string ibExecID = trade.IbExecID;
                bool executionExists = false;
                if (string.IsNullOrEmpty(ibExecID))
                {
                    continue;
                }
                ExecuteDatabaseOperation(connection =>
                {
                    using (var transaction = connection.BeginTransaction())
                    {
                        executionExists = RecordExists(connection, transaction,
                    "SELECT COUNT(*) FROM dbo.TradeExecutions WHERE ibExecID = @ibExecID",
                    new Dictionary<string, object> { { "@ibExecID", ibExecID } });
                        transaction.Commit();
                    }
                });

                if (!executionExists)
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

                        Position? openPosition = null;
                        ExecuteDatabaseOperation(connection =>
                        {
                            using (var transaction = connection.BeginTransaction())
                            {
                                // Check for open position for the trade's symbol and instrument (within the same transaction)
                                openPosition = GetOpenPosition(connection, transaction, trade.Symbol, instrumentId.Value);
                                transaction.Commit();
                            }
                        });
                        // If no open position executionExists, create a new position and get its ID
                        // If Open Position executionExists, trade.PositionId = existingPosition.Id
                        if (openPosition == null)
                        {
                            ExecuteDatabaseOperation(connection =>
                            {
                                using (var transaction = connection.BeginTransaction())
                                {
                                    trade.PositionId = CreatePosition(connection, transaction, instrumentId.Value, trade.Symbol, trade.TradeDate, trade.TradePrice);
                                    transaction.Commit();
                                }
                            });
                            
                        }
                        else
                        {
                            trade.PositionId = openPosition.Id;
                            //TODO:  IF TRANSACTIONID AND BROKERAGEORDERID ARE NULL, THEN THE EXISTING DATABASE ENTRY WAS CREATED FROM A TRADECONFIRM AND NEEDS TO BE FULLY UPDATED WITH THE TRADEEXECUTION DATA.
                            //NEED TO ADD A METHOD TO UPDATE THE EXISTING TRADEEXECUTION RECORD WITH THE NEW DATA.
                            //IF NOT A TRADECONFIRM UPDATE - ONLY THE CLOSEPRICE, COST,FIFOPNLREALIZED, MTMPNL AND TRADEMONEY FIELDS NEED UPDATED
                        }

                        trade.Id = InsertTrade(trade);

                        // Check if latest trade execution closes out the position (i.e., if the sum of quantities for that position is zero)
                        var totalQuantity = 0m;
                        ExecuteDatabaseOperation(connection =>
                        {
                            using (var transaction = connection.BeginTransaction())
                            {
                                totalQuantity = GetTotalQuantityForPosition(connection, transaction, trade.PositionId);
                                transaction.Commit();
                            }
                        });
                        

                        // If position is closed, update the Psitions table to mark it as closed and set the close date
                        if (totalQuantity == 0)
                        {
                            ExecuteDatabaseOperation(connection =>
                            {
                                using (var transaction = connection.BeginTransaction())
                                {
                                    ClosePosition(connection, transaction, trade.PositionId, trade.TradeDate);
                                    transaction.Commit();
                                }
                            });
                            
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error inserting trade with ibExecID {ibExecID}: {ex.Message}");
                    }
                }
            }
        }
        /// <summary>
        /// Gets all positions from the database
        /// </summary>
        List<Position> ITradeExecutionRepository.GetAllPositions()
        {
            return GetPositions("SELECT p.Id, p.OpenDate, p.CloseDate, p.Status, p.InstrumentId, p.LastReportedPrice, p.LastReportedPriceUpdated, " +
                    "i.InstrumentName, i.DataName,i.Currency, i.ConId " +
                    "FROM [dbo].[Positions] p " +
                    "INNER JOIN [dbo].[Instruments] i ON p.InstrumentId = i.Id " +
                    "ORDER BY p.OpenDate DESC");
        }
        /// <summary>
        /// Gets all positions from the database
        /// </summary>
        List<Position> ITradeExecutionRepository.GetOpenPositions()
        {
            return GetPositions("SELECT p.Id, p.OpenDate, p.Status, p.InstrumentId, p.LastReportedPrice, p.LastReportedPriceUpdated, " +
                    "i.InstrumentName, i.DataName, i.Currency, i.ConId, i.ContractUnitType " +
                    "FROM [dbo].[Positions] p " +
                    "INNER JOIN [dbo].[Instruments] i ON p.InstrumentId = i.Id " +
                    "WHERE p.Status = 'Open' " +
                    "ORDER BY p.OpenDate DESC");
        }

        private List<Position> GetPositions(string sqlCommand)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                var positions = new List<Position>();

                using (var cmd = new SqlCommand(
                    sqlCommand, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                DateTime? lastReported = null;
                                if (!reader.IsDBNull(reader.GetOrdinal("LastReportedPriceUpdated")))
                                {
                                    lastReported = reader.GetDateTime(reader.GetOrdinal("LastReportedPriceUpdated"));
                                }

                                positions.Add(new Position
                                {
                                    Id = reader.GetInt32("Id"),
                                    InstrumentId = reader.GetInt32(reader.GetOrdinal("InstrumentId")),
                                    OpenDate = reader.GetDateTime(reader.GetOrdinal("OpenDate")),
                                    LastReportedPriceUpdated = lastReported,
                                    LastReportedPrice = reader["LastReportedPrice"] is DBNull ? 0 : Convert.ToDecimal(reader["LastReportedPrice"]),
                                    Status = reader.GetString(reader.GetOrdinal("Status")),
                                    Instrument = new Instrument
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("InstrumentId")),
                                        InstrumentName = reader["InstrumentName"] is DBNull ? null : reader["InstrumentName"].ToString(),
                                        DataName = reader["DataName"] is DBNull ? null : reader["DataName"].ToString(),
                                        Currency = reader["Currency"] is DBNull ? null : reader["Currency"].ToString(),
                                        ConId = reader["ConId"] is DBNull ? null : reader["ConId"].ToString(),
                                        ContractUnitType = reader["ContractUnitType"] is DBNull ? null : reader["ContractUnitType"].ToString()
                                    }
                                });
                            }
                            catch(Exception ex)
                            {
                                Console.WriteLine("Open Position Read failed.", ex.Message);
                            }
                        }
                    }
                }
                foreach (var position in positions)
                {
                    position.TradeExecutions = ((ITradeExecutionRepository)this).GetByPositionId(position.Id);
                }
                return positions;
            });
        }

        
        /// <summary>
        /// Closes a position by setting its status to 'Closed' and close date
        /// </summary>
        private void ClosePosition(SqlConnection connection, SqlTransaction transaction, int positionId, DateTime closeDate)
        {
            const string updateQuery = @"
                UPDATE [dbo].[Positions]
                SET Status = 'Closed', CloseDate = @closeDate
                WHERE Id = @positionId";

            var parameters = new Dictionary<string, object>
            {
                { "@positionId", positionId },
                { "@closeDate", closeDate }
            };

            using (var cmd = new SqlCommand(updateQuery, connection, transaction))
            {
                try
                {
                    foreach (var param in parameters)
                    {
                        cmd.Parameters.AddWithValue(param.Key, param.Value);
                    }

                    cmd.ExecuteNonQuery();
                    Console.WriteLine($"Closed Position (Id: {positionId}) on {closeDate:yyyy-MM-dd}");
                }
                catch
                {
                    Console.WriteLine("Error closing position with Id: {positionId}. Please check if the position executionExists and is open.");
                }
            }
        }
        /// <summary>
        /// Gets an open position by symbol and instrument ID within a transaction
        /// </summary>
        private Position? GetOpenPosition(SqlConnection connection, SqlTransaction transaction, string symbol, int instrumentId)
        {
            const string query = @"
                SELECT p.Id, p.InstrumentId, p.OpenDate, p.Status, p.LastReportedPrice, p.LastReportedPriceUpdated
                FROM [dbo].[Positions] p WITH (UPDLOCK, ROWLOCK)
                WHERE p.InstrumentId = @instrumentId
                AND p.Status = 'Open'";

            var parameters = new Dictionary<string, object>
            {
                { "@instrumentId", instrumentId }
            };

            using (var cmd = new SqlCommand(query, connection, transaction))
            {
                foreach (var param in parameters)
                {
                    cmd.Parameters.AddWithValue(param.Key, param.Value);
                }

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Position
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            InstrumentId = reader.GetInt32(reader.GetOrdinal("InstrumentId")),
                            OpenDate = reader.GetDateTime(reader.GetOrdinal("OpenDate")),
                            Status = reader.GetString(reader.GetOrdinal("Status")),
                            LastReportedPrice = reader.IsDBNull(reader.GetOrdinal("LastReportedPrice")) ? 0 : reader.GetDecimal(reader.GetOrdinal("LastReportedPrice")),
                            LastReportedPriceUpdated = reader.IsDBNull(reader.GetOrdinal("LastReportedPriceUpdated")) ? null : reader.GetDateTime(reader.GetOrdinal("LastReportedPriceUpdated"))
                        };
                    }
                }
            }

            return null;
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
        List<TradeExecution> ITradeExecutionRepository.GetTradeExecutions()
        {
            return ExecuteDatabaseOperation(connection =>
            {
                var tradeExecutions = new List<TradeExecution>();

                using (var cmd = new SqlCommand(
                    "SELECT te.PositionID, te.ibOrderID, te.symbol, CONVERT(varchar(8), TRY_CAST(te.tradeDate AS datetime), 112) AS tradeDate, te.quantity, te.tradePrice, te.openCloseIndicator, p.InstrumentId, te.currency, te.conid, te.ibExecID, te.IBCommission, te.IBCommissionCurrency, i.ListingExchange " +
                    "FROM [dbo].[TradeExecutions] te " +
                    "INNER JOIN [dbo].[Positions] p ON te.PositionID = p.Id " +
                    "INNER JOIN [dbo].[Instruments] i ON p.InstrumentId = i.Id " +
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
                                IbCommission = reader["ibCommission"] is DBNull ? null : Convert.ToDecimal(reader["ibCommission"]),
                                IbCommissionCurrency = reader["ibCommissionCurrency"] is DBNull ? null : reader["ibCommissionCurrency"].ToString(),
                                ListingExchange = reader.GetString("ListingExchange")
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
        void ITradeExecutionRepository.InsertTradeConfirmations(List<TradeConfirm> tradeConfirms)
        {
            if (tradeConfirms == null || !tradeConfirms.Any())
            {
                Console.WriteLine("No trade confirmations to process.");
                return;
            }

            foreach (var tradeConfirm in tradeConfirms)
            {
                if (string.IsNullOrEmpty(tradeConfirm.IbExecID))
                {
                    Console.WriteLine("TradeExecution confirmation missing execID. Skipping.");
                    continue;
                }
                bool tradeExecutionExists = false;
                ExecuteDatabaseOperation(connection =>
                {
                    using (var transaction = connection.BeginTransaction())
                    {
                        bool tradeExecutionExists = RecordExists(connection, transaction,
                            "SELECT COUNT(*) FROM dbo.TradeExecutions WHERE ibExecID = @execID",
                            new Dictionary<string, object> { { "@execID", tradeConfirm.IbExecID } });
                        //transaction.Commit();  DON'T THINK THIS IS REQUIRED - COMMENTING OUT FOR NOW - CHECK ON NEXT RUN
                    }

                });

                if (!tradeExecutionExists)
                {
                    tradeConfirm.OpenCloseIndicator = "O";  // THIS MAY NOT BE TRUE - TRADE CONFIRMS COULD BE CLOSING FUNCTIONS - MONITOR
                    var instrumentId = _instrumentRepository.GetInstrumentIdFromConId(tradeConfirm.Conid);
                    
                    if (instrumentId.HasValue)
                    {
                        Position? existingPosition = null;
                        ExecuteDatabaseOperation(connection =>
                        {
                            using (var transaction = connection.BeginTransaction())
                            {
                                // Check for open position for the trade's symbol and instrument (within the same transaction)
                                existingPosition = GetOpenPosition(connection, transaction, tradeConfirm.Symbol, instrumentId.Value);
                                transaction.Commit();
                            }
                        });
                        if (existingPosition != null)
                        {
                            tradeConfirm.PositionId = existingPosition.Id;
                        }
                        else
                        {
                            ExecuteDatabaseOperation(connection =>
                            {
                                using (var transaction = connection.BeginTransaction())
                                {
                                    
                                    tradeConfirm.PositionId = CreatePosition(connection, transaction, instrumentId.Value, tradeConfirm.Symbol, tradeConfirm.TradeDate, tradeConfirm.TradePrice);
                                    transaction.Commit();
                                }
                            });
                        }                        
                        tradeConfirm.Id = InsertTradeConfirmation(tradeConfirm);
                    }
                    else
                    {
                        Console.WriteLine($"Instrument not found for symbol {tradeConfirm.Symbol} with Conid {tradeConfirm.Conid}. Skipping trade confirmation.");
                    }
                }
            }              
            Console.WriteLine("Successfully processed today's trade confirmations.");
                
        }
        private int CreatePosition(SqlConnection connection, SqlTransaction transaction, int instrumentId, string symbol, DateTime openDate, decimal openPrice)
        {
            const string insertQuery = @"
                INSERT INTO [dbo].[Positions] (OpenDate, Status, InstrumentId, LastReportedPrice, LastReportedPriceUpdated)
                VALUES (@openDate, @status, @instrumentId, @lastReportedPrice, @LastReportedPriceUpdated);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var parameters = new Dictionary<string, object>
            {
                { "@openDate", openDate },
                { "@status", "Open" },
                { "@instrumentId", instrumentId },
                { "@lastReportedPrice", openPrice },
                { "@LastReportedPriceUpdated", DateTime.Now}
            };

            using (var cmd = new SqlCommand(insertQuery, connection, transaction))
            {
                foreach (var param in parameters)
                {
                    cmd.Parameters.AddWithValue(param.Key, param.Value);
                }

                var result = cmd.ExecuteScalar();
                int newPositionId = Convert.ToInt32(result);

                Console.WriteLine($"Created new Position (Id: {newPositionId}) for symbol {symbol}, InstrumentId {instrumentId} on {openDate:yyyy-MM-dd}");

                return newPositionId;
            }
        }
        private int InsertTrade(TradeExecution trade)
        {
            int newTradeId = 0;
            ExecuteDatabaseOperation(connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    var parameters = TradeParameterBuilder.GetTradeExecutionParams(trade);
                    var insertQuery = new TradeExecutionInsert().Script();
                    newTradeId = ExecuteScalar<int>(connection, transaction, insertQuery, parameters);
                    Console.WriteLine($"Created new Trade (Id: {newTradeId}) for symbol {trade.Symbol}, on {trade.TradeDate:yyyy-MM-dd}");
                }
            });
            return newTradeId;
        }

        private int InsertTradeConfirmation(TradeConfirm tradeConfirm)
        {
            // THIS METHOD HAS BEEN REFACTORED 14/08/26 TO USE THE BASE REPO PROPERLY.  IF IT WORKS, APPLY SAME PATTER TO CreatePosition, InsertTrade
            // and any other methods that use transactions and connections directly.
            // This will ensure proper disposal of resources and consistent error handling.
            int newTradeId = 0;
            ExecuteDatabaseOperation(connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    var parameters = TradeParameterBuilder.GetTradeConfirmationParams(tradeConfirm);
                    var insertQuery = new TradeConfirmationInsert().Script();
                    newTradeId = ExecuteScalar<int>(connection, transaction, insertQuery, parameters);                    
                    Console.WriteLine($"Created new Trade (Id: {newTradeId}) for symbol {tradeConfirm.Symbol}, on {tradeConfirm.TradeDate:yyyy-MM-dd}");
                }
            });
            return newTradeId;
        }
        

        /// <summary>
        /// Gets aggregated trade summary by position ID
        /// Tracks the position from opening through closing executions
        /// </summary>
        TradeSummary? ITradeExecutionRepository.GetTradeSummaryByPositionId(int positionId)
        {
            return ExecuteDatabaseOperation(connection =>
            {
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
                            WHERE te.PositionID = @PositionId
                        ),
                        PositionLifecycle AS (
                            SELECT *,
                                ROW_NUMBER() OVER (ORDER BY tradeDate, dateTime) as RowNum,
                                CASE WHEN RunningQuantity = 0 THEN 1 ELSE 0 END as IsClosed
                            FROM TradeChain
                            WHERE tradeDate >= (SELECT MIN(tradeDate) FROM TradeChain WHERE PositionID = @PositionId)
                        )
                        SELECT 
                            @PositionId as Id,
                            InstrumentId,
                            PositionID as PositionId,
                            symbol as Symbol,
                            CONVERT(VARCHAR(8), CAST(MIN(tradeDate) AS DATETIME), 112) as EntryDate,
		                    CONVERT(VARCHAR(8), CAST(MAX(CASE WHEN IsClosed = 1 THEN tradeDate END) AS DATETIME), 112) as ExitDate,
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
                command.Parameters.AddWithValue("@PositionId", positionId);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    var entryDateStr = reader.GetString("EntryDate");
                    return new TradeSummary
                    {
                        InstrumentId = reader.GetInt32("InstrumentId"),
                        PositionId = reader.GetInt32("PositionId"),
                        Symbol = reader.GetString("Symbol"),
                        EntryDate = DateTime.ParseExact(reader.GetString("EntryDate"), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture),
                        ExitDate = reader.IsDBNull(reader.GetOrdinal("ExitDate")) ? DateTime.ParseExact(reader.GetString("EntryDate"), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture) : DateTime.ParseExact(reader.GetString("ExitDate"), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture),
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
        /// Gets trade executions for a specific ConId and AccountId, ordered by trade date and time
        /// </summary>
        List<(DateTime TradeDate, decimal Quantity, string OpenCloseIndicator)> ITradeExecutionRepository.GetTradeExecutionsByConIdAndAccount(long? conid, string accountId)
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
                                TypeConverters.ConvertStringToDate(reader.GetString("tradeDate")),
                                reader.GetDecimal("quantity"),
                                reader.IsDBNull(reader.GetOrdinal("openCloseIndicator")) ? string.Empty : reader.GetString("openCloseIndicator")
                            ));
                        }
                    }
                }

                return trades;
            });
        }

        /// <summary>
        /// Gets trade executions for a specific position ID
        /// </summary>
        List<TradeExecution> ITradeExecutionRepository.GetByPositionId(int positionId)
        {
            var executions = new List<TradeExecution>();
            ExecuteDatabaseOperation(connection =>
                {
                    const string query = @"
                        SELECT 
                            te.Id, p.InstrumentId, te.symbol, te.tradeID, te.dateTime, te.tradeDate, 
                            te.quantity, te.tradePrice, te.buySell, te.fifoPnlRealized, te.ibCommission,
                            te.openCloseIndicator
                        FROM TradeExecutions te
                        INNER JOIN [dbo].[Positions] p ON te.PositionID = p.Id 
                        WHERE PositionID = @PositionId
                        ORDER BY tradeDate, dateTime";

                    using var command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@PositionId", positionId);

                    try
                    {
                        using var reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            executions.Add(new TradeExecution
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                InstrumentId = reader.GetInt32("InstrumentId"),
                                PositionId = positionId,
                                Symbol = reader.GetString("symbol"),
                                TradeID = reader.GetInt64("tradeID"),
                                DateTime = reader.GetDateTime("dateTime"),
                                TradeDate = reader.GetDateTime("tradeDate"),
                                Quantity = reader.GetDecimal("quantity"),
                                TradePrice = reader.GetDecimal("tradePrice"),
                                BuySell = reader.GetString("buySell"),
                                FifoPnlRealized = reader.GetDecimal("fifoPnlRealized"),
                                IbCommission = reader.GetDecimal("ibCommission"),
                                OpenCloseIndicator = reader.GetString("openCloseIndicator")
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error retrieving trade executions for PositionID {positionId}: {ex.Message}");
                    }
                });
            return executions;
        }
        List<TradeExecution> ITradeExecutionRepository.GetTradeExecutionsByPosition(int positionId)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                var executions = new List<TradeExecution>();

                using (var cmd = new SqlCommand(@"
                        SELECT 
                            id, InstrumentId, symbol, tradeID, dateTime, tradeDate, 
                            quantity, tradePrice, buySell, fifoPnlRealized, ibCommission
                        FROM TradeExecutions
                        WHERE PositionID = @PositionId
                        ORDER BY tradeDate, dateTime", connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            executions.Add(new TradeExecution
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("id")),
                                InstrumentId = reader.GetInt32(reader.GetOrdinal("InstrumentId")),
                                PositionId = positionId,
                                Symbol = reader.GetString("symbol"),
                                TradeID = reader.GetInt64("tradeID"),
                                DateTime = TypeConverters.ConvertStringToDate(reader.GetString("dateTime")),
                                TradeDate = TypeConverters.ConvertStringToDate(reader.GetString("tradeDate")),
                                Quantity = reader.GetDecimal("quantity"),
                                TradePrice = reader.GetDecimal("tradePrice"),
                                BuySell = reader.GetString("buySell"),
                                FifoPnlRealized = reader.GetDecimal("fifoPnlRealized"),
                                IbCommission = reader.GetDecimal("ibCommission")
                            });
                        }
                    }
                }
                return executions;
            });
        }

        /// <summary>
        /// Inserts or updates positions in the database
        /// </summary>
        void ITradeExecutionRepository.UpsertPositions(List<Position> positions)
        {
            if (positions == null || !positions.Any())
            {
                Console.WriteLine("No positions to upsert.");
                return;
            }

            ExecuteDatabaseOperation(connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    int insertedCount = 0;
                    int updatedCount = 0;

                    foreach (var position in positions)
                    {
                        // Ensure instrument executionExists before upserting position
                        if (position.Id == 0)
                        {
                            // New Position
                            // Insert new position
                            string insertQuery = @"
                                INSERT INTO [dbo].[Positions] (OpenDate, Status, InstrumentId, LastReportedPrice, LastReportedPriceUpdated)
                                VALUES (@openDate, @status, @instrumentId, @lastReportedPrice, @lastReportedPriceUpdated)";

                            var insertParameters = new Dictionary<string, object>
                            {
                                { "@openDate", position.OpenDate },
                                { "@status", position.Status },
                                { "@instrumentId", position.InstrumentId },
                                { "@lastReportedPrice", position.LastReportedPrice },
                                { "@lastReportedPriceUpdated", position.LastReportedPriceUpdated ?? (object)DBNull.Value }
                            };

                            ExecuteCommand(connection, transaction, insertQuery, insertParameters);
                            insertedCount++;
                        }
                        else
                        {
                            // Check if position already executionExists for the same InstrumentId and OpenDate
                            bool exists = RecordExists(connection, transaction,
                                "SELECT COUNT(*) FROM dbo.Positions WHERE Id = @positionId",
                                new Dictionary<string, object>
                                {
                                { "@positionId", position.Id }
                                });

                            if (exists)
                            {
                                // Update existing position
                                string updateQuery = @"
                                UPDATE [dbo].[Positions]
                                SET Status = @status, LastReportedPrice = @lastReportedPrice, LastReportedPriceUpdated = @lastReportedPriceUpdated
                                WHERE Id = @positionId";

                                var updateParameters = new Dictionary<string, object>
                            {
                                { "@status", position.Status },
                                { "@positionId", position.Id },
                                { "@lastReportedPrice", position.LastReportedPrice },
                                { "@lastReportedPriceUpdated", position.LastReportedPriceUpdated ?? (object)DBNull.Value }
                            };

                                ExecuteCommand(connection, transaction, updateQuery, updateParameters);
                                updatedCount++;
                            }
                        }
                    }
                    transaction.Commit();

                    Console.WriteLine($"Successfully processed {positions.Count} positions: {insertedCount} inserted, {updatedCount} updated.");
                }
            });
        }
    }
}