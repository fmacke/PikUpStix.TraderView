# Trade Viewer Launcher
# This script starts both the API and React frontend

Write-Host "🚀 Starting Trade Viewer..." -ForegroundColor Green
Write-Host ""

# Check if .NET is installed
Write-Host "Checking prerequisites..." -ForegroundColor Yellow
$dotnetVersion = dotnet --version 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ .NET SDK not found. Please install .NET 9 SDK" -ForegroundColor Red
    exit 1
}
Write-Host "✅ .NET SDK $dotnetVersion found" -ForegroundColor Green

# Check if Node is installed
$nodeVersion = node --version 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Node.js not found. Please install Node.js 16+" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Node.js $nodeVersion found" -ForegroundColor Green
Write-Host ""

# Start API in a new window
Write-Host "🔧 Starting TradeViewer.API..." -ForegroundColor Cyan
$apiPath = Join-Path $PSScriptRoot "TradeViewer.API"
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$apiPath'; Write-Host '🔧 Starting API...' -ForegroundColor Cyan; dotnet run"

Write-Host "⏳ Waiting for API to start..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

# Start React app in a new window
Write-Host "⚛️  Starting React Frontend..." -ForegroundColor Cyan
$reactPath = Join-Path $PSScriptRoot "trade-viewer-ui"

# Check if node_modules exists
if (-not (Test-Path (Join-Path $reactPath "node_modules"))) {
    Write-Host "📦 Installing npm packages (first time only)..." -ForegroundColor Yellow
    $installProcess = Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$reactPath'; npm install; Write-Host '✅ Packages installed. Starting app...' -ForegroundColor Green; npm start" -PassThru
} else {
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$reactPath'; Write-Host '⚛️  Starting React app...' -ForegroundColor Cyan; npm start"
}

Write-Host ""
Write-Host "✅ Trade Viewer is starting!" -ForegroundColor Green
Write-Host ""
Write-Host "📍 API: https://localhost:7001/swagger" -ForegroundColor White
Write-Host "📍 UI:  http://localhost:3000" -ForegroundColor White
Write-Host ""
Write-Host "Use ← → arrow keys to navigate between trades" -ForegroundColor Yellow
Write-Host ""
Write-Host "Press any key to exit this window..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
