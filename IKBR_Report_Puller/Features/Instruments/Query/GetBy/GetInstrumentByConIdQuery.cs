namespace TraderView.Application.Features.Instruments.Query.GetBy
{
    public class GetInstrumentByConIdQuery
    {
        public string Script()
        {
            return "SELECT Id FROM dbo.Instruments WHERE ConId = @conid";
        }
    }
}
