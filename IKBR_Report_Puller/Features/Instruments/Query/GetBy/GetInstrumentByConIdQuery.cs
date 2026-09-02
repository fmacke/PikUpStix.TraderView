namespace TraderView.Application.Features.Instruments.Query.GetBy
{
    public class GetInstrumentByConIdQuery : IQueryWithParameters
    {
        private readonly string _conId;
        public GetInstrumentByConIdQuery(string conid) {
            _conId = conid;
        }

        public Dictionary<string, object> Parameters { get => new Dictionary<string, object>
                        {
                            { "@conid", _conId }
                        }; }
        public string Script { get => "SELECT Id FROM dbo.Instruments WHERE ConId = @conid"; }
    }; 
}


