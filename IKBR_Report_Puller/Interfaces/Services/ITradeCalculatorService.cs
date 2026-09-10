using System;
using System.Collections.Generic;
using System.Text;
using TraderView.Application.Models;

namespace TraderView.Application.Interfaces.Services
{
    public interface ITradeCalculatorService
    {
        TradeCalculationResponse CalculatePosition(TradeCalculationRequest request);
    }
}
