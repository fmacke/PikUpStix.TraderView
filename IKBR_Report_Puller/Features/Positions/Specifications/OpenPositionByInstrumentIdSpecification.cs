using System.Linq.Expressions;
using TraderView.Application.Interfaces.Persistence;
using TraderView.Domain.Entities;

namespace TraderView.Application.Features.Positions.Specifications;

/// <summary>
/// Specification for retrieving an open position by instrument ID
/// </summary>
public class OpenPositionByInstrumentIdSpecification : ISpecification<Position>
{
    private readonly int _instrumentId;

    public OpenPositionByInstrumentIdSpecification(int instrumentId)
    {
        _instrumentId = instrumentId;
    }

    public Expression<Func<Position, bool>>? Criteria => x => 
        x.InstrumentId == _instrumentId && x.Status == "Open";

    public List<Expression<Func<Position, object>>> Includes => new()
    {
        x => x.Instrument,
        x => x.TradeExecutions
    };

    public List<string> IncludeStrings => new();

    public List<(Expression<Func<Position, object>> KeySelector, bool IsDescending)> OrderBys => new();

    public int? Take => null;

    public int? Skip => null;

    public bool IsPagingEnabled => false;
}
