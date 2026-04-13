# Trade Viewer - Complete Setup Guide

This guide will help you set up and run the Trade Viewer application, which consists of:
- **Backend**: ASP.NET Core 9 Web API (TradeViewer.API)
- **Frontend**: React TypeScript application with Lightweight Charts

## Prerequisites

✅ Visual Studio 2022/2026
✅ .NET 9 SDK
✅ Node.js 16+ and npm
✅ SQL Server with TradingBE database

## Step 1: Configure Database Connection

1. Open `TradeViewer.API/appsettings.json`
2. Update the connection string if needed:
   ```json
   "ConnectionStrings": {
     "TradingDatabase": "Server=localhost;Database=TradingBE;Integrated Security=true;TrustServerCertificate=true;"
   }
   ```

## Step 2: Start the Backend API

### Option A: Using Visual Studio
1. In Visual Studio, right-click on `TradeViewer.API` project
2. Select "Set as Startup Project"
3. Press F5 or click the Start button
4. The API will start on `https://localhost:7001`
5. Swagger UI should open automatically at `https://localhost:7001/swagger`

### Option B: Using Command Line
```powershell
cd TradeViewer.API
dotnet run
```

### Verify API is Running
Visit https://localhost:7001/swagger to see the API documentation and test endpoints:
- `GET /api/trades` - List all trades
- `GET /api/trades/{id}` - Get trade details
- `GET /api/trades/{id}/context` - Get trade with chart data

## Step 3: Start the React Frontend

1. Open a new terminal/PowerShell window
2. Navigate to the React app folder:
   ```powershell
   cd trade-viewer-ui
   ```

3. **If using PowerShell and you get an execution policy error**, run this first:
   ```powershell
   Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
   ```

   **Alternative**: Use Command Prompt (cmd) instead of PowerShell.

4. Install dependencies (first time only):
   ```powershell
   npm install
   ```

5. Start the development server:
   ```powershell
   npm start
   ```

6. The application will automatically open in your browser at http://localhost:3000

## Using the Application

### Navigation
- **Arrow Keys**: Press ← (left) and → (right) to navigate between trades
- **Mouse**: Click "Previous" and "Next" buttons

### Features
- **Trade Summary**: Shows all trade details including P&L, entry/exit prices, dates
- **Candlestick Chart**: 
  - Displays price action with OHLC bars
  - Blue arrow marks entry point
  - Red arrow marks exit point
  - Dashed lines show entry and exit price levels
  - 30 days of data before and after the trade

### Chart Interactions
- **Zoom**: Mouse wheel or trackpad pinch
- **Pan**: Click and drag the chart
- **Crosshair**: Hover to see exact values

## Troubleshooting

### API Connection Errors

**Error**: "Failed to load trades"
- Verify TradeViewer.API is running (https://localhost:7001)
- Check appsettings.json connection string
- Ensure SQL Server is running and TradingBE database exists
- Check CORS settings in Program.cs

### No Trades Showing

**Issue**: Empty trade list
- Verify you have data in the TradeExecutions table
- Check that trades have both BUY and SELL executions
- Look at browser console (F12) for error messages

### Chart Not Displaying

**Issue**: Trade summary shows but no chart
- Verify HistoricalData table has candlestick data for the instrument
- Check that InstrumentId in trades matches HistoricalData
- Ensure date ranges overlap between trade dates and historical data
- Check browser console for JavaScript errors

### CORS Issues

**Error**: "CORS policy" errors in browser console
- Ensure API is running on https://localhost:7001
- Check that CORS is configured in Program.cs
- Try clearing browser cache
- Verify setupProxy.js exists in trade-viewer-ui/src

### PowerShell Execution Policy Error

**Error**: "cannot be loaded because running scripts is disabled"
- Run `Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass` before npm commands
- Or use Command Prompt (cmd) instead of PowerShell
- Or run PowerShell as Administrator and execute: `Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy RemoteSigned`

## Project Structure

```
IKBR_Report_Puller/
├── TradeViewer.API/               # ASP.NET Core Web API
│   ├── DTOs/
│   │   └── TradeDtos.cs          # Data transfer objects
│   ├── Services/
│   │   ├── ITradeViewerService.cs
│   │   └── TradeViewerService.cs # Business logic and data access
│   ├── Program.cs                 # API endpoints and configuration
│   └── appsettings.json          # Connection string
│
└── trade-viewer-ui/               # React TypeScript frontend
    ├── src/
    │   ├── components/
    │   │   ├── TradingChart.tsx  # Chart component
    │   │   └── TradeSummary.tsx  # Trade details
    │   ├── services/
    │   │   └── apiService.ts     # API client
    │   ├── types/
    │   │   └── api.ts            # TypeScript interfaces
    │   └── App.tsx               # Main application
    ├── .env                       # Environment config
    └── package.json
```

## Development Tips

### Modifying the Chart Window
To change how much historical data is shown around trades, edit the query parameters in `App.tsx`:
```typescript
const context = await apiService.getTradeContext(currentTrade.id, 30, 30);
//                                                                 ^^  ^^
//                                                     days before  |  days after
```

### Customizing Chart Colors
Edit `trade-viewer-ui/src/components/TradingChart.tsx`:
- Entry marker color: Line with `color: '#2196F3'`
- Exit marker color: Line with `color: '#f44336'`
- Background: `background: { color: '#1e1e1e' }`

### Adding More Trade Metrics
1. Add fields to `TradeDto` in DTOs/TradeDtos.cs
2. Update SQL query in TradeViewerService.cs
3. Add fields to TypeScript interface in trade-viewer-ui/src/types/api.ts
4. Display in TradeSummary.tsx

## API Endpoints Details

### GET /api/trades
Returns all completed trades (trades with both buy and sell executions)
```json
[
  {
    "id": 12345,
    "symbol": "ES",
    "entryDate": "2024-01-15T09:30:00",
    "exitDate": "2024-01-15T15:45:00",
    "entryPrice": 4800.50,
    "exitPrice": 4805.25,
    "quantity": 2,
    "pnl": 475.00,
    "buySell": "BUY"
  }
]
```

### GET /api/trades/{id}
Returns detailed information about a specific trade including instrument details and all executions

### GET /api/trades/{id}/context?daysBefore=30&daysAfter=30
Returns trade information plus candlestick data for charting

## Next Steps

- Customize chart timeframes
- Add trade filtering capabilities
- Implement trade statistics dashboard
- Add export functionality
- Create mobile-responsive layouts

## Support

If you encounter issues:
1. Check browser console (F12) for errors
2. Verify API is responding at https://localhost:7001/swagger
3. Check database connectivity and data
4. Review this troubleshooting section

Happy trading! 📈
