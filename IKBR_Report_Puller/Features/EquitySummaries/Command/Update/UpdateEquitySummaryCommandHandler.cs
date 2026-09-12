using TraderView.Application.Interfaces.Services;

namespace TraderView.Application.Features.EquitySummaries.Command.Update;

/// <summary>
/// Handler for UpdateEquitySummaryCommand
/// </summary>
public class UpdateEquitySummaryCommandHandler
{
    private readonly IEquitySummaryService _equitySummaryService;

    public UpdateEquitySummaryCommandHandler(IEquitySummaryService equitySummaryService)
    {
        _equitySummaryService = equitySummaryService;
    }

    /// <summary>
    /// Handles the update equity summary command
    /// </summary>
    /// <param name="command">The update equity summary command</param>
    public async Task Handle(UpdateEquitySummaryCommand command)
    {
        var equitySummary = command.ToEntity();
        await _equitySummaryService.UpdateAsync(equitySummary);
    }
}
