# Trade Viewer Application

A full-stack web application for reviewing and analyzing historical trades with interactive candlestick charts.

## 🎯 Features

- **Interactive Trade Navigation**: Use arrow keys or buttons to browse through your trade history
- **Detailed Trade Summary**: View P&L, entry/exit prices, quantities, and trade metadata
- **Candlestick Charts**: High-performance charts using TradingView's Lightweight Charts library
- **Visual Trade Markers**: Entry and exit points clearly marked with arrows and price lines
- **Responsive Design**: Clean, modern dark-themed UI optimized for trading analysis

## 🏗️ Architecture

### Backend: ASP.NET Core 9 Web API
- **Framework**: .NET 9
- **Database**: SQL Server (TradingBE)
- **API**: RESTful endpoints with Swagger documentation
- **Data Access**: ADO.NET with SqlClient

### Frontend: React + TypeScript
- **Framework**: React 18 with TypeScript
- **Charting**: Lightweight Charts by TradingView
- **HTTP Client**: Axios
- **Build Tool**: Create React App

## 📋 Prerequisites

- Visual Studio 2022/2026
- .NET 9 SDK
- Node.js 16+ and npm
- SQL Server with TradingBE database containing:
  - `TradeExecutions` table
  - `HistoricalData` table  
  - `Instruments` table

## 🚀 Quick Start

See **[QUICKSTART.md](QUICKSTART.md)** for 3-step setup instructions.

## 📚 Documentation

- **[QUICKSTART.md](QUICKSTART.md)** - Get up and running in 3 steps
- **[TRADE_VIEWER_SETUP.md](TRADE_VIEWER_SETUP.md)** - Comprehensive setup guide with troubleshooting
- **[trade-viewer-ui/SETUP.md](trade-viewer-ui/SETUP.md)** - Frontend-specific instructions

## 🎮 Usage

1. **Start the API**: Run TradeViewer.API from Visual Studio (F5)
2. **Start the UI**: Run `npm start` in the trade-viewer-ui folder
3. **Navigate trades**: Use ← → arrow keys or click Previous/Next buttons
4. **Analyze charts**: Zoom with mouse wheel, pan by dragging, hover for values

## 📊 API Endpoints

| Endpoint | Description |
|----------|-------------|
| `GET /api/trades` | List all completed trades |
| `GET /api/trades/{id}` | Get detailed trade information |
| `GET /api/trades/{id}/context` | Get trade with candlestick data for charting |

API Documentation: https://localhost:7001/swagger

## 🗄️ Database Schema

The application queries these tables:

- **TradeExecutions**: Individual buy/sell executions
- **HistoricalData**: OHLCV candlestick data
- **Instruments**: Trading instrument metadata

## 🎨 Customization

### Chart Colors
Edit `trade-viewer-ui/src/components/TradingChart.tsx`:
```typescript
upColor: '#26a69a',      // Green for up candles
downColor: '#ef5350',    // Red for down candles
entryColor: '#2196F3',   // Blue for entry
exitColor: '#f44336',    // Red for exit
```

### Data Window
Edit `trade-viewer-ui/src/App.tsx`:
```typescript
await apiService.getTradeContext(
  currentTrade.id, 
  30,  // days before entry
  30   // days after exit
);
```

### Connection String
Edit `TradeViewer.API/appsettings.json`:
```json
"ConnectionStrings": {
  "TradingDatabase": "Server=localhost;Database=TradingBE;..."
}
```

## 🛠️ Development

### API Development
```bash
cd TradeViewer.API
dotnet run
```

### Frontend Development  
```bash
cd trade-viewer-ui
npm start
```

### Build for Production
```bash
# API
cd TradeViewer.API
dotnet publish -c Release

# Frontend
cd trade-viewer-ui
npm run build
```

## 📝 Project Structure

```
IKBR_Report_Puller/
├── TradeViewer.API/              # ASP.NET Core Web API
│   ├── DTOs/                     # Data transfer objects
│   ├── Services/                 # Business logic & data access
│   ├── Program.cs                # API configuration & endpoints
│   └── appsettings.json          # Configuration
│
├── trade-viewer-ui/              # React TypeScript frontend
│   ├── src/
│   │   ├── components/           # React components
│   │   ├── services/             # API client
│   │   ├── types/                # TypeScript interfaces
│   │   └── App.tsx               # Main application
│   ├── public/                   # Static assets
│   └── package.json              # Dependencies
│
└── IKBR_Report_Puller/           # Original data import project
    ├── Data/                     # Repositories
    ├── Domain/                   # Domain models
    └── Services/                 # Business services
```

## 🐛 Troubleshooting

### Common Issues

**API won't start**
- Check .NET 9 SDK is installed: `dotnet --version`
- Verify SQL Server is running
- Check connection string in appsettings.json

**Frontend won't start**
- Run `npm install` in trade-viewer-ui folder
- Check Node version: `node --version` (should be 16+)
- Clear node_modules and reinstall if needed

**No trades appearing**
- Verify TradeExecutions table has data
- Check trades have both BUY and SELL executions (grouped by ibOrderID)
- Look at browser console (F12) for API errors

**Chart not showing**
- Ensure HistoricalData table has candlestick data
- Verify InstrumentId matches between TradeExecutions and HistoricalData
- Check date ranges overlap between trades and historical data

**CORS errors**
- Ensure API is running on https://localhost:7001
- Check CORS configuration in Program.cs
- Verify proxy configuration in setupProxy.js

## 🔒 Security Notes

This is a development setup. For production:

- Use environment variables for connection strings
- Enable authentication/authorization
- Configure HTTPS properly
- Use secure cookie settings
- Implement rate limiting
- Add input validation

## 📦 Technologies Used

### Backend
- ASP.NET Core 9
- Microsoft.Data.SqlClient
- Swashbuckle (Swagger/OpenAPI)

### Frontend
- React 18
- TypeScript 4.9
- Lightweight Charts 5.x
- Axios
- Create React App

## 🤝 Contributing

This is part of the IKBR Report Puller project. Feel free to extend with:

- Additional trade metrics and analytics
- Filter and search capabilities
- Export functionality (CSV, PDF)
- Multiple timeframe analysis
- Trade performance statistics
- Mobile-responsive improvements

## 📄 License

Part of IKBR Report Puller project.

## 🎯 Future Enhancements

- [ ] Trade filtering by symbol, date range, P&L
- [ ] Statistics dashboard (win rate, avg P&L, etc.)
- [ ] Multiple chart timeframes
- [ ] Compare multiple trades side-by-side
- [ ] Export trades to Excel/PDF
- [ ] Mobile app version
- [ ] Real-time trade notifications
- [ ] Machine learning trade analysis

---

**Ready to review your trades?** See [QUICKSTART.md](QUICKSTART.md) to get started! 📈
