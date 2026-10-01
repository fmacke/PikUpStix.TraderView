using TraderView.Domain.Entities;
using TraderView.Application.Interfaces.Persistence;

namespace TraderView.Application.Interfaces.Repositories;

public interface IPositionCalculatorRepository : IRepository<PositionCalculator>
{
    // Additional position-calculator-specific methods can be added here in future
}
