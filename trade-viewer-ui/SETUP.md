# Trade Viewer - Setup Instructions

## Quick Start

### 1. Start the API
First, run the TradeViewer.API project from Visual Studio:
- Set TradeViewer.API as startup project
- Press F5 or click Start
- API should be running on https://localhost:7001

### 2. Start the React Frontend

Open a terminal in the `trade-viewer-ui` folder and run:

```powershell
npm start
```

The application will open in your browser at http://localhost:3000

### 3. Using the Application

- **Navigate**: Use ← and → arrow keys to move between trades
- **View Details**: Trade summary and chart update automatically
- **Interact with Chart**: 
  - Zoom with mouse wheel
  - Pan by clicking and dragging
  - View exact values with crosshair hover

## Features

✅ Keyboard navigation with arrow keys
✅ Trade summary with P&L, dates, prices
✅ Candlestick charts with entry/exit markers
✅ Price lines showing trade levels
✅ Responsive design
✅ Real-time data from your SQL Server database

## API Endpoints Used

- `GET /api/trades` - List all trades
- `GET /api/trades/{id}` - Get trade details
- `GET /api/trades/{id}/context` - Get trade with candlestick data

## Environment Variables

Edit `.env` file to change API URL:
```
REACT_APP_API_URL=https://localhost:7001/api
```
