using System.Xml.Linq;

namespace IKBR_Report_Puller.Interfaces
{
    public interface IExcelReportService
    {
        void CreateOpenPositionsReport(XDocument reportXml, string outputFilePath);
    }
}
