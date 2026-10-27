//@version=6
indicator("Pocket Pivot (Kacher & Morales) v6", overlay = true, max_labels_count = 500)

// ==========================================
// INPUTS & PARAMETERS
// ==========================================
int lookbackPeriod       = input.int(10, title = "Down-Day Lookback Period", minval = 5, maxval = 30, group = "Pocket Pivot Rules")
float closeRangeThresh   = input.float(50.0, title = "Min Close Position in Day's Range (%)", minval = 0.0, maxval = 100.0, group = "Pocket Pivot Rules")
float smaDistThresh      = input.float(2.0, title = "Max Distance from Moving Average (%)", minval = 0.5, maxval = 5.0, group = "Pocket Pivot Rules")

bool use10SMA            = input.bool(true, title = "Enable 10-Day SMA Pocket Pivot", group = "Moving Averages")
bool use50SMA            = input.bool(true, title = "Enable 50-Day SMA Pocket Pivot", group = "Moving Averages")
bool showBgHighlight     = input.bool(true, title = "Highlight Chart Background on Signal", group = "Visual Options")

// ==========================================
// CALCULATIONS & MOVING AVERAGES
// ==========================================
float sma10  = ta.sma(close, 10)
float sma50  = ta.sma(close, 50)
float sma200 = ta.sma(close, 200)

// Calculate highest down-day volume in the last N sessions
// Down-day: Close is strictly lower than previous close
float downVolume = (close < close[1]) ? volume : 0.0
float maxDownVol = ta.highest(downVolume, lookbackPeriod)[1]

// Intraday close location value (0% = at low of day, 100% = at high of day)
float dayRange      = high - low
float closePosition = (dayRange > 0.0) ? ((close - low) / dayRange) * 100.0 : 50.0

// Distance from moving averages
float distTo10SMA = math.abs((close - sma10) / sma10) * 100.0
float distTo50SMA = math.abs((close - sma50) / sma50) * 100.0

// Check if price range touched or came within 1.5% of the moving average during the session
bool touched10SMA = (low <= sma10 and high >= sma10) or (low <= sma10 * 1.015)
bool touched50SMA = (low <= sma50 and high >= sma50) or (low <= sma50 * 1.015)

// Trend filter: Stock must be above its 200 SMA or 200 SMA is not yet available
bool trendFilter = close > sma200 or na(sma200)

// ==========================================
// POCKET PIVOT CONDITIONS
// ==========================================
bool isUpDay       = close > close[1]
bool isVolumeValid = volume > maxDownVol
bool isCloseStrong = closePosition >= closeRangeThresh

// 10-SMA Pocket Pivot Trigger
bool pp10Signal = use10SMA and isUpDay and isVolumeValid and isCloseStrong and touched10SMA and (distTo10SMA <= smaDistThresh) and trendFilter

// 50-SMA Pocket Pivot Trigger
bool pp50Signal = use50SMA and isUpDay and isVolumeValid and isCloseStrong and touched50SMA and (distTo50SMA <= smaDistThresh) and trendFilter

// Combined Signal
bool isPocketPivot = pp10Signal or pp50Signal

// ==========================================
// PLOTTING & VISUALIZATION
// ==========================================
plot(sma10, title = "10 SMA", color = color.new(color.blue, 0), linewidth = 2)
plot(sma50, title = "50 SMA", color = color.new(color.orange, 0), linewidth = 2)
plot(sma200, title = "200 SMA", color = color.new(color.red, 20), linewidth = 1)

// Chart Signal Markers
plotshape(pp10Signal and not pp50Signal, title = "Pocket Pivot (10 SMA)", style = shape.triangleup, location = location.belowbar, color = color.blue, size = size.small, text = "PP10")
plotshape(pp50Signal, title = "Pocket Pivot (50 SMA)", style = shape.triangleup, location = location.belowbar, color = color.purple, size = size.small, text = "PP50")

// Color Volume Bar & Background Highlight
barcolor(isPocketPivot ? color.lime : na, title = "Highlight PP Candle Bar")
bgcolor(showBgHighlight and isPocketPivot ? color.new(color.green, 85) : na, title = "Background Highlight")

// ==========================================
// TRADINGVIEW ALERTS
// ==========================================
alertcondition(isPocketPivot, title = "Pocket Pivot Signal", message = "Pocket Pivot buy point detected on {{ticker}} at {{close}}!")
alertcondition(pp10Signal, title = "10-SMA Pocket Pivot Signal", message = "10-SMA Pocket Pivot buy point detected on {{ticker}}!")
alertcondition(pp50Signal, title = "50-SMA Pocket Pivot Signal", message = "50-SMA Pocket Pivot buy point detected on {{ticker}}!")