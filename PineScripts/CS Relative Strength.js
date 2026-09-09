//@version=6
indicator("CAN SLIM Relative Strength Line", overlay = false, precision = 4)

// ==========================================
// 1. INPUTS & BENCHMARK
// ==========================================
string indexMode = input.string("AUTO", "Benchmark Mode", options = ["AUTO", "SPY", "QQQ", "Manual"])
string manualSymbol = input.symbol("SPY", "└─ Custom Symbol")
int    maLength = input.int(21, "RS Line MA Length")
bool   showMA = input.bool(true, "Show RS Moving Average")
bool   showDots = input.bool(true, "Show Blue Dot / High Signals")

// Stage 2 Visual Inputs
bool   showLight = input.bool(true, "Show Stage 2 Status Light (Bottom-Left)")
bool   showRibbon = input.bool(true, "Show Stage 2 Ribbon Dots on RS Pane")

// ==========================================
// 2. BENCHMARK SELECTION
// ==========================================
string benchmark = "SPY"
if indexMode == "AUTO"
    bool isNasdaq = (syminfo.prefix == "NASDAQ") or str.contains(syminfo.tickerid, "NASDAQ") or(str.length(syminfo.ticker) == 4 and syminfo.ticker != "HNGE")
    benchmark:= isNasdaq ? "QQQ" : "SPY"
else if indexMode == "SPY"
    benchmark:= "SPY"
else if indexMode == "QQQ"
    benchmark:= "QQQ"
else
    benchmark:= manualSymbol

// ==========================================
// 3. DAILY STAGE 2 CALCULATION (TIMEFRAME AGNOSTIC)
// ==========================================
f_daily_stage2() =>
    d_close = close
    d_high = high
    d_low = low

    d_sma50 = ta.sma(d_close, 50)
    d_sma150 = ta.sma(d_close, 150)
    d_sma200 = ta.sma(d_close, 200)

    // 200-day SMA in an uptrend (strictly higher than ~1 month ago / 22 trading days)
    d_sma200_slope_up = d_sma200 > d_sma200[22]

    // 52-week High/Low lookbacks (~252 trading days)
    int d_lookback = math.min(bar_index + 1, 252)
    d_high252 = ta.highest(d_high, d_lookback)
    d_low252 = ta.lowest(d_low, d_lookback)

    // Minervini Trend Template Rules:
    // 1. Price > 150 SMA and 200 SMA
    // 2. 150 SMA > 200 SMA
    // 3. 200 SMA trending up
    // 4. 50 SMA > 150 SMA and 200 SMA
    // 5. Price > 50 SMA
    // 6. Price >= 25% above 52-week low
    // 7. Price within 25% of 52-week high
    bool ma_alignment = d_close > d_sma50 and d_sma50 > d_sma150 and d_sma150 > d_sma200
    bool low_condition = d_close >= (d_low252 * 1.25)
    bool high_condition = d_close >= (d_high252 * 0.75)

    bool is_stage2 = ma_alignment and d_sma200_slope_up and low_condition and high_condition
    float dist_from_high = ((d_high252 - d_close) / d_high252) * 100

    [is_stage2, dist_from_high]

// Pull daily calculation into whatever intraday timeframe is loaded
[stage2_active, daily_dist_high] = request.security(syminfo.tickerid, "D", f_daily_stage2(), lookahead = barmerge.lookahead_off)

// ==========================================
// 4. RS CALCULATION (PURE RATIO)
// ==========================================
float benchClose = request.security(benchmark, timeframe.period, close, lookahead = barmerge.lookahead_off)
float rsRatio = close / benchClose

// Trend and Highs
float rsMA = ta.sma(rsRatio, maLength)

// Protect against IPO history < 252 bars
int   lookback = math.min(bar_index + 1, 252)
float rsHigh252 = ta.highest(rsRatio, lookback)
bool  isRS50High = rsRatio >= ta.highest(rsRatio, math.min(bar_index + 1, 50))

// Only signal when the stock has at least 50 bars of history
bool  hasMinHistory = bar_index >= 50
bool  isRS252High = hasMinHistory and(rsRatio >= rsHigh252[1]) // Compare to PRIOR bar's high
float priceHigh252 = ta.highest(close, lookback)
bool  isBlueDot = isRS252High and(close < priceHigh252)

// ==========================================
// 5. PLOTS & STATUS SIGNALS
// ==========================================
color lineCol = isRS50High ? color.yellow : (rsRatio > rsMA ? color.teal : color.rgb(180, 40, 70))
plot(rsRatio, "RS Line", color = lineCol, linewidth = 2)
plot(showMA ? rsMA : na, "RS 21 SMA", color = color.gray, linewidth = 1)

// Plot shapes at the EXACT rsRatio level
plotshape(showDots and isBlueDot ? rsRatio : na,
    title = "IBD Blue Dot",
    style = shape.circle,
    location = location.absolute,
    color = color.blue,
    size = size.tiny)

plotshape(showDots and(isRS252High and not isBlueDot) ? rsRatio : na,
    title = "RS 52W High",
    style = shape.circle,
    location = location.absolute,
    color = color.teal,
    size = size.tiny)

// Historical Stage 2 confirmation strip at the bottom of the indicator pane
plotshape(showRibbon and stage2_active,
    title = "Stage 2 Bar Active",
    style = shape.square,
    location = location.bottom,
    color = color.new(color.green, 20),
    size = size.auto)

// ==========================================
// 6. STAGE 2 "STATUS LIGHT" (HUD)
// ==========================================
var table s2Hud = table.new(position.bottom_left, 2, 1, border_width = 1, frame_color = color.new(color.gray, 60))

if barstate.islast and showLight
    color lightColor = stage2_active ? color.rgb(16, 185, 129) : color.rgb(45, 52, 64)
    color textCol = stage2_active ? color.white : color.rgb(156, 163, 175)
    string statusTxt = stage2_active ? "● STAGE 2: ON" : "○ STAGE 2: OFF"
    string detailTxt = str.tostring(daily_dist_high, "#.#") + "% off 52W high"

    table.cell(s2Hud, 0, 0, statusTxt, bgcolor = lightColor, text_color = textCol, text_size = size.small)
    table.cell(s2Hud, 1, 0, detailTxt, bgcolor = color.new(color.black, 40), text_color = color.white, text_size = size.small)