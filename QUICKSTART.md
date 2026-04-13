# 🚀 Trade Viewer - Quick Start

## What You Just Got

A complete full-stack application to review your historical trades:

**Backend (TradeViewer.API)**: ASP.NET Core 9 Web API that queries your SQL Server database
**Frontend (trade-viewer-ui)**: React app with interactive candlestick charts powered by TradingView's Lightweight Charts

## Start Using It (3 Simple Steps)

### 1️⃣ Start the API

In Visual Studio:
- Right-click `TradeViewer.API` → Set as Startup Project
- Press **F5**
	- API will run at https://localhost:7001
- Swagger UI opens automatically

### 2️⃣ Start the React App

Open PowerShell/Terminal in the project root:

**If you get a PowerShell execution policy error**, run this first:
```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
cd trade-viewer-ui
npm start
```

**Alternative**: Use Command Prompt (cmd) instead of PowerShell:
```cmd
cd trade-viewer-ui
npm start
```

Browser opens automatically at http://localhost:3000

### 3️⃣ Navigate Your Trades

✅ Use **← →** arrow keys to move between trades
✅ Trade summary shows P&L, prices, dates
✅ Chart displays candlesticks with entry/exit markers
✅ Zoom/pan the chart for detailed analysis

## Key Features

📊 **Candlestick Charts**
- Entry point marked with blue arrow
- Exit point marked with red arrow  
- Dashed lines show entry/exit price levels
- Shows 30 days before and after each trade

🎯 **Trade Summary**
- Profit/Loss (green/red)
- Entry and exit dates/prices
- Quantity traded
- Instrument details

⌨️ **Keyboard Navigation**
- ← Previous trade
- → Next trade

🖱️ **Chart Interactions**
- Mouse wheel to zoom
- Click and drag to pan
- Hover for crosshair values

## Troubleshooting

**No trades showing?**
- Check your TradeExecutions table has data
- Verify trades have both BUY and SELL executions

**Chart not appearing?**
- Ensure HistoricalData table has candlestick data
- Verify InstrumentId matches between tables

**Connection errors?**
- Update connection string in `TradeViewer.API/appsettings.json`
- Default: `Server=localhost;Database=TradingBE;...`

## API Endpoints

The API provides 3 endpoints:

```
GET /api/trades                    → List all trades
GET /api/trades/{id}               → Trade details
GET /api/trades/{id}/context       → Trade + chart data
```

Test them at: https://localhost:7001/swagger

## Files Created

```
TradeViewer.API/
├── DTOs/TradeDtos.cs              # Data models
├── Services/
│   ├── ITradeViewerService.cs
│   └── TradeViewerService.cs      # Database queries
├── Program.cs                      # API endpoints
└── appsettings.json               # Database connection

trade-viewer-ui/
├── src/
│   ├── components/
│   │   ├── TradingChart.tsx       # Chart component
│   │   └── TradeSummary.tsx       # Trade info
│   ├── services/apiService.ts     # API calls
│   ├── types/api.ts               # TypeScript types
│   └── App.tsx                    # Main app
└── .env                           # Config
```

## Next Steps

Want to customize?

**Change chart colors**: Edit `TradingChart.tsx`
**Modify trade window**: Adjust days before/after in `App.tsx`
**Add more metrics**: Update DTOs and SQL queries
**Filter trades**: Add query parameters to API

## Full Documentation

See `TRADE_VIEWER_SETUP.md` for detailed setup instructions, troubleshooting, and customization options.

---

**Ready to go!** Start both the API and React app, then press → to review your trades 📈
