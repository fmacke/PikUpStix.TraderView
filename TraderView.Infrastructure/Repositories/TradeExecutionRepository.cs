using Microsoft.Data.SqlClient;
using System.Data;
using System.Transactions;
using TraderView.Application.Features;
using TraderView.Application.Features.Instruments.Query.GetBy;
using TraderView.Application.Features.Positions.Command.Create;
using TraderView.Application.Features.Positions.Command.Update;
using TraderView.Application.Features.Positions.Query.Get;
using TraderView.Application.Features.Positions.Query.GetBy;
using TraderView.Application.Features.TradeExecutions.Command.Create;
using TraderView.Application.Features.TradeExecutions.Command.Update;
using TraderView.Application.Features.TradeExecutions.Query.Get;
using TraderView.Application.Features.TradeExecutions.Query.GetBy;
using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Mappers;
using TraderView.Application.Utils;
using TraderView.Domain.Entities;

namespace TraderView.Infrastructure.Repositories
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
                string ibExecID = trade.IbExecId;
                bool executionExists = false;
                if (string.IsNullOrEmpty(ibExecID))
                {
                    continue;
                }
                executionExists = TradeExists(ibExecID);

                if (!executionExists)
                {
                    try
                    {
                        trade.Position.InstrumentId = Convert.ToInt32(_instrumentRepository.GetInstrumentIdByConId(trade.Conid).Value);
                        trade.PositionId = Convert.ToInt32(GetOpenPosition(trade.Position.InstrumentId)?.Id ?? CreatePosition(trade.Position.InstrumentId, trade.Symbol, trade.TradeDate, Convert.ToDecimal(trade.TradePrice), "O"));
                        trade.Id = CreateTradeExecution(trade);
                        var totalQuantity = GetTotalQuantityForPosition(Convert.ToInt32(trade.PositionId));
                        if (totalQuantity == 0)
                            ClosePosition(Convert.ToInt32(trade.PositionId), trade.DateTime);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error inserting trade with ibExecID {ibExecID}: {ex.Message}");
                    }
                }
                else
                {
                    var tradeExecInDb = GetTradeExecutionByExecID(ibExecID);
                    if(!tradeExecInDb.TransactionId.HasValue)
                    {
                        // Entry was made by TradeConfirmation so will be missing key details. Update the record with the new trade execution details.
                        trade.Id = tradeExecInDb.Id; // Ensure we have the correct Id for the update
                        trade.PositionId = tradeExecInDb.PositionId; // Preserve the existing PositionId
                        UpdateTradeExecution(trade);
                    }
                }
            }
        }

        private bool TradeExists(string ibExecID)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        return RecordExists(connection, transaction, new TransactionExecutionRecordExists(ibExecID));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error checking existence of trade execution with ibExecID {ibExecID}: {ex.Message}");
                        return false;
                    }
                }
            });
        }
        private TradeExecution GetTradeExecutionByExecID(string ibExecID)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                return ExecuteSingle(connection, null, MapFromReader.MapTradeExecution, new GetByIbExecIdQuery(ibExecID));
            });
        }

        /// <summary>
        /// Gets all positions from the database
        /// </summary>
        List<Position> ITradeExecutionRepository.GetAllPositions()
        {
            return GetPositions(new GetAllPositionsQuery());
        }
        /// <summary>
        /// Gets all positions from the database
        /// </summary>
        List<Position> ITradeExecutionRepository.GetOpenPositions()
        {
            return GetPositions(new GetOpenPositionsQuery());
        }

        private List<Position> GetPositions(IQueryWithParameters sqlCommand)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                // 1. Fetch Positions
                var positions = ExecuteList(connection, null, MapFromReader.MapPositionWithInstrumentData, sqlCommand);

                if (!positions.Any())
                    return positions;

                // 2. Fetch TradeExecutions for all retrieved Positions
                var tradeExecutions = ExecuteList(connection, null, MapFromReader.MapTradeExecution, 
                    new TransactionExecutionRecordsByPositionsQuery(positions.Select(p => p.Id).ToList()));

                // 3. Group and assign TradeExecutions to their parent Position
                var executionLookup = tradeExecutions.ToLookup(te => te.PositionId);
                foreach (var position in positions)
                {
                    if (executionLookup.Contains(position.Id))
                    {
                        foreach (var execution in executionLookup[position.Id])
                        {
                            position.TradeExecutions.Add(execution);
                        }
                    }
                }

                return positions;
            });
        }

        public void UpdateTradeExecution(TradeExecution execution)
        {
           ExecuteDatabaseOperation(connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    ExecuteCommand(connection, transaction, new UpdateTradeExecutionCommand(execution));
                    transaction.Commit();
                }
            });
        }
        /// <summary>
        /// Closes a position by setting its status to 'Closed' and close date
        /// </summary>
        private void ClosePosition(int positionId, DateTime closeDate)
        {
            ExecuteDatabaseOperation(connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        ExecuteCommand(connection, transaction, new ClosePositionCommand(positionId, closeDate));
                        transaction.Commit();
                        Console.WriteLine($"Closed Position (Id: {positionId}) on {closeDate:yyyy-MM-dd}");
                    }
                    catch
                    {
                        Console.WriteLine("Error closing position with Id: {positionId}. Please check if the position executionExists and is open.");
                    }
                }
            });
        }
        private Position? GetOpenPosition(int instrumentId)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var position =  ExecuteSingle(connection, transaction, MapFromReader.MapPosition, new GetOpenPositionByInstrumentIdQuery(instrumentId));
                        if(position != null)
                        {
                            var tradeExecutions = ExecuteList(connection, transaction, MapFromReader.MapTradeExecution, new GetByPositionIdQuery(position.Id));
                            position.TradeExecutions = tradeExecutions;
                        }
                        return position;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error retrieving open position for InstrumentId {instrumentId}: {ex.Message}");
                        return null;
                    }
                }
            }); 
        }        


        /// <summary>
        /// Gets the total quantity for a position by summing all trade executions
        /// </summary>
        private decimal GetTotalQuantityForPosition(int positionId)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var quantity = ExecuteScalar<decimal>(connection, transaction, new GetPositionQuantityByPositionId(positionId));
                        return quantity;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error calculating total quantity for PositionId {positionId}: {ex.Message}");
                        throw;
                    }
                }
            });
        }

        /// <summary>
        /// Gets all trade executions ordered by order ID and date
        /// </summary>        
        List<TradeExecution> ITradeExecutionRepository.GetTradeExecutions()
        {
            return ExecuteDatabaseOperation(connection =>
            {
                return ExecuteList(connection, null, MapFromReader.MapTradeExecution, new GetAllTradeExecutionsQuery());
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

                if (!TradeExists(tradeConfirm.IbExecID))
                {
                    var instrumentId = _instrumentRepository.GetInstrumentIdByConId(tradeConfirm.Conid);
                    
                    if (instrumentId.HasValue)
                    {
                        Position? existingPosition = null;
 
                        // Check for open position for the trade's symbol and instrument (within the same transaction)
                        existingPosition = GetOpenPosition(instrumentId.Value);
                        var openDirection = existingPosition.TradeExecutions.MinBy(x => x.Id).BuySell;
                        tradeConfirm.PositionId = existingPosition.Id;
                        tradeConfirm.OpenCloseIndicator = "O";
                        tradeConfirm.PositionId = existingPosition.Id;

                        if (existingPosition != null)
                        {
                            if (tradeConfirm.BuySell != openDirection)
                            {
                                tradeConfirm.OpenCloseIndicator = "C";
                                if (existingPosition?.TradeExecutions.Sum(x => x.Quantity) + tradeConfirm.Quantity == 0)
                                {
                                    ClosePosition(existingPosition.Id, tradeConfirm.DateTime);
                                }
                            }
                        }
                        else
                        {
                            tradeConfirm.OpenCloseIndicator = "O";
                            tradeConfirm.PositionId = CreatePosition(instrumentId.Value, tradeConfirm.Symbol, tradeConfirm.TradeDate, tradeConfirm.TradePrice, tradeConfirm.OpenCloseIndicator);
                        }
                        tradeConfirm.Id = CreateTradeConfirmation(tradeConfirm);
                    }
                    else
                    {
                        Console.WriteLine($"Instrument not found for symbol {tradeConfirm.Symbol} with Conid {tradeConfirm.Conid}. Skipping trade confirmation.");
                    }
                }
            }              
            Console.WriteLine("Successfully processed today's trade confirmations.");
                
        }
        private int CreatePosition(int instrumentId, string symbol, DateTime openDate, decimal openPrice, string openCloseIndicator)
        {
            var instrument = _instrumentRepository.Get(instrumentId);
            return ExecuteDatabaseOperation(connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {                        
                        int newPositionId = ExecuteScalar<int>(connection, transaction, new CreatePositionCommand(instrumentId, openDate, openPrice, openCloseIndicator, instrument.ContractUnitType == "CASH" ? true : false));
                        transaction.Commit();
                        Console.WriteLine($"Created new Position (Id: {newPositionId}) for symbol {symbol}, InstrumentId {instrumentId} on {openDate:yyyy-MM-dd}");
                        return newPositionId;

                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error inserting position for instrumentId {instrumentId} on {openDate:yyyy-MM-dd}: {ex.Message}");
                        throw;
                    }
                }
            });
        }
        private void UpdatePosition(int positionId, DateTime latestPriceUpdated, decimal latestPrice, string openCloseIndicator)
        {
            ExecuteDatabaseOperation(connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Check if position already executionExists for the same InstrumentId and OpenDate
                        bool exists = RecordExists(connection, transaction, new PositionRecordExists(positionId));

                        if (exists)
                        {
                            // Update existing position
                            ExecuteCommand(connection, transaction, new UpdatePositionPricesCommand(positionId, latestPrice, latestPriceUpdated));
                            transaction.Commit();
                        }

                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error updating position for positionId {positionId }: {ex.Message}");
                        throw;
                    }
                }
            });
        }
        private int CreateTradeExecution(TradeExecution trade)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    int tradeId = ExecuteScalar<int>(connection, transaction, new InsertTradeExecutionCommand(trade));
                    transaction.Commit();
                    Console.WriteLine($"Created new Trade Excecution (Id: {tradeId}) for symbol {trade.Symbol}, on {trade.TradeDate:yyyy-MM-dd}");
                    return tradeId;
                }
            });
        }

        private int CreateTradeConfirmation(TradeConfirm tradeConfirm)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        int tradeId = ExecuteScalar<int>(connection, transaction, new InsertTradeConfirmationCommand(tradeConfirm));
                        transaction.Commit();
                        Console.WriteLine($"Created new Trade Confirmation (PositionId: {tradeConfirm.PositionId}) for symbol {tradeConfirm.Symbol}, on {tradeConfirm.TradeDate:yyyy-MM-dd}");
                        return tradeId;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error inserting trade confirmation for symbol {tradeConfirm.Symbol} with PositionId {tradeConfirm.PositionId} on {tradeConfirm.TradeDate:yyyy-MM-dd}: {ex.Message}");
                        throw;
                    }
                }
            });
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
        List<TradeExecution> ITradeExecutionRepository.GetTradeExecutionsByPositionId(int positionId)
        {
           return ExecuteDatabaseOperation(connection =>
            {
                return ExecuteList(connection,null, MapFromReader.MapTradeExecution, new GetByPositionIdQuery(positionId));
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

            int insertedCount = 0;
            int updatedCount = 0;

            foreach (var position in positions)
            {
                // Ensure instrument executionExists before upserting position
                if (position.Id == 0)
                {  
                    CreatePosition(position.InstrumentId, position.Instrument?.DataName ?? "Unknown",position.OpenDate, position.LastReportedPrice ?? 0m, "O"); 
                    insertedCount++;
                }
                else
                {
                    UpdatePosition(position.Id, DateTime.Now, position.LastReportedPrice ?? 0m, "O");
                    updatedCount++;
                }
            }
            Console.WriteLine($"Successfully processed {positions.Count} positions: {insertedCount} inserted, {updatedCount} updated.");
        }
    }
}