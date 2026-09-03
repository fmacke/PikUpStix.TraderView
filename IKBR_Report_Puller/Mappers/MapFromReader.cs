using Microsoft.Data.SqlClient;
using TraderView.Domain.Entities;
using TraderView.Domain.Entities.FMP;

namespace TraderView.Application.Mappers
{
    public static class MapFromReader
    {
        public static TradeExecution MapTradeExecution(SqlDataReader reader)
        {
            string? GetStringVal(string col) => reader[col] is DBNull ? null : reader[col].ToString();
            decimal? GetDecimalVal(string col) => reader[col] is DBNull ? null : Convert.ToDecimal(reader[col]);
            DateTime? GetDateTimeVal(string col) => reader[col] is DBNull ? null : Convert.ToDateTime(reader[col]);
            int? GetIntVal(string col) => reader[col] is DBNull ? null : Convert.ToInt32(reader[col]);
            long? GetLongVal(string col) => reader[col] is DBNull ? null : Convert.ToInt64(reader[col]);
            bool? GetBoolVal(string col) => reader[col] is DBNull ? null : Convert.ToBoolean(reader[col]);
            return new TradeExecution
            { 
                
            //    Id = reader.GetInt32(reader.GetOrdinal("Id")),
            //    PositionId = reader.GetInt32(reader.GetOrdinal("PositionId")),
            //    Symbol = reader.GetString(reader.GetOrdinal("symbol")),
            //    TradeDate = reader.GetDateTime(reader.GetOrdinal("tradeDate")),
            //    Quantity = reader.GetDecimal(reader.GetOrdinal("quantity")),
            //    TradePrice = reader.GetDecimal(reader.GetOrdinal("tradePrice")),
            //    BuySell = reader.GetString(reader.GetOrdinal("buySell")),
            //    FifoPnlRealized = reader.GetDecimal(reader.GetOrdinal("fifoPnlRealized")),
            //    IbCommission = reader.GetDecimal(reader.GetOrdinal("ibCommission")),
            //    OpenCloseIndicator = reader.GetString(reader.GetOrdinal("openCloseIndicator"))

                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                PositionId = GetIntVal("PositionId") ?? 0,
                Symbol = GetStringVal("symbol"),
                SecurityId = GetStringVal("securityID"),
                TradeId = GetLongVal("tradeID") ?? 0,
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
                TransactionId = GetLongVal("transactionID"),
                IbExecId = GetStringVal("ibExecID"),
                BrokerageOrderId = GetStringVal("brokerageOrderID"),
                ExchOrderId = GetStringVal("exchOrderId"),
                ExtExecId = GetStringVal("extExecID"),
                OrderType = GetStringVal("orderType"),
                TraderId = GetStringVal("traderID"),
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
                OrigTradeId = GetStringVal("origTradeID"),
                OrigOrderId = GetLongVal("origOrderID"),
                OrigTransactionId = GetLongVal("origTransactionID"),
                IbOrderId = GetLongVal("ibOrderID"),
                OpenDateTime = GetStringVal("openDateTime"),
                InitialInvestment = GetDecimalVal("initialInvestment"),
                AccountId = GetStringVal("accountId"),
                AcctAlias = GetStringVal("acctAlias"),
                Model = GetStringVal("model"),
                FxRateToBase = GetDecimalVal("fxRateToBase"),
                SubCategory = GetStringVal("subCategory"),
                SecurityIdtype = GetStringVal("securityIDType"),
                Cusip = GetStringVal("cusip"),
                Isin = GetStringVal("isin"),
                Figi = GetStringVal("figi"),
                ListingExchange = GetStringVal("listingExchange"),
                UnderlyingConid = GetStringVal("underlyingConid"),
                UnderlyingSymbol = GetStringVal("underlyingSymbol"),
                UnderlyingSecurityId = GetStringVal("underlyingSecurityID"),
                UnderlyingListingExchange = GetStringVal("underlyingListingExchange"),
                Issuer = GetStringVal("issuer"),
                IssuerCountryCode = GetStringVal("issuerCountryCode"),
                Multiplier = GetIntVal("multiplier"),
                RelatedTradeId = GetStringVal("relatedTradeID"),
                Strike = GetDecimalVal("strike"),
                ReportDate = Convert.ToDateTime(GetStringVal("reportDate")).ToLongDateString(),
                PutCall = GetStringVal("putCall"),
                PrincipalAdjustFactor = GetDecimalVal("principalAdjustFactor"),
                SettleDateTarget = Convert.ToDateTime(GetStringVal("settleDateTarget")).ToLongDateString(),
                TradeMoney = GetDecimalVal("tradeMoney"),
                OpenCloseIndicator = GetStringVal("openCloseIndicator"),
                Notes = GetStringVal("notes"),
                ClearingFirmId = GetStringVal("clearingFirmID"),
                RelatedTransactionId = GetStringVal("relatedTransactionID"),
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
                IsApiorder = GetStringVal("isAPIOrder"),
                AccruedInt = GetDecimalVal("accruedInt"),
                PositionActionId = GetStringVal("positionActionID"),
                SerialNumber = GetStringVal("serialNumber"),
                DeliveryType = GetStringVal("deliveryType"),
                CommodityType = GetStringVal("commodityType"),
                Fineness = GetDecimalVal("fineness"),
                Weight = GetDecimalVal("weight"),
                Position = new Position
                {
                    Id = reader.GetInt32(reader.GetOrdinal("PositionId")),
                    InstrumentId = reader.GetInt32(reader.GetOrdinal("InstrumentId")),
                    Status = GetStringVal("Status")
                }
            };
        }
        public static Position MapPositionWithInstrumentData(SqlDataReader reader)
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
                    : reader.GetDateTime(reader.GetOrdinal("LastReportedPriceUpdated")),
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
            };
        }
        public static Position MapPosition(SqlDataReader reader)
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
        public static Instrument MapInstrument(SqlDataReader reader)
        {
            return new Instrument
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                InstrumentName = reader["InstrumentName"]?.ToString(),
                DataName = reader["DataName"]?.ToString(),
                DataSource = reader.GetString(reader.GetOrdinal("DataSource")),
                Currency = reader["Currency"]?.ToString(),
                ConId = reader["ConId"]?.ToString(),
                ContractUnitType = reader["ContractUnitType"]?.ToString(),
                Provider = reader["Provider"]?.ToString(),
                ListingExchange = reader["ListingExchange"]?.ToString(),
                Format = reader["Format"]?.ToString(),
                Frequency = reader["Frequency"]?.ToString(),
                ContractUnit = reader.IsDBNull(reader.GetOrdinal("ContractUnit"))
                    ? null
                    : reader.GetDouble(reader.GetOrdinal("ContractUnit")),
                PriceQuotation = reader["PriceQuotation"]?.ToString(),
                MinimumPriceFluctuation = reader.IsDBNull(reader.GetOrdinal("MinimumPriceFluctuation"))
                    ? null
                    : reader.GetDouble(reader.GetOrdinal("MinimumPriceFluctuation"))
            };
        }
        public static CanSlimScreenerSnapshot MapScreenerSnapshot(SqlDataReader reader)
        {
            return new CanSlimScreenerSnapshot
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }
        public static CanSlimCandidate MapCanSlimCandiate(SqlDataReader reader)
        {
            return new CanSlimCandidate
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Symbol = reader.GetString(reader.GetOrdinal("Symbol")),
                CompanyName = reader.GetString(reader.GetOrdinal("CompanyName")),
                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                Volume = reader.GetDecimal(reader.GetOrdinal("Volume")),
                MarketCap = reader.GetDecimal(reader.GetOrdinal("MarketCap")),
                Exchange = reader.GetString(reader.GetOrdinal("Exchange")),
                Sector = reader.GetString(reader.GetOrdinal("Sector")),
                Industry = reader.GetString(reader.GetOrdinal("Industry")),
                CurrentQuarterLatestQuarterDate = reader.GetString(reader.GetOrdinal("CurrentQuarter_LatestQuarterDate")),
                CurrentQuarterLatestQuarterEps = reader.GetDecimal(reader.GetOrdinal("CurrentQuarter_LatestQuarterEps")),
                CurrentQuarterPriorYearQuarterEps = reader.GetDecimal(reader.GetOrdinal("CurrentQuarter_PriorYearQuarterEps")),
                CurrentQuarterEpsGrowthYoYpercent = reader.GetDecimal(reader.GetOrdinal("CurrentQuarter_EpsGrowthYoYPercent")),
                CurrentQuarterRevenueGrowthYoYpercent = reader.GetDecimal(reader.GetOrdinal("CurrentQuarter_RevenueGrowthYoYPercent")),
                CurrentQuarterIsAccelerating = reader.GetBoolean(reader.GetOrdinal("CurrentQuarter_IsAccelerating")),
                CurrentQuarterPassesCriteria = reader.GetBoolean(reader.GetOrdinal("CurrentQuarter_PassesCriteria")),
                EvaluationDateUtc = reader.GetDateTime(reader.GetOrdinal("EvaluationDateUtc")),
                AnnualEpsCagr3YearPercent = reader.GetDecimal(reader.GetOrdinal("Annual_EpsCagr3YearPercent")),
                AnnualEpsCagr5YearPercent = reader.IsDBNull(reader.GetOrdinal("Annual_EpsCagr5YearPercent")) ? null : reader.GetDecimal(reader.GetOrdinal("Annual_EpsCagr5YearPercent")),
                AnnualReturnOnEquityPercent = reader.GetDecimal(reader.GetOrdinal("Annual_ReturnOnEquityPercent")),
                AnnualHasConsecutiveAnnualGrowth = reader.GetBoolean(reader.GetOrdinal("Annual_HasConsecutiveAnnualGrowth")),
                AnnualLatestFiscalYearEps = reader.GetDecimal(reader.GetOrdinal("Annual_LatestFiscalYearEps")),
                AnnualLatestFiscalYear = reader.GetString(reader.GetOrdinal("Annual_LatestFiscalYear")),
                AnnualPriorYear1Eps = reader.GetDecimal(reader.GetOrdinal("Annual_PriorYear1Eps")),
                AnnualPriorYear2Eps = reader.GetDecimal(reader.GetOrdinal("Annual_PriorYear2Eps")),
                AnnualPriorYear3Eps = reader.GetDecimal(reader.GetOrdinal("Annual_PriorYear3Eps")),
                AnnualOperatingMarginPercent = reader.GetDecimal(reader.GetOrdinal("Annual_OperatingMarginPercent")),
                AnnualReturnOnAssetsPercent = reader.GetDecimal(reader.GetOrdinal("Annual_ReturnOnAssetsPercent")),
                AnnualPassesCriteria = reader.GetBoolean(reader.GetOrdinal("Annual_PassesCriteria")),
                AnnualFundamentalGrade = reader.GetString(reader.GetOrdinal("Annual_FundamentalGrade"))
                // Annual History is not included in this query; it would require a separate query to fetch the annual history for each candidate.
            };
        }
    }
}
