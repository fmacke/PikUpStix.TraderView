//@version=6
indicator("CS Distribution", overlay=true, max_boxes_count=500, max_labels_count=500)

// ==========================================
// 1. INPUTS & CONSTANTS
// ========================================== 
int LOOKBACK_WINDOW = input.int(25, "Distribution Expiry (Bars)", minval=1)
float INVALIDATE_PCT = input.float(5.0, "Price Invalidation Threshold (%)", minval=0.1)

// ==========================================
// 2. DATA ACQUISITION (MULTI-SYMBOL)
// ==========================================
// Explicitly requesting daily resolution to match CAN SLIM institutional tracking
[spx_c, spx_h, spx_l, spx_v] = request.security("SP:SPX", "D", [close, high, low, volume])
[nas_c, nas_h, nas_l, nas_v] = request.security("NASDAQ:IXIC", "D", [close, high, low, volume])

// ==========================================
// 3. CAN SLIM DISTRIBUTION & CHURNING LOGIC
// ==========================================
calc_distribution_score(float c, float h, float l, float v) =>
    bool price_down   = c < c[1]
    bool volume_up    = v > v[1]
    
    // Churning: Closes in the lower half of the daily range on higher volume, stalling out
    float daily_range = h - l
    bool closing_low  = daily_range > 0 and (c - l) / daily_range <= 0.5
    bool is_churning  = not price_down and closing_low and volume_up
    
    (price_down and volume_up) or is_churning ? c : 0.0

float spx_trigger_price = calc_distribution_score(spx_c, spx_h, spx_l, spx_v)
float nas_trigger_price = calc_distribution_score(nas_c, nas_h, nas_l, nas_v)

// ==========================================
// 4. DYNAMIC REGIME STATE ENGINE (ARRAYS)
// ==========================================
// Persistent historical tracking arrays for active distribution day closing points
var float[] spx_dist_prices = array.new_float(0)
var int[]   spx_dist_ages   = array.new_int(0)
var float[] nas_dist_prices = array.new_float(0)
var int[]   nas_dist_ages   = array.new_int(0)

// Helper function to update state vectors per bar
update_regime_matrix(float trigger_price, float current_close, float[] price_arr, int[] age_arr, int max_age, float invalid_pct) =>
    // 1. Age existing elements
    if array.size(age_arr) > 0
        for i = 0 to array.size(age_arr) - 1
            array.set(age_arr, i, array.get(age_arr, i) + 1)
            
    // 2. Insert fresh distribution item
    if trigger_price > 0.0
        array.push(price_arr, trigger_price)
        array.push(age_arr, 0)
        
    // 3. Filter out expired or price-invalidated (5% rally) items using downward mutation
    if array.size(price_arr) > 0
        for i = array.size(price_arr) - 1 to 0
            float orig_price = array.get(price_arr, i)
            int item_age     = array.get(age_arr, i)
            bool price_invalidated = current_close >= orig_price * (1.0 + (invalid_pct / 100.0))
            
            if item_age > max_age or price_invalidated
                array.remove(price_arr, i)
                array.remove(age_arr, i)
                
    array.size(price_arr)

int active_spx_count = update_regime_matrix(spx_trigger_price, spx_c, spx_dist_prices, spx_dist_ages, LOOKBACK_WINDOW, INVALIDATE_PCT)
int active_nas_count = update_regime_matrix(nas_trigger_price, nas_c, nas_dist_prices, nas_dist_ages, LOOKBACK_WINDOW, INVALIDATE_PCT)

// Determine worst-case scenario constraint across indices
int max_active_distribution = math.max(active_spx_count, active_nas_count)

// ==========================================
// 5. MOVING AVERAGE VIOLATION CHECKS
// ==========================================
nas_sma50 = ta.sma(nas_c, 50)
bool nas_break_50_heavy = (nas_c < nas_sma50) and (nas_v > nas_v[1])

// ==========================================
// 6. REGIME EVALUATION & COLOR EXTRACTION
// ==========================================
bool is_correction = max_active_distribution >= 6 or nas_break_50_heavy
bool is_under_pressure = not is_correction and (max_active_distribution >= 4)

color regime_color = is_correction ? color.new(color.red, 90) : is_under_pressure ? color.new(color.orange, 90) : color.new(color.green, 93)
bgcolor(regime_color)

// ==========================================
// 7. DIAGNOSTIC DATA MONITOR
// ==========================================
var table info_table = table.new(position.top_right, 2, 4, bgcolor=color.new(color.black, 30), border_color=color.black, border_width=1)
if barstate.islast
    table.cell(info_table, 0, 0, "Index Metric", text_color=color.white, text_size=size.small)
    table.cell(info_table, 1, 0, "Active Dist. Days", text_color=color.white, text_size=size.small)
    
    table.cell(info_table, 0, 1, "S&P 500 (SPX)", text_color=color.white, text_size=size.small)
    table.cell(info_table, 1, 1, str.tostring(active_spx_count), text_color=color.white, text_size=size.small)
    
    table.cell(info_table, 0, 2, "Nasdaq (IXIC)", text_color=color.white, text_size=size.small)
    table.cell(info_table, 1, 2, str.tostring(active_nas_count), text_color=color.white, text_size=size.small)
    
    string status_text = is_correction ? "MARKET IN CORRECTION" : is_under_pressure ? "UPTREND UNDER PRESSURE" : "CONFIRMED UPTREND"
    color status_color = is_correction ? color.red : is_under_pressure ? color.orange : color.green
    table.cell(info_table, 0, 3, "Current Status:", text_color=color.white, text_size=size.small)
    table.cell(info_table, 1, 3, status_text, text_color=status_color, text_size=size.small)//@version=6
