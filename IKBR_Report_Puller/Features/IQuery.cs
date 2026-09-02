namespace TraderView.Application.Features
{
    public interface IQuery
    {
        string Script { get; }
    }
    public interface IQueryWithParameters : IQuery
    {
        Dictionary<string, object> Parameters { get; }
    }
    public interface IQueryListWithParameters : IQuery
    {        
        List<Dictionary<string, object>> Parameters { get; }
    }
}