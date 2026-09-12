namespace TraderView.Application.Features.EquitySummaries.Command.Delete;

/// <summary>
/// Command to delete an equity summary
/// </summary>
public class DeleteEquitySummaryCommand
{
    public int Id { get; set; }

    public DeleteEquitySummaryCommand(int id)
    {
        Id = id;
    }
}