indicator("CS Distribution", overlay=true, max_boxes_count=500, max_labels_count=500)

// ==========================================
// 1. INPUTS & CONSTANTS
// ========================================== 
int LOOKBACK_WINDOW = input.int(25, "Distribution Expiry (Bars)", minval=1)
float INVALIDATE_PCT = input.float(5.0, "Price Invalidation Threshold (%)", minval=0.1)

// ==========================================
// 2. DATA ACQUISITION (MULTI-SYMBOL)
// ==========================================
// Explicitly requesting daily resolution to match CAN SLIM institutional tracking
[spx_c, spx_h, spx_l, spx_v] = request.security("SP:SPX", "D", [close, high, low, volume])
[nas_c, nas_h, nas_l, nas_v] = request.security("NASDAQ:IXIC", "D", [close, high, low, volume])

// ==========================================
// 3. CAN SLIM DISTRIBUTION & CHURNING LOGIC
// ==========================================
calc_distribution_score(float c, float h, float l, float v) =>
    bool price_down   = c < c[1]
    bool volume_up    = v > v[1]
    
    // Churning: Closes in the lower half of the daily range on higher volume, stalling out
    float daily_range = h - l
    bool closing_low  = daily_range > 0 and (c - l) / daily_range <= 0.5
    bool is_churning  = not price_down and closing_low and volume_up
    
    (price_down and volume_up) or is_churning ? c : 0.0

float spx_trigger_price = calc_distribution_score(spx_c, spx_h, spx_l, spx_v)
float nas_trigger_price = calc_distribution_score(nas_c, nas_h, nas_l, nas_v)

// ==========================================
// 4. DYNAMIC REGIME STATE ENGINE (ARRAYS)
// ==========================================
// Persistent historical tracking arrays for active distribution day closing points
var float[] spx_dist_prices = array.new_float(0)
var int[]   spx_dist_ages   = array.new_int(0)
var float[] nas_dist_prices = array.new_float(0)
var int[]   nas_dist_ages   = array.new_int(0)

// Helper function to update state vectors per bar
update_regime_matrix(float trigger_price, float current_close, float[] price_arr, int[] age_arr, int max_age, float invalid_pct) =>
    // 1. Age existing elements
    if array.size(age_arr) > 0
        for i = 0 to array.size(age_arr) - 1
            array.set(age_arr, i, array.get(age_arr, i) + 1)
            
    // 2. Insert fresh distribution item
    if trigger_price > 0.0
        array.push(price_arr, trigger_price)
        array.push(age_arr, 0)
        
    // 3. Filter out expired or price-invalidated (5% rally) items using downward mutation
    if array.size(price_arr) > 0
        for i = array.size(price_arr) - 1 to 0
            float orig_price = array.get(price_arr, i)
            int item_age     = array.get(age_arr, i)
            bool price_invalidated = current_close >= orig_price * (1.0 + (invalid_pct / 100.0))
            
            if item_age > max_age or price_invalidated
                array.remove(price_arr, i)
                array.remove(age_arr, i)
                
    array.size(price_arr)

int active_spx_count = update_regime_matrix(spx_trigger_price, spx_c, spx_dist_prices, spx_dist_ages, LOOKBACK_WINDOW, INVALIDATE_PCT)
int active_nas_count = update_regime_matrix(nas_trigger_price, nas_c, nas_dist_prices, nas_dist_ages, LOOKBACK_WINDOW, INVALIDATE_PCT)

// Determine worst-case scenario constraint across indices
int max_active_distribution = math.max(active_spx_count, active_nas_count)

// ==========================================
// 5. MOVING AVERAGE VIOLATION CHECKS
// ==========================================
nas_sma50 = ta.sma(nas_c, 50)
bool nas_break_50_heavy = (nas_c < nas_sma50) and (nas_v > nas_v[1])

// ==========================================
// 6. REGIME EVALUATION & COLOR EXTRACTION
// ==========================================
bool is_correction = max_active_distribution >= 6 or nas_break_50_heavy
bool is_under_pressure = not is_correction and (max_active_distribution >= 4)

color regime_color = is_correction ? color.new(color.red, 90) : is_under_pressure ? color.new(color.orange, 90) : color.new(color.green, 93)
bgcolor(regime_color)

// ==========================================
// 7. DIAGNOSTIC DATA MONITOR
// ==========================================
var table info_table = table.new(position.top_right, 2, 4, bgcolor=color.new(color.black, 30), border_color=color.black, border_width=1)
if barstate.islast
    table.cell(info_table, 0, 0, "Index Metric", text_color=color.white, text_size=size.small)
    table.cell(info_table, 1, 0, "Active Dist. Days", text_color=color.white, text_size=size.small)
    
    table.cell(info_table, 0, 1, "S&P 500 (SPX)", text_color=color.white, text_size=size.small)
    table.cell(info_table, 1, 1, str.tostring(active_spx_count), text_color=color.white, text_size=size.small)
    
    table.cell(info_table, 0, 2, "Nasdaq (IXIC)", text_color=color.white, text_size=size.small)
    table.cell(info_table, 1, 2, str.tostring(active_nas_count), text_color=color.white, text_size=size.small)
    
    string status_text = is_correction ? "MARKET IN CORRECTION" : is_under_pressure ? "UPTREND UNDER PRESSURE" : "CONFIRMED UPTREND"
    color status_color = is_correction ? color.red : is_under_pressure ? color.orange : color.green
    table.cell(info_table, 0, 3, "Current Status:", text_color=color.white, text_size=size.small)
    table.cell(info_table, 1, 3, status_text, text_color=status_color, text_size=size.small)