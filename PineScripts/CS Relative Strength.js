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

// ==========================================
// 2. BENCHMARK SELECTION
// ==========================================
// Using ETFs (SPY/QQQ) avoids cash index scaling disparities
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
// 3. CALCULATION (PURE RATIO)
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
// 4. PLOTS
// ==========================================
color lineCol = isRS50High ? color.yellow : (rsRatio > rsMA ? color.teal : color.rgb(180, 40, 70))

plot(rsRatio, "RS Line", color = lineCol, linewidth = 2)
plot(showMA ? rsMA : na, "RS 21 SMA", color = color.gray, linewidth = 1)

// Plot shapes at the EXACT rsRatio level (eliminates bottom-floor artifacts)
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