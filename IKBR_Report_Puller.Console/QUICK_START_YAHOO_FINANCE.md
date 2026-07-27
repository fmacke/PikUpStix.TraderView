# Quick Start Guide - Yahoo Finance Integration

## What Was Added

✅ **YahooFinanceService** - A new market data service that uses Yahoo Finance API
✅ **Flexible Configuration** - Switch between Yahoo Finance and Financial Modeling Prep
✅ **Both Services Available** - Both services are registered and can be used simultaneously

## Changes Made

### 1. Program.cs Updates
- Both services (`YahooFinanceService` and `FinancialModellingPrepService`) are now registered
- Added configuration-based service selection via `MarketData:PreferredService`
- Default service is Financial Modeling Prep (backwards compatible)

### 2. New Files Created
- `YahooFinanceService.cs` - The Yahoo Finance implementation
- `appsettings.json` - Configuration template
- Configuration documentation

## Quick Setup

### Step 1: Choose Your Service

Edit your configuration (User Secrets or appsettings.json):

**For Yahoo Finance:**
```json
{
  "MarketData": {
	"PreferredService": "yahoo"
  },
  "YahooFinance": {
	"BaseUrl": "https://query1.finance.yahoo.com",
	"OutputFilePath": "C:\\temp\\MarketData"
  }
}
```

**For Financial Modeling Prep:**
```json
{
  "MarketData": {
	"PreferredService": "fmp"
  },
  "FinancialModelingPrep": {
	"ApiKey": "your_api_key",
	"BaseUrl": "https://financialmodelingprep.com/api/v3",
	"OutputFilePath": "C:\\temp\\MarketData"
  }
}
```

### Step 2: Build and Run

```bash
cd IKBR_Report_Puller.Console
dotnet build
dotnet run
```

## Using User Secrets (Recommended)

```bash
# Navigate to console project
cd IKBR_Report_Puller.Console

# Set to use Yahoo Finance
dotnet user-secrets set "MarketData:PreferredService" "yahoo"
dotnet user-secrets set "YahooFinance:BaseUrl" "https://query1.finance.yahoo.com"
dotnet user-secrets set "YahooFinance:OutputFilePath" "C:\temp\MarketData"

# OR set to use Financial Modeling Prep
dotnet user-secrets set "MarketData:PreferredService" "fmp"
dotnet user-secrets set "FinancialModelingPrep:ApiKey" "YOUR_API_KEY"
dotnet user-secrets set "FinancialModelingPrep:BaseUrl" "https://financialmodelingprep.com/api/v3"
dotnet user-secrets set "FinancialModelingPrep:OutputFilePath" "C:\temp\MarketData"
```

## Testing the Integration

### Check Which Service Is Active

Look for console output when the app runs:
- Yahoo Finance: `"Using market data service: YahooFinance"`
- FMP: `"Using market data service: FinancialModellingPrep"`

### Test Data Fetching

The service will automatically be used when:
- Fetching chart data for trades
- Fetching chart data for symbols
- Fetching economic calendar (FMP only)

## Dependency Injection Options

### Option 1: Use the Configured Default
```csharp
public class MyClass
{
	private readonly IMarketDataService _marketDataService;

	public MyClass(IMarketDataService marketDataService)
	{
		_marketDataService = marketDataService;
	}
}
```

### Option 2: Use Yahoo Finance Directly
```csharp
public class MyClass
{
	private readonly YahooFinanceService _yahooService;

	public MyClass(YahooFinanceService yahooService)
	{
		_yahooService = yahooService;
	}
}
```

### Option 3: Use Financial Modeling Prep Directly
```csharp
public class MyClass
{
	private readonly FinancialModellingPrepService _fmpService;

	public MyClass(FinancialModellingPrepService fmpService)
	{
		_fmpService = fmpService;
	}
}
```

### Option 4: Use Both Services
```csharp
public class MyClass
{
	private readonly YahooFinanceService _yahooService;
	private readonly FinancialModellingPrepService _fmpService;

	public MyClass(YahooFinanceService yahooService, FinancialModellingPrepService fmpService)
	{
		_yahooService = yahooService;
		_fmpService = fmpService;
	}

	public async Task FetchData()
	{
		// Use Yahoo for stock data
		await _yahooService.FetchAndSaveChartData(stockSymbols, 365);

		// Use FMP for economic calendar
		await _fmpService.FetchAndSaveEconomicCalendarAsync(fromDate, toDate);
	}
}
```

## Key Differences

| Feature | Yahoo Finance | Financial Modeling Prep |
|---------|--------------|-------------------------|
| API Key | Not Required | Required |
| Economic Calendar | ❌ Not Available | ✅ Available |
| Historical Prices | ✅ Available | ✅ Available |
| Cost | Free | Free tier + Paid |
| Rate Limits | Informal | Plan-based |

## Symbol Formats

### Yahoo Finance
- Stocks: `AAPL`, `MSFT`, `GOOGL`
- Indices: `^GSPC`, `^DJI`, `^IXIC`
- Forex: `EURUSD=X`, `GBPUSD=X`
- Crypto: `BTC-USD`, `ETH-USD`

### Financial Modeling Prep
- Stocks: `AAPL`, `MSFT`, `GOOGL`
- Indices: `GSPC`, `DJI`, `IXIC` (no ^ prefix)
- Forex: `EURUSD`, `GBPUSD`

## Next Steps

1. **Configure Your Preferred Service** - Update your configuration
2. **Build the Solution** - Run `dotnet build`
3. **Test the Integration** - Run the console app and verify the correct service is used
4. **Review the Documentation** - Check `MARKET_DATA_CONFIGURATION.md` for detailed configuration options

## Troubleshooting

If the service isn't switching:
1. Verify `MarketData:PreferredService` is set correctly
2. Check that User Secrets or appsettings.json contains the configuration
3. Restart the application after configuration changes
4. Check console output for which service is being initialized

## Need Help?

- Full configuration guide: `MARKET_DATA_CONFIGURATION.md`
- Service implementation: `YahooFinanceService.cs`
- Original implementation: `FinancialModellingPrepService.cs`
