# TradeViewer Debug Flow

## Visual Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        VISUAL STUDIO                            │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  Press F5 / Start Debugging                              │   │
│  │  Profile: "API + React UI"                               │   │
│  └────────────────────┬─────────────────────────────────────┘   │
│                       │                                          │
│                       ▼                                          │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  MSBuild runs StartReactDevServer.targets                │   │
│  └────────────────────┬─────────────────────────────────────┘   │
│                       │                                          │
└───────────────────────┼──────────────────────────────────────────┘
						│
		┌───────────────┴────────────────┐
		│                                 │
		▼                                 ▼
┌───────────────────┐          ┌──────────────────────┐
│  start-react-     │          │   TradeViewer.API    │
│  dev.ps1          │          │   Starts              │
└────────┬──────────┘          └──────────┬───────────┘
		 │                                 │
		 ▼                                 ▼
┌───────────────────┐          ┌──────────────────────┐
│  Check if React   │          │  Listening on:       │
│  is running on    │          │  https://localhost:  │
│  port 3000        │          │  7001                │
└────────┬──────────┘          │  http://localhost:   │
		 │                     │  5001                │
		 ▼                     └──────────┬───────────┘
┌───────────────────┐                    │
│  Start            │                    │
│  start-dev-       │                    │
│  server.bat       │                    │
└────────┬──────────┘                    │
		 │                                │
		 ▼                                │
┌───────────────────┐                    │
│  npm start        │                    │
│  React Dev Server │                    │
│  Port 3000        │                    │
└────────┬──────────┘                    │
		 │                                │
		 │                                │
		 ▼                                ▼
┌────────────────────────────────────────────────────┐
│              BROWSER OPENS                         │
│         http://localhost:3000                      │
└────────┬───────────────────────────────────────────┘
		 │
		 │ User clicks, loads trades
		 │
		 ▼
┌────────────────────────────────────────────────────┐
│  React App makes request: /api/trades              │
└────────┬───────────────────────────────────────────┘
		 │
		 ▼
┌────────────────────────────────────────────────────┐
│  setupProxy.js intercepts                          │
│  Forwards to: https://localhost:7001/api/trades    │
└────────┬───────────────────────────────────────────┘
		 │
		 ▼
┌────────────────────────────────────────────────────┐
│  TradeViewer.API receives request                  │
│  Queries database                                  │
│  Returns JSON                                      │
└────────┬───────────────────────────────────────────┘
		 │
		 ▼
┌────────────────────────────────────────────────────┐
│  React receives data                               │
│  Displays trades with charts                       │
└────────────────────────────────────────────────────┘
```

## File Interaction Map

```
Visual Studio Debug (F5)
│
├─ TradeViewer.API.csproj
│  └─ Imports: StartReactDevServer.targets
│     └─ Executes: start-react-dev.ps1
│        └─ Launches: trade-viewer-ui/start-dev-server.bat
│           └─ Runs: npm start (in trade-viewer-ui folder)
│
└─ launchSettings.json
   └─ Profile: "API + React UI"
	  ├─ Starts API on ports 7001/5001
	  └─ Opens browser to http://localhost:3000
```

## Request Flow

```
Browser
  ↓ [GET http://localhost:3000/api/trades]
React Dev Server (proxy middleware)
  ↓ [Rewrites to https://localhost:7001/api/trades]
TradeViewer.API
  ↓ [Queries SQL Server]
Database
  ↓ [Returns trade data]
TradeViewer.API
  ↓ [Serializes to JSON]
React Dev Server
  ↓ [Passes response back]
Browser
  ↓ [React renders UI]
User sees trade list and charts! 🎉
```

## Key Components

### Backend (TradeViewer.API)
- **Language**: C# / .NET 9
- **Framework**: ASP.NET Core Minimal API
- **Database**: SQL Server (TradingBE)
- **Ports**: 7001 (HTTPS), 5001 (HTTP)
- **Endpoints**: `/api/trades`, `/api/trades/{id}`, `/api/trades/{id}/context`

### Frontend (trade-viewer-ui)
- **Language**: TypeScript
- **Framework**: React 19
- **Build Tool**: react-scripts (webpack)
- **Charting**: lightweight-charts
- **HTTP Client**: axios
- **Port**: 3000
- **Proxy**: All `/api/*` → `https://localhost:7001`

### Glue Components
- **setupProxy.js**: Configures the proxy middleware
- **apiService.ts**: HTTP client wrapper
- **StartReactDevServer.targets**: MSBuild integration
- **start-react-dev.ps1**: Launch automation

## Debugging Points

You can set breakpoints at:

**Backend (Visual Studio)**:
- `Program.cs` - API endpoints
- `TradeViewerService.cs` - Business logic
- `DataService.cs` - Database queries

**Frontend (Browser DevTools)**:
- `App.tsx` - Component lifecycle
- `apiService.ts` - HTTP requests
- `TradingChart.tsx` - Chart rendering

## Environment Variables

### API
- `ASPNETCORE_ENVIRONMENT` = Development
- Connection string from `appsettings.json`

### React
- `REACT_APP_API_URL` = /api (uses proxy)
- `NODE_ENV` = development (set by react-scripts)

---

This setup gives you a professional full-stack development experience! 🚀
