using TraderView.Domain.Entities;

namespace TraderView.Application.Interfaces.Services
{
    public interface ICompanyChecklistReportExcelService
    {
        /// <summary>
        /// Generates an Excel workbook containing CompanyChecklistReport data.
        /// </summary>
        /// <param name="reports">The list of CompanyChecklistReport entities to include in the workbook.</param>
        /// <returns>A byte array representing the Excel file content.</returns>
        Task<byte[]> GenerateChecklistReportsExcelAsync(IReadOnlyList<CompanyChecklistReport> reports);
    }
}
