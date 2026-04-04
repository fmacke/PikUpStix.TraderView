# Data Layer Architecture

## Overview

The data layer has been refactored to follow the **Repository Pattern** and **Unit of Work Pattern** for improved organization, maintainability, and testability.

## Architecture

### Before Refactoring
- **Single DataService**: 628 lines containing all CRUD operations
- Mixed concerns: database connection, query building, parameter mapping
- Difficult to maintain and test
- No clear separation of entity operations

### After Refactoring
- **DataService**: 136 lines - coordinates operations (Unit of Work)
- **Entity-specific Repositories**: Separate files for each domain entity
- **Parameter Builders**: Dedicated classes for building SQL parameters
- **Base Repository**: Common database operations
- **Type Converters**: Shared utility for type conversions

## Directory Structure

```
IKBR_Report_Puller/
├── Data/
│   ├── BaseRepository.cs                      # Base class with common DB operations
│   ├── TypeConverters.cs                      # Type conversion utilities
│   └── Repositories/
│       ├── TradeRepository.cs                 # Trade CRUD operations
│       ├── TradeParameterBuilder.cs           # Trade SQL parameter builder
│       ├── OpenPositionRepository.cs          # OpenPosition CRUD operations
│       ├── OpenPositionParameterBuilder.cs    # OpenPosition parameter builder
│       ├── HistoricalDataRepository.cs        # Historical data CRUD operations
│       └── InstrumentRepository.cs            # Instrument CRUD operations
└── Services/
    └── DataService.cs                         # Coordinates all repositories
```

## Components

### 1. BaseRepository
**Location**: `Data/BaseRepository.cs`

Provides common database operations:
- `ExecuteDatabaseOperation()` - Connection management with error handling
- `ExecuteCommand()` - Parameterized SQL execution
- `ExecuteScalar<T>()` - Single value queries
- `RecordExists()` - Existence checks

### 2. DataService (Unit of Work)
**Location**: `Services/DataService.cs`

Coordinates repository operations:
- **Trade Operations**
  - `InsertTradeExecutions(IKBRReport)` - Upsert trades
  - `GetTradeExecutions()` - Retrieve all trades
  - `InsertTodayExecutions(IKBRReport)` - Upsert today's confirmations

- **OpenPosition Operations**
  - `InsertOpenPositions(IKBRReport)` - Insert positions
  - `GetOpenPositionInstrumentNames(IKBRReport)` - Get instrument details

- **HistoricalData Operations**
  - `InsertChartData(string, List<Bar>)` - Insert chart bars

- **Instrument Operations**
  - `UpsertTimeSeriesData(...)` - Upsert instrument metadata

### 3. Entity Repositories

#### TradeRepository
**Location**: `Data/Repositories/TradeRepository.cs`

Handles all trade-related database operations:
- `UpsertTradeExecutions(List<Trade>)` - Insert or update trades
- `GetTradeExecutions()` - Retrieve trades ordered by date
- `UpsertTodayExecutions(List<TradeConfirm>)` - Handle today's confirmations

**Features**:
- Checks for existing records before insert/update
- Updates only incomplete records (missing critical fields)
- Transaction management for data integrity
- Detailed console logging

#### OpenPositionRepository
**Location**: `Data/Repositories/OpenPositionRepository.cs`

Manages open position data:
- `InsertOpenPositions(DateTime, List<OpenPosition>)` - Bulk insert positions
- `GetOpenPositionInstrumentNames(List<OpenPosition>)` - Extract instrument details

#### HistoricalDataRepository
**Location**: `Data/Repositories/HistoricalDataRepository.cs`

Manages chart/historical data:
- `InsertChartData(string, List<Bar>)` - Insert bars, skipping duplicates
- Efficient duplicate checking via date comparison

#### InstrumentRepository
**Location**: `Data/Repositories/InstrumentRepository.cs`

Handles instrument metadata:
- `UpsertTimeSeriesData(...)` - Insert or update instrument records
- Checks existence before insertion

### 4. Parameter Builders

#### TradeParameterBuilder
**Location**: `Data/Repositories/TradeParameterBuilder.cs`

Maps `Trade` domain objects to SQL parameters:
- 80+ property mappings
- Handles nullable types
- Single source of truth for trade parameters

