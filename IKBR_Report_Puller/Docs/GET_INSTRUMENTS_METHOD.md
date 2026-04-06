# Instrument Domain Model and GetInstruments Method

## Overview
Added the `Instrument` domain model and `GetInstruments()` method to retrieve all IBKR instruments from the database.

## New Files

### Domain/Instrument.cs
Represents an instrument in the trading system with all database columns mapped.

```csharp
public class Instrument
{
    public int Id { get; set; }
    public string InstrumentName { get; set; }
    public string Provider { get; set; }
    public string DataName { get; set; }
    public string DataSource { get; set; }
    public string Format { get; set; }
    public string Frequency { get; set; }
    public double? ContractUnit { get; set; }
    public string ContractUnitType { get; set; }
    public string PriceQuotation { get; set; }
    public double? MinimumPriceFluctuation { get; set; }
    public string Currency { get; set; }
    public string ListingExchange { get; set; }
    public string SecurityId { get; set; }
}
```

## Updated Files

### InstrumentRepository.cs

#### New Method: GetInstruments()

**Signature**:
```csharp
public List<Instrument> GetInstruments()
```

**Purpose**: Retrieves all instruments with `Provider = 'IBKR'` from the database.

**Returns**: `List<Instrument>` - List of all IBKR instruments, ordered by InstrumentName

**SQL Query**:
```sql
SELECT Id, InstrumentName, Provider, DataName, DataSource, Format, Frequency, 
       ContractUnit, ContractUnitType, PriceQuotation, MinimumPriceFluctuation, 
       Currency, ListingExchange, SecurityId
FROM dbo.Instruments
WHERE Provider = @provider
ORDER BY InstrumentName
```

**Features**:
- Filters by `Provider = 'IBKR'`
- Returns instruments sorted alphabetically by name
- Handles NULL values for all nullable fields
- Uses ExecuteDatabaseOperation for consistent error handling

**Example Usage**:
```csharp
var instruments = _instrumentRepository.GetInstruments();
foreach (var instrument in instruments)
{
    Console.WriteLine($"{instrument.InstrumentName} ({instrument.SecurityId})");
}
```

### DataService.cs

#### Added Method: GetInstruments()

**Signature**:
```csharp
public List<Instrument> GetInstruments()
```

**Purpose**: Exposes the GetInstruments functionality through the service layer.

**Implementation**:
```csharp
public List<Instrument> GetInstruments()
{
    return _instrumentRepository.GetInstruments();
}
```

**Example Usage**:
```csharp
var instruments = _dataService.GetInstruments();
Console.WriteLine($"Found {instruments.Count} IBKR instruments");
```

### IDataService.cs

#### Added Interface Method

```csharp
List<Instrument> GetInstruments();
```

This ensures the method is part of the public contract for IDataService.

## Field Mapping

### Database to Domain

| Database Column | Domain Property | Type | Nullable |
|----------------|-----------------|------|----------|
| Id | Id | int | No |
| InstrumentName | InstrumentName | string | Yes |
| Provider | Provider | string | Yes |
| DataName | DataName | string | Yes |
| DataSource | DataSource | string | Yes |
| Format | Format | string | Yes |
| Frequency | Frequency | string | Yes |
| ContractUnit | ContractUnit | double? | Yes |
| ContractUnitType | ContractUnitType | string | Yes |
| PriceQuotation | PriceQuotation | string | Yes |
| MinimumPriceFluctuation | MinimumPriceFluctuation | double? | Yes |
| Currency | Currency | string | Yes |
| ListingExchange | ListingExchange | string | Yes |
| SecurityId | SecurityId | string | Yes |

### NULL Handling

The implementation uses `reader.IsDBNull()` checks for all fields:

```csharp
InstrumentName = reader.IsDBNull(1) ? null : reader.GetString(1),
ContractUnit = reader.IsDBNull(7) ? (double?)null : reader.GetDouble(7),
```

This ensures proper NULL handling for both reference types (strings) and nullable value types (double?).

## Use Cases

### 1. List All Available Instruments
```csharp
var instruments = _dataService.GetInstruments();
Console.WriteLine("Available IBKR Instruments:");
foreach (var instrument in instruments)
{
    Console.WriteLine($"- {instrument.InstrumentName} ({instrument.SecurityId})");
}
```

### 2. Find Instruments by Currency
```csharp
var instruments = _dataService.GetInstruments();
var usdInstruments = instruments.Where(i => i.Currency == "USD").ToList();
Console.WriteLine($"Found {usdInstruments.Count} USD instruments");
```

### 3. Check if Instrument Exists
```csharp
var instruments = _dataService.GetInstruments();
bool exists = instruments.Any(i => i.SecurityId == "265598");
```

### 4. Group by Asset Category
```csharp
var instruments = _dataService.GetInstruments();
var grouped = instruments.GroupBy(i => i.DataName);
foreach (var group in grouped)
{
    Console.WriteLine($"{group.Key}: {group.Count()} instruments");
}
```

### 5. Export Instrument List
```csharp
var instruments = _dataService.GetInstruments();
var csv = string.Join("\n", instruments.Select(i => 
    $"{i.InstrumentName},{i.SecurityId},{i.Currency},{i.ListingExchange}"));
File.WriteAllText("instruments.csv", csv);
```

## Performance Considerations

