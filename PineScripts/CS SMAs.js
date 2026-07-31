//@version=6
indicator(title = "4x Simple Moving Average Suite", shorttitle = "4x SMA", overlay = true, timeframe = "", timeframe_gaps = true)

// ==========================================
// 1. GLOBAL SETTINGS
// ==========================================
src = input.source(close, title = "Source")
offset = input.int(title = "Offset", defval = 0, minval = -500, maxval = 500, display = display.none)

// Multi-Length Inputs
GRP_LEN = "MA Lengths"
len1 = input.int(10, "MA 1 Length (e.g., Short / 10 MA)", group = GRP_LEN, minval = 1)
len2 = input.int(21, "MA 2 Length (e.g., Medium / 21 SMA)", group = GRP_LEN, minval = 1)
len3 = input.int(50, "MA 3 Length (e.g., Intermediate / 50 SMA)", group = GRP_LEN, minval = 1)
len4 = input.int(200, "MA 4 Length (e.g., Long / 200 SMA)", group = GRP_LEN, minval = 1)

// Shared Smoothing Controls
GRP_SM = "Smoothing Settings"
TT_BB = "Only applies when 'SMA + Bollinger Bands' is selected. Determines the distance between the primary MA and the bands."
maType = input.string("None", "Smoothing Type", options = ["None", "SMA", "SMA + Bollinger Bands", "EMA", "SMMA (RMA)", "WMA", "VWMA"], group = GRP_SM, display = display.none)
smLength = input.int(14, "Smoothing Length", group = GRP_SM, display = display.none, active = maType != "None")
bbMult = input.float(2.0, "BB StdDev", minval = 0.001, maxval = 50, step = 0.5, tooltip = TT_BB, group = GRP_SM, display = display.none, active = maType == "SMA + Bollinger Bands")

// Flags
isBB = maType == "SMA + Bollinger Bands"
enableSM = maType != "None"

// ==========================================
// 2. REUSABLE FUNCTIONS
// ==========================================

// Primary Calculation Switch
calcMa(float sourceSeries, int length, string typeStr) =>
switch typeStr
        "SMA"                   => ta.sma(sourceSeries, length)
"SMA + Bollinger Bands" => ta.sma(sourceSeries, length)
"EMA"                   => ta.ema(sourceSeries, length)
"SMMA (RMA)"            => ta.rma(sourceSeries, length)
"WMA"                   => ta.wma(sourceSeries, length)
"VWMA"                  => ta.vwma(sourceSeries, length)
        => sourceSeries

// Higher-Order Function: Calculates Base MA + Secondary Smoothing + BB Bands
computeMaPipeline(float priceSeries, int maLen, string smType, int smLen, float devMult) =>
    float baseMa = ta.sma(priceSeries, maLen)
    float smMa = smType != "None" ? calcMa(baseMa, smLen, smType) : baseMa

    // Calculate StdDev off the smoothed base if BB is selected
    float bbDev = (smType == "SMA + Bollinger Bands") ? ta.stdev(baseMa, smLen) * devMult : na
    float bbUp = smMa + bbDev
    float bbLow = smMa - bbDev

[baseMa, smMa, bbUp, bbLow]

// ==========================================
// 3. COMPUTATIONS (4 OUTS)
// ==========================================
[ma1, smMa1, bbUp1, bbLow1] = computeMaPipeline(src, len1, maType, smLength, bbMult)
[ma2, smMa2, bbUp2, bbLow2] = computeMaPipeline(src, len2, maType, smLength, bbMult)
[ma3, smMa3, bbUp3, bbLow3] = computeMaPipeline(src, len3, maType, smLength, bbMult)
[ma4, smMa4, bbUp4, bbLow4] = computeMaPipeline(src, len4, maType, smLength, bbMult)

// ==========================================
// 4. PLOTTING & STYLING
// ==========================================

// Base MAs
pMa1 = plot(ma1, title = "Primary MA 1", color = color.rgb(33, 150, 243), offset = offset, linewidth = 1) // Blue
pMa2 = plot(ma2, title = "Primary MA 2", color = color.rgb(255, 152, 0), offset = offset, linewidth = 2) // Orange
pMa3 = plot(ma3, title = "Primary MA 3", color = color.rgb(156, 39, 176), offset = offset, linewidth = 2) // Purple
pMa4 = plot(ma4, title = "Primary MA 4", color = color.rgb(244, 67, 54), offset = offset, linewidth = 2) // Red

// Secondary Smoothed Lines (hidden by default unless enabled)
plot(smMa1, title = "Smoothed MA 1", color = color.new(color.blue, 30), offset = offset, display = enableSM ? display.all : display.none, editable = enableSM)
plot(smMa2, title = "Smoothed MA 2", color = color.new(color.orange, 30), offset = offset, display = enableSM ? display.all : display.none, editable = enableSM)
plot(smMa3, title = "Smoothed MA 3", color = color.new(color.purple, 30), offset = offset, display = enableSM ? display.all : display.none, editable = enableSM)
plot(smMa4, title = "Smoothed MA 4", color = color.new(color.red, 30), offset = offset, display = enableSM ? display.all : display.none, editable = enableSM)

// Bollinger Bands Plots for MA 1 & MA 2 (Example BB rendering for active channels)
pUp1 = plot(bbUp1, title = "MA 1 BB Upper", color = color.new(color.blue, 60), display = isBB ? display.all : display.none, editable = isBB)
pLow1 = plot(bbLow1, title = "MA 1 BB Lower", color = color.new(color.blue, 60), display = isBB ? display.all : display.none, editable = isBB)
fill(pUp1, pLow1, color = isBB ? color.new(color.blue, 95) : na, title = "MA 1 BB Fill", display = isBB ? display.all : display.none, editable = isBB)

pUp2 = plot(bbUp2, title = "MA 2 BB Upper", color = color.new(color.orange, 60), display = isBB ? display.all : display.none, editable = isBB)
pLow2 = plot(bbLow2, title = "MA 2 BB Lower", color = color.new(color.orange, 60), display = isBB ? display.all : display.none, editable = isBB)
fill(pUp2, pLow2, color = isBB ? color.new(color.orange, 95) : na, title = "MA 2 BB Fill", display = isBB ? display.all : display.none, editable = isBB)