#### OpenPositionParameterBuilder
**Location**: `Data/Repositories/OpenPositionParameterBuilder.cs`

Maps `OpenPosition` domain objects to SQL parameters:
- 50+ property mappings
- Includes whenGenerated timestamp
- Centralized parameter creation

### 5. TypeConverters
**Location**: `Data/TypeConverters.cs`

Provides safe type conversion utilities:
- `ConvertToDecimal(string)` - String to decimal?
- `ConvertToLong(string)` - String to long?
- `ConvertToInt(string)` - String to int?
- `ConvertToDate(string)` - String to DateTime?

## Benefits

### 1. **Separation of Concerns**
- Each repository handles one entity type
- DataService coordinates operations (no direct SQL)
- Parameter builders isolate mapping logic

### 2. **Improved Maintainability**
- 78% reduction in DataService size (628 → 136 lines)
- Easy to locate entity-specific operations
- Changes to one entity don't affect others

### 3. **Better Testability**
- Can mock individual repositories
- Test parameter builders independently
- Easier to write unit tests

### 4. **Reusability**
- BaseRepository methods reused across all repositories
- Parameter builders eliminate code duplication
- TypeConverters shared across the application

### 5. **Scalability**
- Easy to add new repositories for new entities
- Can extend BaseRepository with additional operations
- Clear pattern for future development

### 6. **Error Handling**
- Centralized in BaseRepository
- Consistent error messages
- Transaction rollback on failures

## Usage Examples

### Adding a New Entity Repository

```csharp
public class NewEntityRepository : BaseRepository
{
    public NewEntityRepository(string connectionString) : base(connectionString)
    {
    }

    public void InsertNewEntity(NewEntity entity)
    {
        ExecuteDatabaseOperation(connection =>
        {
            using (var transaction = connection.BeginTransaction())
            {
                const string query = "INSERT INTO...";
                var parameters = NewEntityParameterBuilder.GetParameters(entity);
                ExecuteCommand(connection, transaction, query, parameters);
                transaction.Commit();
            }
        });
    }
}
```

### Using DataService

```csharp
// Inject IDataService
public Application(IDataService dataService)
{
    _dataService = dataService;
}

// Use coordinated operations
public void ProcessReport(IKBRReport report)
{
    _dataService.InsertTradeExecutions(report);
    _dataService.InsertOpenPositions(report);
    var trades = _dataService.GetTradeExecutions();
}
```

## Migration Notes

- **No Breaking Changes**: IDataService interface remains the same
- **Backward Compatible**: All existing code continues to work
- **Internal Refactoring**: Implementation details changed, not the API

## Future Enhancements

1. **Add Query Methods**: Rich querying capabilities per repository
2. **Implement Caching**: Cache frequently accessed data
3. **Add Bulk Operations**: Optimize bulk inserts with SqlBulkCopy
4. **Async/Await**: Convert to async operations for better performance
5. **Stored Procedures**: Option to use stored procedures instead of inline SQL
6. **Connection Pooling**: Optimize connection management

## Testing Strategy

```csharp
// Example unit test structure
[TestClass]
public class TradeRepositoryTests
{
    private TradeRepository _repository;

    [TestInitialize]
    public void Setup()
    {
        _repository = new TradeRepository(connectionString);
    }

    [TestMethod]
    public void UpsertTradeExecutions_NewTrade_InsertsSuccessfully()
    {
        // Arrange
        var trades = new List<Trade> { /* test data */ };

        // Act
        _repository.UpsertTradeExecutions(trades);

        // Assert
        var result = _repository.GetTradeExecutions();
        Assert.AreEqual(1, result.Count);
    }
}
```

## Performance Considerations

- **Transaction Usage**: All multi-record operations use transactions
- **Parameterized Queries**: Prevents SQL injection and improves performance
- **Connection Management**: Using statements ensure proper disposal
- **Batch Operations**: Group related operations in single transactions

## Conclusion

The refactored data layer provides:
- ✅ Clear organization by entity
- ✅ Reduced complexity (78% size reduction in DataService)
- ✅ Better maintainability and testability
- ✅ Consistent patterns for CRUD operations
- ✅ Foundation for future enhancements
