//@version=6
indicator("Minervini - 3-C & Low Cheat", overlay = true, max_lines_count = 100, max_labels_count = 100)

// -------------------------------------------------------------------------
// 1. INPUT PARAMETERS
// -------------------------------------------------------------------------
grp_trend = "1. Trend & Filter Parameters"
use_tt = input.bool(true, "Enforce Stage 2 Structural Uptrend", group = grp_trend)
len_50 = input.int(50, "SMA 50D", minval = 1, group = grp_trend)
len_150 = input.int(150, "SMA 150D", minval = 1, group = grp_trend)
len_200 = input.int(200, "SMA 200D", minval = 1, group = grp_trend)

grp_cup = "2. Base Construction"
cup_lookback = input.int(65, "Base Detection Lookback (Bars)", minval = 20, maxval = 150, group = grp_cup)
min_depth = input.float(12.0, "Min Base Depth (%)", group = grp_cup)
max_depth = input.float(45.0, "Max Base Depth (%)", group = grp_cup)

grp_shelf = "3. Shelf Mechanics & Thresholds"
shelf_len = input.int(7, "Shelf Lookback Window (Bars)", minval = 4, maxval = 15, group = grp_shelf)
max_spread = input.float(8.0, "Max Shelf Range (%)", minval = 2.0, maxval = 12.0, group = grp_shelf)
vdu_ratio = input.float(0.65, "VDU Ratio (% of 50 SMA Vol)", group = grp_shelf)
vol_spike = input.float(1.35, "Breakout Volume Expansion Multiplier", group = grp_shelf)

// Recovery Ratio thresholds
low_cheat_min = input.float(0.12, "Low Cheat Min Recovery", group = grp_shelf)
low_cheat_max = input.float(0.35, "Low Cheat Max Recovery", group = grp_shelf)
c3_cheat_min = input.float(0.35, "Classic 3-C Min Recovery", group = grp_shelf)
c3_cheat_max = input.float(0.85, "Classic 3-C Max Recovery", group = grp_shelf)

// -------------------------------------------------------------------------
// 2. MOVING AVERAGES & STRUCTURAL TREND
// -------------------------------------------------------------------------
sma50 = ta.sma(close, len_50)
sma150 = ta.sma(close, len_150)
sma200 = ta.sma(close, len_200)

// For Low Cheats, 200 SMA can be flattening while price reclaims the 50/150 SMA
sma200_rising = sma200 >= sma200[20]
tt_passed = not use_tt or(close > sma150 and close > sma200 and sma150 > sma200 and sma200_rising)

plot(sma50, "SMA 50", color = color.new(#f31ac8, 0), linewidth = 1)
plot(sma150, "SMA 150", color = color.new(#ad0439, 0), linewidth = 1)
plot(sma200, "SMA 200", color = color.new(#3f0202, 0), linewidth = 2)

// -------------------------------------------------------------------------
// 3. BASE DETECTION & RECOVERY CLASSIFICATION
// -------------------------------------------------------------------------
base_high = ta.highest(high, cup_lookback)
base_low = ta.lowest(low, cup_lookback)
base_depth = ((base_high - base_low) / base_high) * 100.0

shelf_high = ta.highest(high[1], shelf_len)
shelf_low = ta.lowest(low[1], shelf_len)
shelf_spread = ((shelf_high - shelf_low) / shelf_high) * 100.0

// Retracement Position: 0.0 = at base low, 1.0 = at base high
recovery_ratio = (base_high > base_low) ? ((close[1] - base_low) / (base_high - base_low)) : 0.0

// Volume Dry-Up within shelf
vol_ma50 = ta.sma(volume, 50)
vdu_in_shelf = ta.lowest(volume[1], shelf_len) < (vol_ma50 * vdu_ratio)

// Shared baseline criteria
base_valid = tt_passed and(base_depth >= min_depth and base_depth <= max_depth) and(shelf_spread <= max_spread) and vdu_in_shelf

// Classification
is_low_cheat = base_valid and(recovery_ratio >= low_cheat_min and recovery_ratio < low_cheat_max)
is_classic_3c = base_valid and(recovery_ratio >= c3_cheat_min and recovery_ratio <= c3_cheat_max)

// Visual shelf highlights
bgcolor(is_classic_3c ? color.new(color.blue, 90) : is_low_cheat ? color.new(color.purple, 90) : na, title = "Active Cheat Shelf")

// -------------------------------------------------------------------------
// 4. DYNAMIC PIVOT LINES & TRIGGERS
// -------------------------------------------------------------------------
var line cheat_line = na
if is_classic_3c or is_low_cheat
line.delete(cheat_line)
cheat_color = is_classic_3c ? color.yellow : color.fuchsia
cheat_line:= line.new(bar_index - shelf_len, shelf_high, bar_index + 2, shelf_high,
    color = cheat_color, width = 2, style = line.style_solid)

vol_breakout = volume > (vol_ma50 * vol_spike)

trigger_c3 = is_classic_3c and ta.crossover(close, shelf_high) and vol_breakout
trigger_low_cheat = is_low_cheat  and ta.crossover(close, shelf_high) and vol_breakout

// Print Triggers
plotshape(trigger_c3, title = "3-C Breakout", style = shape.triangleup,
    location = location.belowbar, color = color.aqua, size = size.small, text = "3-C")

plotshape(trigger_low_cheat, title = "Low Cheat Breakout", style = shape.triangleup,
    location = location.belowbar, color = color.fuchsia, size = size.small, text = "LOW 3-C")

// Shelf boundary plots
plot(is_classic_3c or is_low_cheat ? shelf_high : na, "Shelf Pivot", color = color.new(color.yellow, 40), style = plot.style_linebr)
plot(is_classic_3c or is_low_cheat ? shelf_low : na, "Shelf Floor", color = color.new(color.gray, 60), style = plot.style_linebr)