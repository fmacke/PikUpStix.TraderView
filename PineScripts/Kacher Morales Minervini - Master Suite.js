//@version=6
indicator("Minervini & Kacher-Morales Master Suite", overlay = true, max_lines_count = 100, max_labels_count = 100, max_boxes_count = 100)

// -------------------------------------------------------------------------
// 0. TIME-FRAME & COMMON INPUTS
// -------------------------------------------------------------------------
grp_cheat = "3. 3-C & Low Cheat Parameters"
bool show_cheat_render = input.bool(true, "Enable 3-C & Low Cheat Rendering & Signals", group = grp_cheat)
bool allow_intraday    = input.bool(false, "Allow 3-C / Low-C on Intraday Timeframes", group = grp_cheat, tooltip = "If false, strictly enforces Daily/Weekly charts for 3-C setups.")
int cup_lookback       = input.int(65, "Cup Lookback Period", group = grp_cheat)
float min_depth        = input.int(12, "Min Base Depth (%)", group = grp_cheat)
int shelf_len          = input.int(7, "Shelf Length", group = grp_cheat)
float max_spread       = input.float(8.0, "Max Shelf Spread (%)", group = grp_cheat)
float vol_spike        = input.float(1.35, "Breakout Volume Spike Multiplier", group = grp_cheat)

bool is_valid_tf       = allow_intraday or timeframe.isdaily or timeframe.isweekly

grp_trend = "1. Trend & Stage 2 Filter"
bool use_tt            = input.bool(true, "Enforce Stage 2 Structural Uptrend", group = grp_trend)
int len_10             = input.int(10, "SMA 10", group = grp_trend)
int len_21             = input.int(21, "SMA 21", group = grp_trend)
int len_50             = input.int(50, "SMA 50", group = grp_trend)
int len_150            = input.int(150, "SMA 150", group = grp_trend)
int len_200            = input.int(200, "SMA 200", group = grp_trend)

grp_vcp = "2. Volatility Contraction Pattern (VCP)"
bool show_vcp_render   = input.bool(true, "Enable VCP Rendering & Signals", group = grp_vcp)
int sw_left            = input.int(5, "Pivot High/Low Left Strength", minval = 2, group = grp_vcp)
int sw_right           = input.int(3, "Pivot High/Low Right Strength", minval = 1, group = grp_vcp)
float vdu_thresh       = input.float(0.60, "VDU Threshold (% of 50 SMA Vol)", minval = 0.1, maxval = 1.0, step = 0.05, group = grp_vcp)
float max_vcp_depth    = input.float(35.0, "Max Initial Base Depth (%)", group = grp_vcp)
bool show_waves        = input.bool(true, "Show Wave Diagonal Lines", group = grp_vcp)
bool show_labels       = input.bool(true, "Show Contraction % Labels", group = grp_vcp)
bool show_dots         = input.bool(true, "Show Swing Tops / Bottoms", group = grp_vcp)

grp_pp = "4. Pocket Pivot Parameters"
int pp_lookback        = input.int(10, "Pocket Pivot Down-Day Lookback", group = grp_pp)
float close_thresh     = input.float(50.0, "Min Close Position in Day's Range (%)", group = grp_pp)
float sma_dist         = input.float(2.0, "Max Distance from SMA (%)", group = grp_pp)
bool enable_pp10       = input.bool(true, "Enable 10-SMA Pocket Pivot", group = grp_pp)
bool enable_pp50       = input.bool(true, "Enable 50-SMA Pocket Pivot", group = grp_pp)

grp_bgu = "5. Buyable Gap-Up (BGU) Parameters"
bool show_bgu_render   = input.bool(true, "Enable BGU Rendering & Signals", group = grp_bgu)
float bgu_min_gap      = input.float(3.0, "Min Gap-Up % from Prior High", minval = 1.0, maxval = 10.0, step = 0.5, group = grp_bgu)
float bgu_vol_mult     = input.float(1.5, "Min Volume Multiplier over 50 SMA Vol", minval = 1.0, maxval = 4.0, step = 0.1, group = grp_bgu)

