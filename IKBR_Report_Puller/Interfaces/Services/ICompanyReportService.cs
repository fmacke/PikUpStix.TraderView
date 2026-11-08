using System;
using System.Collections.Generic;
using System.Text;
using TraderView.Application.Models.Reports;

namespace TraderView.Application.Interfaces.Services
{    public interface ICompanyReportService
    {
        Task<CompanyChecklistReport?> GenerateChecklistReportAsync(string symbol);
    }
}
