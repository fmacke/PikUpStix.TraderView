using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TraderView.Application.Models;
using TraderView.Domain.Entities;

namespace TraderView.Application.Interfaces.Services
{
    public interface ITradingReportService
    {
        Task<List<PositionOutcome>> CalculatePositionOutcomesAsync(CancellationToken cancellationToken = default);
        Task<PositionOutcome> GetPositionReport(Position pos);
        Task<List<OpenPositionReportData>> PrepareOpenPositionReportData(IReadOnlyList<Position> openPositions);
    }
}
