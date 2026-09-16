//@version=6
indicator("Minervini - 3-C & Low Cheat", overlay = true, max_lines_count = 100, max_labels_count = 100)

// -------------------------------------------------------------------------
// 0. TIMEFRAME RESTRICTION (Daily and Weekly Only)
// -------------------------------------------------------------------------
is_allowed_tf = timeframe.isdaily or timeframe.isweekly

// -------------------------------------------------------------------------
// 1. INPUT PARAMETERS
// -------------------------------------------------------------------------
grp_trend = "1. Trend & Filter Parameters"
use_tt = input.bool(true, "Enforce Stage 2 Structural Uptrend", group = grp_trend)
len_50 = 50
len_150 = 150
len_200 = 200

grp_cup = "2. Base Construction"
cup_lookback = 65
min_depth = 12.0
max_depth = 45.0

grp_shelf = "3. Shelf Mechanics & Thresholds"
shelf_len = 7
max_spread = 8.0
vdu_ratio = 0.65
vol_spike = 1.35

// Recovery Ratio thresholds
low_cheat_min = 0.12
low_cheat_max = 0.35
c3_cheat_min = 0.35
c3_cheat_max = 0.85

// -------------------------------------------------------------------------
// 2. MOVING AVERAGES & STRUCTURAL TREND
// -------------------------------------------------------------------------
sma50 = is_allowed_tf ? ta.sma(close, len_50) : na
sma150 = is_allowed_tf ? ta.sma(close, len_150) : na
sma200 = is_allowed_tf ? ta.sma(close, len_200) : na

sma200_rising = is_allowed_tf and(sma200 >= sma200[20])
tt_passed = is_allowed_tf and(not use_tt or(close > sma150 and close > sma200 and sma150 > sma200 and sma200_rising))

// -------------------------------------------------------------------------
// 3. BASE DETECTION & RECOVERY CLASSIFICATION
// -------------------------------------------------------------------------
base_high = is_allowed_tf ? ta.highest(high, cup_lookback) : na
base_low = is_allowed_tf ? ta.lowest(low, cup_lookback) : na
base_depth = is_allowed_tf ? (((base_high - base_low) / base_high) * 100.0) : na

shelf_high = is_allowed_tf ? ta.highest(high[1], shelf_len) : na
shelf_low = is_allowed_tf ? ta.lowest(low[1], shelf_len) : na
shelf_spread = is_allowed_tf ? (((shelf_high - shelf_low) / shelf_high) * 100.0) : na

recovery_ratio = (is_allowed_tf and base_high > base_low) ?((close[1] - base_low) / (base_high - base_low)) : 0.0

vol_ma50 = is_allowed_tf ? ta.sma(volume, 50) : na
vdu_in_shelf = is_allowed_tf and(ta.lowest(volume[1], shelf_len) < (vol_ma50 * vdu_ratio))

base_valid = is_allowed_tf and tt_passed and(base_depth >= min_depth and base_depth <= max_depth) and(shelf_spread <= max_spread) and vdu_in_shelf

is_low_cheat = base_valid and(recovery_ratio >= low_cheat_min and recovery_ratio < low_cheat_max)
is_classic_3c = base_valid and(recovery_ratio >= c3_cheat_min and recovery_ratio <= c3_cheat_max)

bgcolor(is_classic_3c ? color.new(color.blue, 90) : is_low_cheat ? color.new(color.purple, 90) : na, title = "Active Cheat Shelf")

// -------------------------------------------------------------------------
// 4. DYNAMIC PIVOT LINES & TRIGGERS
// -------------------------------------------------------------------------
var line cheat_line = na

if is_allowed_tf and(is_classic_3c or is_low_cheat)
line.delete(cheat_line)
cheat_color = is_classic_3c ? color.yellow : color.fuchsia
cheat_line:= line.new(bar_index - shelf_len, shelf_high, bar_index + 2, shelf_high,
    color = cheat_color, width = 2, style = line.style_solid)

vol_breakout = is_allowed_tf and(volume > (vol_ma50 * vol_spike))

trigger_c3 = is_allowed_tf and is_classic_3c and ta.crossover(close, shelf_high) and vol_breakout
trigger_low_cheat = is_allowed_tf and is_low_cheat  and ta.crossover(close, shelf_high) and vol_breakout

// Print Triggers
plotshape(trigger_c3, title = "3-C Breakout", style = shape.triangleup,
    location = location.belowbar, color = color.aqua, size = size.small, text = "3-C")

plotshape(trigger_low_cheat, title = "Low Cheat Breakout", style = shape.triangleup,
    location = location.belowbar, color = color.fuchsia, size = size.small, text = "LOW 3-C")

// Shelf boundary plots
plot(is_allowed_tf and(is_classic_3c or is_low_cheat) ? shelf_high : na, "Shelf Pivot", color = color.new(color.yellow, 40), style = plot.style_linebr)
plot(is_allowed_tf and(is_classic_3c or is_low_cheat) ? shelf_low : na, "Shelf Floor", color = color.new(color.gray, 60), style = plot.style_linebr)