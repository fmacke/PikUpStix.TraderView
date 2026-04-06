# InstrumentRepository Methods Documentation

## Overview
Documentation for the complete set of CRUD methods in the `InstrumentRepository` class.

## Public Methods

### UpdateInstrument

**Signature**:
```csharp
public bool UpdateInstrument(Instrument instrument)
```

**Purpose**: Updates an existing instrument in the database with new values.

**Parameters**:
- `instrument`: The instrument object to update (must have a valid `Id` > 0)

**Returns**: 
- `true` if the update was successful (at least one row affected)
- `false` if no rows were affected (instrument with that ID doesn't exist)

**Throws**:
- `ArgumentNullException` if `instrument` is null
- `ArgumentException` if `instrument.Id` is ≤ 0

**SQL Operation**:
```sql
UPDATE dbo.Instruments
SET InstrumentName = @instrumentName,
    Provider = @provider,
    DataName = @dataName,
    DataSource = @dataSource,
    Format = @format,
    Frequency = @frequency,
    ContractUnit = @contractUnit,
    ContractUnitType = @contractUnitType,
    PriceQuotation = @priceQuotation,
    MinimumPriceFluctuation = @minimumPriceFluctuation,
    Currency = @currency,
    ListingExchange = @listingExchange,
    ConId = @conId
WHERE Id = @id
```

**Features**:
- Updates all fields of the instrument
- Handles NULL values correctly
- Returns success/failure status
- Validates input before executing

**Usage Example**:
```csharp
var instrument = _instrumentRepository.GetInstruments()
    .FirstOrDefault(i => i.ConId == "265598");

if (instrument != null)
{
    instrument.Currency = "USD";
    instrument.ListingExchange = "NASDAQ";

    bool success = _instrumentRepository.UpdateInstrument(instrument);
    if (success)
        Console.WriteLine("Instrument updated successfully");
    else
        Console.WriteLine("Instrument not found");
}
```

**NULL Handling**:
All nullable fields are handled with explicit casting:
```csharp
{ "@instrumentName", (object)instrument.InstrumentName ?? DBNull.Value }
```

This ensures that C# `null` values are properly converted to `DBNull.Value` for SQL Server.

---

### UpsertInstruments (Trade Overload)

**Signature**:
```csharp
internal void UpsertInstruments(List<Trade> trades)
```

**Purpose**: Ensures all instruments referenced in a list of trades exist in the database, creating missing ones automatically.

**Parameters**:
- `trades`: List of `Trade` objects to process

**Behavior**:
1. Extracts unique `ConId` values from trades
2. Checks if each instrument exists in the database
3. Creates missing instruments using trade data
4. Uses a single transaction for all operations
5. Logs summary of created vs. existing instruments

**Data Extraction from Trades**:
- **ConId**: `Trade.Conid`
- **InstrumentName**: `Trade.Symbol` → `Trade.Description` → "Unknown"
- **Currency**: `Trade.Currency`
- **ListingExchange**: `Trade.ListingExchange`
- **DataName**: `Trade.AssetCategory` → "Unknown"
- **Provider**: "IBKR"
- **DataSource**: "Trade Execution"
- **Format**: "Trade"
- **Frequency**: "Trade"

**Usage Example**:
```csharp
// Before inserting trade executions
_instrumentRepository.UpsertInstruments(report.Trades);
_tradeExecutionRepository.UpsertTradeExecutions(report.Trades);
```

**Console Output**:
```
Created 5 new instrument(s), 15 already existed
```

---

### UpsertInstruments (TradeConfirm Overload)

**Signature**:
```csharp
internal void UpsertInstruments(List<TradeConfirm> tradeConfirms)
```

**Purpose**: Ensures all instruments referenced in a list of trade confirmations exist in the database, creating missing ones automatically.

**Parameters**:
- `tradeConfirms`: List of `TradeConfirm` objects to process

**Behavior**:
Similar to the Trade overload but adapted for TradeConfirm data:
1. Extracts unique `ConId` values from trade confirmations
2. Checks if each instrument exists in the database
3. Creates missing instruments using trade confirmation data
4. Uses a single transaction for all operations
5. Logs summary with specific message for trade confirmations

**Data Extraction from TradeConfirms**:
- **ConId**: `TradeConfirm.ConId`
- **InstrumentName**: `TradeConfirm.Symbol` → "Unknown"
- **Currency**: `TradeConfirm.Currency`
- **Provider**: "IBKR"
- **DataName**: "Trade Confirmation"
- **DataSource**: "Today Report"
- **Format**: "TradeConfirm"
- **Frequency**: "Intraday"

**Usage Example**:
```csharp
// Before inserting today's trade confirmations
_instrumentRepository.UpsertInstruments(report.TradeConfirms);
_tradeExecutionRepository.UpsertTodayExecutions(report.TradeConfirms);
```

**Console Output**:
```
Created 2 new instrument(s) from trade confirmations, 8 already existed
```

---

## Private Helper Methods

### CheckInstrumentExists

**Signature**:
```csharp
private bool CheckInstrumentExists(string instrumentId)
```

**Purpose**: Checks if an instrument exists in the database by its ID.

**Parameters**:
- `instrumentId`: The instrument ID to check (as string)

**Returns**: 
- `true` if the instrument exists
- `false` if the instrument doesn't exist or `instrumentId` is null/empty

**SQL Query**:
```sql
SELECT COUNT(*) FROM dbo.Instruments WHERE Id = @instrumentId
```

**Early Return**:
```csharp
if (string.IsNullOrEmpty(instrumentId))
    return false;
```

Returns `false` immediately if the ID is invalid, avoiding unnecessary database call.

**Usage**:
```csharp
if (CheckInstrumentExists("42"))
{
    // Instrument exists, safe to proceed
}
```

---

### GetInstrumentIdByConid

**Signature**:
```csharp
private int? GetInstrumentIdByConid(
    SqlConnection connection, 
    SqlTransaction transaction, 
    string conid)
```

**Purpose**: Retrieves the database ID of an instrument by its Contract ID (ConId).

**Parameters**:
- `connection`: Open SQL connection
- `transaction`: Active transaction (can be null)
- `conid`: The contract ID to look up

**Returns**: 
- `int?` - The instrument ID if found, `null` otherwise

**SQL Query**:
```sql
SELECT Id FROM dbo.Instruments WHERE ConId = @conid
```

**NULL Handling**:
Returns `null` if:
- `conid` is null or empty
- No instrument found with that ConId
- Query returns 0

**Usage**:
```csharp
int? instrumentId = GetInstrumentIdByConid(connection, transaction, "265598");
if (instrumentId.HasValue)
{
    // Use instrumentId.Value
}
```

---

### InsertInstrumentFromTrade

**Signature**:
```csharp
private void InsertInstrumentFromTrade(
    SqlConnection connection,
    SqlTransaction transaction,
    string conid,
    string symbol,
    string listingExchange,
    string currency,
    string assetCategory,
    string securityID,
    string description)
```

**Purpose**: Creates a new instrument record from trade execution data.

**Parameters**:
- `connection`: Open SQL connection
- `transaction`: Active transaction
- `conid`: Contract ID
- `symbol`: Instrument symbol
- `listingExchange`: Exchange where listed
- `currency`: Trading currency
- `assetCategory`: Asset type (STK, OPT, FUT, etc.)
- `securityID`: Security identifier
- `description`: Instrument description

**Default Values**:
```csharp
Provider = "IBKR"
DataSource = "Trade Execution"
Format = "Trade"
Frequency = "Trade"
```

**NULL Handling with Fallbacks**:
```csharp
InstrumentName: symbol ?? description ?? "Unknown"
DataName: assetCategory ?? "Unknown"
Currency: currency ?? NULL
ListingExchange: listingExchange ?? NULL
```

---

### InsertInstrumentFromTradeConfirm

**Signature**:
```csharp
private void InsertInstrumentFromTradeConfirm(
    SqlConnection connection,
    SqlTransaction transaction,
    string conid,
    string symbol,
    string currency)
```

**Purpose**: Creates a new instrument record from trade confirmation data.

**Parameters**:
- `connection`: Open SQL connection
- `transaction`: Active transaction
- `conid`: Contract ID
- `symbol`: Instrument symbol
- `currency`: Trading currency

**Default Values**:
```csharp
Provider = "IBKR"
DataName = "Trade Confirmation"
DataSource = "Today Report"
Format = "TradeConfirm"
Frequency = "Intraday"
```

**Differences from InsertInstrumentFromTrade**:
- Fewer parameters (TradeConfirm has limited data)
- Different metadata values (DataSource, Format, Frequency)
- Simpler fallback logic

---

## Method Comparison Table

| Method | Access | Purpose | Returns | Transaction |
|--------|--------|---------|---------|-------------|
| **GetInstruments** | Public | Retrieve all IBKR instruments | `List<Instrument>` | No |
| **AddInstrument** | Public | Insert new instrument | `int` (new ID) | No |
| **UpdateInstrument** | Public | Update existing instrument | `bool` (success) | No |
| **UpsertInstruments(Trade)** | Internal | Ensure instruments exist | `void` | Yes |
| **UpsertInstruments(TradeConfirm)** | Internal | Ensure instruments exist | `void` | Yes |
| **CheckInstrumentExists** | Private | Check if exists by ID | `bool` | No |
| **GetInstrumentIdByConid** | Private | Get ID by ConId | `int?` | Optional |
| **InsertInstrumentFromTrade** | Private | Create from trade data | `void` | Required |
| **InsertInstrumentFromTradeConfirm** | Private | Create from confirm data | `void` | Required |

---

## Complete CRUD Operations

### Create
- **AddInstrument()** - Insert with full control
- **UpsertInstruments()** - Batch insert missing instruments
- **InsertInstrumentFromTrade()** - Helper for trade-based creation
- **InsertInstrumentFromTradeConfirm()** - Helper for confirm-based creation

### Read
- **GetInstruments()** - Retrieve all IBKR instruments
- **CheckInstrumentExists()** - Check existence by ID
- **GetInstrumentIdByConid()** - Look up ID by ConId

### Update
- **UpdateInstrument()** - Update existing instrument

### Delete
- ❌ Not implemented (instruments are rarely deleted)

---

## Transaction Patterns

### With Transaction (UpsertInstruments)
```csharp
using (var transaction = connection.BeginTransaction())
{
    try
    {
        // Multiple operations
        InsertInstrumentFromTrade(...);
        transaction.Commit();
    }
    catch (Exception ex)
    {
        transaction.Rollback();
        throw;
    }
}
```

### Without Transaction (Single Operations)
```csharp
ExecuteDatabaseOperation(connection =>
{
    // Single operation
    using (var cmd = new SqlCommand(query, connection))
    {
        cmd.ExecuteNonQuery();
    }
});
```

---

## Error Handling Best Practices

### Input Validation
```csharp
if (instrument == null)
    throw new ArgumentNullException(nameof(instrument));

if (instrument.Id <= 0)
    throw new ArgumentException("Instrument Id must be greater than 0");
```

### Early Returns
```csharp
if (trades == null || !trades.Any())
    return;
```

### Transaction Rollback
```csharp
catch (Exception ex)
{
    transaction.Rollback();
    Console.WriteLine($"Error: {ex.Message}");
    throw;
}
```

---

## Testing Recommendations

### Unit Tests for UpdateInstrument
```csharp
[Test]
public void UpdateInstrument_ValidInstrument_ReturnsTrue()
{
    var instrument = new Instrument { Id = 1, InstrumentName = "Updated" };
    bool result = _repository.UpdateInstrument(instrument);
    Assert.IsTrue(result);
}

[Test]
public void UpdateInstrument_InvalidId_ReturnsFalse()
{
    var instrument = new Instrument { Id = 99999, InstrumentName = "Test" };
    bool result = _repository.UpdateInstrument(instrument);
    Assert.IsFalse(result);
}

[Test]
public void UpdateInstrument_NullInstrument_ThrowsException()
{
    Assert.Throws<ArgumentNullException>(() => 
        _repository.UpdateInstrument(null));
}
```

### Integration Tests for UpsertInstruments
```csharp
[Test]
public void UpsertInstruments_NewTrades_CreatesInstruments()
{
    var trades = new List<Trade>
    {
        new Trade { Conid = "NEW123", Symbol = "TEST" }
    };

    _repository.UpsertInstruments(trades);

    var instruments = _repository.GetInstruments();
    Assert.IsTrue(instruments.Any(i => i.ConId == "NEW123"));
}
```

---

## Related Documentation
- [Get Instruments Method](GET_INSTRUMENTS_METHOD.md)
- [Ensure Instruments Exist](ENSURE_INSTRUMENTS_EXIST.md)
- [Foreign Key Migration](FOREIGN_KEY_MIGRATION.md)
