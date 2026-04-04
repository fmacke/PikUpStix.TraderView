using System.Xml.Linq;
using IKBR_Report_Puller.Domain;

namespace IKBR_Report_Puller.Interfaces
{
    public interface IExcelReportService
    {
        void CreateReport(IKBRReport report, string outputFilePath);
    }
}
