//@version=6
indicator("CS - SMA & VCP", overlay = true, max_boxes_count = 100, max_lines_count = 100, max_labels_count = 100)

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

grp_waves = "3. Wave Geometry & Contraction Display (Optional)"
show_waves = input.bool(true, "Show Wave Diagonal Lines", group = grp_waves)
show_labels = input.bool(true, "Show Contraction % Labels", group = grp_waves)
show_dots = input.bool(true, "Show Swing Tops / Bottoms", group = grp_waves)
max_history = input.int(14, "Retained Wave Lines/Labels", minval = 4, maxval = 40, group = grp_waves)

// -------------------------------------------------------------------------
// 2. TREND TEMPLATE VALIDATION (STAGE 2)
// -------------------------------------------------------------------------
sma10 = ta.sma(close, len_10)
sma21 = ta.sma(close, len_21)
sma50 = ta.sma(close, len_50)
sma150 = ta.sma(close, len_150)
sma200 = ta.sma(close, len_200)

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
// 3. SWING DETECTION & WAVE GEOMETRY
// -------------------------------------------------------------------------
ph = ta.pivothigh(high, sw_left, sw_right)
pl = ta.pivotlow(low, sw_left, sw_right)

// Custom data type to coordinate swing legs accurately
type PivotPoint
    int   bar
    float price
    bool  is_high

var PivotPoint[] pivot_history = array.new < PivotPoint > ()
var line[]  wave_lines = array.new < line > ()
var label[] wave_labels = array.new < label > ()

var float last_ph = na
var int   last_ph_bar = na
var float last_pl = na
var float c1_depth = na
var float c2_depth = na
var float c3_depth = na

// Garbage collection to stay within max element limits
f_prune_objects() =>
if array.size(wave_lines) > max_history
        line.delete(array.shift(wave_lines))
if array.size(wave_labels) > max_history
        label.delete(array.shift(wave_labels))

// Pivot High Confirmed
if not na(ph)
ph_bar = bar_index - sw_right
last_ph:= ph
last_ph_bar:= ph_bar
array.push(pivot_history, PivotPoint.new(ph_bar, ph, true))

// Draw upward diagonal connecting prior low to current high
if array.size(pivot_history) >= 2
        prev = array.get(pivot_history, array.size(pivot_history) - 2)
if not prev.is_high and show_waves
ln = line.new(prev.bar, prev.price, ph_bar, ph,
    color = color.new(color.gray, 50), width = 1, style = line.style_dotted)
array.push(wave_lines, ln)
f_prune_objects()

// Pivot Low Confirmed
if not na(pl)
pl_bar = bar_index - sw_right
last_pl:= pl
array.push(pivot_history, PivotPoint.new(pl_bar, pl, false))

if not na(last_ph)
depth = ((last_ph - pl) / last_ph) * 100.0
c3_depth:= c2_depth
c2_depth:= c1_depth
c1_depth:= depth

// Draw downward contraction leg from top to bottom
if show_waves
            ln = line.new(last_ph_bar, last_ph, pl_bar, pl,
    color = color.new(#9c27b0, 20), width = 2)
array.push(wave_lines, ln)
f_prune_objects()

// Plot contraction percentage beneath trough
if show_labels
            lbl = label.new(pl_bar, pl, text = "-" + str.tostring(depth, "#.#") + "%",
    color = color.new(#1a237e, 10), textcolor = color.white,
    style = label.style_label_up, size = size.small)
array.push(wave_labels, lbl)
f_prune_objects()

// -------------------------------------------------------------------------
// 4. CONTRACTION RATIOS, ATR COILING & VDU
// -------------------------------------------------------------------------
is_contracting = not na(c1_depth) and not na(c2_depth) and
    (c1_depth < c2_depth) and
        (c2_depth <= max_depth) and
            (c1_depth < 15.0)

vol_ma50 = ta.sma(volume, 50)
is_vdu = volume < (vol_ma50 * vdu_thresh)

atr10 = ta.atr(10)
atr50 = ta.atr(atr_len)
atr_coiling = atr10 < (atr50 * 0.75)

vcp_setup = trend_passed and is_contracting and atr_coiling

// -------------------------------------------------------------------------
// 5. VISUALIZATION & HIGHLIGHTING
// -------------------------------------------------------------------------
// Moving average ribbons
plot(sma10, "SMA 10", color = color.new(#248de2, 0), linewidth = 1)
plot(sma21, "SMA 21", color = color.new(#f39c21, 0), linewidth = 1)
plot(sma50, "SMA 50", color = color.new(#f31ac8, 0), linewidth = 1)
plot(sma150, "SMA 150", color = color.new(#ad0439, 0), linewidth = 1)
plot(sma200, "SMA 200", color = color.new(#3f0202, 0), linewidth = 3)

// Swing markers
plotshape(show_dots and not na(ph), "Swing High", shape.triangledown,
    location = location.abovebar, color = color.red, size = size.tiny, offset = -sw_right)
plotshape(show_dots and not na(pl), "Swing Low", shape.triangleup,
    location = location.belowbar, color = color.green, size = size.tiny, offset = -sw_right)

// VCP Coiling Zone & Volume Dry-Up Markers
bgcolor(vcp_setup ? color.new(#47ef47, 88) : na, title = "VCP Coiling Zone")
plotshape(is_vdu and trend_passed, title = "Volume Dry Up", style = shape.circle,
    location = location.belowbar, color = color.rgb(116, 154, 249), size = size.tiny)

// Dynamic Pivot Resistance Line
var line pivot_line = na
if not na(last_ph) and vcp_setup
line.delete(pivot_line)
pivot_line:= line.new(last_ph_bar, last_ph, bar_index + 5, last_ph,
    color = color.yellow, width = 2, style = line.style_dashed)

// Breakout Signal
pivot_level = not na(last_ph) ? last_ph : na
breakout = ta.crossover(close, pivot_level) and(volume > vol_ma50 * 1.40) and trend_passed

plotshape(breakout, title = "VCP Breakout", style = shape.triangleup,
    location = location.belowbar, color = color.green, size = size.small, text = "PVI")