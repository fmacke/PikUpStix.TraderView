# Data Layer Quick Reference

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                         Application Layer                        │
│                      (Application.cs, etc.)                      │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         ├─ IDataService
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│                  DataService (Unit of Work)                      │
│  • Coordinates all repository operations                        │
│  • 136 lines (was 628 - 78% reduction)                         │
│  • No direct SQL - delegates to repositories                    │
└────┬────────┬──────────┬─────────────┬─────────────────────────┘
     │        │          │             │
     ▼        ▼          ▼             ▼
┌─────────┬───────────┬────────────┬──────────────┐
│  Trade  │OpenPosition│Historical  │  Instrument  │
│Repository│ Repository │  Data Repo │  Repository  │
└────┬────┴─────┬─────┴─────┬──────┴──────┬───────┘
     │          │           │             │
     │  Each extends BaseRepository        │
     │          │           │             │
     └──────────┴───────────┴─────────────┘
                    │
                    ▼
         ┌──────────────────────┐
         │   BaseRepository     │
         │  • DB Connection     │
         │  • Error Handling    │
         │  • Common Operations │
         └──────────────────────┘
```

## Repository Responsibilities

| Repository | Primary Entity | Key Operations |
|------------|---------------|----------------|
| **TradeRepository** | Trade, TradeConfirm | • Upsert trade executions<br>• Get all trades<br>• Upsert today's confirmations |
| **OpenPositionRepository** | OpenPosition | • Insert open positions<br>• Get instrument names |
| **HistoricalDataRepository** | Bar (chart data) | • Insert chart data<br>• Skip duplicates |
| **InstrumentRepository** | Instrument metadata | • Upsert time series data |

## Data Flow

```
User Request
    │
    ▼
DataService.InsertTradeExecutions(IKBRReport)
    │
    ├─ Validates report has trades
    │
    ▼
TradeRepository.UpsertTradeExecutions(List<Trade>)
    │
    ├─ For each trade:
    │   ├─ Check if exists (by ibExecID)
    │   ├─ If exists and incomplete → Update
    │   └─ If not exists → Insert
    │
    ├─ Uses TradeParameterBuilder for parameters
    │
    ▼
BaseRepository.ExecuteCommand(...)
    │
    ├─ Manages SQL connection
    ├─ Handles errors
    ├─ Executes within transaction
    │
    ▼
Database (SQL Server)
```

## File Organization

```
Data/
├── BaseRepository.cs                 (Abstract base class)
├── TypeConverters.cs                 (Utility functions)
├── README.md                         (Full documentation)
└── Repositories/
    ├── TradeRepository.cs           (152 lines)
    ├── TradeParameterBuilder.cs     (91 lines)
    ├── OpenPositionRepository.cs    (87 lines)
    ├── OpenPositionParameterBuilder.cs (56 lines)
    ├── HistoricalDataRepository.cs  (97 lines)
    └── InstrumentRepository.cs      (114 lines)

Services/
└── DataService.cs                    (136 lines - was 628!)
```

## Code Metrics

| Metric | Before | After | Improvement |
|--------|--------|-------|-------------|
| **DataService Size** | 628 lines | 136 lines | **↓ 78%** |
| **Coupling** | High (all in one) | Low (separated) | **✓** |
| **Testability** | Difficult | Easy | **✓** |
| **Maintainability** | Low | High | **✓** |
| **Code Duplication** | High | Minimal | **✓** |

## Common Operations Quick Reference

### Insert New Records
```csharp
_dataService.InsertTradeExecutions(report);
_dataService.InsertOpenPositions(report);
_dataService.InsertChartData(instrumentId, bars);
```

### Read Records
```csharp
var trades = _dataService.GetTradeExecutions();
var instruments = _dataService.GetOpenPositionInstrumentNames(report);
```

### Update Records
```csharp
// Trades: Automatically handled by UpsertTradeExecutions
// Instruments: Automatically handled by UpsertTimeSeriesData
```

## Testing Approach

```
Unit Tests
├── Repository Tests (test each repository independently)
│   ├── TradeRepositoryTests
│   ├── OpenPositionRepositoryTests
│   ├── HistoricalDataRepositoryTests
│   └── InstrumentRepositoryTests
│
├── Parameter Builder Tests (test mapping logic)
│   ├── TradeParameterBuilderTests
│   └── OpenPositionParameterBuilderTests
│
└── Integration Tests (test DataService coordination)
    └── DataServiceTests
```

## Key Design Patterns

1. **Repository Pattern** - Encapsulates data access logic
2. **Unit of Work Pattern** - Coordinates multiple repositories
3. **Builder Pattern** - Parameter builders construct SQL parameters
4. **Template Method** - BaseRepository defines common algorithm
5. **Dependency Injection** - Repositories injected into DataService

## Benefits Summary

| Aspect | Benefit |
|--------|---------|
| **Readability** | Clear separation by entity |
| **Maintainability** | Changes isolated to specific repositories |
| **Testability** | Can mock and test each component independently |
| **Scalability** | Easy to add new entities/repositories |
| **Performance** | Optimized with transactions and parameterized queries |
| **Error Handling** | Centralized and consistent |
| **Code Reuse** | Base classes and builders eliminate duplication |
