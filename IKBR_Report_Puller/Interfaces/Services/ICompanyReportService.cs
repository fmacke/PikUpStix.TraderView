using System;
using System.Collections.Generic;
using System.Text;
using TraderView.Application.Models.Reports;
using TraderView.Domain.Entities;

namespace TraderView.Application.Interfaces.Services
{    public interface ICompanyReportService
    {
        Task<TraderView.Application.Models.Reports.CompanyChecklistReport?> GenerateChecklistReportAsync(string symbol);
        Task<int> SaveChecklistReportAsync(TraderView.Domain.Entities.CompanyChecklistReport report);
    }
}
