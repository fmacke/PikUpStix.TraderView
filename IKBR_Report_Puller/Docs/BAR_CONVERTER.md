# BarConverter Helper Class

## Overview
The `BarConverter` class provides conversion functionality between IBKR API's `Bar` type and the application's domain `Bar` type.

## Purpose
IBKR's API returns historical data as `IBApi.Bar` objects, but the application's database and domain layer use a custom `Domain.Bar` type. This converter bridges the gap between these two representations.

## Class Location
**File**: `IKBR_Report_Puller/Helpers/BarConverter.cs`

## Public Methods

### ConvertToDomainBar
Converts a single IBKR bar to a domain bar.

```csharp
public static Domain.Bar ConvertToDomainBar(IBApi.Bar ibkrBar, int instrumentId)
```

**Parameters**:
- `ibkrBar`: The IBKR API bar object containing OHLCV data
- `instrumentId`: The database instrument ID to associate with the bar

**Returns**: A `Domain.Bar` object ready for database insertion

**Throws**: 
- `ArgumentNullException` if `ibkrBar` is null

**Example**:
```csharp
IBApi.Bar ibkrBar = await _chartDataService.GetHistoricalDataAsync(...);
Domain.Bar domainBar = BarConverter.ConvertToDomainBar(ibkrBar, instrumentId: 42);
```

### ConvertToDomainBars
Converts a list of IBKR bars to domain bars.

```csharp
public static List<Domain.Bar> ConvertToDomainBars(List<IBApi.Bar> ibkrBars, int instrumentId)
```

**Parameters**:
- `ibkrBars`: List of IBKR API bar objects
- `instrumentId`: The database instrument ID to associate with all bars

**Returns**: A list of `Domain.Bar` objects ready for database insertion

**Throws**: 
- `ArgumentNullException` if `ibkrBars` is null

**Behavior**:
- Filters out null bars automatically
- Applies the same `instrumentId` to all bars

**Example**:
```csharp
var ibkrBars = await _chartDataService.GetHistoricalDataAsync(conid, startDate, endDate);
var domainBars = BarConverter.ConvertToDomainBars(ibkrBars, instrumentId: 42);
_dataService.InsertChartData(instrumentId.ToString(), domainBars);
```

## Field Mapping

### From IBKR to Domain

| IBApi.Bar Property | Domain.Bar Property | Notes |
|-------------------|---------------------|-------|
| `Time` | `Date` | Parsed from IBKR's string format |
| `Open` | `OpenPrice` | Direct mapping |
| `High` | `HighPrice` | Direct mapping |
| `Low` | `LowPrice` | Direct mapping |
| `Close` | `ClosePrice` | Direct mapping |
| `Volume` | `Volume` | Direct mapping |
| N/A | `Settle` | Set to 0 (not provided by IBKR) |
| N/A | `OpenInterest` | Set to 0 (not provided by IBKR) |
| (parameter) | `InstrumentId` | From method parameter |

### Date Parsing
IBKR returns dates in two possible formats:
1. **Date only**: `"yyyyMMdd"` (e.g., "20240115")
2. **Date and time**: `"yyyyMMdd HH:mm:ss"` (e.g., "20240115 16:00:00")

The converter tries both formats and falls back to standard DateTime parsing.

## Usage in Application

### In Application.cs
```csharp
private async void FetchHistoricalDataStub(string conid, string symbol, 
    DateTime startDate, DateTime endDate, int instrumentId)
{
    // 1. Fetch from IBKR API
    var ibkrBars = await _chartDataService.GetHistoricalDataAsync(conid, startDate, endDate);

    // 2. Convert to domain type
    var domainBars = BarConverter.ConvertToDomainBars(ibkrBars, instrumentId);

    // 3. Save to database
    _dataService.InsertChartData(instrumentId.ToString(), domainBars);
}
```

## Error Handling

