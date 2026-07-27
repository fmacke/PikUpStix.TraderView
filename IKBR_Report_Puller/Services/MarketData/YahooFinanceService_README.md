# Yahoo Finance Service Implementation

## Overview
A new `YahooFinanceService` class has been created that implements the `IMarketDataService` interface, allowing you to fetch market data from Yahoo Finance instead of Financial Modeling Prep.

## File Location
`IKBR_Report_Puller\Services\MarketData\YahooFinanceService.cs`

## Key Features

### 1. Historical Chart Data
- Fetches OHLCV (Open, High, Low, Close, Volume) data for any symbol
- Uses Yahoo Finance v8 API endpoints
- Supports daily interval data
- Converts Unix timestamps to DateTime objects
- Handles null values gracefully

### 2. Economic Calendar
- Yahoo Finance doesn't provide a dedicated economic calendar API endpoint
- The method returns an empty list as a placeholder
- You may want to use a different service for economic calendar data or implement web scraping

### 3. Instrument Support
- Works with stocks, ETFs, indices, and other instruments supported by Yahoo Finance
- Automatically creates instrument entries in the database if they don't exist
- Default instrument type is set to "STOCK" but can be customized

## Usage Example

```csharp
// Setup
var httpClient = new HttpClient();
var baseUrl = "https://query1.finance.yahoo.com"; // or query2.finance.yahoo.com

var yahooService = new YahooFinanceService(
	httpClient,
	economicCalendarRepository,
	historicalDataRepository,
	instrumentRepository,
	baseUrl,
	outputFilePath
);

// Fetch chart data for specific symbols
var symbols = new List<string> { "AAPL", "MSFT", "^GSPC" }; // Apple, Microsoft, S&P 500
await yahooService.FetchAndSaveChartData(symbols, lookBackDays: 365);

// Fetch chart data for historical trades
await yahooService.FetchAndSaveChartData(historicalTrades);
```

## Symbol Format

Yahoo Finance uses specific symbol formats:

- **US Stocks**: Use ticker as-is (e.g., "AAPL", "MSFT", "TSLA")
- **Indices**: Prefix with `^` (e.g., "^GSPC" for S&P 500, "^DJI" for Dow Jones)
- **International Stocks**: Append exchange suffix (e.g., "VOD.L" for Vodafone London)
- **Forex**: Use format like "EURUSD=X"
- **Crypto**: Use format like "BTC-USD"

## API Endpoints

The service uses the Yahoo Finance v8 chart API:
```
https://query1.finance.yahoo.com/v8/finance/chart/{symbol}?period1={fromUnix}&period2={toUnix}&interval=1d
```

## Differences from FinancialModellingPrepService

1. **No API Key Required**: Yahoo Finance API doesn't require authentication (though rate limits may apply)
2. **Economic Calendar**: Not available through Yahoo Finance API
3. **Date Format**: Uses Unix timestamps instead of date strings
4. **Response Structure**: Different JSON structure requiring custom DTOs
5. **Symbol Normalization**: Minimal normalization (mainly trim whitespace)

## Configuration Notes

- **Base URL**: Use either `https://query1.finance.yahoo.com` or `https://query2.finance.yahoo.com`
- **Rate Limits**: Yahoo Finance has informal rate limits; consider implementing retry logic or delays
- **Data Reliability**: Yahoo Finance is free but may have occasional gaps or delays in data

## Future Enhancements

Potential improvements you could add:

1. **Retry Logic**: Add exponential backoff for rate limit handling
2. **Economic Calendar**: Integrate with a dedicated economic calendar service or scrape Yahoo Finance's calendar page
3. **Adjusted Close**: Use adjusted close prices for more accurate historical data
4. **Multiple Intervals**: Support for 1m, 5m, 15m, 1h intervals in addition to daily
5. **Symbol Validation**: Add symbol validation and normalization logic for different asset types
6. **Caching**: Implement response caching to reduce API calls

## Error Handling

The service includes comprehensive error handling for:
- HTTP request failures
- JSON deserialization errors
- Missing or invalid data
- Symbol not found errors

All errors are logged to the console and propagated with context information.
