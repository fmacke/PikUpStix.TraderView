# PowerShell script to setup and initialize the TraderView database in Docker

Write-Host "TraderView Database Setup Script" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan
Write-Host ""

# Check if Docker is running
Write-Host "Checking Docker status..." -ForegroundColor Yellow
try {
    docker version | Out-Null
    Write-Host "✓ Docker is running" -ForegroundColor Green
} catch {
    Write-Host "✗ Docker is not running. Please start Docker Desktop and try again." -ForegroundColor Red
    exit 1
}

# Generate .env from User Secrets if it doesn't exist
if (-not (Test-Path ".env")) {
    Write-Host ""
    Write-Host "Generating .env file from User Secrets..." -ForegroundColor Yellow
    & .\generate-env.ps1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ Failed to generate .env file!" -ForegroundColor Red
        exit 1
    }
}

# Read the SQL password from User Secrets
$sqlPassword = "booof"  # Default password

# Try all User Secrets locations
$userSecretsIds = @(
    "ff3a21ae-059b-4f53-8589-8b474f460b89",  # traderview.Server
    "16bb43dd-74cd-4a0f-bb7f-cf97fda4b833",  # Console
    "c681a7e8-a2d6-4ed7-9098-379bcf579200"   # Library
)

foreach ($secretId in $userSecretsIds) {
    $userSecretsPath = "$env:APPDATA\Microsoft\UserSecrets\$secretId\secrets.json"
    
    if (Test-Path $userSecretsPath) {
        try {
            $secrets = Get-Content $userSecretsPath -Raw | ConvertFrom-Json
            
            # Use PSObject to safely access the key containing a colon
            $passwordProp = $secrets.PSObject.Properties['Database:Password']
            $password = if ($passwordProp) { $passwordProp.Value } else { $null }

            if (-not [string]::IsNullOrWhiteSpace($password)) {
                $sqlPassword = $password
                Write-Host "`n✓ Found password in User Secrets ($secretId)" -ForegroundColor Cyan
                break
            }
        } catch {
            Write-Warning "Failed to parse ${userSecretsPath}: $_"
        }
    }
}

# Then check .env file (overrides User Secrets if present)
# Pass it directly to Docker Compose environment
$env:SQL_PASSWORD = $sqlPassword
docker-compose up -d sqlserver

# Start the SQL Server container
Write-Host ""
Write-Host "Starting SQL Server container..." -ForegroundColor Yellow
docker-compose up -d sqlserver

Write-Host ""
Write-Host "Waiting for SQL Server to be ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Wait for SQL Server to be healthy
$maxAttempts = 30
$attempt = 0
$isHealthy = $false

while ($attempt -lt $maxAttempts -and -not $isHealthy) {
    $attempt++
    Write-Host "Checking SQL Server health (attempt $attempt/$maxAttempts)..." -ForegroundColor Yellow

    $containerHealth = docker inspect --format='{{.State.Health.Status}}' traderview-sqlserver 2>$null

    if ($containerHealth -eq "healthy") {
        $isHealthy = $true
        Write-Host "✓ SQL Server is healthy!" -ForegroundColor Green
    } else {
        Start-Sleep -Seconds 2
    }
}

if (-not $isHealthy) {
    Write-Host "✗ SQL Server did not become healthy in time. Check Docker logs:" -ForegroundColor Red
    Write-Host "  docker logs traderview-sqlserver" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "Running database creation script..." -ForegroundColor Yellow

# Copy and execute the database creation script
docker cp DatabaseCreationScript.sql traderview-sqlserver:/tmp/DatabaseCreationScript.sql

docker exec traderview-sqlserver /opt/mssql-tools18/bin/sqlcmd `
    -S localhost `
    -U sa `
    -P "$sqlPassword" `
    -C `
    -i /tmp/DatabaseCreationScript.sql

if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Database schema created successfully!" -ForegroundColor Green
} else {
    Write-Host "✗ Error creating database schema. Check the output above." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=================================" -ForegroundColor Cyan
Write-Host "Database Setup Complete!" -ForegroundColor Green
Write-Host "=================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Connection Details:" -ForegroundColor Yellow
Write-Host "  Server: localhost,1433" -ForegroundColor White
Write-Host "  Database: TradingBE" -ForegroundColor White
Write-Host "  Username: sa" -ForegroundColor White
Write-Host "  Password: $sqlPassword" -ForegroundColor White
Write-Host ""
Write-Host "Connection String:" -ForegroundColor Yellow
Write-Host "  Server=localhost,1433;Database=TradingBE;User Id=sa;Password=$sqlPassword;TrustServerCertificate=true;Encrypt=false;" -ForegroundColor White
Write-Host ""
Write-Host "Your application will automatically use these credentials from User Secrets!" -ForegroundColor Cyan
Write-Host ""
Write-Host "To stop the database:" -ForegroundColor Yellow
Write-Host "  docker-compose down" -ForegroundColor White
Write-Host ""
Write-Host "To view database logs:" -ForegroundColor Yellow
Write-Host "  docker logs traderview-sqlserver" -ForegroundColor White
Write-Host ""
