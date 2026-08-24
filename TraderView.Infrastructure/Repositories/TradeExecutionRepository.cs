using Microsoft.Data.SqlClient;
using System.Data;
using TraderView.Application.Features.Instruments.Query.GetBy;
using TraderView.Application.Features.Positions.Command.Create;
using TraderView.Application.Features.Positions.Command.Update;
using TraderView.Application.Features.Positions.Query.GetBy;
using TraderView.Application.Features.TradeExecutions.Command.Create;
using TraderView.Application.Features.TradeExecutions.Command.Update;
using TraderView.Application.Features.TradeExecutions.Query.GetBy;
using TraderView.Application.Interfaces.Repositories;
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
                string ibExecID = trade.IbExecID;
                bool executionExists = false;
                if (string.IsNullOrEmpty(ibExecID))
                {
                    continue;
                }
                executionExists = DoesExecutionExist(ibExecID);

                if (!executionExists)
                {
                    try
                    {
                        trade.InstrumentId = _instrumentRepository.GetInstrumentIdByConId(trade.Conid).Value;
                        trade.PositionId = GetOpenPosition(trade.InstrumentId)?.Id ?? CreatePosition(trade.InstrumentId, trade.Symbol, trade.TradeDate, trade.TradePrice, "O");
                        trade.Id = CreateTradeExecution(trade);
                        var totalQuantity = GetTotalQuantityForPosition(trade.PositionId);
                        if (totalQuantity == 0)
                            ClosePosition(trade.PositionId, trade.DateTime);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error inserting trade with ibExecID {ibExecID}: {ex.Message}");
                    }
                }
                else
                {
                    var tradeExecInDb = GetTradeExecutionByExecID(ibExecID);
                    if(!tradeExecInDb.TransactionID.HasValue)
                    {
                        // Entry was made by TradeConfirmation so will be missing key details. Update the record with the new trade execution details.
                        UpdateTradeExecution(trade);
                    }
                }
            }
        }

        private bool DoesExecutionExist(string ibExecID)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        return RecordExists(connection, transaction,
                        "SELECT COUNT(*) FROM dbo.TradeExecutions WHERE ibExecID = @ibExecID",
                        new Dictionary<string, object> { { "@ibExecID", ibExecID } });
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
            string query = new GetByIbExecIdQuery().Script();

            var parameters = new Dictionary<string, object>
                {
                    { "@IbExecId", ibExecID }
                };

            return ExecuteDatabaseOperation(connection =>
            {
                return ExecuteSingle(
                    connection,
                    transaction: null,
                    query: query,
                    mapFunction: reader =>
                    {
                        // Local null-safe helper functions
                        string? GetStringVal(string col) => reader[col] is DBNull ? null : reader[col].ToString();
                        decimal? GetDecimalVal(string col) => reader[col] is DBNull ? null : Convert.ToDecimal(reader[col]);
                        DateTime? GetDateTimeVal(string col) => reader[col] is DBNull ? null : Convert.ToDateTime(reader[col]);
                        int? GetIntVal(string col) => reader[col] is DBNull ? null : Convert.ToInt32(reader[col]);
                        long? GetLongVal(string col) => reader[col] is DBNull ? null : Convert.ToInt64(reader[col]);
                        bool? GetBoolVal(string col) => reader[col] is DBNull ? null : Convert.ToBoolean(reader[col]);

                        return new TradeExecution
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            PositionId = GetIntVal("PositionId") ?? 0,
                            Symbol = GetStringVal("symbol"),
                            SecurityID = GetStringVal("securityID"),
                            TradeID = GetLongVal("tradeID") ?? 0,
                            DateTime = GetDateTimeVal("dateTime") ?? default,
                            TradeDate = GetDateTimeVal("tradeDate") ?? default,
                            Quantity = GetDecimalVal("quantity") ?? 0m,
                            TradePrice = GetDecimalVal("tradePrice") ?? 0m,
                            IbCommission = GetDecimalVal("ibCommission") ?? 0m,
                            IbCommissionCurrency = GetStringVal("ibCommissionCurrency"),
                            ClosePrice = GetDecimalVal("closePrice"),
                            Cost = GetDecimalVal("cost") ?? 0m,
                            FifoPnlRealized = GetDecimalVal("fifoPnlRealized") ?? 0m,
                            BuySell = GetStringVal("buySell"),
                            TransactionID = GetLongVal("transactionID"),
                            IbExecID = GetStringVal("ibExecID"),
                            BrokerageOrderID = GetStringVal("brokerageOrderID"),
                            ExchOrderId = GetStringVal("exchOrderId"),
                            ExtExecID = GetStringVal("extExecID"),
                            OrderType = GetStringVal("orderType"),
                            TraderID = GetStringVal("traderID"),
                            Currency = GetStringVal("currency"),
                            Description = GetStringVal("description"),
                            Conid = GetStringVal("conid"),
                            Taxes = GetDecimalVal("taxes"),
                            AssetCategory = GetStringVal("assetCategory"),
                            Expiry = GetStringVal("expiry"),
                            TransactionType = GetStringVal("transactionType"),
                            Exchange = GetStringVal("exchange"),
                            Proceeds = GetDecimalVal("proceeds"),
                            NetCash = GetDecimalVal("netCash"),
                            MtmPnl = GetDecimalVal("mtmPnl"),
                            OrigTradePrice = GetDecimalVal("origTradePrice"),
                            OrigTradeDate = GetStringVal("origTradeDate"),
                            OrigTradeID = GetStringVal("origTradeID"),
                            OrigOrderID = GetLongVal("origOrderID"),
                            OrigTransactionID = GetLongVal("origTransactionID"),
                            IbOrderID = GetLongVal("ibOrderID"),
                            OpenDateTime = GetStringVal("openDateTime"),
                            InitialInvestment = GetDecimalVal("initialInvestment"),
                            AccountId = GetStringVal("accountId"),
                            AcctAlias = GetStringVal("acctAlias"),
                            Model = GetStringVal("model"),
                            FxRateToBase = GetDecimalVal("fxRateToBase"),
                            SubCategory = GetStringVal("subCategory"),
                            SecurityIDType = GetStringVal("securityIDType"),
                            Cusip = GetStringVal("cusip"),
                            Isin = GetStringVal("isin"),
                            Figi = GetStringVal("figi"),
                            ListingExchange = GetStringVal("listingExchange"),
                            UnderlyingConid = GetStringVal("underlyingConid"),
                            UnderlyingSymbol = GetStringVal("underlyingSymbol"),
                            UnderlyingSecurityID = GetStringVal("underlyingSecurityID"),
                            UnderlyingListingExchange = GetStringVal("underlyingListingExchange"),
                            Issuer = GetStringVal("issuer"),
                            IssuerCountryCode = GetStringVal("issuerCountryCode"),
                            Multiplier = GetIntVal("multiplier"),
                            RelatedTradeID = GetStringVal("relatedTradeID"),
                            Strike = GetDecimalVal("strike"),
                            ReportDate = Convert.ToDateTime(GetStringVal("reportDate")),
                            PutCall = GetStringVal("putCall"),
                            PrincipalAdjustFactor = GetDecimalVal("principalAdjustFactor"),
                            SettleDateTarget = Convert.ToDateTime(GetStringVal("settleDateTarget")),
                            TradeMoney = GetDecimalVal("tradeMoney"),
                            OpenCloseIndicator = GetStringVal("openCloseIndicator"),
                            Notes = GetStringVal("notes"),
                            ClearingFirmID = GetStringVal("clearingFirmID"),
                            RelatedTransactionID = GetStringVal("relatedTransactionID"),
                            Rtn = GetStringVal("rtn"),
                            OrderReference = GetStringVal("orderReference"),
                            VolatilityOrderLink = GetStringVal("volatilityOrderLink"),
                            OrderTime = GetStringVal("orderTime"),
                            HoldingPeriodDateTime = GetStringVal("holdingPeriodDateTime"),
                            WhenRealized = GetStringVal("whenRealized"),
                            WhenReopened = GetStringVal("whenReopened"),
                            LevelOfDetail = GetStringVal("levelOfDetail"),
                            ChangeInPrice = GetDecimalVal("changeInPrice"),
                            ChangeInQuantity = GetDecimalVal("changeInQuantity"),
                            IsAPIOrder = GetStringVal("isAPIOrder"),
                            AccruedInt = GetDecimalVal("accruedInt"),
                            PositionActionID = GetStringVal("positionActionID"),
                            SerialNumber = GetStringVal("serialNumber"),
                            DeliveryType = GetStringVal("deliveryType"),
                            CommodityType = GetStringVal("commodityType"),
                            Fineness = GetDecimalVal("fineness"),
                            Weight = GetDecimalVal("weight")
                        };
                    },
                    parameters: parameters
                );
            });
        }

        /// <summary>
        /// Gets all positions from the database
        /// </summary>
        List<Position> ITradeExecutionRepository.GetAllPositions()
        {
            return GetPositions(new GetAllPositionsQuery().Script());
        }
        /// <summary>
        /// Gets all positions from the database
        /// </summary>
        List<Position> ITradeExecutionRepository.GetOpenPositions()
        {
            return GetPositions(new GetOpenPositionsQuery().Script());
        }

        private List<Position> GetPositions(string sqlCommand)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                // 1. Fetch Positions
                var positions = ExecuteList(
                    connection,
                    transaction: null,
                    query: sqlCommand,
                    mapFunction: reader => new Position
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        InstrumentId = reader.GetInt32(reader.GetOrdinal("InstrumentId")),
                        OpenDate = reader.GetDateTime(reader.GetOrdinal("OpenDate")),
                        LastReportedPriceUpdated = !reader.IsDBNull(reader.GetOrdinal("LastReportedPriceUpdated"))
                            ? reader.GetDateTime(reader.GetOrdinal("LastReportedPriceUpdated"))
                            : null,
                        LastReportedPrice = reader["LastReportedPrice"] is DBNull ? 0 : Convert.ToDecimal(reader["LastReportedPrice"]),
                        Status = reader.GetString(reader.GetOrdinal("Status")),
                        Instrument = new Instrument
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("InstrumentId")),
                            InstrumentName = reader["InstrumentName"]?.ToString(),
                            DataName = reader["DataName"]?.ToString(),
                            DataSource = reader.GetString(reader.GetOrdinal("DataSource")),
                            Currency = reader["Currency"]?.ToString(),
                            ConId = reader["ConId"]?.ToString(),
                            ContractUnitType = reader["ContractUnitType"]?.ToString()
                        },
                        TradeExecutions = new List<TradeExecution>()
                    }
                );

                if (!positions.Any())
                    return positions;

                // 2. Fetch TradeExecutions for all retrieved Positions
                var positionIds = string.Join(",", positions.Select(p => p.Id));
                string executionsQuery = $"SELECT * FROM TradeExecutions WHERE PositionId IN ({positionIds})";

                var tradeExecutions = ExecuteList(
                    connection,
                    transaction: null,
                    query: executionsQuery,
                    mapFunction: reader => new TradeExecution
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        PositionId = reader.GetInt32(reader.GetOrdinal("PositionId")),
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
                    }
                );

                // 3. Group and assign TradeExecutions to their parent Position
                var executionLookup = tradeExecutions.ToLookup(te => te.PositionId);
                foreach (var position in positions)
                {
                    if (executionLookup.Contains(position.Id))
                    {
                        position.TradeExecutions.AddRange(executionLookup[position.Id]);
                    }
                }

                return positions;
            });
        }

        public void UpdateTradeExecution(TradeExecution execution)
        {
            string query = new UpdateTradeExecutionCommand().Script();

            var parameters = new Dictionary<string, object>
            {
                { "@Id", execution.Id },
                { "@PositionId", execution.PositionId },
                { "@Symbol", execution.Symbol ?? (object)DBNull.Value },
                { "@SecurityID", execution.SecurityID ?? (object)DBNull.Value },
                { "@TradeID", execution.TradeID },
                { "@DateTime", execution.DateTime },
                { "@TradeDate", execution.TradeDate },
                { "@Quantity", execution.Quantity },
                { "@TradePrice", execution.TradePrice },
                { "@IbCommission", execution.IbCommission },
                { "@IbCommissionCurrency", execution.IbCommissionCurrency ?? (object)DBNull.Value },
                { "@ClosePrice", execution.ClosePrice ?? (object)DBNull.Value },
                { "@Cost", execution.Cost },
                { "@FifoPnlRealized", execution.FifoPnlRealized },
                { "@BuySell", execution.BuySell ?? (object)DBNull.Value },
                { "@TransactionID", execution.TransactionID ?? (object)DBNull.Value },
                { "@IbExecID", execution.IbExecID ?? (object)DBNull.Value },
                { "@BrokerageOrderID", execution.BrokerageOrderID ?? (object)DBNull.Value },
                { "@ExchOrderId", execution.ExchOrderId ?? (object)DBNull.Value },
                { "@ExtExecID", execution.ExtExecID ?? (object)DBNull.Value },
                { "@OrderType", execution.OrderType ?? (object)DBNull.Value },
                { "@TraderID", execution.TraderID ?? (object)DBNull.Value },
                { "@Currency", execution.Currency ?? (object)DBNull.Value },
                { "@Description", execution.Description ?? (object)DBNull.Value },
                { "@Conid", execution.Conid ?? (object)DBNull.Value },
                { "@Taxes", execution.Taxes ?? (object)DBNull.Value },
                { "@AssetCategory", execution.AssetCategory ?? (object)DBNull.Value },
                { "@Expiry", execution.Expiry ?? (object)DBNull.Value },
                { "@TransactionType", execution.TransactionType ?? (object)DBNull.Value },
                { "@Exchange", execution.Exchange ?? (object)DBNull.Value },
                { "@Proceeds", execution.Proceeds ?? (object)DBNull.Value },
                { "@NetCash", execution.NetCash ?? (object)DBNull.Value },
                { "@MtmPnl", execution.MtmPnl ?? (object)DBNull.Value },
                { "@OrigTradePrice", execution.OrigTradePrice ?? (object)DBNull.Value },
                { "@OrigTradeDate", execution.OrigTradeDate ?? (object)DBNull.Value },
                { "@OrigTradeID", execution.OrigTradeID ?? (object)DBNull.Value },
                { "@OrigOrderID", execution.OrigOrderID ?? (object)DBNull.Value },
                { "@OrigTransactionID", execution.OrigTransactionID ?? (object)DBNull.Value },
                { "@IbOrderID", execution.IbOrderID ?? (object)DBNull.Value },
                { "@OpenDateTime", execution.OpenDateTime ?? (object)DBNull.Value },
                { "@InitialInvestment", execution.InitialInvestment ?? (object)DBNull.Value },
                { "@AccountId", execution.AccountId ?? (object)DBNull.Value },
                { "@AcctAlias", execution.AcctAlias ?? (object)DBNull.Value },
                { "@Model", execution.Model ?? (object)DBNull.Value },
                { "@FxRateToBase", execution.FxRateToBase ?? (object)DBNull.Value },
                { "@SubCategory", execution.SubCategory ?? (object)DBNull.Value },
                { "@SecurityIDType", execution.SecurityIDType ?? (object)DBNull.Value },
                { "@Cusip", execution.Cusip ?? (object)DBNull.Value },
                { "@Isin", execution.Isin ?? (object)DBNull.Value },
                { "@Figi", execution.Figi ?? (object)DBNull.Value },
                { "@ListingExchange", execution.ListingExchange ?? (object)DBNull.Value },
                { "@UnderlyingConid", execution.UnderlyingConid ?? (object)DBNull.Value },
                { "@UnderlyingSymbol", execution.UnderlyingSymbol ?? (object)DBNull.Value },
                { "@UnderlyingSecurityID", execution.UnderlyingSecurityID ?? (object)DBNull.Value },
                { "@UnderlyingListingExchange", execution.UnderlyingListingExchange ?? (object)DBNull.Value },
                { "@Issuer", execution.Issuer ?? (object)DBNull.Value },
                { "@IssuerCountryCode", execution.IssuerCountryCode ?? (object)DBNull.Value },
                { "@Multiplier", execution.Multiplier ?? (object)DBNull.Value },
                { "@RelatedTradeID", execution.RelatedTradeID ?? (object)DBNull.Value },
                { "@Strike", execution.Strike ?? (object)DBNull.Value },
                { "@ReportDate", execution.ReportDate },
                { "@PutCall", execution.PutCall ?? (object)DBNull.Value },
                { "@PrincipalAdjustFactor", execution.PrincipalAdjustFactor ?? (object)DBNull.Value },
                { "@SettleDateTarget", execution.SettleDateTarget },
                { "@TradeMoney", execution.TradeMoney ?? (object)DBNull.Value },
                { "@OpenCloseIndicator", execution.OpenCloseIndicator ?? (object)DBNull.Value },
                { "@Notes", execution.Notes ?? (object)DBNull.Value },
                { "@ClearingFirmID", execution.ClearingFirmID ?? (object)DBNull.Value },
                { "@RelatedTransactionID", execution.RelatedTransactionID ?? (object)DBNull.Value },
                { "@Rtn", execution.Rtn ?? (object)DBNull.Value },
                { "@OrderReference", execution.OrderReference ?? (object)DBNull.Value },
                { "@VolatilityOrderLink", execution.VolatilityOrderLink ?? (object)DBNull.Value },
                { "@OrderTime", execution.OrderTime ?? (object)DBNull.Value },
                { "@HoldingPeriodDateTime", execution.HoldingPeriodDateTime ?? (object)DBNull.Value },
                { "@WhenRealized", execution.WhenRealized ?? (object)DBNull.Value },
                { "@WhenReopened", execution.WhenReopened ?? (object)DBNull.Value },
                { "@LevelOfDetail", execution.LevelOfDetail ?? (object)DBNull.Value },
                { "@ChangeInPrice", execution.ChangeInPrice ?? (object)DBNull.Value },
                { "@ChangeInQuantity", execution.ChangeInQuantity ?? (object)DBNull.Value },
                { "@IsAPIOrder", execution.IsAPIOrder ?? (object)DBNull.Value },
                { "@AccruedInt", execution.AccruedInt ?? (object)DBNull.Value },
                { "@PositionActionID", execution.PositionActionID ?? (object)DBNull.Value },
                { "@SerialNumber", execution.SerialNumber ?? (object)DBNull.Value },
                { "@DeliveryType", execution.DeliveryType ?? (object)DBNull.Value },
                { "@CommodityType", execution.CommodityType ?? (object)DBNull.Value },
                { "@Fineness", execution.Fineness ?? (object)DBNull.Value },
                { "@Weight", execution.Weight ?? (object)DBNull.Value }
            };

            ExecuteDatabaseOperation(connection =>
            {
                ExecuteCommand(
                    connection,
                    transaction: null,
                    query: query,
                    parameters: parameters
                );
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
                        string closeQuery = new ClosePositionCommand().Script();

                        var parameters = new Dictionary<string, object>
                        {
                            { "@positionId", positionId },
                            { "@closeDate", closeDate }
                        };
                        ExecuteCommand(connection, transaction, closeQuery, parameters);
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
                        string query = new GetInstrumentByIdQuery().Script();

                        var parameters = new Dictionary<string, object>
                        {
                            { "@instrumentId", instrumentId }
                        };

                        return ExecuteSingle(connection, transaction, query, MapPosition, parameters);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error retrieving open position for InstrumentId {instrumentId}: {ex.Message}");
                        return null;
                    }
                }
            }); 
        }

        private static Position MapPosition(SqlDataReader reader)
        {
            return new Position
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                InstrumentId = reader.GetInt32(reader.GetOrdinal("InstrumentId")),
                OpenDate = reader.GetDateTime(reader.GetOrdinal("OpenDate")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                LastReportedPrice = reader.IsDBNull(reader.GetOrdinal("LastReportedPrice"))
                    ? 0m
                    : reader.GetDecimal(reader.GetOrdinal("LastReportedPrice")),
                LastReportedPriceUpdated = reader.IsDBNull(reader.GetOrdinal("LastReportedPriceUpdated"))
                    ? null
                    : reader.GetDateTime(reader.GetOrdinal("LastReportedPriceUpdated"))
            };
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
                        var quantity = 0M;
                        const string query = @"
                            SELECT ISNULL(SUM(quantity), 0) as TotalQuantity
                            FROM [dbo].[TradeExecutions]
                            WHERE PositionID = @positionId";

                        var parameters = new Dictionary<string, object>
                        {
                            { "@positionId", positionId }
                        };

                        quantity = ExecuteScalar<decimal>(connection, transaction, query, parameters);
                        transaction.Commit();
                        return quantity;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
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
                bool tradeExecutionExists = DoesExecutionExist(tradeConfirm.IbExecID);

                if (!tradeExecutionExists)
                {
                    var instrumentId = _instrumentRepository.GetInstrumentIdByConId(tradeConfirm.Conid);
                    
                    if (instrumentId.HasValue)
                    {
                        Position? existingPosition = null;
 
                        // Check for open position for the trade's symbol and instrument (within the same transaction)
                        existingPosition = GetOpenPosition(instrumentId.Value);

                        if (existingPosition != null)
                        {
                            tradeConfirm.OpenCloseIndicator = (existingPosition.Quantity + tradeConfirm.Quantity) == 0 ? "O" : "C";                            
                            tradeConfirm.PositionId = existingPosition.Id;
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
            return ExecuteDatabaseOperation(connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string insertQuery = new CreatePositionCommand().Script();

                        var parameters = new Dictionary<string, object>
                        {
                            { "@openDate", openDate },
                            { "@status", "Open" },
                            { "@instrumentId", instrumentId },
                            { "@lastReportedPrice", openPrice },
                            { "@LastReportedPriceUpdated", DateTime.Now},
                            { "@openCloseIndicator", openCloseIndicator }
                        };

                        int newPositionId = ExecuteScalar<int>(connection, transaction, insertQuery, parameters);
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
        private int CreateTradeExecution(TradeExecution trade)
        {
            // MODEL EXAMPLE
            return ExecuteDatabaseOperation(connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    var parameters = TradeParameterBuilder.GetTradeExecutionParams(trade);
                    var insertQuery = new InsertTradeExecutionCommand().Script();
                    int tradeId = ExecuteScalar<int>(connection, transaction, insertQuery, parameters);
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
                        var parameters = TradeParameterBuilder.GetTradeConfirmationParams(tradeConfirm);
                        var insertQuery = new InsertTradeConfirmationCommand().Script();
                        int tradeId = ExecuteScalar<int>(connection, transaction, insertQuery, parameters);
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
        List<TradeExecution> ITradeExecutionRepository.GetByPositionId(int positionId)
        {
            string query = new GetByPositionIdQuery().Script();
            var parameters = new Dictionary<string, object>
            {
                { "@PositionId", positionId }
            };

            return ExecuteDatabaseOperation(connection =>
            {
                return ExecuteList(
                    connection,
                    transaction: null,
                    query: query,
                    mapFunction: reader => new TradeExecution
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        InstrumentId = reader.GetInt32(reader.GetOrdinal("InstrumentId")),
                        PositionId = positionId,
                        Symbol = reader.GetString(reader.GetOrdinal("symbol")),
                        TradeID = reader.GetInt64(reader.GetOrdinal("tradeID")),
                        DateTime = reader.GetDateTime(reader.GetOrdinal("dateTime")),
                        TradeDate = reader.GetDateTime(reader.GetOrdinal("tradeDate")),
                        Quantity = reader.GetDecimal(reader.GetOrdinal("quantity")),
                        TradePrice = reader.GetDecimal(reader.GetOrdinal("tradePrice")),
                        BuySell = reader.GetString(reader.GetOrdinal("buySell")),
                        FifoPnlRealized = reader.GetDecimal(reader.GetOrdinal("fifoPnlRealized")),
                        IbCommission = reader.GetDecimal(reader.GetOrdinal("ibCommission")),
                        OpenCloseIndicator = reader.GetString(reader.GetOrdinal("openCloseIndicator"))
                    },
                    parameters: parameters
                );
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