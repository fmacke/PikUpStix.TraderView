//@version=6
indicator("CS - EPS Growth", overlay = true)

// --- Inputs ---
float minGrowth = input.float(25.0, "CAN SLIM Min EPS Growth %", minval = 0.0)
bool  showTable = input.bool(true, "Show CAN SLIM Summary Table")
string tblPos = input.string("top_right", "Table Position",
    options = ["top_right", "bottom_right", "top_left", "bottom_left"])

// --- Fetch Quarterly Diluted Normalized EPS ---
// FactSet provides both normalized and standard diluted EPS; normalized tracks closer to IBD operating EPS
float epsQ = request.financial(syminfo.tickerid, "EARNINGS_PER_SHARE_DILUTED", "FQ")

// Detect reporting update
bool isNewQuarter = ta.change(epsQ) != 0 and not na(epsQ)

// Array to store historical fiscal quarters (Q0 = latest, Q4 = same quarter prior year)
var float[] epsHistory = array.new_float(0)

if isNewQuarter
    array.unshift(epsHistory, epsQ)
if array.size(epsHistory) > 8
        array.pop(epsHistory)

// Calculate YoY EPS Growth: (Q_0 - Q_4) / |Q_4| * 100
var float yoyGrowth = na
if isNewQuarter and array.size(epsHistory) >= 5
    float currEps = array.get(epsHistory, 0)
    float priorEps = array.get(epsHistory, 4)
if priorEps != 0 and not na(priorEps)
yoyGrowth:= ((currEps - priorEps) / math.abs(priorEps)) * 100.0

// --- Visual Overlay ---
// Plot a marker directly under the price bar where the earnings filed
plotshape(isNewQuarter, title = "Earnings Filing Event", style = shape.diamond,
    location = location.belowbar,
    color = yoyGrowth >= minGrowth ? color.green : color.red,
    size = size.tiny)

// Inline label anchored to price action showing exact growth rate
if isNewQuarter and not na(yoyGrowth)
    string growthText = str.tostring(yoyGrowth, "+#.##;-#.##") + "%"
    color  lblColor = yoyGrowth >= minGrowth ? color.teal : color.maroon
label.new(bar_index, low * 0.99, text = growthText, style = label.style_label_up,
    color = lblColor, textcolor = color.white, size = size.small)

// --- CAN SLIM Dashboard Table ---
var table canSlimTable = na
if barstate.islast and showTable
    string p = tblPos == "top_right" ? position.top_right :
    tblPos == "bottom_right" ? position.bottom_right :
        tblPos == "top_left" ? position.top_left : position.bottom_left

canSlimTable:= table.new(p, 2, 3, bgcolor = color.new(color.black, 20), border_color = color.gray, border_width = 1)

table.cell(canSlimTable, 0, 0, "Metric", text_color = color.white, text_size = size.small)
table.cell(canSlimTable, 1, 0, "Value", text_color = color.white, text_size = size.small)
    
    float latestEps = array.size(epsHistory) > 0 ? array.get(epsHistory, 0) : na
table.cell(canSlimTable, 0, 1, "Latest Qtr EPS", text_color = color.white, text_size = size.small)
table.cell(canSlimTable, 1, 1, na(latestEps) ? "N/A" : "$" + str.tostring(latestEps, "#.##"),
    text_color = color.white, text_size = size.small)
    
    color metricColor = na(yoyGrowth) ? color.white : (yoyGrowth >= minGrowth ? color.green : color.red)
table.cell(canSlimTable, 0, 2, "YoY Qtr Growth", text_color = color.white, text_size = size.small)
table.cell(canSlimTable, 1, 2, na(yoyGrowth) ? "N/A" : str.tostring(yoyGrowth, "+#.##;-#.##") + "%",
    text_color = metricColor, text_size = size.small)