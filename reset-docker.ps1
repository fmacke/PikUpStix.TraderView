# Docker Desktop Manual Reset Script
# This will reset Docker Desktop configuration while preserving your database backup

Write-Host "=== Docker Desktop Reset Script ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Your database backup is safe at:" -ForegroundColor Green
Write-Host "C:\Users\finn\source\repos\IKBR_Report_Puller\db_backup\TradingBE.bak"
Write-Host ""

# Step 1: Stop Docker Desktop
Write-Host "Step 1: Stopping Docker Desktop..." -ForegroundColor Yellow
Stop-Process -Name "Docker Desktop" -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 5

# Step 2: Stop WSL
Write-Host "Step 2: Stopping WSL..." -ForegroundColor Yellow
wsl --shutdown
Start-Sleep -Seconds 3

# Step 3: Backup current Docker settings (just in case)
Write-Host "Step 3: Backing up current Docker settings..." -ForegroundColor Yellow
$backupPath = "$env:USERPROFILE\docker-settings-backup-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
New-Item -ItemType Directory -Path $backupPath -Force | Out-Null
Copy-Item "$env:APPDATA\Docker\*" $backupPath -Recurse -ErrorAction SilentlyContinue
Write-Host "Settings backed up to: $backupPath" -ForegroundColor Green

# Step 4: Delete Docker configuration
Write-Host "Step 4: Removing Docker configuration..." -ForegroundColor Yellow
Remove-Item "$env:APPDATA\Docker" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "$env:LOCALAPPDATA\Docker" -Recurse -Force -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "=== Reset Complete! ===" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Start Docker Desktop manually"
Write-Host "2. Wait for it to fully initialize"
Write-Host "3. Run the database restore script"
Write-Host ""
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
