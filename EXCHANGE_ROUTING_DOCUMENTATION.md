# Exchange-Based Market Data Routing

## Overview
The `ReportRunnerService` now automatically routes market data requests to the appropriate service based on the stock's listing exchange:
- **American Exchanges** (NYSE, NASDAQ, etc.) → Financial Modeling Prep (FMP)
- **International Exchanges** (LSE, etc.) → Yahoo Finance

## Changes Made

### File: `ReportRunnerService.cs`

#### 1. Added Service Dependencies
```csharp
private readonly YahooFinanceService _yahooFinanceService;
private readonly FinancialModellingPrepService _fmpService;
```

Both services are now injected into the constructor, allowing the service to route requests intelligently.

#### 2. Updated Constructor
```csharp
public ReportRunnerService(
	// ... existing parameters ...
	YahooFinanceService yahooFinanceService,
	FinancialModellingPrepService fmpService,
	IConfiguration config)
```

#### 3. Intelligent Exchange Routing
The `RunReportAsync` method now:
1. Groups trades by exchange type (American vs International)
2. Routes American exchange trades to FMP
3. Routes international exchange trades to Yahoo Finance

### Supported American Exchanges
- NYSE (New York Stock Exchange)
- NASDAQ
- AMEX (American Stock Exchange)
- ARCA (NYSE Arca)
- BATS
- IEX (Investors Exchange)
- NYSEARCA (NYSE Arca)
- NYSEMKT (NYSE American)

### How It Works

```csharp
// Example: Trade history has mixed exchanges
var trades = [
	{ Symbol: "AAPL", ListingExchange: "NASDAQ" },    // → FMP
	{ Symbol: "VOD", ListingExchange: "LSE" },        // → Yahoo Finance
	{ Symbol: "MSFT", ListingExchange: "NYSE" },      // → FMP
	{ Symbol: "BP", ListingExchange: "LSE" }          // → Yahoo Finance
];

// Automatically splits and routes:
// FMP gets: AAPL (NASDAQ), MSFT (NYSE)
// Yahoo gets: VOD (LSE), BP (LSE)
```

## Benefits

### 1. **Optimal Data Source**
- American stocks get data from FMP (better coverage for US markets)
- International stocks get data from Yahoo Finance (broader global coverage)

### 2. **Automatic Detection**
- No manual configuration needed
- System automatically detects exchange from instrument data
- Works with existing trade history

### 3. **Fallback Logic**
- If `ListingExchange` is null or empty, defaults to international (Yahoo)
- Case-insensitive exchange matching
- Robust error handling

### 4. **Console Feedback**
```
Fetching 15 American exchange trades using Financial Modeling Prep...
Fetching 8 international exchange trades using Yahoo Finance...
```

## Configuration

### Required Services
Both services must be registered in your DI container. If using the console application, this is already configured in `Program.cs`:

```csharp
services.AddSingleton<YahooFinanceService>(...);
services.AddSingleton<FinancialModellingPrepService>(...);
```

### Environment Variables (Docker)
```env
# For American exchanges (FMP)
FMP_API_KEY=your_fmp_api_key

# For international exchanges (Yahoo)
YAHOO_FINANCE_BASE_URL=https://query1.finance.yahoo.com
```

## Exchange Detection Details

### Detection Logic
```csharp
bool isAmericanExchange = !string.IsNullOrEmpty(trade.ListingExchange) && 
						  americanExchanges.Contains(trade.ListingExchange);
```

### Adding More Exchanges

To add more American exchanges, modify the `americanExchanges` HashSet:

```csharp
var americanExchanges = new HashSet<string>(StringComparer.OrdinalIgnoreCase) 
{ 
	"NYSE", "NASDAQ", "AMEX", "ARCA", "BATS", "IEX", "NYSEARCA", "NYSEMKT",
	"YOUR_NEW_EXCHANGE" // Add here
};
```

### International Exchange Examples
Any exchange not in the American list will use Yahoo Finance:
- **LSE** - London Stock Exchange
- **TSE** - Tokyo Stock Exchange
- **HKEX** - Hong Kong Exchange
- **FWB** - Frankfurt Stock Exchange
- **XETRA** - Deutsche Börse
- **TSX** - Toronto Stock Exchange
- **ASX** - Australian Securities Exchange

## Index and Other Instruments

The index symbols (like ^GSPC, ^VIX) continue to use the configured default `marketDataService`:

```csharp
await marketDataService.FetchAndSaveChartData(new List<string>()
{
	"^GSPC",  // S&P 500
	"^RUT",   // Russell 2000
	"BTCUSD", // Bitcoin
	"GCUSD",  // Gold
	"XAGUSD", // Silver
	"QQQ",    // Nasdaq ETF
	"^VIX"    // Volatility Index
}, 300);
```

These use whichever service is set in `MarketData:PreferredService` configuration.

## Troubleshooting

### Issue: All trades going to Yahoo Finance
**Cause**: `ListingExchange` field may be null or empty
**Solution**: Check that IBKR data includes exchange information

### Issue: Exchange not recognized as American
**Cause**: Exchange name doesn't match the predefined list
**Solution**: Add the exchange to the `americanExchanges` HashSet (case-insensitive)

### Issue: Service not injected
**Cause**: Missing service registration in DI container
**Solution**: Ensure both services are registered in `Program.cs` or `Startup.cs`

## Performance Notes

- **Parallel Fetching**: Trades are fetched sequentially per service (American batch, then International batch)
- **API Rate Limits**: Each service respects its own rate limits independently
- **Error Handling**: Failures in one service don't affect the other

## Example Output

```
Retrieved 50 trade executions.
Fetching 35 American exchange trades using Financial Modeling Prep...
Retrieved 200 rows of chart data for AAPL.
Retrieved 180 rows of chart data for MSFT.
...
Fetching 15 international exchange trades using Yahoo Finance...
Retrieved 200 rows of chart data for VOD.
Retrieved 180 rows of chart data for BP.
...
```

## Future Enhancements

Potential improvements:
1. **Configurable Exchange Lists**: Load American exchanges from configuration
2. **Exchange-Specific Providers**: Allow custom provider mapping per exchange
3. **Parallel Fetching**: Fetch American and International trades concurrently
4. **Cache Exchange Mapping**: Cache exchange detection for performance
5. **Custom Exchange Rules**: Support regex or pattern matching for exchanges

## Related Documentation

- [Yahoo Finance Service](../IKBR_Report_Puller/Services/MarketData/YahooFinanceService_README.md)
- [Market Data Configuration](../IKBR_Report_Puller.Console/MARKET_DATA_CONFIGURATION.md)
- [Docker Configuration](../DOCKER_MARKET_DATA_CONFIGURATION.md)
