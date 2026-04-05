# TradeExecutions to Instruments Foreign Key Migration

## Overview
This document describes the changes made to support the new foreign key relationship between `TradeExecutions` and `Instruments` tables.

## Database Schema Change
The `TradeExecutions` table now includes an `InstrumentId` column that references the `Instruments` table:

```sql
ALTER TABLE [dbo].[TradeExecutions]  WITH CHECK ADD  
CONSTRAINT [FK_TradeExecutions_Instruments] FOREIGN KEY([InstrumentId])
REFERENCES [dbo].[Instruments] ([Id])
```

## Code Changes

### 1. InstrumentRepository.cs
Added new methods to support instrument lookup and creation based on trade data:

#### `GetOrCreateInstrumentByConid()`
- **Purpose**: Gets an existing instrument by `conid` or creates it if it doesn't exist
- **Returns**: `int` - The `InstrumentId` from the Instruments table
- **Parameters**:
  - `conid` - The contract ID (matches `Trade.Conid`)
  - `symbol` - The instrument symbol
  - `listingExchange` - The exchange where the instrument is listed
  - `currency` - The currency of the instrument
  - `assetCategory` - The asset type (Stock, Option, Future, etc.)
  - `securityID` - The security identifier
  - `description` - The instrument description

#### `GetInstrumentIdByConid()` (Private)
- Queries the Instruments table by `SecurityId` (which stores the `conid`)
- Returns the `InstrumentId` if found, otherwise `null`

#### `InsertInstrumentFromTrade()` (Private)
- Creates a new instrument record from trade execution data
- Sets default values for fields not available in trade data:
  - `Provider` = "IBKR"
  - `DataSource` = "Trade Execution"
  - `Format` = "Trade"
  - `Frequency` = "Trade"

### 2. TradeRepository.cs
Updated to work with the new foreign key relationship:

#### Constructor Changes
```csharp
// OLD
public TradeRepository(string connectionString) : base(connectionString)

// NEW
public TradeRepository(string connectionString, InstrumentRepository instrumentRepository) : base(connectionString)
```
- Now accepts `InstrumentRepository` as a dependency
- Stores reference to use for instrument lookups/creation

#### `InsertTrade()` Method
- **Before**: Inserted trades without `InstrumentId`
- **After**: 
  1. Calls `_instrumentRepository.GetOrCreateInstrumentByConid()` to get the `InstrumentId`
  2. Includes `InstrumentId` in the INSERT statement
  3. Adds `@instrumentId` parameter to the query

#### `UpdateTrade()` Method
- **Before**: Updated trades without `InstrumentId`
- **After**: 
  1. Calls `_instrumentRepository.GetOrCreateInstrumentByConid()` to get the `InstrumentId`
  2. Includes `InstrumentId` in the UPDATE statement
  3. Adds `@instrumentId` parameter to the query

### 3. DataService.cs
Updated initialization order to support the dependency:

```csharp
// OLD - Repositories initialized independently
_tradeRepository = new TradeRepository(_connectionString);
_instrumentRepository = new InstrumentRepository(_connectionString);

// NEW - InstrumentRepository must be created first
_instrumentRepository = new InstrumentRepository(_connectionString);
_tradeRepository = new TradeRepository(_connectionString, _instrumentRepository);
```

## Behavior

### When Inserting/Updating Trades
1. **Instrument Lookup**: The system checks if an instrument exists with the trade's `conid`
2. **Instrument Creation**: If not found, creates a new instrument using available trade data
3. **Foreign Key Assignment**: The returned `InstrumentId` is assigned to the trade execution record

### Instrument Creation from Trades
When creating instruments from trade executions, the following mapping is used:

| Instrument Field | Source | Default Value |
|-----------------|--------|---------------|
| SecurityId | Trade.Conid | (required) |
| InstrumentName | Trade.Symbol or Trade.Description | "Unknown" |
| Currency | Trade.Currency | NULL |
| ListingExchange | Trade.ListingExchange | NULL |
| DataName | Trade.AssetCategory | "Unknown" |
| Provider | N/A | "IBKR" |
| DataSource | N/A | "Trade Execution" |
| Format | N/A | "Trade" |
| Frequency | N/A | "Trade" |
| ContractUnit | N/A | NULL |
| ContractUnitType | N/A | NULL |
| PriceQuotation | N/A | NULL |
| MinimumPriceFluctuation | N/A | NULL |

## Migration Considerations

### Existing Data
If you have existing `TradeExecutions` records without `InstrumentId` values:
1. Run the application to process historical reports
2. The system will automatically create instruments and update the foreign keys
3. Alternatively, create a migration script to backfill `InstrumentId` values

### Performance
- Instrument lookups use indexed `SecurityId` column
- Instruments are created once per unique `conid`
- Subsequent trades with the same `conid` reuse the existing instrument

### Data Integrity
- All trades are now linked to a valid instrument
- Orphaned instruments can be identified and cleaned up if needed
- Cascade delete behavior should be defined based on business requirements

## Testing
To verify the changes:
1. Process a report with trade executions
2. Verify instruments are created in the `Instruments` table
3. Verify `TradeExecutions` records have valid `InstrumentId` values
4. Verify foreign key constraint is satisfied
