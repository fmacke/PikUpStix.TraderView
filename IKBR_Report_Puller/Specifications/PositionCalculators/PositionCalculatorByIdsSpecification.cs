using TraderView.Application.Specifications;
using TraderView.Domain.Entities;
using System;
using System.Collections.Generic;

namespace TraderView.Application.Specifications.PositionCalculators
{
    public class PositionCalculatorByIdsSpecification : BaseSpecification<PositionCalculator>
    {
        public PositionCalculatorByIdsSpecification(IEnumerable<int> ids)
        {
            var idSet = new HashSet<int>(ids);
            Criteria = pc => idSet.Contains(pc.Id);
        }
    }
}
