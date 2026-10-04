using Microsoft.AspNetCore.Mvc;
using TraderView.Application.Interfaces.Services;
using TraderView.Application.Models;
using OfficeOpenXml;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace traderview.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TradeCalculatorController : ControllerBase
    {
        private readonly ITradeCalculatorService _calculatorService;
        private readonly ILogger<TradeCalculatorController> _logger;

        public TradeCalculatorController(ILogger<TradeCalculatorController> logger, ITradeCalculatorService calculatorService)
        {
            _logger = logger;
            _calculatorService = calculatorService;
        }

        [HttpPost("calculate")]
        public ActionResult<TradeCalculationResponse> Calculate([FromBody] TradeCalculationRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid calculation request.");
            }

            var result = _calculatorService.CalculatePosition(request);
            return Ok(result);
        }
        [HttpPost("calculate-compound")]
        public ActionResult<CompoundPositions> CalculateCompound([FromBody] TradeCalculationRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid calculation request.");
            }

            var quarterPosition = _calculatorService.CalculatePosition(request);
            var result = _calculatorService.CalculateCompoundPositions(request, quarterPosition);
            return Ok(result);
        }

        [HttpGet("compound-report")]
        public async Task<IActionResult> DownloadCompoundReport()
        {
            // Generate compound positions for all open positions
            var compounds = await _calculatorService.GenerateCompoundPositionsReportAsync();

            // Create Excel package in memory
            ExcelPackage.License.SetNonCommercialPersonal("DFM");
            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("CompoundPositions");

                // Header row
                var headers = new[] { "Instrument", "Tier", "TradeDate", "BuyPriceUsd", "TotalShares", "AverageSharePriceUsd", "StopLossAtUsd", "TargetSharePriceUsd", "ProfitTargetUsd", "WinUsd", "LossUsd", "PositionSizeUsd", "PositionRiskUsd", "AccountRiskPercentage" };
                for (int i = 0; i < headers.Length; i++) ws.Cells[1, i + 1].Value = headers[i];

                int row = 2;
                foreach (var cp in compounds)
                {
                    void WriteTier(string tierName, TraderView.Application.Models.TradeCalculationCompoundedPosition pos)
                    {
                        if (pos == null) return;
                        ws.Cells[row, 1].Value = pos.Instrument;
                        ws.Cells[row, 2].Value = tierName;
                        ws.Cells[row, 3].Value = pos.TradeDate.ToString("yyyy-MM-dd");
                        ws.Cells[row, 4].Value = (double)pos.BuyPriceUsd;
                        ws.Cells[row, 5].Value = (double)pos.TotalShares;
                        ws.Cells[row, 6].Value = (double)pos.AverageSharePriceUsd;
                        ws.Cells[row, 7].Value = (double)pos.StopLossAtUsd;
                        ws.Cells[row, 8].Value = (double)pos.TargetSharePriceUsd;
                        ws.Cells[row, 9].Value = (double)pos.ProfitTargetUsd;
                        ws.Cells[row, 10].Value = (double)pos.WinUsd;
                        ws.Cells[row, 11].Value = (double)pos.LossUsd;
                        ws.Cells[row, 12].Value = (double)pos.PositionSizeUsd;
                        ws.Cells[row, 13].Value = (double)pos.PositionRiskUsd;
                        ws.Cells[row, 14].Value = (double)pos.AccountRiskPercentage;
                        row++;
                    }

                    WriteTier("Quarter", cp.QuarterPosition);
                    WriteTier("Half", cp.HalfPosition);
                    WriteTier("Full", cp.FullPosition);
                }

                // Auto-fit columns
                ws.Cells[ws.Dimension.Address].AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                var fileName = $"CompoundPositions_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }
    }
}
