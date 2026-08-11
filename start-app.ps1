# Quick Start Script - TraderView Application

Write-Host @"

+------------------------------------------------------------+
¦       TraderView - Quick Start                             ¦
+------------------------------------------------------------+

"@ -ForegroundColor Cyan

Write-Host "This script will set up and start your entire application." -ForegroundColor White
Write-Host ""

# Step 1: Check Docker
Write-Host "Step 1: Checking Docker..." -ForegroundColor Yellow
try {
    $null = docker version 2>&1
    if ($LASTEXITCODE -ne 0) { throw "Docker daemon is not running." }
    Write-Host "? Docker is running" -ForegroundColor Green
} catch {
    Write-Host "? Docker is not running!" -ForegroundColor Red
    Write-Host "  Please start Docker Desktop and run this script again." -ForegroundColor Yellow
    exit 1
}
Write-Host ""

# Step 2: Generate .env from User Secrets
Write-Host "Step 2: Generating .env from User Secrets..." -ForegroundColor Yellow
& .\generate-env.ps1
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Failed to generate .env file!" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Step 3: Check Database
Write-Host "Step 3: Checking database..." -ForegroundColor Yellow
$dbExists = docker ps -a --filter "name=traderview-sqlserver" --format "{{.Names}}" | Select-String "traderview-sqlserver"

if (-not $dbExists) {
    Write-Host "Database not found. Setting up..." -ForegroundColor Yellow
    Write-Host ""
    & .\setup-database.ps1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "? Database setup failed!" -ForegroundColor Red
        exit 1
    }
} else {
    $dbRunning = docker ps --filter "name=traderview-sqlserver" --format "{{.Names}}" | Select-String "traderview-sqlserver"
    if ($dbRunning) {
        Write-Host "? Database is already running" -ForegroundColor Green
    } else {
        Write-Host "Starting database..." -ForegroundColor Yellow
        docker compose up -d sqlserver
        Start-Sleep -Seconds 5
        Write-Host "? Database started" -ForegroundColor Green
    }
}
Write-Host ""

# Step 4: Start Application
Write-Host "Step 4: Starting application services..." -ForegroundColor Yellow
docker compose up -d

if ($LASTEXITCODE -eq 0) {
    Write-Host "? Services started successfully!" -ForegroundColor Green
} else {
    Write-Host "? Failed to start services" -ForegroundColor Red
    Write-Host "Run 'docker compose logs' to see errors" -ForegroundColor Yellow
    exit 1
}
Write-Host ""

# Step 5: Wait & Check Status
Write-Host "Waiting for services to initialize..." -ForegroundColor Yellow
Start-Sleep -Seconds 3
Write-Host ""

Write-Host "Step 5: Checking service status..." -ForegroundColor Yellow
docker compose ps
Write-Host ""

Write-Host @"
+------------------------------------------------------------+
¦       Setup Complete! ??                                   ¦
+------------------------------------------------------------+

"@ -ForegroundColor Green

Write-Host "Your application is now running!" -ForegroundColor White
Write-Host ""
Write-Host "Access Points:" -ForegroundColor Cyan
Write-Host "  • Frontend:  http://localhost:3000" -ForegroundColor White
Write-Host "  • Backend:   http://localhost:5000" -ForegroundColor White
Write-Host "  • Swagger:   http://localhost:5000/swagger" -ForegroundColor White
Write-Host "  • Database:  localhost,1433" -ForegroundColor White
Write-Host ""
Write-Host "Useful Commands:" -ForegroundColor Cyan
Write-Host "  • View logs:       docker compose logs -f" -ForegroundColor White
Write-Host "  • Stop services:   docker compose stop" -ForegroundColor White
Write-Host "  • Restart:         docker compose restart" -ForegroundColor White
Write-Host "  • Full shutdown:   docker compose down" -ForegroundColor White
Write-Host ""
Write-Host "To see logs now, run: " -NoNewline -ForegroundColor Yellow
Write-Host "docker compose logs -f" -ForegroundColor White
Write-Host ""
