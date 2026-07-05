using IKBR_Report_Puller.Domain;

namespace PikUpStix.TraderView.Interfaces
{
    public interface IPositionRepository
    {
        /// <summary>
        /// Gets all positions from the database
        /// </summary>
        /// <returns>List of positions</returns>
        List<Position> GetAllPositions();
        /// <summary>
        /// Inserts a list of positions into the database
        /// </summary>
        /// <param name="positions">List of positions to insert</param>
        void UpsertPositions(List<Position> positions);
    }
}
    