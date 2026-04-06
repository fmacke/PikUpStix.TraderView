# Historical Data Validation Implementation

## Overview
This document describes the implementation of the `UpdateHistoricalDataForPositions` method, which validates that historical price data exists for all trades in the required date ranges.

## Business Requirements

For each trade in the trade history:
- **Required Start Date**: Trade opening date - 100 days
- **Required End Date**: Trade closing date + 20 days (capped at today's date)

This ensures we have sufficient historical context before the trade opened and follow-up data after it closed.

## Implementation

### 1. HistoricalDataRepository Enhancement

#### `GetMissingDateRanges()` Method
```csharp
public List<(DateTime startDate, DateTime endDate)> GetMissingDateRanges(
    int instrumentId, 
    DateTime startDate, 
    DateTime endDate)
```

**Purpose**: Identifies gaps in historical data coverage

**Logic**:
1. Retrieves all existing dates from the database for the instrument
2. Generates expected trading dates (weekdays only - rough approximation)
3. Identifies missing dates by comparing expected vs. actual
4. Groups consecutive missing dates into date ranges

**Returns**: List of date ranges where data is missing

**Example Output**:
```
[(2024-01-15, 2024-01-20), (2024-02-10, 2024-02-15)]
```

### 2. InstrumentRepository Integration

The existing `GetOrCreateInstrumentByConid()` method is leveraged to:
- Look up instruments by their contract ID (conid)
- Create instruments on-the-fly if they don't exist
- Return the InstrumentId needed for historical data queries

### 3. DataService Enhancements

Added two new methods to expose repository functionality:

#### `GetMissingDateRanges()`
```csharp
public List<(DateTime startDate, DateTime endDate)> GetMissingDateRanges(
    int instrumentId, 
    DateTime startDate, 
    DateTime endDate)
```

#### `GetOrCreateInstrumentByConid()`
```csharp
public int GetOrCreateInstrumentByConid(
    string conid,
    string symbol,
    string listingExchange,
    string currency,
    string assetCategory,
    string securityID,
    string description)
```

### 4. Application.cs Implementation

#### `UpdateHistoricalDataForPositions()` Method

**Workflow**:

1. **Generate Trade History**
   - Calls `CreateTradeHistoryReport()` to build aggregated trade history
   - Processes each closed trade

2. **Calculate Date Ranges**
   ```csharp
   DateTime requiredStartDate = trade.TradeOpened.AddDays(-100);
   DateTime requiredEndDate = trade.TradeClosed.AddDays(20);

   // Cap at today
   if (requiredEndDate > DateTime.Today)
       requiredEndDate = DateTime.Today;
   ```

3. **Instrument Lookup/Creation**
   - Gets or creates the instrument record
   - Retrieves the InstrumentId for database queries

4. **Missing Data Detection**
   - Calls `GetMissingDateRanges()` to identify gaps
   - Logs each missing range to console

5. **Stub for Future Implementation**
   - Calls `FetchHistoricalDataStub()` for each gap
   - Placeholder for actual IBKR API integration

#### `FetchHistoricalDataStub()` Method

**Purpose**: Placeholder for future data fetching implementation

**Parameters**:
- `conid`: Contract ID for IBKR API
- `symbol`: Instrument symbol
- `startDate`: Start of missing range
- `endDate`: End of missing range
- `instrumentId`: Database key for saving data

**Future Implementation**:
```csharp
// TODO: Replace stub with actual implementation
var bars = await _chartDataService.GetHistoricalDataAsync(conid, startDate, endDate);
_dataService.InsertChartData(instrumentId.ToString(), bars);
```

## Console Output Examples

### When Data is Complete
```
Checking historical data for AAPL from 2023-10-15 to 2024-03-20
All required historical data exists for AAPL
```

### When Data is Missing
```
Checking historical data for MSFT from 2023-11-01 to 2024-04-15
Found 2 missing date range(s) for MSFT:
  Missing data from 2023-11-01 to 2023-11-05
  Missing data from 2024-02-10 to 2024-02-15
[STUB] Would fetch historical data for MSFT (conid: 272093) from 2023-11-01 to 2023-11-05
[STUB] InstrumentId: 42
[STUB] Would fetch historical data for MSFT (conid: 272093) from 2024-02-10 to 2024-02-15
[STUB] InstrumentId: 42
```

### When Instrument Cannot Be Created
```
Warning: Could not get or create instrument for XYZ (SecurityId: 123456)
```

## Date Calculation Logic

### Trading Days Approximation
The implementation uses a simple weekday filter:
- **Included**: Monday through Friday
- **Excluded**: Saturday and Sunday
- **Note**: Does not account for market holidays

This is a rough approximation. For production use, consider:
- Integrating a trading calendar library
- Using exchange-specific holiday calendars
- Handling half-days and early closures

### Consecutive Date Grouping
Missing dates are grouped into ranges using:
1. Sequential weekdays
2. Friday → Monday transitions (weekend gaps)

**Example**:
```
Missing: [2024-01-15, 2024-01-16, 2024-01-17, 2024-01-22, 2024-01-23]
Grouped: [(2024-01-15, 2024-01-17), (2024-01-22, 2024-01-23)]
```

## Integration Points

### Current Integration
- ✅ Trade history generation (`TradeHistoryReportService`)
- ✅ Instrument lookup/creation (`InstrumentRepository`)
- ✅ Missing data detection (`HistoricalDataRepository`)
- ✅ Stub for data fetching

### Future Integration Required
- ⏳ IBKR API data fetching (replace `FetchHistoricalDataStub`)
- ⏳ Error handling for API failures
- ⏳ Rate limiting for IBKR API calls
- ⏳ Async/await conversion for better performance
- ⏳ Progress reporting for long-running operations

## Performance Considerations

### Current Implementation
- Synchronous database calls
- One query per trade to check missing ranges
- Sequential processing of trades

### Optimization Opportunities
1. **Batch Processing**: Group multiple instruments into single queries
2. **Async Operations**: Convert to async/await pattern
3. **Caching**: Cache instrument lookups for repeated trades
4. **Parallel Processing**: Fetch data for multiple instruments concurrently (respecting API rate limits)

## Error Handling

### Current Behavior
- Logs warnings for instruments that cannot be created
- Continues processing remaining trades on error
- Does not throw exceptions that would halt the entire operation

### Recommended Enhancements
1. Add retry logic for transient database failures
2. Implement exponential backoff for API rate limit errors
3. Log errors to a structured logging system
4. Track success/failure metrics for monitoring

## Testing Recommendations

### Unit Tests
- Test date range calculation logic
- Test weekend/holiday handling
- Test missing range grouping algorithm
- Mock database responses for missing data scenarios

### Integration Tests
- Verify end-to-end flow with test database
- Test with various trade date combinations
- Verify instrument creation workflow
- Test error handling paths

### Manual Testing
- Run against production-like data
- Verify console output readability
- Test with trades spanning years
- Verify today's date cap logic

## Future Enhancements

1. **Smart Data Fetching**
   - Only fetch data for actively traded instruments
   - Skip weekends/holidays in date calculations
   - Use IBKR trading calendar API

2. **Progress Tracking**
   - Show percentage complete
   - Estimate time remaining
   - Report data downloaded/saved

3. **Configuration Options**
   - Configurable buffer days (currently hardcoded -100/+20)
   - Toggle for historical data validation
   - Dry-run mode to preview missing data

4. **Data Quality Checks**
   - Verify price data reasonableness
   - Check for duplicate dates
   - Validate OHLCV relationships (High >= Low, etc.)

## Related Files

- **Implementation**: `Application.cs` (lines 66-125)
- **Repository**: `HistoricalDataRepository.cs` (GetMissingDateRanges method)
- **Service**: `DataService.cs` (GetMissingDateRanges, GetOrCreateInstrumentByConid)
- **Interface**: `IDataService.cs` (interface definitions)
- **Domain**: `HistoricalTrade.cs`, `TradeBase.cs`
