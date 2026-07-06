using System.Xml.Linq;
using IKBR_Report_Puller.Domain;

namespace PikUpStix.TraderView.Interfaces
{
    public interface IExcelReportService
    {
        /// <summary>
        /// Creates an Excel report based on the provided IKBRReport data and saves it to the specified file path.
        /// </summary>
        /// <param name="report">The IKBRReport containing the data to be included in the Excel report.</param>
        /// <param name="outputFilePath">The file path where the Excel report will be saved.</param>    
        /// </summary>
        void CreateExcelFileReport(List<OpenPosition> openPositions, List<Trade> tradeExecutions, string outputFilePath);
    }
}
