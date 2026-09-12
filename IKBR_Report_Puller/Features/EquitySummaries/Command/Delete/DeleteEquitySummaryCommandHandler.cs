using TraderView.Application.Interfaces.Services;

namespace TraderView.Application.Features.EquitySummaries.Command.Delete;

/// <summary>
/// Handler for DeleteEquitySummaryCommand
/// </summary>
public class DeleteEquitySummaryCommandHandler
{
    private readonly IEquitySummaryService _equitySummaryService;

    public DeleteEquitySummaryCommandHandler(IEquitySummaryService equitySummaryService)
    {
        _equitySummaryService = equitySummaryService;
    }

    /// <summary>
    /// Handles the delete equity summary command
    /// </summary>
    /// <param name="command">The delete equity summary command</param>
    /// <returns>True if deleted successfully, false if not found</returns>
    public async Task<bool> Handle(DeleteEquitySummaryCommand command)
    {
        return await _equitySummaryService.DeleteAsync(command.Id);
    }
}
