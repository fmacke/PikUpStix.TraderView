namespace IKBR_Report_Puller.Models
{
    public class IbkrSettings
    {
        public string Token { get; set; }
        public string QueryId { get; set; }
        public string BaseUrl { get; set; }
        public string QueryTodayExecutionsId { get; set; }
        public string OutputFilePath { get; set; }
    }

    public class DatabaseSettings
    {
        public string User { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public string DbName { get; set; }

        public string GetConnectionString() => $"Server={Host};Database={DbName};User ID={User};Password={Password};TrustServerCertificate=True;";
    }

    public class FinancialModelingPrepSettings
    {
        public string ApiKey { get; set; }
        public string BaseUrl { get; set; } = "https://financialmodelingprep.com/stable";
        public string OutputFilePath { get; set; } = @"C:\Users\finn\OneDrive\Documents\Wealth\Business\trading\TradeExecution Diaries";
    }
}