### Query Performance
- Uses indexed `Provider` column for filtering
- Returns all columns (no SELECT *)
- Orders by `InstrumentName` for consistent results

### Memory Usage
- Loads all IBKR instruments into memory
- Typical count: 100-10,000 instruments
- Memory impact: ~1-10 MB

### Optimization Opportunities
1. **Pagination**: Add skip/take parameters for large datasets
2. **Filtering**: Add additional filter parameters (currency, exchange, etc.)
3. **Projection**: Return only specific fields if needed
4. **Caching**: Cache results with expiration if data changes infrequently

## Testing Recommendations

### Unit Tests

```csharp
[Test]
public void GetInstruments_ReturnsOnlyIBKRInstruments()
{
    // Arrange
    var repository = new InstrumentRepository(connectionString);

    // Act
    var instruments = repository.GetInstruments();

    // Assert
    Assert.IsTrue(instruments.All(i => i.Provider == "IBKR"));
}

[Test]
public void GetInstruments_OrdersByName()
{
    // Arrange
    var repository = new InstrumentRepository(connectionString);

    // Act
    var instruments = repository.GetInstruments();

    // Assert
    var sortedNames = instruments.Select(i => i.InstrumentName).ToList();
    Assert.AreEqual(sortedNames, sortedNames.OrderBy(n => n).ToList());
}

[Test]
public void GetInstruments_HandlesNullValues()
{
    // Arrange
    var repository = new InstrumentRepository(connectionString);

    // Act
    var instruments = repository.GetInstruments();

    // Assert - Should not throw, even with NULL database values
    Assert.DoesNotThrow(() => 
    {
        foreach (var i in instruments)
        {
            var _ = i.ContractUnit;
            var __ = i.Currency;
        }
    });
}
```

### Integration Tests

```csharp
[Test]
public void DataService_GetInstruments_ReturnsValidData()
{
    // Arrange
    var dataService = CreateDataService();

    // Act
    var instruments = dataService.GetInstruments();

    // Assert
    Assert.IsNotNull(instruments);
    Assert.IsNotEmpty(instruments);
    Assert.IsTrue(instruments.All(i => i.Id > 0));
    Assert.IsTrue(instruments.All(i => !string.IsNullOrEmpty(i.SecurityId)));
}
```

## Error Handling

### Database Errors
The method uses `ExecuteDatabaseOperation` which handles:
- SQL connection failures
- Query execution errors
- Data reader errors

**Error Example**:
```
Database error: Invalid column name 'InstrumentName'
```

### Empty Result Set
Returns an empty list (not null) if no IBKR instruments exist:
```csharp
var instruments = _dataService.GetInstruments();
Console.WriteLine(instruments.Count); // 0 if no data
```

### NULL Handling
All nullable fields are properly handled:
```csharp
var instrument = instruments.First();
string currency = instrument.Currency ?? "N/A";
double unit = instrument.ContractUnit ?? 0.0;
```

## Future Enhancements

### 1. Filtering Parameters
```csharp
public List<Instrument> GetInstruments(
    string provider = "IBKR",
    string currency = null,
    string exchange = null)
{
    // Add WHERE clause filters dynamically
}
```

### 2. Pagination
```csharp
public PagedResult<Instrument> GetInstruments(
    int pageNumber = 1,
    int pageSize = 100)
{
    // Implement OFFSET/FETCH pagination
}
```

### 3. Search
```csharp
public List<Instrument> SearchInstruments(string searchTerm)
{
    // WHERE InstrumentName LIKE @search 
    //    OR SecurityId LIKE @search
}
```

### 4. Caching
```csharp
private List<Instrument> _cachedInstruments;
private DateTime _cacheExpiry;

public List<Instrument> GetInstruments()
{
    if (_cachedInstruments == null || DateTime.Now > _cacheExpiry)
    {
        _cachedInstruments = _instrumentRepository.GetInstruments();
        _cacheExpiry = DateTime.Now.AddMinutes(30);
    }
    return _cachedInstruments;
}
```

### 5. Async Support
```csharp
public async Task<List<Instrument>> GetInstrumentsAsync()
{
    // Use ExecuteDatabaseOperationAsync
}
```

## Related Files

- **Domain Model**: `Domain/Instrument.cs` (NEW)
- **Repository**: `Data/Repositories/InstrumentRepository.cs`
- **Service**: `Services/DataService.cs`
- **Interface**: `Interfaces/IDataService.cs`
- **Database**: `dbo.Instruments` table

## Database Schema Reference

```sql
CREATE TABLE [dbo].[Instruments](
    [Id] [int] IDENTITY(1,1) PRIMARY KEY,
    [InstrumentName] [nvarchar](max) NULL,
    [Provider] [nvarchar](max) NULL,
    [DataName] [nvarchar](max) NULL,
    [DataSource] [nvarchar](max) NULL,
    [Format] [nvarchar](max) NULL,
    [Frequency] [nvarchar](max) NULL,
    [ContractUnit] [float] NULL,
    [ContractUnitType] [nvarchar](max) NULL,
    [PriceQuotation] [nvarchar](max) NULL,
    [MinimumPriceFluctuation] [float] NULL,
    [Currency] [nvarchar](max) NULL,
    [ListingExchange] [varchar](200) NULL,
    [SecurityId] [varchar](200) NULL
)
```
