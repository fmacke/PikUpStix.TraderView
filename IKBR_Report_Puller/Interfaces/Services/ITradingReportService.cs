using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TraderView.Application.Interfaces.Services
{
    public interface ITradingReportService
    {
        Task<string> DetermineRegimeAsync(DateTime openDate, CancellationToken cancellationToken);
    }
}
