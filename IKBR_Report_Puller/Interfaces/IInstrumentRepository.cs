using IKBR_Report_Puller.Domain;
using System.Collections.Generic;

namespace IKBR_Report_Puller.Interfaces
{
    /// <summary>
    /// Repository interface for Instrument-related database operations
    /// </summary>
    public interface IInstrumentRepository
    {
        /// <summary>
        /// Gets the InstrumentId for a given ConId
        /// </summary>
        /// <param name="conid">The contract ID</param>
        /// <returns>The instrument ID, or null if not found</returns>
        int? GetInstrumentIdFromConId(string conid);

        /// <summary>
        /// Gets an instrument by its ID
        /// </summary>
        /// <param name="instrumentId">The instrument ID</param>
        /// <returns>The instrument, or null if not found</returns>
        Instrument Get(int instrumentId);

        /// <summary>
        /// Inserts a new instrument into the database
        /// </summary>
        /// <param name="conid">Contract ID</param>
        /// <param name="symbol">Trading symbol</param>
        /// <param name="listingExchange">Exchange where the instrument is listed</param>
        /// <param name="currency">Currency of the instrument</param>
        /// <returns>The newly created instrument ID, or null if insertion failed</returns>
        int? InsertInstrument(string conid, string symbol, string listingExchange, string currency);

        /// <summary>
        /// Ensures instruments exist for the given trades.
        /// Creates missing instruments automatically and updates trade.InstrumentId
        /// </summary>
        /// <param name="trades">List of trades to process</param>
        void UpsertInstruments(List<Trade> trades);

        /// <summary>
        /// Ensures instruments exist for the given trade confirmations.
        /// Creates missing instruments automatically and populates InstrumentID on each trade confirm
        /// </summary>
        /// <param name="tradeConfirms">List of trade confirmations to process</param>
        void UpsertInstruments(List<TradeConfirm> tradeConfirms);
    }
}
