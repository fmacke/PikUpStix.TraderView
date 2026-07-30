//@version=6
indicator("CS FTD Tracker", overlay=true, max_labels_count=500)

// --- Inputs ---
ftd_pct = input.float(1.2, "Min FTD % Change", minval=0.5, step=0.1)
lookback_period = input.int(20, "Lookback for Low", minval=5)

// --- Logic: Rally Attempt Counter ---
var int rallyDay = 0
var float lowPoint = na

isNewLow = low < ta.lowest(low[1], lookback_period)

if isNewLow
    rallyDay := 0
    lowPoint := low
else if rallyDay == 0 and close > close[1] and low > lowPoint
    rallyDay := 1
else if rallyDay > 0
    if low < lowPoint
        rallyDay := 0
        lowPoint := low
    else
        rallyDay := rallyDay + 1

// --- FTD Criteria ---
isFTD = rallyDay >= 4 and (close - close[1]) / close[1] * 100 >= ftd_pct and volume > volume[1]

// --- Visuals using Labels (Fixed) ---
if rallyDay > 0
    // Create a label for each day of the rally attempt
    label.new(
         bar_index, 
         high, 
         text=str.tostring(rallyDay), 
         style=label.style_none, 
         textcolor=color.gray, 
         yloc=yloc.abovebar
         )

if isFTD
    label.new(
         bar_index, 
         low, 
         text="FTD", 
         style=label.style_label_up, 
         color=color.green, 
         textcolor=color.white, 
         yloc=yloc.belowbar
         )

// Plot "The Floor"
plot(lowPoint, "Correction Low", color=color.red, style=plot.style_linebr, linewidth=2)

// Alert Logic
alertcondition(isFTD, "FTD Detected", "FTD on {{ticker}} at {{close}}")//@version=6
indicator("CS FTD Tracker", overlay = true, max_labels_count = 500)

// --- Inputs ---
ftd_pct = input.float(1.2, "Min FTD % Change", minval = 0.5, step = 0.1)
lookback_period = input.int(20, "Lookback for Low", minval = 5)

// --- Logic: Rally Attempt Counter ---
var int rallyDay = 0
var float lowPoint = na

isNewLow = low < ta.lowest(low[1], lookback_period)

if isNewLow
    rallyDay:= 0
lowPoint:= low
else if rallyDay == 0 and close > close[1] and low > lowPoint
rallyDay:= 1
else if rallyDay > 0
    if low < lowPoint
        rallyDay:= 0
lowPoint:= low
    else
rallyDay:= rallyDay + 1

// --- FTD Criteria ---
isFTD = rallyDay >= 4 and(close - close[1]) / close[1] * 100 >= ftd_pct and volume > volume[1]

// --- Visuals using Labels (Fixed) ---
if rallyDay > 0
    // Create a label for each day of the rally attempt
    label.new(
    bar_index,
    high,
    text = str.tostring(rallyDay),
    style = label.style_none,
    textcolor = color.gray,
    yloc = yloc.abovebar
)

if isFTD
    label.new(
    bar_index,
    low,
    text = "FTD",
    style = label.style_label_up,
    color = color.green,
    textcolor = color.white,
    yloc = yloc.belowbar
)

// Plot "The Floor"
plot(lowPoint, "Correction Low", color = color.red, style = plot.style_linebr, linewidth = 2)

// Alert Logic
alertcondition(isFTD, "FTD Detected", "FTD on {{ticker}} at {{close}}")alertcondition(isFTD, "FTD Detected", "FTD on {{ticker}} at {{close}}")