using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
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

                int newTradesCount = 0;
                using (var transaction = connection.BeginTransaction())
                {
                    foreach (var trade in trades)
                    {
                        string ibExecID = trade.Attribute("ibExecID")?.Value;
                        if (string.IsNullOrEmpty(ibExecID) || existingTrades.Contains(ibExecID))
                        {
                            continue;
                        }

                        ExecuteInsertCommand(connection, transaction, "INSERT INTO [dbo].[TradeExecutions] ([ibExecID], [symbol], [tradeDate], [quantity], [tradePrice]) VALUES (@ibExecID, @symbol, @tradeDate, @quantity, @tradePrice)",
                            new Dictionary<string, object>
                            {
                                { "@ibExecID", ibExecID },
                                { "@symbol", trade.Attribute("symbol")?.Value },
                                { "@tradeDate", trade.Attribute("tradeDate")?.Value },
                                { "@quantity", trade.Attribute("quantity")?.Value },
                                { "@tradePrice", trade.Attribute("tradePrice")?.Value }
                            });

                        newTradesCount++;
                    }
                    transaction.Commit();
                }

                Console.WriteLine($"Successfully inserted {newTradesCount} new trades into the database.");
            });
        }

        public void InsertTodayExecutions(XDocument reportXml)
        {
            throw new NotImplementedException();
        }

        public void UpsertTimeSeriesData(string instrumentName, string provider, string dataName, string dataSource, string format, string frequency, string currency, DateTime date, double openPrice, double closePrice, double lowPrice, double highPrice, double volume)
        {
            throw new NotImplementedException();
        }

        public string ConnectionString => _connectionString;

        public List<string> GetOpenPositionInstrumentNames()
        {
            throw new NotImplementedException();
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