grp_vis = "6. Moving Average Display Options & Timeframe"
bool show_smas         = input.bool(true, "Show Moving Average Ribbons", group = grp_vis)
bool use_chart_tf      = input.bool(false, "Use Chart Timeframe Instead of Daily", group = grp_vis, tooltip = "If false, SMAs are calculated strictly on daily bars regardless of chart timeframe.")
bool show_10           = input.bool(true, "Show SMA10", group = grp_vis)
bool show_21           = input.bool(true, "Show SMA21", group = grp_vis)
bool show_50           = input.bool(true, "Show SMA50", group = grp_vis)
bool show_150          = input.bool(true, "Show SMA150", group = grp_vis)
bool show_200           = input.bool(true, "Show SMA200", group = grp_vis)

// -------------------------------------------------------------------------
// 1. SHARED CALCULATIONS & TIME-FRAME MULTI-RES MAs
// -------------------------------------------------------------------------
string target_tf = use_chart_tf ? timeframe.period : "D"

float sma10  = request.security(syminfo.tickerid, target_tf, ta.sma(close, len_10), barmerge.gaps_off, barmerge.lookahead_off)
float sma21  = request.security(syminfo.tickerid, target_tf, ta.sma(close, len_21), barmerge.gaps_off, barmerge.lookahead_off)
float sma50  = request.security(syminfo.tickerid, target_tf, ta.sma(close, len_50), barmerge.gaps_off, barmerge.lookahead_off)
float sma150 = request.security(syminfo.tickerid, target_tf, ta.sma(close, len_150), barmerge.gaps_off, barmerge.lookahead_off)
float sma200 = request.security(syminfo.tickerid, target_tf, ta.sma(close, len_200), barmerge.gaps_off, barmerge.lookahead_off)

float vol_ma50 = ta.sma(volume, 50)
float high52   = ta.highest(high, 252)
float low52    = ta.lowest(low, 252)
bool sma200_rising = (sma200 >= sma200[20])

// Stage 2 Trend Template Validation
bool tt_condition = (close > sma150 and close > sma200) and (sma150 > sma200) and sma200_rising and (sma50 > sma150 and sma50 > sma200) and (close > sma50) and (close >= low52 * 1.25) and (close >= high52 * 0.75)
bool trend_passed = use_tt ? tt_condition : true

// -------------------------------------------------------------------------
// 2. VCP & WAVE GEOMETRY ENGINE
// -------------------------------------------------------------------------
float ph = ta.pivothigh(high, sw_left, sw_right)
float pl = ta.pivotlow(low, sw_left, sw_right)

type PivotPoint
    int   bar
    float price
    bool  is_high

var PivotPoint[] pivot_history = array.new<PivotPoint>()
var line[]  wave_lines = array.new<line>()
var label[] wave_labels = array.new<label>()

var float last_ph = na
var int   last_ph_bar = na
var float c1_depth = na
var float c2_depth = na
var float c3_depth = na

f_prune_objects() =>
    if array.size(wave_lines) > 14
        line.delete(array.shift(wave_lines))
    if array.size(wave_labels) > 14
        label.delete(array.shift(wave_labels))

if show_vcp_render and not na(ph)
    int ph_bar = bar_index - sw_right
    last_ph := ph
    last_ph_bar := ph_bar
    array.push(pivot_history, PivotPoint.new(ph_bar, ph, true))
    if array.size(pivot_history) >= 2
        PivotPoint prev = array.get(pivot_history, array.size(pivot_history) - 2)
        if not prev.is_high and show_waves
            array.push(wave_lines, line.new(prev.bar, prev.price, ph_bar, ph, color = color.new(color.gray, 50), width = 1, style = line.style_dotted))
    f_prune_objects()

