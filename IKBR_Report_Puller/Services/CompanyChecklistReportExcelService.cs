using OfficeOpenXml;
using TraderView.Application.Interfaces.Services;
using TraderView.Domain.Entities;

namespace PikUpStix.TraderView.Services
{
    /// <summary>
    /// Service for generating Excel reports from CompanyChecklistReport data.
    /// </summary>
    public class CompanyChecklistReportExcelService : ICompanyChecklistReportExcelService
    {
        public CompanyChecklistReportExcelService()
        {
        }

        public async Task<byte[]> GenerateChecklistReportsExcelAsync(IReadOnlyList<CompanyChecklistReport> reports)
        {
            if (reports == null || reports.Count == 0)
            {
                throw new ArgumentException("Reports list cannot be null or empty.", nameof(reports));
            }

            try
            {
                // Set the EPPlus license for non-commercial use
                ExcelPackage.License.SetNonCommercialPersonal("DFM");

                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Company Checklists");

                    // Add headers
                    AddHeaders(worksheet);

                    // Add data rows
                    AddDataRows(worksheet, reports);

                    // Format columns
                    FormatColumns(worksheet);

                    // Convert to byte array
                    return await Task.FromResult(package.GetAsByteArray());
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error generating Excel file for company checklist reports.", ex);
            }
        }

        private void AddHeaders(ExcelWorksheet worksheet)
        {
            var headers = new[]
            {
                "Symbol",
                "Company Name",
                "Created At (UTC)",
                "Composite Rating",
                "Market Exposure Status",
                "Industry Group",
                "Industry Group Rank",
                "Group RS Rating",
                "EPS Due Date",
                "EPS Rating",
                "Last Qtr EPS Growth YoY (%)",
                "Last 3 Qtrs Avg EPS Growth (%)",
                "Qtrs EPS Acceleration Count",
                "Current Qtr EPS Est Change (%)",
                "Estimate Revisions Trend",
                "Last Qtr Earnings Surprise (%)",
                "EPS 3-Yr CAGR (%)",
                "Consecutive Yrs Annual EPS Growth",
                "Current Year EPS Est Change (%)",
                "SMR Rating",
                "Last Qtr Sales Growth YoY (%)",
                "Sales 3-Yr CAGR (%)",
                "Annual Pre-Tax Margin (%)",
                "Annual ROE (%)",
                "Debt to Equity Ratio",
                "Price",
                "Relative Strength Rating",
                "% Off 52-Week High",
                "Price vs 50-Day SMA (%)",
                "Volume 50-Day Average",
                "Market Cap (Billions)",
                "Accumulation/Distribution Rating",
                "Up/Down Volume Ratio",
                "Fund Ownership Change (%)"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = headers[i];
            }

            // Format header row
            var headerRow = worksheet.Cells[1, 1, 1, headers.Length];
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            headerRow.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(217, 225, 242));
            headerRow.Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(31, 78, 121));
        }

        private void AddDataRows(ExcelWorksheet worksheet, IReadOnlyList<CompanyChecklistReport> reports)
        {
            for (int i = 0; i < reports.Count; i++)
            {
                var report = reports[i];
                int row = i + 2; // Start from row 2 (row 1 is headers)

                worksheet.Cells[row, 1].Value = report.Symbol;
                worksheet.Cells[row, 2].Value = report.CompanyName;
                worksheet.Cells[row, 3].Value = report.CreatedAtUtc;
                worksheet.Cells[row, 4].Value = report.CompositeRating;
                worksheet.Cells[row, 5].Value = report.MarketExposureStatus;
                worksheet.Cells[row, 6].Value = report.IndustryGroup;
                worksheet.Cells[row, 7].Value = report.IndustryGroupRank;
                worksheet.Cells[row, 8].Value = report.GroupRsRating;
                worksheet.Cells[row, 9].Value = report.EpsDueDate;
                worksheet.Cells[row, 10].Value = report.EpsRating;
                worksheet.Cells[row, 11].Value = report.LastQtrEpsGrowthYoY;
                worksheet.Cells[row, 12].Value = report.Last3QtrsAvgEpsGrowth;
                worksheet.Cells[row, 13].Value = report.QtrsEpsAccelerationCount;
                worksheet.Cells[row, 14].Value = report.CurrentQtrEpsEstChg;
                worksheet.Cells[row, 15].Value = report.EstimateRevisionsTrend;
                worksheet.Cells[row, 16].Value = report.LastQtrEarningsSurprisePercent;
                worksheet.Cells[row, 17].Value = report.Eps3YrCagr;
                worksheet.Cells[row, 18].Value = report.ConsecutiveYrsAnnualEpsGrowth;
                worksheet.Cells[row, 19].Value = report.CurrentYearEpsEstChg;
                worksheet.Cells[row, 20].Value = report.SmrRating;
                worksheet.Cells[row, 21].Value = report.LastQtrSalesGrowthYoY;
                worksheet.Cells[row, 22].Value = report.Sales3YrCagr;
                worksheet.Cells[row, 23].Value = report.AnnualPreTaxMarginPercent;
                worksheet.Cells[row, 24].Value = report.AnnualRoePercent;
                worksheet.Cells[row, 25].Value = report.DebtToEquityRatio;
                worksheet.Cells[row, 26].Value = report.Price;
                worksheet.Cells[row, 27].Value = report.RelativeStrengthRating;
                worksheet.Cells[row, 28].Value = report.PercentOff52WeekHigh;
                worksheet.Cells[row, 29].Value = report.PriceVs50DaySmaPercent;
                worksheet.Cells[row, 30].Value = report.Volume50DayAverage;
                worksheet.Cells[row, 31].Value = report.MarketCapBillions;
                worksheet.Cells[row, 32].Value = report.AccumulationDistributionRating;
                worksheet.Cells[row, 33].Value = report.UpDownVolumeRatio;
                worksheet.Cells[row, 34].Value = report.FundOwnershipChangePercent;
            }
        }

        private void FormatColumns(ExcelWorksheet worksheet)
        {
            // Auto-fit columns
            worksheet.Cells.AutoFitColumns();

            // Set minimum column widths for better readability
            for (int col = 1; col <= 34; col++)
            {
                if (worksheet.Column(col).Width < 12)
                {
                    worksheet.Column(col).Width = 12;
                }
            }

            // Format date columns
            worksheet.Cells["C:C"].Style.Numberformat.Format = "yyyy-MM-dd HH:mm:ss";
            worksheet.Cells["I:I"].Style.Numberformat.Format = "yyyy-MM-dd";

            // Format numeric columns with 2 decimal places
            for (int col = 11; col <= 34; col++)
            {
                if (col != 15 && col != 20) // Skip text columns (EstimateRevisionsTrend, SmrRating)
                {
                    worksheet.Column(col).Style.Numberformat.Format = "0.00";
                }
            }

            // Freeze the header row
            worksheet.View.FreezePanes(2, 1);
        }
    }
}
