using System.Linq.Expressions;
using TraderView.Application.Interfaces.Persistence;
using TraderView.Domain.Entities;

namespace TraderView.Application.Features.Positions.Specifications;

/// <summary>
/// Specification for checking if a position exists
/// </summary>
public class PositionExistsSpecification : ISpecification<Position>
{
    private readonly int _positionId;

    public PositionExistsSpecification(int positionId)
    {
        _positionId = positionId;
    }

    public Expression<Func<Position, bool>>? Criteria => x => x.Id == _positionId;

    public List<Expression<Func<Position, object>>> Includes => new();

    public List<string> IncludeStrings => new();

    public List<(Expression<Func<Position, object>> KeySelector, bool IsDescending)> OrderBys => new();

    public int? Take => null;

    public int? Skip => null;

    public bool IsPagingEnabled => false;
}
