using traderview.Server.DTOs;
using PikUpStix.TraderView.Interfaces;

namespace traderview.Server.Services
{
    public class OpenPositionService : IOpenPositionService
    {
        private readonly IOpenPositionRepository _openPositionRepository;
        private readonly ILogger<OpenPositionService> _logger;

        public OpenPositionService(
            IOpenPositionRepository openPositionRepository,
            ILogger<OpenPositionService> logger)
        {
            _openPositionRepository = openPositionRepository;
            _logger = logger;
        }

        public async Task<List<OpenPositionDto>> GetAllOpenPositionsAsync()
        {
            try
            {
                var openPositions = await Task.Run(() => _openPositionRepository.GetAllOpenPositions());

                return openPositions.Select(op => new OpenPositionDto
                {
                    Symbol = op.Symbol,
                    Description = op.Description,
                    AssetCategory = op.AssetCategory,
                    Currency = op.Currency,
                    Position = op.Position,
                    MarkPrice = op.MarkPrice,
                    PositionValue = op.PositionValue,
                    CostBasisPrice = op.CostBasisPrice,
                    CostBasisMoney = op.CostBasisMoney,
                    FifoPnlUnrealized = op.FifoPnlUnrealized,
                    PercentOfNAV = op.PercentOfNAV,
                    ReportDate = op.ReportDate,
                    ListingExchange = op.ListingExchange,
                    AccountId = op.AccountId
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching open positions");
                throw;
            }
        }
    }
}
