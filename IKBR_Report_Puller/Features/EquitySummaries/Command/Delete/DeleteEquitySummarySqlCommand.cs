using TraderView.Application.Features;

namespace TraderView.Application.Features.EquitySummaries.Command.Delete
{
    public class DeleteEquitySummarySqlCommand : IQueryWithParameters
    {
        private readonly int _id;
        public DeleteEquitySummarySqlCommand(int id) { _id = id; }

        public Dictionary<string, object> Parameters => new Dictionary<string, object>
        {
            { "@id", _id }
        };

        // Return number of rows affected
        public string Script => @"DELETE FROM [dbo].[EquitySummaries]
                    WHERE Id = @id;
                    SELECT @@ROWCOUNT;";
    }
}
