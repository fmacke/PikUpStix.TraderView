using System.Text.Json.Serialization;

namespace TraderView.Domain.Entities.FMP
{
    public class FmpAnnualIncomeStatementDto
    {
        [JsonPropertyName("date")]
        public string Date { get; set; } = string.Empty;

        [JsonPropertyName("symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonPropertyName("reportedCurrency")]
        public string ReportedCurrency { get; set; } = string.Empty;

        [JsonPropertyName("cik")]
        public string Cik { get; set; } = string.Empty;

        [JsonPropertyName("filingDate")]
        public string FilingDate { get; set; } = string.Empty;

        [JsonPropertyName("acceptedDate")]
        public string AcceptedDate { get; set; } = string.Empty;

        [JsonPropertyName("fiscalYear")]
        public string CalendarYear { get; set; } = string.Empty;

        [JsonPropertyName("period")]
        public string Period { get; set; } = string.Empty;

        [JsonPropertyName("revenue")]
        public decimal Revenue { get; set; }

        [JsonPropertyName("costOfRevenue")]
        public decimal CostOfRevenue { get; set; }

        [JsonPropertyName("grossProfit")]
        public decimal GrossProfit { get; set; }

        [JsonPropertyName("researchAndDevelopmentExpenses")]
        public decimal ResearchAndDevelopmentExpenses { get; set; }

        [JsonPropertyName("generalAndAdministrativeExpenses")]
        public decimal GeneralAndAdministrativeExpenses { get; set; }

        [JsonPropertyName("sellingAndMarketingExpenses")]
        public decimal SellingAndMarketingExpenses { get; set; }

        [JsonPropertyName("sellingGeneralAndAdministrativeExpenses")]
        public decimal SellingGeneralAndAdministrativeExpenses { get; set; }

        [JsonPropertyName("otherExpenses")]
        public decimal OtherExpenses { get; set; }

        [JsonPropertyName("operatingExpenses")]
        public decimal OperatingExpenses { get; set; }

        [JsonPropertyName("costAndExpenses")]
        public decimal CostAndExpenses { get; set; }

        [JsonPropertyName("netInterestIncome")]
        public decimal NetInterestIncome { get; set; }

        [JsonPropertyName("interestIncome")]
        public decimal InterestIncome { get; set; }

        [JsonPropertyName("interestExpense")]
        public decimal InterestExpense { get; set; }

        [JsonPropertyName("depreciationAndAmortization")]
        public decimal DepreciationAndAmortization { get; set; }

        [JsonPropertyName("ebitda")]
        public decimal Ebitda { get; set; }

        [JsonPropertyName("ebit")]
        public decimal Ebit { get; set; }

        [JsonPropertyName("nonOperatingIncomeExcludingInterest")]
        public decimal NonOperatingIncomeExcludingInterest { get; set; }

        [JsonPropertyName("operatingIncome")]
        public decimal OperatingIncome { get; set; }

        [JsonPropertyName("totalOtherIncomeExpensesNet")]
        public decimal TotalOtherIncomeExpensesNet { get; set; }

        [JsonPropertyName("incomeBeforeTax")]
        public decimal IncomeBeforeTax { get; set; }

        [JsonPropertyName("incomeTaxExpense")]
        public decimal IncomeTaxExpense { get; set; }

        [JsonPropertyName("netIncomeFromContinuingOperations")]
        public decimal NetIncomeFromContinuingOperations { get; set; }

        [JsonPropertyName("netIncomeFromDiscontinuedOperations")]
        public decimal NetIncomeFromDiscontinuedOperations { get; set; }

        [JsonPropertyName("otherAdjustmentsToNetIncome")]
        public decimal OtherAdjustmentsToNetIncome { get; set; }

        [JsonPropertyName("netIncome")]
        public decimal NetIncome { get; set; }

        [JsonPropertyName("netIncomeDeductions")]
        public decimal NetIncomeDeductions { get; set; }

        [JsonPropertyName("bottomLineNetIncome")]
        public decimal BottomLineNetIncome { get; set; }

        [JsonPropertyName("eps")]
        public decimal Eps { get; set; }

        [JsonPropertyName("epsDiluted")]
        public decimal EpsDiluted { get; set; }

        [JsonPropertyName("weightedAverageShsOut")]
        public long WeightedAverageShsOut { get; set; }

        [JsonPropertyName("weightedAverageShsOutDil")]
        public long WeightedAverageShsOutDil { get; set; }
    }
}
