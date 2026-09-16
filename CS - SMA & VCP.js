//@version=6
indicator("CS - SMA & VCP", overlay = true, max_boxes_count = 50, max_lines_count = 50)

// -------------------------------------------------------------------------
// 1. INPUT PARAMETERS
// -------------------------------------------------------------------------
grp_trend = "1. Minervini Trend Template"
use_tt = input.bool(true, "Require Stage 2 Trend Template", group = grp_trend)
len_10 = input.int(10, "SMA 10D", minval = 1, group = grp_trend)
len_21 = input.int(21, "SMA 21D", minval = 1, group = grp_trend)
len_50 = input.int(50, "SMA Fast", minval = 1, group = grp_trend)
len_150 = input.int(150, "SMA Intermediate", minval = 1, group = grp_trend)
len_200 = input.int(200, "SMA Long", minval = 1, group = grp_trend)

grp_vcp = "2. Volatility Contraction Pattern"
sw_left = input.int(5, "Pivot High/Low Left Strength", minval = 2, group = grp_vcp)
sw_right = input.int(3, "Pivot High/Low Right Strength", minval = 1, group = grp_vcp)
atr_len = input.int(20, "ATR Length", group = grp_vcp)
vdu_thresh = input.float(0.60, "VDU Threshold (% of 50 SMA Vol)", minval = 0.1, maxval = 1.0, step = 0.05, group = grp_vcp)
max_depth = input.float(35.0, "Max Initial Base Depth (%)", group = grp_vcp)

// -------------------------------------------------------------------------
// 2. TREND TEMPLATE VALIDATION (STAGE 2)
// -------------------------------------------------------------------------
sma10 = ta.sma(close, len_10)
sma21 = ta.sma(close, len_21)
sma50 = ta.sma(close, len_50)
sma150 = ta.sma(close, len_150)
sma200 = ta.sma(close, len_200)

// Trend Template Rules:
// 1. Price > 150 & 200 SMA
// 2. 150 SMA > 200 SMA
// 3. 200 SMA trending upward (at least flat to up over 1 month / 20 trading bars)
// 4. 50 SMA > 150 & 200 SMA
// 5. Price > 50 SMA
// 6. Price at least 25-30% above 52-week low
// 7. Price within 25% of 52-week high
high52 = ta.highest(high, 252)
low52 = ta.lowest(low, 252)
sma200_rising = sma200 >= sma200[20]

tt_condition = (close > sma150 and close > sma200) and
    (sma150 > sma200) and
               sma200_rising and
    (sma50 > sma150 and sma50 > sma200) and
        (close > sma50) and
            (close >= low52 * 1.25) and
                (close >= high52 * 0.75)

trend_passed = use_tt ? tt_condition : true

// -------------------------------------------------------------------------
// 3. SWING DETECTION & VOLATILITY CONTRACTION
// -------------------------------------------------------------------------
ph = ta.pivothigh(high, sw_left, sw_right)
pl = ta.pivotlow(low, sw_left, sw_right)

var float last_ph = na
var float last_pl = na
var float prev_ph = na
var float prev_pl = na
var float swing_hi_depth = na

if not na(ph)
prev_ph:= last_ph
last_ph:= ph

if not na(pl)
prev_pl:= last_pl
last_pl:= pl
if not na(last_ph)
// Depth of current contraction wave in percentage
swing_hi_depth:= ((last_ph - pl) / last_ph) * 100.0

// Tracking Contraction Waves (Tights)
var float c1_depth = na
var float c2_depth = na
var float c3_depth = na

// Evaluate wave narrowing when a new swing low forms
if not na(pl) and not na(swing_hi_depth)
c3_depth:= c2_depth
c2_depth:= c1_depth
c1_depth:= swing_hi_depth

// Condition: Each subsequent wave is shallower than the prior wave
is_contracting = not na(c1_depth) and not na(c2_depth) and
    (c1_depth < c2_depth) and
        (c2_depth <= max_depth) and
            (c1_depth < 15.0) // Final tight contraction typically < 10-15%

// -------------------------------------------------------------------------
// 4. VOLUME DRY-UP (VDU) & TIGHT CONSOLIDATION
// -------------------------------------------------------------------------
vol_ma50 = ta.sma(volume, 50)
is_vdu = volume < (vol_ma50 * vdu_thresh)

// ATR Compression: 10-bar ATR is below 50-bar ATR
atr10 = ta.atr(10)
atr50 = ta.atr(atr_len)
atr_coiling = atr10 < (atr50 * 0.75)

// Ready state: Stage 2 + Contracting Swings + ATR Coiling + VDU
vcp_setup = trend_passed and is_contracting and atr_coiling

// -------------------------------------------------------------------------
// 5. VISUALIZATION & HIGHLIGHTING
// -------------------------------------------------------------------------
// Moving average ribbons
plot(sma10, "SMA 10", color = color.new(#248de2, 0), linewidth = 1)
plot(sma21, "SMA 21", color = color.new(#f39c21, 0), linewidth = 1)
plot(sma50, "SMA 50", color = color.new(#f31ac8, 0), linewidth = 1)
plot(sma150, "SMA 150", color = color.new(#ad0439, 0), linewidth = 1)
plot(sma200, "SMA 200", color = color.new(#3f0202, 0), linewidth = 2)

// Paint background when volatility is coiled and ready for pivot breakout
bgcolor(vcp_setup ? color.new(color.purple, 88) : na, title = "VCP Coiling Zone")

// Mark Volume Dry-Up days on chart
plotshape(is_vdu and trend_passed, title = "Volume Dry Up", style = shape.circle,
    location = location.belowbar, color = color.teal, size = size.tiny)

// Dynamic Pivot Resistance Line (Cheats / High-Handle Breakout Level)
var line pivot_line = na
if not na(last_ph) and vcp_setup
line.delete(pivot_line)
pivot_line:= line.new(bar_index - sw_right, last_ph, bar_index + 5, last_ph,
    color = color.yellow, width = 2, style = line.style_dashed)

// Plot Breakout Signal
pivot_level = not na(last_ph) ? last_ph : na
breakout = ta.crossover(close, pivot_level) and(volume > vol_ma50 * 1.40) and trend_passed

plotshape(breakout, title = "VCP Breakout", style = shape.triangleup,
    location = location.belowbar, color = color.green, size = size.small, text = "PVI")