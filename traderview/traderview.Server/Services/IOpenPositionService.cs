using traderview.Server.DTOs;

namespace traderview.Server.Services
{
    public interface IOpenPositionService
    {
        Task<List<OpenPositionDto>> GetAllOpenPositionsAsync();
    }
}
