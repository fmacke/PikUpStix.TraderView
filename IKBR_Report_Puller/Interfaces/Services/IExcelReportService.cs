using System.Xml.Linq;
using TraderView.Domain.Entities;
using TraderView.Application.Models;

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
        void CreateExcelFileReport(List<Position> openPositions, List<TradeExecution> tradeExecutions, string outputFilePath);

        /// <summary>
        /// Prepares the open position report data by calculating all necessary values.
        /// This method can be reused for different report formats (Excel, Web, etc.)
        /// </summary>
        /// <param name="openPositions">List of open positions to process</param>
        /// <returns>List of calculated open position report data</returns>
        List<OpenPositionReportData> PrepareOpenPositionReportData(List<Position> openPositions);
    }
}
