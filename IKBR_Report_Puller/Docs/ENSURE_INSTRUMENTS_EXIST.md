# EnsureInstrumentsExist Method

## Overview
The `EnsureInstrumentsExist` method ensures that all instruments referenced in a collection of trades exist in the database before processing the trades. It automatically creates missing instruments to prevent foreign key constraint violations.

## Purpose
This method solves the problem of inserting trade executions that reference instruments which don't yet exist in the database. By pre-creating all necessary instruments, it:
- Prevents foreign key constraint violations
- Ensures data integrity
- Enables batch processing of trades
- Provides visibility into instrument creation

## Method Signature

```csharp
public void EnsureInstrumentsExist(List<Trade> trades)
```

**Parameters**:
- `trades`: List of `Trade` objects to check and create instruments for

**Returns**: `void` (logs results to console)

## Implementation Details

### Workflow

1. **Validation**
   - Returns early if trades list is null or empty
   - No-op if no trades to process

2. **Extract Unique Instruments**
   - Gets all unique `Conid` values from trades
   - Filters out null/empty conids
   - Uses `Distinct()` to avoid duplicates

3. **Batch Check and Create**
   - Uses a single transaction for all operations
   - For each unique conid:
     - Checks if instrument exists
     - If not, creates it using trade data
   - Commits all changes together

4. **Reporting**
   - Logs count of instruments checked
   - Reports created vs. existing counts
   - Provides transaction rollback on error

### Transaction Handling

```csharp
ExecuteDatabaseOperation(connection =>
{
    using (var transaction = connection.BeginTransaction())
    {
        try
        {
            // Check and create instruments
            transaction.Commit();
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            throw;
        }
    }
});
```

**Benefits**:
- **Atomicity**: All instruments created or none
- **Consistency**: No partial updates on failure
- **Performance**: Batch operation with single commit

## Usage

### In DataService

```csharp
public void InsertTradeExecutions(IKBRReport report)
{
    if (report == null || !report.Trades.Any())
    {
        Console.WriteLine("No trades found in the report.");
        return;
    }

    // Ensure all instruments exist before inserting trades
    _instrumentRepository.EnsureInstrumentsExist(report.Trades);

    // Now safe to insert trades (foreign keys will be satisfied)
    _tradeExecutionRepository.UpsertTradeExecutions(report.Trades);
}
```

### Standalone Usage

```csharp
var trades = new List<Trade> 
{
    new Trade { Conid = "265598", Symbol = "AAPL", Currency = "USD", ... },
    new Trade { Conid = "272093", Symbol = "MSFT", Currency = "USD", ... },
    new Trade { Conid = "265598", Symbol = "AAPL", Currency = "USD", ... } // Duplicate conid
};

_instrumentRepository.EnsureInstrumentsExist(trades);
// Output: Checking 2 unique instruments...
// Output: Created 2 new instrument(s), 0 already existed
```

## Console Output Examples

### First Time Processing (All New)
```
Checking 15 unique instruments...
Created 15 new instrument(s), 0 already existed
```

### Subsequent Processing (All Exist)
```
Checking 15 unique instruments...
All 15 instrument(s) already exist
```

### Mixed Scenario
```
Checking 20 unique instruments...
Created 5 new instrument(s), 15 already existed
```

### Error Scenario
```
Checking 10 unique instruments...
Error ensuring instruments exist: Cannot insert duplicate key in object 'dbo.Instruments'
```

## Data Extraction from Trades

For each new instrument, the method extracts:

| Field | Source | Fallback |
|-------|--------|----------|
| SecurityId | `Trade.Conid` | Required |
| InstrumentName | `Trade.Symbol` | `Trade.Description` → "Unknown" |
| Currency | `Trade.Currency` | NULL |
| ListingExchange | `Trade.ListingExchange` | NULL |
| DataName | `Trade.AssetCategory` | "Unknown" |
| Provider | - | "IBKR" |
| DataSource | - | "Trade Execution" |
| Format | - | "Trade" |
| Frequency | - | "Trade" |

**Note**: If multiple trades have the same `Conid`, the method uses the **first trade** in the list to extract instrument details.

## Performance Characteristics

### Time Complexity
- **Best Case**: O(n) - All instruments exist, just lookup
- **Worst Case**: O(n) - All instruments need creation
- Where n = number of unique conids

