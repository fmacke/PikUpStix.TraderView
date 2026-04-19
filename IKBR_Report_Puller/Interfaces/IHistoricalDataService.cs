using IKBR_Report_Puller.Domain;

namespace IKBR_Report_Puller.Interfaces
{
    public interface IHistoricalDataService
    {
        /// <summary>
        /// Updates historical data for all positions/trades by identifying and filling missing date ranges
        /// </summary>
        void UpdateHistoricalDataForPositions(List<HistoricalTrade> trades);
    }
}
