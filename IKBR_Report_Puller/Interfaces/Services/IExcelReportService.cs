using System.Xml.Linq;
using TraderView.Domain.Entities;

namespace TraderView.Application.Interfaces.Services
{
    public interface IExcelReportService
    {
        /// <summary>
        /// Creates an Excel report based on the provided IKBRReport data and saves it to the specified file path.
        /// </summary>
        /// <param name="openPositions">The list of open positions to include in the report.</param>
        /// <param name="tradeExecutions">The list of trade executions to include in the report.</param>
        /// <param name="outputFilePath">The file path where the Excel report will be saved.</param>    
        Task CreateExcelFileReport(IReadOnlyList<Position> openPositions, IReadOnlyList<TradeExecution> tradeExecutions, string outputFilePath);

    }
}