### Database Operations
- **Lookups**: 1 query per unique conid
- **Inserts**: 1 insert per missing instrument
- **Transactions**: 1 transaction for all operations

### Optimization Features
1. **Deduplication**: Uses `Distinct()` to check each conid once
2. **Batch Transaction**: Single commit for all inserts
3. **Early Return**: No-op for null/empty trade lists
4. **Filtered Processing**: Skips trades with null conids

## Error Handling

### Handled Scenarios

1. **Null/Empty Input**
   ```csharp
   if (trades == null || !trades.Any())
       return; // Silent return
   ```

2. **Transaction Failure**
   ```csharp
   catch (Exception ex)
   {
       transaction.Rollback();
       Console.WriteLine($"Error ensuring instruments exist: {ex.Message}");
       throw; // Re-throw to caller
   }
   ```

3. **Null Conids**
   ```csharp
   var uniqueConids = trades
       .Where(t => !string.IsNullOrEmpty(t.Conid)) // Filter nulls
       .Select(t => t.Conid)
       .Distinct()
       .ToList();
   ```

### Potential Exceptions

| Exception | Cause | Resolution |
|-----------|-------|------------|
| `SqlException` | Database connection failure | Check connection string |
| `InvalidOperationException` | Transaction already committed | Verify transaction logic |
| `ArgumentException` | Invalid conid format | Validate trade data |

## Testing Recommendations

### Unit Tests

```csharp
[Test]
public void EnsureInstrumentsExist_NullTrades_ReturnsEarly()
{
    // Arrange
    var repository = new InstrumentRepository(connectionString);

    // Act & Assert
    Assert.DoesNotThrow(() => repository.EnsureInstrumentsExist(null));
}

[Test]
public void EnsureInstrumentsExist_EmptyList_ReturnsEarly()
{
    // Arrange
    var repository = new InstrumentRepository(connectionString);
    var trades = new List<Trade>();

    // Act & Assert
    Assert.DoesNotThrow(() => repository.EnsureInstrumentsExist(trades));
}

[Test]
public void EnsureInstrumentsExist_DuplicateConids_CreatesOnce()
{
    // Arrange
    var repository = new InstrumentRepository(connectionString);
    var trades = new List<Trade>
    {
        new Trade { Conid = "123", Symbol = "TEST" },
        new Trade { Conid = "123", Symbol = "TEST" },
        new Trade { Conid = "123", Symbol = "TEST" }
    };

    // Act
    repository.EnsureInstrumentsExist(trades);

    // Assert - Should only create 1 instrument
    var instruments = repository.GetInstruments();
    Assert.AreEqual(1, instruments.Count(i => i.SecurityId == "123"));
}

[Test]
public void EnsureInstrumentsExist_MixedExistingAndNew_CreatesOnlyNew()
{
    // Arrange
    var repository = new InstrumentRepository(connectionString);

    // Create first instrument
    var existingTrades = new List<Trade>
    {
        new Trade { Conid = "AAA", Symbol = "EXISTING" }
    };
    repository.EnsureInstrumentsExist(existingTrades);

    // Create mixed batch
    var mixedTrades = new List<Trade>
    {
        new Trade { Conid = "AAA", Symbol = "EXISTING" },
        new Trade { Conid = "BBB", Symbol = "NEW" }
    };

    // Act
    repository.EnsureInstrumentsExist(mixedTrades);

    // Assert
    var instruments = repository.GetInstruments();
    Assert.IsTrue(instruments.Any(i => i.SecurityId == "AAA"));
    Assert.IsTrue(instruments.Any(i => i.SecurityId == "BBB"));
}
```

### Integration Tests

```csharp
[Test]
public void EnsureInstrumentsExist_RealDatabase_CreatesInstruments()
{
    // Arrange
    var dataService = CreateDataService();
    var report = new IKBRReport
    {
        Trades = new List<Trade>
        {
            new Trade 
            { 
                Conid = Guid.NewGuid().ToString(), 
                Symbol = "TEST1",
                Currency = "USD",
                AssetCategory = "STK",
                ListingExchange = "NASDAQ"
            }
        }
    };

    // Act
    dataService.InsertTradeExecutions(report);

    // Assert - No foreign key exceptions thrown
    var instruments = dataService.GetInstruments();
    Assert.IsTrue(instruments.Any(i => i.InstrumentName == "TEST1"));
}
```

