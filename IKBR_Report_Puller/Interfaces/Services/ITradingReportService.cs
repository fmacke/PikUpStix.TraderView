using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TraderView.Domain.Entities;

namespace TraderView.Application.Interfaces.Services
{
    public interface ITradingReportService
    {
        Task<List<PositionOutcome>> CalculatePositionOutcomesAsync(CancellationToken cancellationToken = default);
    }
}