if show_vcp_render and not na(pl)
    int pl_bar = bar_index - sw_right
    if not na(last_ph)
        float depth = ((last_ph - pl) / last_ph) * 100.0
        c3_depth := c2_depth
        c2_depth := c1_depth
        c1_depth := depth
        if show_waves
            array.push(wave_lines, line.new(last_ph_bar, last_ph, pl_bar, pl, color = color.new(#9c27b0, 20), width = 2))
            f_prune_objects()
        if show_labels
            float label_y = pl - (ta.atr(14) * 0.4)
            array.push(wave_labels, label.new(pl_bar, label_y, text = "-" + str.tostring(depth, "#.#") + "%", color = color.new(#1a237e, 70), textcolor = color.white, style = label.style_label_up, size = size.small))
            f_prune_objects()

bool is_contracting = not na(c1_depth) and not na(c2_depth) and (c1_depth < c2_depth) and (c2_depth <= max_vcp_depth) and (c1_depth < 15.0)
bool is_vdu = volume < (vol_ma50 * vdu_thresh)
float atr10 = ta.atr(10)
float atr50 = ta.atr(20)
bool vcp_setup = show_vcp_render and trend_passed and is_contracting and (atr10 < (atr50 * 0.75))

// -------------------------------------------------------------------------
// 3. 3-C & LOW CHEAT ENGINE
// -------------------------------------------------------------------------
float base_high   = ta.highest(high, cup_lookback)
float base_low    = ta.lowest(low, cup_lookback)
float base_depth  = ((base_high - base_low) / base_high) * 100.0

float shelf_high  = ta.highest(high[1], shelf_len)
float shelf_low   = ta.lowest(low[1], shelf_len)
float shelf_spread= ((shelf_high - shelf_low) / shelf_high) * 100.0

float recovery_ratio = (base_high > base_low) ? ((close[1] - base_low) / (base_high - base_low)) : 0.0
bool vdu_in_shelf    = (ta.lowest(volume[1], shelf_len) < (vol_ma50 * 0.65))

bool base_valid      = is_valid_tf and trend_passed and (base_depth >= min_depth and base_depth <= 45.0) and (shelf_spread <= max_spread) and vdu_in_shelf

bool is_low_cheat    = base_valid and (recovery_ratio >= 0.12 and recovery_ratio < 0.35)
bool is_classic_3c   = base_valid and (recovery_ratio >= 0.35 and recovery_ratio <= 0.85)

// -------------------------------------------------------------------------
// 4. POCKET PIVOT ENGINE
// -------------------------------------------------------------------------
float down_volume   = (close < close[1]) ? volume : 0.0
float max_down_vol  = ta.highest(down_volume, pp_lookback)[1]
float day_range     = high - low
float close_pos     = (day_range > 0.0) ? ((close - low) / day_range) * 100.0 : 50.0

float dist_10       = math.abs((close - sma10) / sma10) * 100.0
float dist_50       = math.abs((close - sma50) / sma50) * 100.0

bool touched_10     = (low <= sma10 and high >= sma10) or (low <= sma10 * 1.015)
bool touched_50     = (low <= sma50 and high >= sma50) or (low <= sma50 * 1.015)

bool is_up_day      = close > close[1]
bool pp_vol_valid   = volume > max_down_vol
bool pp_close_strong= close_pos >= close_thresh

bool pp10_signal    = enable_pp10 and is_up_day and pp_vol_valid and pp_close_strong and touched_10 and (dist_10 <= sma_dist) and (close > sma200 or na(sma200)) and trend_passed
bool pp50_signal    = enable_pp50 and is_up_day and pp_vol_valid and pp_close_strong and touched_50 and (dist_50 <= sma_dist) and (close > sma200 or na(sma200)) and trend_passed
bool is_pocket_pivot= pp10_signal or pp50_signal

// -------------------------------------------------------------------------
// 5. BUYABLE GAP-UP (BGU) ENGINE
// -------------------------------------------------------------------------
bool is_bgu_raw     = (open >= high[1] * (1.0 + (bgu_min_gap / 100.0))) and (close > open) and (volume > vol_ma50 * bgu_vol_mult) and trend_passed
bool trigger_bgu    = show_bgu_render and is_bgu_raw

// -------------------------------------------------------------------------
// 6. BREAKOUT TRIGGERS & COMBINED RENDERING LOGIC
// -------------------------------------------------------------------------
bool vol_breakout   = volume > (vol_ma50 * vol_spike)
bool trigger_c3     = is_valid_tf and show_cheat_render and is_classic_3c and ta.crossover(close, shelf_high) and vol_breakout
bool trigger_low    = is_valid_tf and show_cheat_render and is_low_cheat and ta.crossover(close, shelf_high) and vol_breakout
bool vcp_breakout   = show_vcp_render and not na(last_ph) and ta.crossover(close, last_ph) and (volume > vol_ma50 * 1.40) and trend_passed

bool any_major_trigger = vcp_breakout or trigger_bgu or trigger_c3 or trigger_low or pp10_signal or pp50_signal

string combined_text = ""
if any_major_trigger
    if vcp_breakout
        combined_text := combined_text + (combined_text != "" ? " / " : "") + "PVI"
    if trigger_bgu
        combined_text := combined_text + (combined_text != "" ? " / " : "") + "BGU"
    if trigger_c3
        combined_text := combined_text + (combined_text != "" ? " / " : "") + "3-C"
    if trigger_low
        combined_text := combined_text + (combined_text != "" ? " / " : "") + "LOW-C"
    if pp10_signal
        combined_text := combined_text + (combined_text != "" ? " / " : "") + "PP10"
    if pp50_signal
        combined_text := combined_text + (combined_text != "" ? " / " : "") + "PP50"

// -------------------------------------------------------------------------
// 7. VISUALIZATION & CONFLICT-FREE RENDERING HIERARCHY
// -------------------------------------------------------------------------
plot(show_10 and show_smas ? sma10 : na, "SMA 10", color = color.new(#3de224, 0), linewidth = 1)
plot(show_21 and show_smas ? sma21 : na, "SMA 21", color = color.new(#f39c21, 0), linewidth = 1)
plot(show_50 and show_smas ? sma50 : na, "SMA 50", color = color.new(#f31ac8, 0), linewidth = 1)
plot(show_150 and show_smas ? sma150 : na, "SMA 150", color = color.new(#ad0439, 0), linewidth = 1)
plot(show_200 and show_smas ? sma200 : na, "SMA 200", color = color.new(#3f0202, 0), linewidth = 2)

plotshape(show_vcp_render and show_dots and not na(ph), "Swing High", style = shape.triangledown, location = location.abovebar, color = color.red, size = size.tiny, offset = -sw_right)
plotshape(show_vcp_render and show_dots and not na(pl), "Swing Low", style = shape.triangleup, location = location.belowbar, color = color.green, size = size.tiny, offset = -sw_right)

color bg_color = (show_cheat_render and is_classic_3c) ? color.new(color.blue, 75) : (show_cheat_render and is_low_cheat) ? color.new(color.purple, 75) : vcp_setup ? color.new(#47ef47, 75) : is_pocket_pivot ? color.new(color.green, 75) : na
bgcolor(bg_color, title = "Master Setup Zone")

barcolor(is_pocket_pivot ? color.lime : na, title = "Pocket Pivot Bar Color")

// Render Combined Breakout & Pocket Pivot Labels dynamically via label.new
if any_major_trigger
    label.new(bar_index, low - (ta.atr(14) * 0.8), text = combined_text, color = color.green, textcolor = color.white, style = label.style_label_up, size = size.small)

// Secondary Volume Dry Up Marker
plotshape(show_vcp_render and is_vdu and trend_passed, title = "Volume Dry Up", style = shape.circle, location = location.belowbar, color = color.rgb(116, 154, 249), size = size.tiny)

var line cheat_line = na
if show_cheat_render and (is_classic_3c or is_low_cheat)
    line.delete(cheat_line)
    cheat_line := line.new(bar_index - shelf_len, shelf_high, bar_index + 2, shelf_high, color = is_classic_3c ? color.yellow : color.fuchsia, width = 2, style = line.style_solid)

var line vcp_line = na
if show_vcp_render and not na(last_ph) and vcp_setup
    line.delete(vcp_line)
    vcp_line := line.new(last_ph_bar, last_ph, bar_index + 5, last_ph, color = color.yellow, width = 2, style = line.style_dashed)

// -------------------------------------------------------------------------
// 8. TRADINGVIEW ALERTS
// -------------------------------------------------------------------------
alertcondition(trigger_c3, title = "3-C Breakout Alert", message = "3-C Breakout triggered on {{ticker}}!")
alertcondition(trigger_low, title = "Low Cheat Breakout Alert", message = "Low Cheat Breakout triggered on {{ticker}}!")
alertcondition(vcp_breakout, title = "VCP Breakout Alert", message = "VCP Pivot Breakout triggered on {{ticker}}!")
alertcondition(trigger_bgu, title = "Buyable Gap-Up Alert", message = "Buyable Gap-Up triggered on {{ticker}}!")
alertcondition(is_pocket_pivot, title = "Pocket Pivot Alert", message = "Pocket Pivot detected on {{ticker}}!")