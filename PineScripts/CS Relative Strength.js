//@version=6
indicator("CS Relative Strength", overlay = true, dynamic_requests = true)

// ==========================================
// 1. INPUTS & BENCHMARK SELECTION
// ==========================================
string indexMode = input.string("AUTO", "RS Benchmark Mode", options = ["AUTO", "SPX", "COMP", "Manual"])
string manualSymbol = input.symbol("QQQ", "└─ Custom Manual Ticker")
int    maLength = input.int(21, "RS Line MA Length")
bool   showMA = input.bool(true, "Show RS Moving Average")
bool   showDots = input.bool(true, "Show RS High Dots")

// Rebase options
int    anchorBarsBack = input.int(250, "Anchor RS Base (Bars Back)", minval = 20)

// Institutional Thresholds
float  inst_min_pct = input.float(30.0, "Min Inst. Ownership %", minval = 0)

// ==========================================
// AUTOMATED & MANUAL SWITCHING LOGIC
// ==========================================
string benchmarkIndex = "SPX"

if indexMode == "AUTO"
    is_nasdaq = (syminfo.prefix == "NASDAQ") or str.contains(syminfo.tickerid, "NASDAQ")
    bool auto_comp = is_nasdaq or(str.length(syminfo.ticker) == 4 and syminfo.ticker != "HNGE")
benchmarkIndex:= auto_comp ? "COMP" : "SPX"
else if indexMode == "SPX"
    benchmarkIndex:= "SPX"
else if indexMode == "COMP"
    benchmarkIndex:= "COMP"
else
    benchmarkIndex:= manualSymbol

// ==========================================
// 2. DATA FETCHING
// ==========================================
float benchmarkClose = request.security(benchmarkIndex, timeframe.period, close, lookahead = barmerge.lookahead_off)

// Institutional Data Fetch
f_get_inst_data(_ticker) =>
    float _pct_q = request.financial(_ticker, "INSTITUTIONAL_HOLDERS_PERCENTAGE", "FQ", ignore_invalid_symbol = true)
    float _cnt_q = request.financial(_ticker, "TOTAL_INSTITUTIONS_HOLDING_SHARES", "FQ", ignore_invalid_symbol = true)
    float _pct_final = not na(_pct_q) ? _pct_q : request.financial(_ticker, "INSTITUTIONAL_HOLDERS_PERCENTAGE", "FY", ignore_invalid_symbol = true)
    float _cnt_final = not na(_cnt_q) ? _cnt_q : request.financial(_ticker, "TOTAL_INSTITUTIONS_HOLDING_SHARES", "FY", ignore_invalid_symbol = true)
[_pct_final, _cnt_final]

[raw_pct, raw_cnt] = f_get_inst_data(syminfo.tickerid)

var float inst_percent = na
var float inst_count = na
if not na(raw_pct)
inst_percent:= raw_pct
if not na(raw_cnt)
inst_count:= raw_cnt

// ==========================================
// 3. INDEXED RS LINE CALCULATIONS
// ==========================================
// Raw Ratio for High calculations
float rawRsRatio = close / benchmarkClose

// Base anchor prices for rescaling to price chart
float baseClose = ta.valuewhen(bar_index == last_bar_index - anchorBarsBack, close, 0)
float baseBench = ta.valuewhen(bar_index == last_bar_index - anchorBarsBack, benchmarkClose, 0)

// Indexed RS line mapped to current stock price base
float rsLineScaled = (close / baseClose) / (benchmarkClose / baseBench) * baseClose
float rsMA = ta.sma(rsLineScaled, maLength)

// RS High logic using raw ratios (scale-independent)
bool isRS_50High = rawRsRatio >= ta.highest(rawRsRatio, 50)
float rsHigh252 = ta.highest(rawRsRatio, 252)
bool isRSNewHigh = rawRsRatio >= rsHigh252

float priceHigh252 = ta.highest(close, 252)
bool isBlueDot = isRSNewHigh and close < priceHigh252

// ==========================================
// 4. TREND TEMPLATE & VISUALS
// ==========================================
sma50 = ta.sma(close, 50)
sma150 = ta.sma(close, 150)
sma200 = ta.sma(close, 200)
high52 = ta.highest(high, 252)
is_bullish_trend = close > sma50 and sma50 > sma150 and sma150 > sma200

color lineCol = isRS_50High ? color.yellow : (rsLineScaled > rsMA ? color.teal : color.rgb(119, 2, 64))

// Plot RS Line on Price Chart
plot(rsLineScaled, "RS Line (Overlay)", lineCol, 2)
plot(showMA ? rsMA : na, "RS MA", color.gray, 1)

// Plot RS High Dots on Price Bar Highs
plotshape(showDots and isRSNewHigh,
    title = "RS 52W High",
    style = shape.circle,
    location = location.abovebar,
    color = isBlueDot ? color.blue : color.new(color.teal, 30),
    size = size.tiny)

// Background highlight removed from main chart to avoid cluttering price action; 
// using subtle line coloring instead.

// ==========================================
// 5. DASHBOARD TABLE
// ==========================================
var table tb = table.new(position.bottom_left, 2, 6, border_width = 1, frame_color = color.new(color.gray, 50))

if barstate.islast
    header_col = color.new(#160a85, 20)
text_col = color.white

table.cell(tb, 0, 0, "Benchmark", text_color = text_col, bgcolor = header_col)
table.cell(tb, 1, 0, benchmarkIndex + " (" + syminfo.prefix + ":" + syminfo.tickerid + ")", text_color = text_col, bgcolor = color.new(color.blue, 40))

table.cell(tb, 0, 3, "RS New High", text_color = text_col, bgcolor = header_col)
table.cell(tb, 1, 3, isRS_50High ? "YES" : "NO", text_color = text_col, bgcolor = isRS_50High ? color.new(color.yellow, 40) : color.new(color.gray, 40))

table.cell(tb, 0, 4, "Stage 2 Trend", text_color = text_col, bgcolor = header_col)
table.cell(tb, 1, 4, is_bullish_trend ? "YES" : "NO", text_color = text_col, bgcolor = is_bullish_trend ? color.new(color.green, 40) : color.new(color.red, 40))

    float dist = ((high52 - close) / high52) * 100
table.cell(tb, 0, 5, "Off 52W High", text_color = text_col, bgcolor = header_col)
table.cell(tb, 1, 5, str.tostring(dist, "#.##") + "%", text_color = text_col, bgcolor = dist < 15 ? color.new(color.teal, 40) : color.new(color.gray, 40))