# TradeViewer Debug Setup

This solution is configured to automatically launch both the TradeViewer.API and the React UI when debugging.

## How It Works

When you press F5 or click "Start Debugging" in Visual Studio:

1. **TradeViewer.API** starts on:
   - HTTPS: `https://localhost:7001`
   - HTTP: `http://localhost:5001`

2. **React Development Server** automatically starts on:
   - `http://localhost:3000`

3. Your browser opens to `http://localhost:3000` showing the Trade Viewer UI

## Launch Profile

The solution includes a new launch profile: **"API + React UI"**

To use it:
1. In Visual Studio, click the dropdown next to the Start button (green play button)
2. Select **"API + React UI"**
3. Press F5 or click Start

## What Happens Behind the Scenes

1. The `StartReactDevServer.targets` file is imported into the TradeViewer.API project
2. When building in Debug mode, it runs `start-react-dev.ps1`
3. The PowerShell script checks if the React dev server is already running on port 3000
4. If not running, it launches `trade-viewer-ui/start-dev-server.bat`
5. The React app compiles and starts serving on `http://localhost:3000`
6. The API starts and the browser opens to the React UI

## Manual Start (Alternative)

If you prefer to start the React app manually:

1. Open a terminal in the `trade-viewer-ui` folder
2. Run: `npm start`
3. Then start debugging the TradeViewer.API project in Visual Studio

## Stopping the Servers

- **API**: Stop debugging in Visual Studio (Shift+F5)
- **React**: Close the command window or press Ctrl+C in the terminal

## Troubleshooting

### React app doesn't start automatically
- Ensure Node.js and npm are installed
- Navigate to `trade-viewer-ui` and run `npm install`
- Check that port 3000 is not already in use

### API connection errors
- Verify the API is running on `https://localhost:7001`
- Check that CORS is configured correctly (already set up)
- The React proxy in `src/setupProxy.js` forwards `/api` requests to the API

### Port conflicts
- If port 3000 or 7001 is in use, stop other services
- Or modify the ports in:
  - `launchSettings.json` (API)
  - `package.json` (React - not recommended)

## Configuration Files

- `TradeViewer.API/Properties/launchSettings.json` - Launch profiles
- `TradeViewer.API/StartReactDevServer.targets` - MSBuild target to start React
- `start-react-dev.ps1` - PowerShell script to launch React dev server
- `trade-viewer-ui/start-dev-server.bat` - Batch script to start React
- `trade-viewer-ui/src/setupProxy.js` - Proxy configuration for API calls
