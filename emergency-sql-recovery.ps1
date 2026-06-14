# Emergency SQL Server Container Creation
# Downloads SQL Server image files manually and creates container

Write-Host "=== Emergency SQL Server Recovery ===" -ForegroundColor Cyan
Write-Host "Attempting alternative image download method..." -ForegroundColor Yellow

# Try using Docker's skopeo/umoci tools to manually fetch the image
docker run --rm quay.io/skopeo/stable copy docker://mcr.microsoft.com/mssql/server:2019-latest docker-archive:mssql-2019.tar

if ($LASTEXITCODE -eq 0) {
	Write-Host "Image downloaded successfully" -ForegroundColor Green
	docker load -i mssql-2019.tar

	Write-Host "Creating SQL Server container..." -ForegroundColor Yellow
	docker run -d `
		--name sql-server `
		-e "ACCEPT_EULA=Y" `
		-e "SA_PASSWORD=Gogogo123!" `
		-p 1433:1433 `
		mcr.microsoft.com/mssql/server:2019-latest

	Write-Host "SQL Server is running on localhost:1433" -ForegroundColor Green
} else {
	Write-Host "Cannot download SQL Server image" -ForegroundColor Red
	Write-Host "MCR is completely blocked on your network" -ForegroundColor Red
}