## Integration Points

### Called By
- `DataService.InsertTradeExecutions()`

### Calls
- `GetInstrumentIdByConid()` - Check if instrument exists
- `InsertInstrumentFromTrade()` - Create new instrument

### Database Tables
- **Reads**: `dbo.Instruments` (SELECT by SecurityId)
- **Writes**: `dbo.Instruments` (INSERT new records)

## Benefits

### 1. Prevents Foreign Key Violations
```csharp
// Before: Would throw FK violation if instrument doesn't exist
_tradeExecutionRepository.UpsertTradeExecutions(trades);

// After: Instruments guaranteed to exist
_instrumentRepository.EnsureInstrumentsExist(trades);
_tradeExecutionRepository.UpsertTradeExecutions(trades); // Safe
```

### 2. Batch Efficiency
- Single transaction for all instruments
- Reduces database round trips
- Faster than creating instruments one-by-one

### 3. Automatic Deduplication
- Handles duplicate conids in trade list
- Only checks each unique conid once
- Prevents duplicate instrument creation

### 4. Visibility
- Console logging shows what's happening
- Reports created vs. existing counts
- Helps with debugging and monitoring

## Comparison: EnsureInstrumentsExist vs GetOrCreateInstrumentByConid

| Feature | EnsureInstrumentsExist | GetOrCreateInstrumentByConid |
|---------|----------------------|------------------------------|
| **Purpose** | Batch pre-creation | Single instrument lookup/create |
| **Input** | List of trades | Individual instrument details |
| **Transaction** | Single for all | One per call |
| **Use Case** | Before batch insert | On-demand during processing |
| **Return** | void | int (InstrumentId) |
| **Logging** | Summary counts | Individual warnings |

**When to use each**:
- **EnsureInstrumentsExist**: Batch processing, importing reports
- **GetOrCreateInstrumentByConid**: Real-time single trade processing

## Future Enhancements

### 1. Async Support
```csharp
public async Task EnsureInstrumentsExistAsync(List<Trade> trades)
{
    // Async database operations
}
```

### 2. Progress Reporting
```csharp
public void EnsureInstrumentsExist(
    List<Trade> trades, 
    IProgress<int> progress = null)
{
    for (int i = 0; i < uniqueConids.Count; i++)
    {
        // Process
        progress?.Report((i + 1) * 100 / uniqueConids.Count);
    }
}
```

### 3. Return Created Instruments
```csharp
public List<Instrument> EnsureInstrumentsExist(List<Trade> trades)
{
    var created = new List<Instrument>();
    // Track and return created instruments
    return created;
}
```

### 4. Validation
```csharp
public void EnsureInstrumentsExist(List<Trade> trades)
{
    // Validate trade data before processing
    var invalidTrades = trades.Where(t => 
        string.IsNullOrEmpty(t.Symbol) || 
        string.IsNullOrEmpty(t.Conid));

    if (invalidTrades.Any())
        throw new ArgumentException("Some trades have invalid data");
}
```

### 5. Caching
```csharp
private HashSet<string> _existingConids = new HashSet<string>();

public void EnsureInstrumentsExist(List<Trade> trades)
{
    // Skip lookup if conid is in cache
    var conidsToCheck = uniqueConids
        .Where(c => !_existingConids.Contains(c))
        .ToList();
}
```

## Related Methods

- **GetInstrumentIdByConid()**: Looks up existing instrument
- **InsertInstrumentFromTrade()**: Creates new instrument
- **GetOrCreateInstrumentByConid()**: Single instrument version
- **GetInstruments()**: Retrieves all IBKR instruments

## Related Documentation

- [Foreign Key Migration](FOREIGN_KEY_MIGRATION.md)
- [Get Instruments Method](GET_INSTRUMENTS_METHOD.md)
- [Repository Pattern](README.md)

## Related Files

- **Implementation**: `Data/Repositories/InstrumentRepository.cs`
- **Service Layer**: `Services/DataService.cs`
- **Domain Models**: `Domain/Trade.cs`, `Domain/Instrument.cs`
