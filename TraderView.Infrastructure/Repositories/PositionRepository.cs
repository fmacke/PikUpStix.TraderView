using Microsoft.Data.SqlClient;
using TraderView.Application.Interfaces.Repositories;
using TraderView.Domain.Entities;

namespace TraderView.Infrastructure.Repositories
{
    public class PositionRepository : BaseRepository, IPositionRepository
    {
        private readonly IInstrumentRepository _instrumentRepository;

        public PositionRepository(string connectionString, IInstrumentRepository instrumentRepository) : base(connectionString)
        {
            _instrumentRepository = instrumentRepository;
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
        /// Creates a new position and returns its ID
        /// </summary>
        int IPositionRepository.CreatePosition(int instrumentId, string symbol, DateTime openDate, decimal openPrice)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    int positionId = ((IPositionRepository)this).CreatePosition(connection, transaction, instrumentId, symbol, openDate, openPrice);
                    transaction.Commit();
                    return positionId;
                }
            });
        }

        /// <summary>
        /// Creates a new position and returns its ID within a transaction
        /// </summary>
        int IPositionRepository.CreatePosition(SqlConnection connection, SqlTransaction transaction, int instrumentId, string symbol, DateTime openDate, decimal openPrice)
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
                    @"SELECT p.Id, p.OpenDate, p.CloseDate, p.Status, p.InstrumentId, p.LastReportedPrice, p.LastReportedPriceUpdated,
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
                                LastReportedPrice = reader.IsDBNull(reader.GetOrdinal("LastReportedPrice")) ? 0 : reader.GetDecimal(reader.GetOrdinal("LastReportedPrice")),
                                LastReportedPriceUpdated = reader.IsDBNull(reader.GetOrdinal("LastReportedPriceUpdated")) ? null : reader.GetDateTime(reader.GetOrdinal("LastReportedPriceUpdated")),
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
                                    Position = position,
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
                                    SecurityIdtype= reader.IsDBNull(reader.GetOrdinal("SecurityIDType")) ? null : reader.GetString(reader.GetOrdinal("SecurityIDType")),
                                    Cusip = reader.IsDBNull(reader.GetOrdinal("Cusip")) ? null : reader.GetString(reader.GetOrdinal("Cusip")),
                                    Isin = reader.IsDBNull(reader.GetOrdinal("Isin")) ? null : reader.GetString(reader.GetOrdinal("Isin")),
                                    Figi = reader.IsDBNull(reader.GetOrdinal("Figi")) ? null : reader.GetString(reader.GetOrdinal("Figi")),
                                    ListingExchange = reader.IsDBNull(reader.GetOrdinal("ListingExchange")) ? null : reader.GetString(reader.GetOrdinal("ListingExchange")),
                                    UnderlyingConid = reader.IsDBNull(reader.GetOrdinal("UnderlyingConid")) ? null : reader.GetString(reader.GetOrdinal("UnderlyingConid")),
                                    UnderlyingSymbol = reader.IsDBNull(reader.GetOrdinal("UnderlyingSymbol")) ? null : reader.GetString(reader.GetOrdinal("UnderlyingSymbol")),
                                    UnderlyingSecurityId = reader.IsDBNull(reader.GetOrdinal("UnderlyingSecurityID")) ? null : reader.GetString(reader.GetOrdinal("UnderlyingSecurityID")),
                                    UnderlyingListingExchange = reader.IsDBNull(reader.GetOrdinal("UnderlyingListingExchange")) ? null : reader.GetString(reader.GetOrdinal("UnderlyingListingExchange")),
                                    Issuer = reader.IsDBNull(reader.GetOrdinal("Issuer")) ? null : reader.GetString(reader.GetOrdinal("Issuer")),
                                    IssuerCountryCode = reader.IsDBNull(reader.GetOrdinal("IssuerCountryCode")) ? null : reader.GetString(reader.GetOrdinal("IssuerCountryCode")),
                                    Multiplier = reader.IsDBNull(reader.GetOrdinal("Multiplier")) ? null : reader.GetInt32(reader.GetOrdinal("Multiplier")),
                                    Strike = reader.IsDBNull(reader.GetOrdinal("Strike")) ? null : reader.GetDecimal(reader.GetOrdinal("Strike")),
                                    Expiry = reader.IsDBNull(reader.GetOrdinal("Expiry")) ? null : reader.GetString(reader.GetOrdinal("Expiry")),
                                    PutCall = reader.IsDBNull(reader.GetOrdinal("PutCall")) ? null : reader.GetString(reader.GetOrdinal("PutCall")),
                                    PrincipalAdjustFactor = reader.IsDBNull(reader.GetOrdinal("PrincipalAdjustFactor")) ? null : reader.GetDecimal(reader.GetOrdinal("PrincipalAdjustFactor")),
                                    ReportDate = reader.GetDateTime(reader.GetOrdinal("ReportDate")).ToLongDateString(),
                                    TradeId = reader.IsDBNull(reader.GetOrdinal("TradeID")) ? null : reader.GetInt64(reader.GetOrdinal("TradeID")),
                                    TradeDate = reader.GetDateTime(reader.GetOrdinal("TradeDate")),
                                    DateTime = reader.GetDateTime(reader.GetOrdinal("DateTime")),
                                    SettleDateTarget = reader.GetDateTime(reader.GetOrdinal("SettleDateTarget")).ToLongDateString(),
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
                                    OrigTradeId = reader.IsDBNull(reader.GetOrdinal("OrigTradeID")) ? null : reader.GetString(reader.GetOrdinal("OrigTradeID")),
                                    OrigOrderId = reader.IsDBNull(reader.GetOrdinal("OrigOrderID")) ? null : reader.GetInt64(reader.GetOrdinal("OrigOrderID")),
                                    OrigTransactionId = reader.IsDBNull(reader.GetOrdinal("OrigTransactionID")) ? null : reader.GetInt64(reader.GetOrdinal("OrigTransactionID")),
                                    ClearingFirmId = reader.IsDBNull(reader.GetOrdinal("ClearingFirmID")) ? null : reader.GetString(reader.GetOrdinal("ClearingFirmID")),
                                    TransactionId = reader.IsDBNull(reader.GetOrdinal("TransactionID")) ? null : reader.GetInt64(reader.GetOrdinal("TransactionID")),
                                    IbOrderId = reader.IsDBNull(reader.GetOrdinal("IbOrderID")) ? null : reader.GetInt64(reader.GetOrdinal("IbOrderID")),
                                    IbExecId = reader.IsDBNull(reader.GetOrdinal("IbExecID")) ? null : reader.GetString(reader.GetOrdinal("IbExecID")),
                                    BrokerageOrderId = reader.IsDBNull(reader.GetOrdinal("BrokerageOrderID")) ? null : reader.GetString(reader.GetOrdinal("BrokerageOrderID")),
                                    OrderReference = reader.IsDBNull(reader.GetOrdinal("OrderReference")) ? null : reader.GetString(reader.GetOrdinal("OrderReference")),
                                    VolatilityOrderLink = reader.IsDBNull(reader.GetOrdinal("VolatilityOrderLink")) ? null : reader.GetString(reader.GetOrdinal("VolatilityOrderLink")),
                                    ExchOrderId = reader.IsDBNull(reader.GetOrdinal("ExchOrderId")) ? null : reader.GetString(reader.GetOrdinal("ExchOrderId")),
                                    ExtExecId = reader.IsDBNull(reader.GetOrdinal("ExtExecID")) ? null : reader.GetString(reader.GetOrdinal("ExtExecID")),
                                    OrderTime = reader.IsDBNull(reader.GetOrdinal("OrderTime")) ? null : reader.GetString(reader.GetOrdinal("OrderTime")),
                                    OpenDateTime = reader.IsDBNull(reader.GetOrdinal("OpenDateTime")) ? null : reader.GetString(reader.GetOrdinal("OpenDateTime")),
                                    HoldingPeriodDateTime = reader.IsDBNull(reader.GetOrdinal("HoldingPeriodDateTime")) ? null : reader.GetString(reader.GetOrdinal("HoldingPeriodDateTime")),
                                    WhenRealized = reader.IsDBNull(reader.GetOrdinal("WhenRealized")) ? null : reader.GetString(reader.GetOrdinal("WhenRealized")),
                                    WhenReopened = reader.IsDBNull(reader.GetOrdinal("WhenReopened")) ? null : reader.GetString(reader.GetOrdinal("WhenReopened")),
                                    LevelOfDetail = reader.IsDBNull(reader.GetOrdinal("LevelOfDetail")) ? null : reader.GetString(reader.GetOrdinal("LevelOfDetail")),
                                    ChangeInPrice = reader.IsDBNull(reader.GetOrdinal("ChangeInPrice")) ? null : reader.GetDecimal(reader.GetOrdinal("ChangeInPrice")),
                                    ChangeInQuantity = reader.IsDBNull(reader.GetOrdinal("ChangeInQuantity")) ? null : reader.GetDecimal(reader.GetOrdinal("ChangeInQuantity")),
                                    OrderType = reader.IsDBNull(reader.GetOrdinal("OrderType")) ? null : reader.GetString(reader.GetOrdinal("OrderType")),
                                    TraderId = reader.IsDBNull(reader.GetOrdinal("TraderID")) ? null : reader.GetString(reader.GetOrdinal("TraderID")),
                                    IsApiorder = reader.IsDBNull(reader.GetOrdinal("IsAPIOrder")) ? null : reader.GetString(reader.GetOrdinal("IsAPIOrder")),
                                    AccruedInt = reader.IsDBNull(reader.GetOrdinal("AccruedInt")) ? null : reader.GetDecimal(reader.GetOrdinal("AccruedInt")),
                                    SubCategory = reader.IsDBNull(reader.GetOrdinal("SubCategory")) ? null : reader.GetString(reader.GetOrdinal("SubCategory")),
                                    BuySell = reader.IsDBNull(reader.GetOrdinal("BuySell")) ? null : reader.GetString(reader.GetOrdinal("BuySell")),
                                    InitialInvestment = reader.IsDBNull(reader.GetOrdinal("InitialInvestment")) ? null : reader.GetDecimal(reader.GetOrdinal("InitialInvestment")),
                                    RelatedTradeId = reader.IsDBNull(reader.GetOrdinal("RelatedTradeID")) ? null : reader.GetString(reader.GetOrdinal("RelatedTradeID")),
                                    RelatedTransactionId = reader.IsDBNull(reader.GetOrdinal("RelatedTransactionID")) ? null : reader.GetString(reader.GetOrdinal("RelatedTransactionID")),
                                    Rtn = reader.IsDBNull(reader.GetOrdinal("Rtn")) ? null : reader.GetString(reader.GetOrdinal("Rtn")),
                                    PositionActionId = reader.IsDBNull(reader.GetOrdinal("PositionActionID")) ? null : reader.GetString(reader.GetOrdinal("PositionActionID")),
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