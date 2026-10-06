using TraderView.Application.Specifications;
using TraderView.Domain.Entities;
using System;

namespace TraderView.Application.Specifications.PositionCalculators
{
    public class PositionCalculatorByPositionSpecification : BaseSpecification<PositionCalculator>
    {
        public PositionCalculatorByPositionSpecification(int positionId)
        {
            Criteria = pc => pc.PositionId == positionId;
        }
    }
}
