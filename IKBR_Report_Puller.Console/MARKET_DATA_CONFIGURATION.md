# Market Data Service Configuration Guide

## Overview
The console application now supports both **Yahoo Finance** and **Financial Modeling Prep** as market data sources. You can switch between them using configuration.

## Configuration Setup

### Option 1: Using appsettings.json

1. Edit `IKBR_Report_Puller.Console\appsettings.json`
2. Set the `PreferredService` to either `"yahoo"` or `"fmp"`:

```json
{
  "MarketData": {
	"PreferredService": "yahoo"
  },
  "YahooFinance": {
	"BaseUrl": "https://query1.finance.yahoo.com",
	"OutputFilePath": "C:\\temp\\MarketData"
  },
  "FinancialModelingPrep": {
	"ApiKey": "your_api_key_here",
	"BaseUrl": "https://financialmodelingprep.com/api/v3",
	"OutputFilePath": "C:\\temp\\MarketData"
  }
}
```

### Option 2: Using User Secrets (Recommended for sensitive data)

For development, use User Secrets to avoid committing API keys:

```bash
# Navigate to the console project directory
cd IKBR_Report_Puller.Console

# Set the preferred service
dotnet user-secrets set "MarketData:PreferredService" "yahoo"

# Configure Yahoo Finance
dotnet user-secrets set "YahooFinance:BaseUrl" "https://query1.finance.yahoo.com"
dotnet user-secrets set "YahooFinance:OutputFilePath" "C:\temp\MarketData"

# Configure Financial Modeling Prep (if using)
dotnet user-secrets set "FinancialModelingPrep:ApiKey" "your_api_key_here"
dotnet user-secrets set "FinancialModelingPrep:BaseUrl" "https://financialmodelingprep.com/api/v3"
dotnet user-secrets set "FinancialModelingPrep:OutputFilePath" "C:\temp\MarketData"
```

## Service Selection

### Automatic Selection (Default)
The application will use the service specified in `MarketData:PreferredService`:
- `"yahoo"` - Use Yahoo Finance
- `"fmp"` - Use Financial Modeling Prep
- Not set or other value - Defaults to Financial Modeling Prep (for backwards compatibility)

### Manual Service Selection
You can also inject specific services in your code:

```csharp
// In Application.cs or any service that needs market data

// Use Yahoo Finance specifically
public class MyService
{
	private readonly YahooFinanceService _yahooService;

	public MyService(YahooFinanceService yahooService)
	{
		_yahooService = yahooService;
	}
}

// Use Financial Modeling Prep specifically
public class MyService
{
	private readonly FinancialModellingPrepService _fmpService;

	public MyService(FinancialModellingPrepService fmpService)
	{
		_fmpService = fmpService;
	}
}

// Use the configured default (based on PreferredService)
public class MyService
{
	private readonly IMarketDataService _marketDataService;

	public MyService(IMarketDataService marketDataService)
	{
		_marketDataService = marketDataService;
	}
}
```

## Service Comparison

| Feature | Yahoo Finance | Financial Modeling Prep |
|---------|--------------|-------------------------|
| **API Key Required** | ❌ No | ✅ Yes |
| **Historical Data** | ✅ Yes | ✅ Yes |
| **Economic Calendar** | ❌ No | ✅ Yes |
| **Cost** | Free | Free tier + Paid plans |
| **Rate Limits** | Informal (may vary) | Depends on plan |
| **Data Quality** | Good | Good |
| **Delay** | Real-time/15min delay | Varies by plan |

## Switching Between Services

### To switch to Yahoo Finance:
```json
{
  "MarketData": {
	"PreferredService": "yahoo"
  }
}
```

### To switch to Financial Modeling Prep:
```json
{
  "MarketData": {
	"PreferredService": "fmp"
  }
}
```

## Configuration Parameters

### Yahoo Finance
- **BaseUrl**: API endpoint (default: `https://query1.finance.yahoo.com`)
  - Alternative: `https://query2.finance.yahoo.com`
- **OutputFilePath**: Where to save downloaded data files

### Financial Modeling Prep
- **ApiKey**: Your FMP API key (required)
- **BaseUrl**: API endpoint (default: `https://financialmodelingprep.com/api/v3`)
- **OutputFilePath**: Where to save downloaded data files

## Important Notes

1. **Yahoo Finance Economic Calendar**: Yahoo Finance doesn't provide economic calendar data through its API. If you need this feature, use Financial Modeling Prep or implement a web scraping solution.

2. **Symbol Formats**: 
   - Yahoo Finance: "AAPL", "^GSPC", "EURUSD=X"
   - FMP: "AAPL", "GSPC", "EURUSD"

3. **Rate Limits**: 
   - Yahoo Finance has informal rate limits; consider adding delays between requests
   - FMP has explicit rate limits based on your subscription plan

4. **Default Service**: If `PreferredService` is not set, the application defaults to Financial Modeling Prep for backwards compatibility.

## Testing Your Configuration

Run the application and check the console output to see which service is being used:

```
Yahoo Finance does not provide economic calendar data...
```
or
```
Fetching economic calendar data from 2024-01-01 to 2024-12-31...
```

You can also check the `SourceName` property to verify which service is active:
```csharp
Console.WriteLine($"Using market data service: {marketDataService.SourceName}");
// Output: "Using market data service: YahooFinance"
// or:     "Using market data service: FinancialModellingPrep"
```

## Troubleshooting

### Issue: Service not switching
- Check that `PreferredService` is spelled correctly (lowercase)
- Verify User Secrets are properly set
- Restart the application after changing configuration

### Issue: Yahoo Finance not working
- Check internet connectivity
- Verify the symbol format is correct for Yahoo Finance
- Try the alternative base URL: `https://query2.finance.yahoo.com`

### Issue: FMP not working
- Verify your API key is correct
- Check your FMP account for rate limit status
- Ensure the API key is not expired

## Environment Variables

You can also use environment variables:
```bash
set MarketData__PreferredService=yahoo
set YahooFinance__BaseUrl=https://query1.finance.yahoo.com
```

Note: Use double underscores (`__`) to represent the colon (`:`) in configuration paths.
