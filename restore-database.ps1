# SQL Server Database Restore Script
# Run this after Docker Desktop has been reset and restarted

Write-Host "=== Database Restore Script ===" -ForegroundColor Cyan
Write-Host ""

$backupFile = "C:\Users\finn\source\repos\IKBR_Report_Puller\db_backup\TradingBE.bak"

if (-not (Test-Path $backupFile)) {
	Write-Host "ERROR: Backup file not found at $backupFile" -ForegroundColor Red
	exit 1
}

Write-Host "Step 1: Creating SQL Server container..." -ForegroundColor Yellow
docker run -d `
	--name sql-server `
	-e "ACCEPT_EULA=Y" `
	-e "SA_PASSWORD=Gogogo123!" `
	-p 1433:1433 `
	mcr.microsoft.com/mssql/server:2019-latest

Write-Host "Step 2: Waiting for SQL Server to start..." -ForegroundColor Yellow
Start-Sleep -Seconds 15

Write-Host "Step 3: Copying backup file to container..." -ForegroundColor Yellow
docker cp $backupFile sql-server:/var/opt/mssql/backup/TradingBE.bak

Write-Host "Step 4: Restoring database..." -ForegroundColor Yellow
docker exec sql-server /opt/mssql-tools18/bin/sqlcmd `
	-S localhost -U sa -P "Gogogo123!" -C `
	-Q "RESTORE DATABASE [TradingBE] FROM DISK = '/var/opt/mssql/backup/TradingBE.bak' WITH REPLACE, MOVE 'TradingBE' TO '/var/opt/mssql/data/TradingBE.mdf', MOVE 'TradingBE_log' TO '/var/opt/mssql/data/TradingBE_log.ldf'"

Write-Host ""
Write-Host "=== Database Restored Successfully! ===" -ForegroundColor Green
Write-Host ""
Write-Host "SQL Server is now running at localhost:1433"
Write-Host "Database: TradingBE"
Write-Host "Username: sa"
Write-Host "Password: Gogogo123!"
Write-Host ""
