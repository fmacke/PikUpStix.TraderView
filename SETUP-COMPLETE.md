# 🎯 TradeViewer Debug Setup - Complete

## ✅ What Was Configured

Your TradeViewer solution is now configured to automatically launch both the **API backend** and **React frontend** when you debug in Visual Studio.

### Files Created/Modified

1. **`TradeViewer.API/Properties/launchSettings.json`** - Added new "API + React UI" profile
2. **`TradeViewer.API/StartReactDevServer.targets`** - MSBuild target to auto-start React
3. **`TradeViewer.API/TradeViewer.API.csproj`** - Imports the targets file
4. **`start-react-dev.ps1`** - PowerShell script to launch React dev server
5. **`trade-viewer-ui/start-dev-server.bat`** - Batch file that starts npm
6. **`trade-viewer-ui/src/services/apiService.ts`** - Updated to use proxy-friendly URL
7. **`TradeViewer.API/Program.cs`** - HTTPS redirection only in production

### Documentation Created

- **`QUICK-START.md`** - Quick reference guide
- **`DEBUGGING.md`** - Detailed documentation
- **`SETUP-COMPLETE.md`** - This file

## 🚀 How to Use

### Method 1: Visual Studio Launch Profile (Recommended)

1. In Visual Studio, find the **debug dropdown** (next to the green play button)
2. Select **"API + React UI"**
3. Press **F5** or click **Start Debugging**
4. Wait ~15 seconds for React to compile
5. Browser opens to `http://localhost:3000`

### Method 2: Manual Start

**Terminal:**
```powershell
cd trade-viewer-ui
npm start
```

**Visual Studio:**
- Press F5 to debug the API

## 📋 What Happens When You Press F5

1. **MSBuild** runs the `StartReactDevServer` target
2. **PowerShell script** (`start-react-dev.ps1`) checks if React is already running
3. If not running, **starts React dev server** in a new window
4. **TradeViewer.API** starts on `https://localhost:7001`
5. **Browser opens** to `http://localhost:3000`
6. **React proxy** forwards `/api/*` requests to the API

## 🌐 URLs

- **Frontend (React)**: `http://localhost:3000`
- **Backend (API)**: `https://localhost:7001` or `http://localhost:5001`
- **Swagger**: `https://localhost:7001/swagger`

## 🔧 Architecture

```
Browser (http://localhost:3000)
	↓
React Dev Server (with proxy)
	↓
/api/* → https://localhost:7001
	↓
TradeViewer.API
	↓
SQL Server Database
```

## ✅ Pre-Flight Checklist

Before your first debug session:

- [ ] Node.js and npm installed
- [ ] Run `npm install` in `trade-viewer-ui` folder
- [ ] SQL Server running with TradingBE database
- [ ] Connection string in `appsettings.json` is correct

## 🛑 How to Stop

- **API**: Press `Shift+F5` in Visual Studio
- **React**: Close the command window or `Ctrl+C`

## 🐛 Troubleshooting

### React doesn't start automatically

**Cause**: npm not in PATH or node_modules not installed

**Fix**:
```powershell
cd trade-viewer-ui
npm install
```

### "Failed to load trades" in browser

**Check**:
1. API is running (look for "Now listening on..." in Output window)
2. Browser console (F12) shows what error
3. Database connection is working

**Quick fix**:
- Restart both servers
- Check `setupProxy.js` is forwarding to correct port

### Port conflicts

**If port 3000 is in use**:
- Close other React apps
- Or kill process: `Stop-Process -Id (Get-NetTCPConnection -LocalPort 3000).OwningProcess`

**If port 7001 is in use**:
- Change in `launchSettings.json`
- Also update `setupProxy.js` target

## 🎓 Understanding the Setup

### Why the proxy?

The React dev server runs on port 3000, the API on port 7001. Without a proxy, the browser would block cross-origin requests (CORS). The proxy makes the API appear to be on the same origin.

### Why the MSBuild target?

Visual Studio doesn't natively support launching non-.NET projects. The MSBuild target runs before building, checking if React needs to be started.

### Why the batch file?

PowerShell can launch processes, but the batch file keeps the npm output visible in its own window for debugging.

## 📚 Next Steps

1. **Try it out**: Press F5 and see both servers start
2. **Explore the code**: Look at how the proxy routes requests
3. **Debug**: Set breakpoints in both frontend (browser DevTools) and backend (Visual Studio)

## 💡 Tips

- **Hot Reload**: Both React and .NET support hot reload - code changes appear automatically
- **Multiple browsers**: You can open multiple tabs to `localhost:3000`
- **Swagger**: Use `https://localhost:7001/swagger` to test API endpoints directly
- **React DevTools**: Install the browser extension for better React debugging

## 📞 Support

If you encounter issues:

1. Check the Output window in Visual Studio
2. Check the browser console (F12)
3. Check the React terminal window
4. Review `DEBUGGING.md` for detailed troubleshooting

---

**Setup completed successfully!** 🎉

Press F5 and enjoy seamless full-stack debugging!
