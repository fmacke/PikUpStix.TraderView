# Quick Start Guide - TradeViewer Debug Setup

## 🚀 How to Debug Both Client and Server Together

### Option 1: Use the New Launch Profile (Recommended)

1. In Visual Studio, locate the **debug target dropdown** (next to the green play button)
2. Select **"API + React UI"** from the dropdown
3. Press **F5** or click the **Start** button
4. Wait 10-30 seconds for React to compile
5. Your browser will open to `http://localhost:3000`

### Option 2: Manual Start

If the automatic startup doesn't work:

**Terminal 1 - React App:**
```powershell
cd trade-viewer-ui
npm start
```

**Visual Studio:**
- Press F5 to start debugging the API

## ✅ What You Should See

After starting:

1. **Visual Studio Output Window** - API starting messages
2. **New Command Window** - React development server compiling
3. **Browser** - Opens to `http://localhost:3000` with the Trade Viewer UI

## 🔧 Servers Running

- **API (Backend)**: `https://localhost:7001` or `http://localhost:5001`
- **React UI (Frontend)**: `http://localhost:3000`

The React app uses a proxy to forward `/api` requests to the backend.

## 🛑 How to Stop

- **Stop Debugging**: Press `Shift+F5` in Visual Studio
- **Stop React**: Close the command window or press `Ctrl+C`

## 🐛 Troubleshooting

### "npm is not recognized"
- Install Node.js from https://nodejs.org/
- Restart Visual Studio after installation

### React app shows "Failed to load trades"
- Check the API is running (look for "Now listening on..." in Output window)
- Check the React console (F12 in browser) for errors
- Verify `http://localhost:3000` can connect to the proxy

### Port 3000 already in use
- Stop any other React apps running
- Or close the existing command window running npm

### Build fails
- Right-click solution → Clean Solution
- Right-click solution → Rebuild Solution

## 📁 Files Created for This Setup

- `DEBUGGING.md` - Detailed documentation
- `start-react-dev.ps1` - PowerShell script to launch React
- `trade-viewer-ui/start-dev-server.bat` - Batch file to start React
- `TradeViewer.API/StartReactDevServer.targets` - MSBuild integration
- Updated: `TradeViewer.API/Properties/launchSettings.json` - Added new profile

## 🎯 First Time Setup

If this is your first time:

```powershell
cd trade-viewer-ui
npm install
```

Then press F5 in Visual Studio!
