using System;
using System.Collections.Generic;
using System.Text;
using TraderView.Application.Models;
using TraderView.Application.Services;

namespace TraderView.Application.Interfaces.Services
{
    public interface ITradeCalculatorService
    {
        TradeCalculationResponse CalculatePosition(TradeCalculationRequest request);
        CompoundPositions CalculateCompoundPositions(TradeCalculationRequest request, TradeCalculationResponse quarterPosition);
        Task<List<CompoundPositions>> GenerateCompoundPositionsReportAsync();
    }
}
