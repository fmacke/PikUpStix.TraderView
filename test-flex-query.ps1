# Quick test script for IBKR Flex Query API
# Run this from the solution root directory

param(
	[switch]$Direct,
	[switch]$App
)

# Load appsettings.json
$appSettings = Get-Content "IKBR_Report_Puller.Console\appsettings.json" | ConvertFrom-Json
$token = $appSettings.IBKR.Token
$queryId = $appSettings.IBKR.QueryId
$queryTodayId = $appSettings.IBKR.QueryTodayExecutionsId
$baseUrl = $appSettings.IBKR.BaseUrl

Write-Host "=== IBKR Flex Query API Test ===" -ForegroundColor Cyan
Write-Host ""

if ($Direct -or !$App) {
	Write-Host "Testing Main Report Query..." -ForegroundColor Yellow
	Write-Host "Token: $token"
	Write-Host "Query ID: $queryId"
	Write-Host "Base URL: $baseUrl"
	Write-Host ""

	$mainUrl = "$baseUrl`?t=$token&q=$queryId&v=3"
	Write-Host "Full URL: $mainUrl" -ForegroundColor Gray
	Write-Host ""

	try {
		$response = Invoke-WebRequest -Uri $mainUrl -Method Get -TimeoutSec 30
		Write-Host "Status: $($response.StatusCode)" -ForegroundColor Green
		Write-Host "Response:" -ForegroundColor Green
		Write-Host $response.Content

		# Parse XML
		[xml]$xml = $response.Content
		$status = $xml.FlexStatementResponse.Status
		$errorCode = $xml.FlexStatementResponse.ErrorCode
		$errorMsg = $xml.FlexStatementResponse.ErrorMessage
		$refCode = $xml.FlexStatementResponse.ReferenceCode

		Write-Host ""
		if ($status -eq "Success") {
			Write-Host "✓ SUCCESS - Reference Code: $refCode" -ForegroundColor Green
		} else {
			Write-Host "✗ FAILED - Error $errorCode`: $errorMsg" -ForegroundColor Red
		}
	}
	catch {
		Write-Host "✗ Request failed: $($_.Exception.Message)" -ForegroundColor Red
	}

	Write-Host ""
	Write-Host "---" -ForegroundColor Gray
	Write-Host ""

	Write-Host "Testing Today Report Query..." -ForegroundColor Yellow
	Write-Host "Query ID: $queryTodayId"
	Write-Host ""

	$todayUrl = "$baseUrl`?t=$token&q=$queryTodayId&v=3"
	Write-Host "Full URL: $todayUrl" -ForegroundColor Gray
	Write-Host ""

	try {
		$response2 = Invoke-WebRequest -Uri $todayUrl -Method Get -TimeoutSec 30
		Write-Host "Status: $($response2.StatusCode)" -ForegroundColor Green
		Write-Host "Response:" -ForegroundColor Green
		Write-Host $response2.Content

		# Parse XML
		[xml]$xml2 = $response2.Content
		$status2 = $xml2.FlexStatementResponse.Status
		$errorCode2 = $xml2.FlexStatementResponse.ErrorCode
		$errorMsg2 = $xml2.FlexStatementResponse.ErrorMessage
		$refCode2 = $xml2.FlexStatementResponse.ReferenceCode

		Write-Host ""
		if ($status2 -eq "Success") {
			Write-Host "✓ SUCCESS - Reference Code: $refCode2" -ForegroundColor Green
		} else {
			Write-Host "✗ FAILED - Error $errorCode2`: $errorMsg2" -ForegroundColor Red
		}
	}
	catch {
		Write-Host "✗ Request failed: $($_.Exception.Message)" -ForegroundColor Red
	}
}

if ($App) {
	Write-Host ""
	Write-Host "Testing via Application..." -ForegroundColor Yellow
	Write-Host ""

	# Run the console app in test mode
	& ".\IKBR_Report_Puller.Console\bin\Debug\net9.0\IKBR_Report_Puller.Console.exe" test
}

Write-Host ""
Write-Host "=== Test Complete ===" -ForegroundColor Cyan
