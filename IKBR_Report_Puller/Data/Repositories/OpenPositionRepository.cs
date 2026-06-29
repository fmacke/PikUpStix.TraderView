using System;
using System.Collections.Generic;
using System.Linq;
using IKBR_Report_Puller.Domain;
using Microsoft.Data.SqlClient;
using PikUpStix.TraderView.Interfaces;

namespace IKBR_Report_Puller.Data.Repositories
{
    /// <summary>
    /// Repository for OpenPosition-related database operations
    /// </summary>
    public class OpenPositionRepository : BaseRepository, IOpenPositionRepository
    {
        public OpenPositionRepository(string connectionString) : base(connectionString)
        {
        }

        /// <summary>
        /// Gets all open positions from the database
        /// </summary>
        public List<OpenPosition> GetAllOpenPositions()
        {
            var openPositions = new List<OpenPosition>();

            ExecuteDatabaseOperation(connection =>
            {
                const string query = @"
                    SELECT [symbol], [description], [assetCategory], [currency], [position], 
                           [markPrice], [positionValue], [costBasisPrice], [costBasisMoney], 
                           [fifoPnlUnrealized], [percentOfNAV], [reportDate], [listingExchange], [accountId]
                    FROM [dbo].[OpenPositions]
                    WHERE [whenGenerated] = (SELECT MAX([whenGenerated]) FROM [dbo].[OpenPositions])
                    ORDER BY [reportDate] DESC, [symbol]";

                using (var command = new SqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        openPositions.Add(new OpenPosition
                        {
                            Symbol = reader["symbol"]?.ToString() ?? string.Empty,
                            Description = reader["description"]?.ToString() ?? string.Empty,
                            AssetCategory = reader["assetCategory"]?.ToString() ?? string.Empty,
                            Currency = reader["currency"]?.ToString() ?? string.Empty,
                            Position = reader["position"] as decimal?,
                            MarkPrice = reader["markPrice"] as decimal?,
                            PositionValue = reader["positionValue"] as decimal?,
                            CostBasisPrice = reader["costBasisPrice"] as decimal?,
                            CostBasisMoney = reader["costBasisMoney"] as decimal?,
                            FifoPnlUnrealized = reader["fifoPnlUnrealized"] as decimal?,
                            PercentOfNAV = reader["percentOfNAV"] as decimal?,
                            ReportDate = reader["reportDate"] as DateTime?,
                            ListingExchange = reader["listingExchange"]?.ToString() ?? string.Empty,
                            AccountId = reader["accountId"]?.ToString() ?? string.Empty
                        });
                    }
                }
            });

            return openPositions;
        }

        /// <summary>
        /// Inserts open positions from a report
        /// </summary>
        public void InsertOpenPositions(DateTime whenGenerated, List<OpenPosition> openPositions)
        {
            if (openPositions == null || !openPositions.Any())
            {
                Console.WriteLine("No open positions to insert.");
                return;
            }

            ExecuteDatabaseOperation(connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    int newPositionsCount = 0;
                    foreach (var position in openPositions)
                    {
                        InsertOpenPosition(connection, transaction, whenGenerated, position);
                        newPositionsCount++;
                    }
                    transaction.Commit();
                    Console.WriteLine($"Successfully inserted {newPositionsCount} new open positions into the database.");
                }
            });
        }

        /// <summary>
        /// Gets instrument details (securityID, listingExchange, symbol) for all open positions
        /// </summary>
        public List<(string securityID, string listingExchange, string symbol)> GetOpenPositionInstrumentNames(List<OpenPosition> openPositions)
        {
            if (openPositions == null || !openPositions.Any())
            {
                return new List<(string securityID, string listingExchange, string symbol)>();
            }

            var instrumentDetails = openPositions
                                  .Select(op => (
                                      securityID: op.SecurityID,
                                      listingExchange: op.ListingExchange,
                                      symbol: op.Symbol
                                  ))
                                  .Where(details => !string.IsNullOrEmpty(details.securityID) &&
                                                    !string.IsNullOrEmpty(details.listingExchange) &&
                                                    !string.IsNullOrEmpty(details.symbol))
                                  .Distinct()
                                  .ToList();

            return instrumentDetails;
        }

        #region Private Helper Methods

        private void InsertOpenPosition(SqlConnection connection, SqlTransaction transaction, DateTime whenGenerated, OpenPosition position)
        {
            const string insertQuery = @"
                INSERT INTO [dbo].[OpenPositions] 
                ([whenGenerated], [accountId], [acctAlias], [model], [currency], [fxRateToBase], [assetCategory], 
                 [subCategory], [symbol], [description], [conid], [securityID], [securityIDType], [cusip], [isin], 
                 [figi], [listingExchange], [underlyingConid], [underlyingSymbol], [underlyingSecurityID], 
                 [underlyingListingExchange], [issuer], [issuerCountryCode], [multiplier], [strike], [expiry], 
                 [putCall], [principalAdjustFactor], [reportDate], [position], [markPrice], [positionValue], 
                 [openPrice], [costBasisPrice], [costBasisMoney], [percentOfNAV], [fifoPnlUnrealized], [side], 
                 [levelOfDetail], [openDateTime], [holdingPeriodDateTime], [vestingDate], [code], [originatingOrderID], 
                 [originatingTransactionID], [accruedInt], [serialNumber], [deliveryType], [commodityType], [fineness], [weight]) 
                VALUES 
                (@whenGenerated, @accountId, @acctAlias, @model, @currency, @fxRateToBase, @assetCategory, 
                 @subCategory, @symbol, @description, @conid, @securityID, @securityIDType, @cusip, @isin, 
                 @figi, @listingExchange, @underlyingConid, @underlyingSymbol, @underlyingSecurityID, 
                 @underlyingListingExchange, @issuer, @issuerCountryCode, @multiplier, @strike, @expiry, 
                 @putCall, @principalAdjustFactor, @reportDate, @position, @markPrice, @positionValue, 
                 @openPrice, @costBasisPrice, @costBasisMoney, @percentOfNAV, @fifoPnlUnrealized, @side, 
                 @levelOfDetail, @openDateTime, @holdingPeriodDateTime, @vestingDate, @code, @originatingOrderID, 
                 @originatingTransactionID, @accruedInt, @serialNumber, @deliveryType, @commodityType, @fineness, @weight)";

            var parameters = OpenPositionParameterBuilder.GetOpenPositionParameters(whenGenerated, position);
            ExecuteCommand(connection, transaction, insertQuery, parameters);
        }

        #endregion
    }
}
