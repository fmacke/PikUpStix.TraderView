using TraderView.Application.Interfaces.Services;

namespace TraderView.Application.Features.EquitySummaries.Command.Create;

/// <summary>
/// Handler for CreateEquitySummaryCommand
/// </summary>
public class CreateEquitySummaryCommandHandler
{
    private readonly IEquitySummaryService _equitySummaryService;

    public CreateEquitySummaryCommandHandler(IEquitySummaryService equitySummaryService)
    {
        _equitySummaryService = equitySummaryService;
    }

    /// <summary>
    /// Handles the create equity summary command
    /// </summary>
    /// <param name="command">The create equity summary command</param>
    /// <returns>The ID of the newly created equity summary</returns>
    public async Task<int> Handle(CreateEquitySummaryCommand command)
    {
        var equitySummary = command.ToEntity();
        return await _equitySummaryService.CreateAsync(equitySummary);
    }
}
