using Microsoft.Data.SqlClient;
using PikUpStix.TraderView.Interfaces;
using PikUpStix.TraderView.Domain;
using IKBR_Report_Puller.Domain;
namespace IKBR_Report_Puller.Data.Repositories
{
    public class PositionRepository : BaseRepository, IPositionRepository
    {
        private readonly IInstrumentRepository _instrumentRepository;

        public PositionRepository(string connectionString, IInstrumentRepository instrumentRepository) : base(connectionString)
        {
            _instrumentRepository = instrumentRepository;
        }

        /// <summary>
        /// Gets all positions from the database
        /// </summary>
        List<Position> IPositionRepository.GetAllPositions()
        {
            return ExecuteDatabaseOperation(connection =>
            {
                var positions = new List<Position>();

                using (var cmd = new SqlCommand(
                    "SELECT p.Id, p.OpenDate, p.CloseDate, p.Status, p.InstrumentId, " +
                    "i.Symbol, i.Currency, i.SecurityId " +
                    "FROM [dbo].[Positions] p " +
                    "INNER JOIN [dbo].[Instruments] i ON p.InstrumentId = i.Id " +
                    "ORDER BY p.OpenDate DESC", connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            positions.Add(new Position
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                InstrumentId = reader.GetInt32(reader.GetOrdinal("InstrumentId")),
                                OpenDate = reader.GetDateTime(reader.GetOrdinal("OpenDate")),
                                CloseDate = reader.GetDateTime(reader.GetOrdinal("CloseDate")),
                                Status = reader.GetString(reader.GetOrdinal("Status")),
                            });
                        }
                    }
                }

                return positions;
            });
        }
        List<Position> IPositionRepository.GetAllOpenPositions()
        {
            return ExecuteDatabaseOperation(connection =>
            {
                var positions = new List<Position>();

                using (var cmd = new SqlCommand(
                    "SELECT p.Id, p.OpenDate, p.CloseDate, p.Status, p.InstrumentId, " +
                    "i.InstrumentName, i.Currency, i.ConId " +
                    "FROM [dbo].[Positions] p " +
                    "INNER JOIN [dbo].[Instruments] i ON p.InstrumentId = i.Id " +
                    "WHERE p.CloseDate IS NULL " +
                    "ORDER BY p.OpenDate DESC", connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            positions.Add(new Position
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                InstrumentId = reader.GetInt32(reader.GetOrdinal("InstrumentId")),
                                OpenDate = reader.GetDateTime(reader.GetOrdinal("OpenDate")),
                                CloseDate = reader.IsDBNull(reader.GetOrdinal("CloseDate")) ? (DateTime?)null : reader. GetDateTime(reader.GetOrdinal("CloseDate")),
                                Status = reader.GetString(reader.GetOrdinal("Status")),
                                Instrument = new Instrument
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("InstrumentId")),
                                    InstrumentName = reader.GetString(reader.GetOrdinal("InstrumentName")),
                                    Currency = reader.GetString(reader.GetOrdinal("Currency")),
                                    ConId = reader.GetString(reader.GetOrdinal("ConId"))
                                }
                            });
                        }
                    }
                }

                return positions;
            });
        }

        /// <summary>
        /// Inserts or updates positions in the database
        /// </summary>
        void IPositionRepository.UpsertPositions(List<Position> positions)
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
                        // Ensure instrument exists before upserting position
                        if (position.InstrumentId == 0)
                        {
                            Console.WriteLine($"Position for {position.Id} missing InstrumentId. Skipping.");
                            continue;
                        }

                        // Check if position already exists for the same InstrumentId and OpenDate
                        bool exists = RecordExists(connection, transaction,
                            "SELECT COUNT(*) FROM dbo.Positions WHERE InstrumentId = @instrumentId AND CAST(OpenDate AS DATE) = CAST(@openDate AS DATE)",
                            new Dictionary<string, object>
                            {
                                { "@instrumentId", position.InstrumentId },
                                { "@openDate", position.OpenDate }
                            });

                        if (exists)
                        {
                            // Update existing position
                            string updateQuery = @"
                                UPDATE [dbo].[Positions]
                                SET Status = @status
                                WHERE InstrumentId = @instrumentId 
                                AND CAST(OpenDate AS DATE) = CAST(@openDate AS DATE)";

                            var updateParameters = new Dictionary<string, object>
                            {
                                { "@status", position.Status },
                                { "@instrumentId", position.InstrumentId },
                                { "@openDate", position.OpenDate }
                            };

                            ExecuteCommand(connection, transaction, updateQuery, updateParameters);
                            updatedCount++;
                        }
                        else
                        {
                            // Insert new position
                            string insertQuery = @"
                                INSERT INTO [dbo].[Positions] (OpenDate, Status, InstrumentId)
                                VALUES (@openDate, @status, @instrumentId)";

                            var insertParameters = new Dictionary<string, object>
                            {
                                { "@openDate", position.OpenDate },
                                { "@status", position.Status },
                                { "@instrumentId", position.InstrumentId }
                            };

                            ExecuteCommand(connection, transaction, insertQuery, insertParameters);
                            insertedCount++;
                        }
                    }
                    transaction.Commit();

                    Console.WriteLine($"Successfully processed {positions.Count} positions: {insertedCount} inserted, {updatedCount} updated.");
                }
            });
        }

        /// <summary>
        /// Gets an open position by symbol and instrument ID
        /// </summary>
        Position? IPositionRepository.GetOpenPosition(string symbol, int instrumentId)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                return ((IPositionRepository)this).GetOpenPosition(connection, null, symbol, instrumentId);
            });
        }

        /// <summary>
        /// Gets an open position by symbol and instrument ID within a transaction
        /// </summary>
        Position? IPositionRepository.GetOpenPosition(SqlConnection connection, SqlTransaction transaction, string symbol, int instrumentId)
        {
            const string query = @"
                SELECT p.Id, p.InstrumentId, p.OpenDate, p.Status
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
                            Status = reader.GetString(reader.GetOrdinal("Status"))
                        };
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Creates a new position and returns its ID
        /// </summary>
        int IPositionRepository.CreatePosition(int instrumentId, string symbol, DateTime openDate)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    int positionId = ((IPositionRepository)this).CreatePosition(connection, transaction, instrumentId, symbol, openDate);
                    transaction.Commit();
                    return positionId;
                }
            });
        }

        /// <summary>
        /// Creates a new position and returns its ID within a transaction
        /// </summary>
        int IPositionRepository.CreatePosition(SqlConnection connection, SqlTransaction transaction, int instrumentId, string symbol, DateTime openDate)
        {
            const string insertQuery = @"
                INSERT INTO [dbo].[Positions] (OpenDate, Status, InstrumentId)
                VALUES (@openDate, @status, @instrumentId);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var parameters = new Dictionary<string, object>
            {
                { "@openDate", openDate },
                { "@status", "Open" },
                { "@instrumentId", instrumentId }
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

        /// <summary>
        /// Closes a position by setting its status to 'Closed' and close date
        /// </summary>
        void IPositionRepository.ClosePosition(SqlConnection connection, SqlTransaction transaction, int positionId, DateTime closeDate)
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
                    Console.WriteLine("Error closing position with Id: {positionId}. Please check if the position exists and is open.");
                }
            }
        }

        /// <summary>
        /// Gets all positions with Status = 'Open' and their associated trade executions
        /// </summary>
        List<Position> IPositionRepository.GetOpenPositions()
        {
            return ExecuteDatabaseOperation(connection =>
            {
                var positions = new List<Position>();
                var positionDict = new Dictionary<int, Position>();

                // First, get all open positions
                using (var cmd = new SqlCommand(
                    @"SELECT p.Id, p.OpenDate, p.CloseDate, p.Status, p.InstrumentId,
                             i.InstrumentName, i.Currency, i.ConId
                      FROM [dbo].[Positions] p
                      INNER JOIN [dbo].[Instruments] i ON p.InstrumentId = i.Id
                      WHERE p.Status = 'Open'
                      ORDER BY p.OpenDate DESC", connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var position = new Position
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                InstrumentId = reader.GetInt32(reader.GetOrdinal("InstrumentId")),
                                OpenDate = reader.GetDateTime(reader.GetOrdinal("OpenDate")),
                                CloseDate = reader.IsDBNull(reader.GetOrdinal("CloseDate")) ? null : reader.GetDateTime(reader.GetOrdinal("CloseDate")),
                                Status = reader.GetString(reader.GetOrdinal("Status")),
                                Instrument = new Instrument
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("InstrumentId")),
                                    InstrumentName = reader.GetString(reader.GetOrdinal("InstrumentName")),
                                    Currency = reader.GetString(reader.GetOrdinal("Currency")),
                                    ConId = reader.GetString(reader.GetOrdinal("ConId"))
                                },
                                TradeExecutions = new List<TradeExecution>()
                            };

                            positions.Add(position);
                            positionDict[position.Id] = position;
                        }
                    }
                }

                // If no open positions found, return empty list
                if (!positions.Any())
                {
                    return positions;
                }

                // Now get all trade executions for these positions
                var positionIds = string.Join(",", positionDict.Keys);
                var tradeQuery = $@"
                    SELECT Id, InstrumentId, PositionId, AccountId, AcctAlias, Model, Currency, 
                           FxRateToBase, AssetCategory, Symbol, Description, Conid, SecurityIDType, 
                           Cusip, Isin, Figi, ListingExchange, UnderlyingConid, UnderlyingSymbol, 
                           UnderlyingSecurityID, UnderlyingListingExchange, Issuer, IssuerCountryCode, 
                           Multiplier, Strike, Expiry, PutCall, PrincipalAdjustFactor, ReportDate, 
                           TradeID, TradeDate, DateTime, SettleDateTarget, TransactionType, Exchange, 
                           Quantity, TradePrice, TradeMoney, Proceeds, Taxes, IbCommission, 
                           IbCommissionCurrency, NetCash, ClosePrice, OpenCloseIndicator, Notes, Cost, 
                           FifoPnlRealized, MtmPnl, OrigTradePrice, OrigTradeDate, OrigTradeID, 
                           OrigOrderID, OrigTransactionID, ClearingFirmID, TransactionID, IbOrderID, 
                           IbExecID, BrokerageOrderID, OrderReference, VolatilityOrderLink, ExchOrderId, 
                           ExtExecID, OrderTime, OpenDateTime, HoldingPeriodDateTime, WhenRealized, 
                           WhenReopened, LevelOfDetail, ChangeInPrice, ChangeInQuantity, OrderType, 
                           TraderID, IsAPIOrder, AccruedInt, SubCategory, BuySell, InitialInvestment, 
                           RelatedTradeID, RelatedTransactionID, Rtn, PositionActionID, SerialNumber, 
                           DeliveryType, CommodityType, Fineness, Weight
                    FROM [dbo].[TradeExecutions]
                    WHERE PositionID IN ({positionIds})
                    ORDER BY TradeDate, DateTime";

                using (var cmd = new SqlCommand(tradeQuery, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int positionId = reader.GetInt32(reader.GetOrdinal("PositionId"));

                            if (positionDict.TryGetValue(positionId, out var position))
                            {
                                var tradeExecution = new TradeExecution
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    InstrumentId = reader.GetInt32(reader.GetOrdinal("InstrumentId")),
                                    PositionId = positionId,
                                    AccountId = reader.IsDBNull(reader.GetOrdinal("AccountId")) ? null : reader.GetString(reader.GetOrdinal("AccountId")),
                                    AcctAlias = reader.IsDBNull(reader.GetOrdinal("AcctAlias")) ? null : reader.GetString(reader.GetOrdinal("AcctAlias")),
                                    Model = reader.IsDBNull(reader.GetOrdinal("Model")) ? null : reader.GetString(reader.GetOrdinal("Model")),
                                    Currency = reader.IsDBNull(reader.GetOrdinal("Currency")) ? null : reader.GetString(reader.GetOrdinal("Currency")),
                                    FxRateToBase = reader.IsDBNull(reader.GetOrdinal("FxRateToBase")) ? null : reader.GetDecimal(reader.GetOrdinal("FxRateToBase")),
                                    AssetCategory = reader.IsDBNull(reader.GetOrdinal("AssetCategory")) ? null : reader.GetString(reader.GetOrdinal("AssetCategory")),
                                    Symbol = reader.IsDBNull(reader.GetOrdinal("Symbol")) ? null : reader.GetString(reader.GetOrdinal("Symbol")),
                                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                                    Conid = reader.IsDBNull(reader.GetOrdinal("Conid")) ? null : reader.GetString(reader.GetOrdinal("Conid")),
                                    SecurityIDType = reader.IsDBNull(reader.GetOrdinal("SecurityIDType")) ? null : reader.GetString(reader.GetOrdinal("SecurityIDType")),
                                    Cusip = reader.IsDBNull(reader.GetOrdinal("Cusip")) ? null : reader.GetString(reader.GetOrdinal("Cusip")),
                                    Isin = reader.IsDBNull(reader.GetOrdinal("Isin")) ? null : reader.GetString(reader.GetOrdinal("Isin")),
                                    Figi = reader.IsDBNull(reader.GetOrdinal("Figi")) ? null : reader.GetString(reader.GetOrdinal("Figi")),
                                    ListingExchange = reader.IsDBNull(reader.GetOrdinal("ListingExchange")) ? null : reader.GetString(reader.GetOrdinal("ListingExchange")),
                                    UnderlyingConid = reader.IsDBNull(reader.GetOrdinal("UnderlyingConid")) ? null : reader.GetString(reader.GetOrdinal("UnderlyingConid")),
                                    UnderlyingSymbol = reader.IsDBNull(reader.GetOrdinal("UnderlyingSymbol")) ? null : reader.GetString(reader.GetOrdinal("UnderlyingSymbol")),
                                    UnderlyingSecurityID = reader.IsDBNull(reader.GetOrdinal("UnderlyingSecurityID")) ? null : reader.GetString(reader.GetOrdinal("UnderlyingSecurityID")),
                                    UnderlyingListingExchange = reader.IsDBNull(reader.GetOrdinal("UnderlyingListingExchange")) ? null : reader.GetString(reader.GetOrdinal("UnderlyingListingExchange")),
                                    Issuer = reader.IsDBNull(reader.GetOrdinal("Issuer")) ? null : reader.GetString(reader.GetOrdinal("Issuer")),
                                    IssuerCountryCode = reader.IsDBNull(reader.GetOrdinal("IssuerCountryCode")) ? null : reader.GetString(reader.GetOrdinal("IssuerCountryCode")),
                                    Multiplier = reader.IsDBNull(reader.GetOrdinal("Multiplier")) ? null : reader.GetInt32(reader.GetOrdinal("Multiplier")),
                                    Strike = reader.IsDBNull(reader.GetOrdinal("Strike")) ? null : reader.GetDecimal(reader.GetOrdinal("Strike")),
                                    Expiry = reader.IsDBNull(reader.GetOrdinal("Expiry")) ? null : reader.GetString(reader.GetOrdinal("Expiry")),
                                    PutCall = reader.IsDBNull(reader.GetOrdinal("PutCall")) ? null : reader.GetString(reader.GetOrdinal("PutCall")),
                                    PrincipalAdjustFactor = reader.IsDBNull(reader.GetOrdinal("PrincipalAdjustFactor")) ? null : reader.GetDecimal(reader.GetOrdinal("PrincipalAdjustFactor")),
                                    ReportDate = reader.GetDateTime(reader.GetOrdinal("ReportDate")),
                                    TradeID = reader.IsDBNull(reader.GetOrdinal("TradeID")) ? null : reader.GetInt64(reader.GetOrdinal("TradeID")),
                                    TradeDate = reader.GetDateTime(reader.GetOrdinal("TradeDate")),
                                    DateTime = reader.GetDateTime(reader.GetOrdinal("DateTime")),
                                    SettleDateTarget = reader.GetDateTime(reader.GetOrdinal("SettleDateTarget")),
                                    TransactionType = reader.IsDBNull(reader.GetOrdinal("TransactionType")) ? null : reader.GetString(reader.GetOrdinal("TransactionType")),
                                    Exchange = reader.IsDBNull(reader.GetOrdinal("Exchange")) ? null : reader.GetString(reader.GetOrdinal("Exchange")),
                                    Quantity = reader.GetDecimal(reader.GetOrdinal("Quantity")),
                                    TradePrice = reader.GetDecimal(reader.GetOrdinal("TradePrice")),
                                    TradeMoney = reader.IsDBNull(reader.GetOrdinal("TradeMoney")) ? null : reader.GetDecimal(reader.GetOrdinal("TradeMoney")),
                                    Proceeds = reader.IsDBNull(reader.GetOrdinal("Proceeds")) ? null : reader.GetDecimal(reader.GetOrdinal("Proceeds")),
                                    Taxes = reader.IsDBNull(reader.GetOrdinal("Taxes")) ? null : reader.GetDecimal(reader.GetOrdinal("Taxes")),
                                    IbCommission = reader.IsDBNull(reader.GetOrdinal("IbCommission")) ? null : reader.GetDecimal(reader.GetOrdinal("IbCommission")),
                                    IbCommissionCurrency = reader.IsDBNull(reader.GetOrdinal("IbCommissionCurrency")) ? null : reader.GetString(reader.GetOrdinal("IbCommissionCurrency")),
                                    NetCash = reader.IsDBNull(reader.GetOrdinal("NetCash")) ? null : reader.GetDecimal(reader.GetOrdinal("NetCash")),
                                    ClosePrice = reader.IsDBNull(reader.GetOrdinal("ClosePrice")) ? null : reader.GetDecimal(reader.GetOrdinal("ClosePrice")),
                                    OpenCloseIndicator = reader.IsDBNull(reader.GetOrdinal("OpenCloseIndicator")) ? null : reader.GetString(reader.GetOrdinal("OpenCloseIndicator")),
                                    Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString(reader.GetOrdinal("Notes")),
                                    Cost = reader.IsDBNull(reader.GetOrdinal("Cost")) ? null : reader.GetDecimal(reader.GetOrdinal("Cost")),
                                    FifoPnlRealized = reader.IsDBNull(reader.GetOrdinal("FifoPnlRealized")) ? null : reader.GetDecimal(reader.GetOrdinal("FifoPnlRealized")),
                                    MtmPnl = reader.IsDBNull(reader.GetOrdinal("MtmPnl")) ? null : reader.GetDecimal(reader.GetOrdinal("MtmPnl")),
                                    OrigTradePrice = reader.IsDBNull(reader.GetOrdinal("OrigTradePrice")) ? null : reader.GetDecimal(reader.GetOrdinal("OrigTradePrice")),
                                    OrigTradeDate = reader.IsDBNull(reader.GetOrdinal("OrigTradeDate")) ? null : reader.GetString(reader.GetOrdinal("OrigTradeDate")),
                                    OrigTradeID = reader.IsDBNull(reader.GetOrdinal("OrigTradeID")) ? null : reader.GetString(reader.GetOrdinal("OrigTradeID")),
                                    OrigOrderID = reader.IsDBNull(reader.GetOrdinal("OrigOrderID")) ? null : reader.GetInt64(reader.GetOrdinal("OrigOrderID")),
                                    OrigTransactionID = reader.IsDBNull(reader.GetOrdinal("OrigTransactionID")) ? null : reader.GetInt64(reader.GetOrdinal("OrigTransactionID")),
                                    ClearingFirmID = reader.IsDBNull(reader.GetOrdinal("ClearingFirmID")) ? null : reader.GetString(reader.GetOrdinal("ClearingFirmID")),
                                    TransactionID = reader.IsDBNull(reader.GetOrdinal("TransactionID")) ? null : reader.GetInt64(reader.GetOrdinal("TransactionID")),
                                    IbOrderID = reader.IsDBNull(reader.GetOrdinal("IbOrderID")) ? null : reader.GetInt64(reader.GetOrdinal("IbOrderID")),
                                    IbExecID = reader.IsDBNull(reader.GetOrdinal("IbExecID")) ? null : reader.GetString(reader.GetOrdinal("IbExecID")),
                                    BrokerageOrderID = reader.IsDBNull(reader.GetOrdinal("BrokerageOrderID")) ? null : reader.GetString(reader.GetOrdinal("BrokerageOrderID")),
                                    OrderReference = reader.IsDBNull(reader.GetOrdinal("OrderReference")) ? null : reader.GetString(reader.GetOrdinal("OrderReference")),
                                    VolatilityOrderLink = reader.IsDBNull(reader.GetOrdinal("VolatilityOrderLink")) ? null : reader.GetString(reader.GetOrdinal("VolatilityOrderLink")),
                                    ExchOrderId = reader.IsDBNull(reader.GetOrdinal("ExchOrderId")) ? null : reader.GetString(reader.GetOrdinal("ExchOrderId")),
                                    ExtExecID = reader.IsDBNull(reader.GetOrdinal("ExtExecID")) ? null : reader.GetString(reader.GetOrdinal("ExtExecID")),
                                    OrderTime = reader.IsDBNull(reader.GetOrdinal("OrderTime")) ? null : reader.GetString(reader.GetOrdinal("OrderTime")),
                                    OpenDateTime = reader.IsDBNull(reader.GetOrdinal("OpenDateTime")) ? null : reader.GetString(reader.GetOrdinal("OpenDateTime")),
                                    HoldingPeriodDateTime = reader.IsDBNull(reader.GetOrdinal("HoldingPeriodDateTime")) ? null : reader.GetString(reader.GetOrdinal("HoldingPeriodDateTime")),
                                    WhenRealized = reader.IsDBNull(reader.GetOrdinal("WhenRealized")) ? null : reader.GetString(reader.GetOrdinal("WhenRealized")),
                                    WhenReopened = reader.IsDBNull(reader.GetOrdinal("WhenReopened")) ? null : reader.GetString(reader.GetOrdinal("WhenReopened")),
                                    LevelOfDetail = reader.IsDBNull(reader.GetOrdinal("LevelOfDetail")) ? null : reader.GetString(reader.GetOrdinal("LevelOfDetail")),
                                    ChangeInPrice = reader.IsDBNull(reader.GetOrdinal("ChangeInPrice")) ? null : reader.GetDecimal(reader.GetOrdinal("ChangeInPrice")),
                                    ChangeInQuantity = reader.IsDBNull(reader.GetOrdinal("ChangeInQuantity")) ? null : reader.GetDecimal(reader.GetOrdinal("ChangeInQuantity")),
                                    OrderType = reader.IsDBNull(reader.GetOrdinal("OrderType")) ? null : reader.GetString(reader.GetOrdinal("OrderType")),
                                    TraderID = reader.IsDBNull(reader.GetOrdinal("TraderID")) ? null : reader.GetString(reader.GetOrdinal("TraderID")),
                                    IsAPIOrder = reader.IsDBNull(reader.GetOrdinal("IsAPIOrder")) ? null : reader.GetString(reader.GetOrdinal("IsAPIOrder")),
                                    AccruedInt = reader.IsDBNull(reader.GetOrdinal("AccruedInt")) ? null : reader.GetDecimal(reader.GetOrdinal("AccruedInt")),
                                    SubCategory = reader.IsDBNull(reader.GetOrdinal("SubCategory")) ? null : reader.GetString(reader.GetOrdinal("SubCategory")),
                                    BuySell = reader.IsDBNull(reader.GetOrdinal("BuySell")) ? null : reader.GetString(reader.GetOrdinal("BuySell")),
                                    InitialInvestment = reader.IsDBNull(reader.GetOrdinal("InitialInvestment")) ? null : reader.GetDecimal(reader.GetOrdinal("InitialInvestment")),
                                    RelatedTradeID = reader.IsDBNull(reader.GetOrdinal("RelatedTradeID")) ? null : reader.GetString(reader.GetOrdinal("RelatedTradeID")),
                                    RelatedTransactionID = reader.IsDBNull(reader.GetOrdinal("RelatedTransactionID")) ? null : reader.GetString(reader.GetOrdinal("RelatedTransactionID")),
                                    Rtn = reader.IsDBNull(reader.GetOrdinal("Rtn")) ? null : reader.GetString(reader.GetOrdinal("Rtn")),
                                    PositionActionID = reader.IsDBNull(reader.GetOrdinal("PositionActionID")) ? null : reader.GetString(reader.GetOrdinal("PositionActionID")),
                                    SerialNumber = reader.IsDBNull(reader.GetOrdinal("SerialNumber")) ? null : reader.GetString(reader.GetOrdinal("SerialNumber")),
                                    DeliveryType = reader.IsDBNull(reader.GetOrdinal("DeliveryType")) ? null : reader.GetString(reader.GetOrdinal("DeliveryType")),
                                    CommodityType = reader.IsDBNull(reader.GetOrdinal("CommodityType")) ? null : reader.GetString(reader.GetOrdinal("CommodityType")),
                                    Fineness = reader.IsDBNull(reader.GetOrdinal("Fineness")) ? null : reader.GetDecimal(reader.GetOrdinal("Fineness")),
                                    Weight = reader.IsDBNull(reader.GetOrdinal("Weight")) ? null : reader.GetDecimal(reader.GetOrdinal("Weight"))
                                };

                                position.TradeExecutions.Add(tradeExecution);
                            }
                        }
                    }
                }

                return positions;
            });
        }
    }
}