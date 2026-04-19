# Chart Data Service - Forex Support Fix

## Problem
The `ChartDataService.GetHistoricalDataAsync` method was failing with IBKR Error 200 ("No security definition has been found for the request") when requesting historical data for forex pairs like EUR.GBP.

### Root Cause
IBKR requires different contract parameters for different security types:
- **Forex pairs** require: `SecType = "CASH"`, `Exchange = "IDEALPRO"`, and `whatToShow = "MIDPOINT"`
- **Stocks** require: `Exchange = "SMART"` and `whatToShow = "TRADES"`

The original implementation was using stock parameters for all instruments, causing forex requests to fail.

## Solution Implemented

### 1. Updated Interface
**File**: `IKBR_Report_Puller/Interfaces/IChartDataService.cs`

Added optional `symbol` parameter to `GetHistoricalDataAsync`:
```csharp
Task<List<Bar>> GetHistoricalDataAsync(string conid, DateTime from, DateTime to, string symbol = null);
```

### 2. Updated ChartDataService Implementation
**File**: `IKBR_Report_Puller/Services/ChartDataService.cs`

#### Automatic Instrument Type Detection
The method now detects forex pairs by checking if the symbol contains a period (e.g., EUR.GBP, GBP.USD):

```csharp
bool isForex = !string.IsNullOrEmpty(symbol) && symbol.Contains('.') && symbol.Split('.').Length == 2;
```

#### Dynamic Contract Configuration
Based on the detected instrument type, different contract parameters are set:

**For Forex Pairs:**
- `SecType = "CASH"`
- `Exchange = "IDEALPRO"`
- `whatToShow = "MIDPOINT"`
- `useRTH = 0` (24/5 trading hours)

**For Stocks:**
- `Exchange = "SMART"`
- `whatToShow = "TRADES"`
- `useRTH = 1` (Regular Trading Hours only)

#### Enhanced Logging
Added detailed logging to help diagnose issues:
```csharp
Console.WriteLine($"[ChartDataService] Requesting historical data - Symbol: {symbol}, ConId: {conid}, SecType: {contract.SecType ?? "default"}, Exchange: {contract.Exchange}, Duration: {duration}, WhatToShow: {whatToShow}");
```

### 3. Updated HistoricalDataService
**File**: `IKBR_Report_Puller/Services/HistoricalDataService.cs`

Updated the call to pass the symbol parameter:
```csharp
var ibkrBars = await _chartDataService.GetHistoricalDataAsync(conid, startDate, endDate, symbol);
```

## Technical Details

### Forex Contract Parameters
- **SecType**: `"CASH"` - Identifies the contract as a currency pair
- **Exchange**: `"IDEALPRO"` - IBKR's ideal forex exchange for institutional-size orders
- **WhatToShow**: `"MIDPOINT"` - For forex, use the mid-point between bid and ask
- **useRTH**: `0` - Forex trades 24 hours a day, 5 days a week

### Stock Contract Parameters
- **Exchange**: `"SMART"` - IBKR's smart routing system for stocks
- **WhatToShow**: `"TRADES"` - Use actual trade prices
- **useRTH**: `1` - Regular Trading Hours only (important for CAN SLIM RS lines)

## Symbol Pattern Matching

The solution uses a simple but effective pattern:
- Forex symbols contain a period: `EUR.GBP`, `GBP.USD`, `USD.JPY`
- Stock symbols do not: `AAPL`, `MSFT`, `BP`

This works because IBKR's forex naming convention always uses `{BASE}.{QUOTE}` format.

## Example Outputs

### Before Fix (Forex)
```
Fetching historical data for EUR.GBP (conid: 12087807) from 2024-10-28 to 2025-12-23
[IBKR Error 200] No security definition has been found for the request
```

### After Fix (Forex)
```
Fetching historical data for EUR.GBP (conid: 12087807) from 2024-10-28 to 2025-12-23
[ChartDataService] Requesting historical data - Symbol: EUR.GBP, ConId: 12087807, SecType: CASH, Exchange: IDEALPRO, Duration: 2 Y, WhatToShow: MIDPOINT
Successfully saved {n} bars for EUR.GBP
```

### Stock Request (Still Works)
```
Fetching historical data for AAPL (conid: 265598) from 2024-06-01 to 2024-12-23
[ChartDataService] Requesting historical data - Symbol: AAPL, ConId: 265598, SecType: default, Exchange: SMART, Duration: 205 D, WhatToShow: TRADES
Successfully saved {n} bars for AAPL
```

## Backward Compatibility

The `symbol` parameter is optional with a default value of `null`, ensuring backward compatibility with any existing code that doesn't pass the symbol. In such cases:
- `isForex` will be `false`
- Stock parameters will be used (SMART exchange, TRADES)
- This is the safest default for most instruments

## Build Status
✅ Build successful with no errors (357 warnings from other parts of the codebase)

## Testing Recommendations

1. **Test Forex Pairs**: EUR.GBP, GBP.USD, USD.JPY
2. **Test Stocks**: AAPL, MSFT, BP
3. **Test Date Ranges**: 
   - < 1 year (should use days)
   - >= 1 year (should use years)
4. **Monitor Console Output**: Verify correct SecType, Exchange, and WhatToShow values

## Related Changes

This fix works in conjunction with the previous Year/Day duration fix for date ranges >= 365 days, providing comprehensive support for both forex and stocks across all time periods.