### Null Checks
- **Input validation**: Throws `ArgumentNullException` for null inputs
- **Filtering**: Automatically filters out null bars in list conversion

### Date Parsing Errors
- Tries multiple date formats
- Falls back to standard parsing
- Throws `FormatException` if all parsing attempts fail

### Example Error Handling
```csharp
try
{
    var domainBars = BarConverter.ConvertToDomainBars(ibkrBars, instrumentId);
    _dataService.InsertChartData(instrumentId.ToString(), domainBars);
}
catch (ArgumentNullException ex)
{
    Console.WriteLine($"Invalid input: {ex.Message}");
}
catch (FormatException ex)
{
    Console.WriteLine($"Date parsing error: {ex.Message}");
}
```

## Design Decisions

### Static Class
- No instance state needed
- Pure conversion logic
- Utility pattern for common operations

### Separate Namespace
- Located in `Helpers` namespace
- Keeps domain models clean
- Centralized conversion logic

### Type Aliasing
```csharp
using DomainBar = IKBR_Report_Puller.Domain.Bar;
```
This avoids naming conflicts with `IBApi.Bar` while keeping code readable.

## Testing Considerations

### Unit Test Scenarios
1. **Null handling**
   - Null single bar
   - Null bar list
   - List containing null bars

2. **Date parsing**
   - Date only format ("20240115")
   - Date and time format ("20240115 16:00:00")
   - Invalid date formats

3. **Data mapping**
   - Verify all OHLCV fields map correctly
   - Verify InstrumentId is assigned
   - Verify Settle and OpenInterest default to 0

4. **Edge cases**
   - Empty list
   - List with single bar
   - Large lists (performance)

### Sample Unit Test
```csharp
[Test]
public void ConvertToDomainBar_ValidInput_MapsAllFields()
{
    // Arrange
    var ibkrBar = new IBApi.Bar
    {
        Time = "20240115",
        Open = 100.0,
        High = 105.0,
        Low = 99.0,
        Close = 103.0,
        Volume = 1000000
    };

    // Act
    var domainBar = BarConverter.ConvertToDomainBar(ibkrBar, instrumentId: 42);

    // Assert
    Assert.AreEqual(new DateTime(2024, 1, 15), domainBar.Date);
    Assert.AreEqual(100.0, domainBar.OpenPrice);
    Assert.AreEqual(105.0, domainBar.HighPrice);
    Assert.AreEqual(99.0, domainBar.LowPrice);
    Assert.AreEqual(103.0, domainBar.ClosePrice);
    Assert.AreEqual(1000000, domainBar.Volume);
    Assert.AreEqual(42, domainBar.InstrumentId);
}
```

## Performance Considerations

### List Conversion
- Uses LINQ for clean, functional code
- Filters null entries efficiently
- Single-pass enumeration

### Date Parsing
- Tries specific formats before fallback
- Avoids exceptions for common formats
- Uses InvariantCulture for consistency

### Memory
- No object pooling (bars are short-lived)
- Minimal allocations beyond output list
- No caching (conversion is cheap)

## Related Files

- **Domain Model**: `Domain/Bar.cs`
- **IBKR Service**: `Services/ChartDataService.cs`
- **Application Logic**: `Application.cs` (FetchHistoricalDataStub method)
- **Data Repository**: `Data/Repositories/HistoricalDataRepository.cs`

## Future Enhancements

1. **Additional Validation**
   - Validate OHLCV relationships (High >= Low, etc.)
   - Check for reasonable price ranges
   - Validate volume is non-negative

2. **Settle and OpenInterest**
   - If IBKR provides these fields in future APIs
   - Add mapping logic when data becomes available

3. **Async Support**
   - Currently synchronous (conversion is fast)
   - Could add async overloads if needed

4. **Batch Optimization**
   - Parallel processing for very large lists
   - Chunking for memory efficiency

5. **Logging**
   - Log conversion statistics
   - Track parsing failures
   - Monitor data quality issues
