# Script to Generate .env file from User Secrets
# This reads User Secrets from all three projects and creates a .env file for Docker

Write-Host "Generating .env file from User Secrets..." -ForegroundColor Cyan
Write-Host ""

# User Secrets IDs for the three projects
$userSecretsIds = @{
    "Server"  = "ff3a21ae-059b-4f53-8589-8b474f460b89"
    "Console" = "16bb43dd-74cd-4a0f-bb7f-cf97fda4b833"
    "Library" = "c681a7e8-a2d6-4ed7-9098-379bcf579200"
}

# Try to read secrets from all projects (prefer Server, then Console, then Library)
$secrets = $null
foreach ($project in @("Server", "Console", "Library")) {
    $secretId = $userSecretsIds[$project]
    $userSecretsPath = "$env:APPDATA\Microsoft\UserSecrets\$secretId\secrets.json"
    
    if (Test-Path $userSecretsPath) {
        try {
            $secrets = Get-Content $userSecretsPath | ConvertFrom-Json
            Write-Host "✓ Found User Secrets for $project project" -ForegroundColor Green
            break
        } catch {
            Write-Host "✗ Could not read User Secrets for $project project" -ForegroundColor Yellow
        }
    }
}

if (-not $secrets) {
    Write-Host "✗ Could not find any User Secrets!" -ForegroundColor Red
    Write-Host "  Creating .env with default values..." -ForegroundColor Yellow

    # Create default .env file
    $envContent = @"
# SQL Server Configuration
SQL_PASSWORD=your_password

# IBKR Configuration
IBKR_TOKEN=your_token_here
IBKR_QUERY_ID=your_query_id_here
IBKR_TODAY_EXEC_ID=your_today_exec_id_here

# Financial Modeling Prep API
FMP_API_KEY=your_fmp_api_key_here

# Market Data Service (yahoo or fmp)
MARKET_DATA_SERVICE=fmp

# Yahoo Finance
YAHOO_FINANCE_BASE_URL=https://query1.finance.yahoo.com
"@

    $envContent | Out-File -FilePath ".env" -Encoding UTF8
    Write-Host "✓ Created .env with default values" -ForegroundColor Green
    Write-Host "  Please update User Secrets in Visual Studio with your actual values" -ForegroundColor Yellow
    exit 0
}

# Build .env content from User Secrets
$envLines = @()
$envLines += "# AUTO-GENERATED FROM USER SECRETS - DO NOT EDIT MANUALLY"
$envLines += "# To update: modify User Secrets in Visual Studio, then re-run generate-env.ps1"
$envLines += "# Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
$envLines += ""

# Helper to read flat keys with colons safely
function Get-SecretValue ($jsonObject, [string]$keyName, [string]$defaultValue = "") {
    $prop = $jsonObject.PSObject.Properties[$keyName]
    if ($prop -and -not [string]::IsNullOrWhiteSpace($prop.Value)) {
        return $prop.Value
    }
    return $defaultValue
}

# Database Configuration
$envLines += "# SQL Server Configuration"
$sqlPassword = Get-SecretValue $secrets "Database:Password" "DevPassword123!@#"
$envLines += "SQL_PASSWORD=$sqlPassword"
$envLines += ""

# IBKR Configuration
$envLines += "# IBKR Configuration"
$ibkrToken       = Get-SecretValue $secrets "IBKR:Token" "your_token_here"
$ibkrQueryId     = Get-SecretValue $secrets "IBKR:QueryId" "your_query_id_here"
$ibkrTodayExecId = Get-SecretValue $secrets "IBKR:QueryTodayExecutionsId" "your_today_exec_id_here"

$envLines += "IBKR_TOKEN=$ibkrToken"
$envLines += "IBKR_QUERY_ID=$ibkrQueryId"
$envLines += "IBKR_TODAY_EXEC_ID=$ibkrTodayExecId"
$envLines += ""

# Financial Modeling Prep API
$envLines += "# Financial Modeling Prep API"
$fmpApiKey = Get-SecretValue $secrets "FinancialModelingPrep:ApiKey" "your_fmp_api_key_here"
$envLines += "FMP_API_KEY=$fmpApiKey"
$envLines += ""
$envLines += ""

# Market Data Service
$envLines += "# Market Data Service (yahoo or fmp)"
$envLines += "MARKET_DATA_SERVICE=fmp"
$envLines += ""

# Write to .env file
$envContent = $envLines -join "`r`n"
$envContent | Out-File -FilePath ".env" -Encoding UTF8

Write-Host ""
Write-Host "✓ Successfully generated .env file from User Secrets!" -ForegroundColor Green
Write-Host ""
Write-Host "Configuration Summary:" -ForegroundColor Cyan
Write-Host "  • SQL Password:     $(if ($secrets.Database) { $secrets.Database.Password } else { 'DevPassword123!@#' })" -ForegroundColor White
Write-Host "  • IBKR Token:       $(if ($secrets.IBKR.Token -ne 'your_token_here') { '✓ Configured' } else { '✗ Not set' })" -ForegroundColor $(if ($secrets.IBKR.Token -ne 'your_token_here') { 'Green' } else { 'Yellow' })
Write-Host "  • FMP API Key:      $(if ($secrets.FinancialModelingPrep.ApiKey -ne 'your_fmp_api_key_here') { '✓ Configured' } else { '✗ Not set' })" -ForegroundColor $(if ($secrets.FinancialModelingPrep.ApiKey -ne 'your_fmp_api_key_here') { 'Green' } else { 'Yellow' })
Write-Host ""
Write-Host "The .env file is now ready for Docker!" -ForegroundColor Green
Write-Host "Note: .env is in .gitignore and will not be committed to source control" -ForegroundColor Yellow
Write-Host ""
