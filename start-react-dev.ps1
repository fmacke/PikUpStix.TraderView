# Start React Development Server for TradeViewer
# This script is automatically called when debugging TradeViewer.API in Visual Studio

$ErrorActionPreference = "SilentlyContinue"
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$reactAppPath = Join-Path $scriptPath "trade-viewer-ui"

Write-Host "=== TradeViewer React Development Server ===" -ForegroundColor Cyan

# Check if npm is installed
$npmVersion = npm --version 2>$null
if (-not $npmVersion) {
	Write-Host "ERROR: npm is not installed or not in PATH" -ForegroundColor Red
	Write-Host "Please install Node.js from https://nodejs.org/" -ForegroundColor Yellow
	exit 1
}

# Check if node_modules exists, if not run npm install
$nodeModulesPath = Join-Path $reactAppPath "node_modules"
if (-not (Test-Path $nodeModulesPath)) {
	Write-Host "node_modules not found. Running npm install..." -ForegroundColor Yellow
	Push-Location $reactAppPath
	npm install
	Pop-Location
}

# Check if the React dev server is already running on port 3000
try {
	$port3000Process = Get-NetTCPConnection -LocalPort 3000 -State Listen -ErrorAction Stop | Select-Object -First 1

	if ($null -ne $port3000Process) {
		Write-Host "React development server is already running on port 3000" -ForegroundColor Green
		Write-Host "UI available at: http://localhost:3000" -ForegroundColor Cyan
		exit 0
	}
} catch {
	# Port is not in use, continue to start the server
}

Write-Host "Starting React development server..." -ForegroundColor Green
Write-Host "Location: $reactAppPath" -ForegroundColor Gray

# Start the React app in a new window
$reactScriptPath = Join-Path $reactAppPath "start-dev-server.bat"

if (Test-Path $reactScriptPath) {
	Start-Process -FilePath $reactScriptPath -WorkingDirectory $reactAppPath
	Write-Host "React development server is starting..." -ForegroundColor Green
	Write-Host "UI will be available at: http://localhost:3000" -ForegroundColor Cyan
	Write-Host "API will be available at: https://localhost:7001" -ForegroundColor Cyan
	Write-Host "" -ForegroundColor White
	Write-Host "Please wait for the React app to compile (this may take 10-30 seconds)..." -ForegroundColor Yellow
} else {
	Write-Host "ERROR: Could not find start-dev-server.bat" -ForegroundColor Red
	Write-Host "Expected location: $reactScriptPath" -ForegroundColor Gray
	exit 1
}

