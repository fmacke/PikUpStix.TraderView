//@version=6
indicator("CS - Fundamentals", overlay=true)

// =============================================================================
// 1. INPUTS & CAN SLIM PARAMETERS
// =============================================================================
group_can = "CAN SLIM Thresholds"
float minEpsGrowth      = input.float(25.0, "Min YoY EPS Growth % (C)", minval=0.0, group=group_can)
float minSalesGrowth    = input.float(20.0, "Min YoY Sales Growth % (C)", minval=0.0, group=group_can)
float minDailyDollarVol = input.float(20.0, "Min 50D Dollar Vol ($M) (S)", minval=0.0, group=group_can,
                          tooltip="Institutional liquidity threshold in Millions USD (O'Neil/Minervini minimum)")

group_mc = "Market Capitalization & Share Structure"
float manualMcOverride  = input.float(0.0, "Manual MC Override ($B, 0 = Auto)", minval=0.0, group=group_mc,
                          tooltip="Manually set Market Cap (e.g. 7.64) if dual-class shares skew calculation")
float shareMultiplier   = input.float(1.0, "Float Multiplier (Dual-Class)", minval=0.1, step=0.05, group=group_mc,
                          tooltip="Factor up balance sheet shares if an unlisted voting class exists")

group_ui = "Display Settings"
bool   showTable        = input.bool(true, "Show Dashboard Table", group=group_ui)
string tblPos           = input.string("bottom_left", "Table Position", 
                          options=["top_right", "bottom_right", "top_left", "bottom_left"], group=group_ui)

// =============================================================================
// 2. TECHNICALS: SUPPLY & DEMAND (50-DAY LIQUIDITY)
// =============================================================================
float barDollarVol   = volume * close
float avg50DollarVol = ta.sma(barDollarVol, 50)
float avg50ShareVol  = ta.sma(volume, 50)

// =============================================================================
// 3. FUNDAMENTALS VIA REQUEST.FINANCIAL
// =============================================================================
float epsQ        = request.financial(syminfo.tickerid, "EARNINGS_PER_SHARE_DILUTED", "FQ")
float salesQ      = request.financial(syminfo.tickerid, "TOTAL_REVENUE", "FQ")
float netIncQ     = request.financial(syminfo.tickerid, "NET_INCOME", "FQ")
float sharesBasic = request.financial(syminfo.tickerid, "TOTAL_SHARES_OUTSTANDING", "FQ")
float sharesDil   = request.financial(syminfo.tickerid, "DILUTED_SHARES_OUTSTANDING", "FQ")

// Calculate consolidated float
float bestShares      = not na(sharesDil) and sharesDil > nz(sharesBasic) ? sharesDil : nz(sharesBasic)
float effectiveShares = bestShares * shareMultiplier

// Market Capitalization
float calculatedMC    = effectiveShares > 0 ? (effectiveShares * close) : na
float finalMarketCap  = (manualMcOverride > 0.0) ? (manualMcOverride * 1e9) : calculatedMC

// Net Margin
float marginQ         = (salesQ != 0 and not na(salesQ) and not na(netIncQ)) ? (netIncQ / salesQ) * 100.0 : na

// Detect earnings reporting event
bool isNewQuarter     = ta.change(epsQ) != 0 and not na(epsQ)

// =============================================================================
// 4. HISTORICAL QUARTER BUFFERS & YOY METRICS
// =============================================================================
var float[] epsHist    = array.new_float(0)
var float[] salesHist  = array.new_float(0)
var float[] marginHist = array.new_float(0)

if isNewQuarter
    array.unshift(epsHist, epsQ)
    array.unshift(salesHist, salesQ)
    array.unshift(marginHist, marginQ)
    
    if array.size(epsHist) > 8
        array.pop(epsHist)
        array.pop(salesHist)
        array.pop(marginHist)

// YoY Calculations (Comparing Q0 to Q4 - same quarter 1 year ago)
var float yoyEpsGrowth   = na
var float yoySalesGrowth = na
var float yoyMarginDelta = na
var bool  epsAnomaly     = false
var bool  isNegativeBase = false

if isNewQuarter and array.size(epsHist) >= 5
    float cEps = array.get(epsHist, 0)
    float pEps = array.get(epsHist, 4)
    
    if pEps != 0 and not na(pEps)
        yoyEpsGrowth   := ((cEps - pEps) / math.abs(pEps)) * 100.0
        isNegativeBase := (pEps < 0)

    float cSales = array.get(salesHist, 0)
    float pSales = array.get(salesHist, 4)
    if pSales != 0 and not na(pSales)
        yoySalesGrowth := ((cSales - pSales) / math.abs(pSales)) * 100.0

    float cMargin = array.get(marginHist, 0)
    float pMargin = array.get(marginHist, 4)
    if not na(cMargin) and not na(pMargin)
        yoyMarginDelta := cMargin - pMargin

    // Accounting Quality Check: Flag extreme GAAP EPS jumps unsupported by sales
    epsAnomaly := (yoyEpsGrowth >= 300.0 and yoySalesGrowth < 35.0) or (cEps <= 0.0)

// =============================================================================
// 5. CHART VISUALS & EVENT ANNOTATIONS
// =============================================================================
float latEpsVal = array.size(epsHist) > 0 ? array.get(epsHist, 0) : na

bool canSlimPass = (yoyEpsGrowth >= minEpsGrowth) and 
                   (yoySalesGrowth >= minSalesGrowth) and 
                   (yoyMarginDelta > 0) and 
                   (not epsAnomaly) and 
                   (latEpsVal > 0)

color signalColor = canSlimPass ? color.teal : epsAnomaly  ? color.orange : (yoyEpsGrowth >= minEpsGrowth ? color.blue : color.maroon)

plotshape(isNewQuarter, title="Filing Marker", style=shape.diamond, 
          location=location.belowbar, color=signalColor, size=size.tiny)

if isNewQuarter and not na(yoyEpsGrowth)
    string anomalyTag = epsAnomaly ? " [! Non-Op/GAAP]" : (isNegativeBase ? " [Turnaround]" : "")
    string reportTxt = "EPS: " + str.tostring(yoyEpsGrowth, "+#.##;-#.##") + "%" + anomalyTag + "\nRev: " + str.tostring(yoySalesGrowth, "+#.##;-#.##") + "%\nMargin Δ: " + str.tostring(yoyMarginDelta, "+#.##;-#.##") + " pp"
    label.new(bar_index, low * 0.99, text=reportTxt, style=label.style_label_up, color=signalColor, textcolor=color.white, size=size.small)

// =============================================================================
// 6. FORMATTING HELPERS
// =============================================================================
formatCurrency(float val) =>
    if na(val)
        "N/A"
    else if val >= 1e12
        "$" + str.tostring(val / 1e12, "#.##") + "T"
    else if val >= 1e9
        "$" + str.tostring(val / 1e9, "#.##") + "B"
    else if val >= 1e6
        "$" + str.tostring(val / 1e6, "#.##") + "M"
    else
        "$" + str.tostring(val, "#,###")

formatShares(float val) =>
    if na(val)
        "N/A"
    else if val >= 1e6
        str.tostring(val / 1e6, "#.##") + "M shs"
    else if val >= 1e3
        str.tostring(val / 1e3, "#.##") + "K shs"
    else
        str.tostring(val, "#,###") + " shs"

// =============================================================================
// 7. CAN SLIM DASHBOARD TABLE
// =============================================================================
var table fundamentalTable = na
if barstate.islast and showTable
    string p = tblPos == "top_right" ? position.top_right : 
               tblPos == "bottom_right" ? position.bottom_right : 
               tblPos == "top_left" ? position.top_left : position.bottom_left
               
    fundamentalTable := table.new(p, 3, 6, bgcolor=color.new(color.black, 20), border_color=color.gray, border_width=1)
    
    // Header
    table.cell(fundamentalTable, 0, 0, "CAN SLIM Metric", text_color=color.white, text_size=size.small)
    table.cell(fundamentalTable, 1, 0, "Current / Qtr",   text_color=color.white, text_size=size.small)
    table.cell(fundamentalTable, 2, 0, "Benchmark / Δ",   text_color=color.white, text_size=size.small)
    
    // Row 1: Market Cap (S)
    string mcSuffix = manualMcOverride > 0.0 ? " (Man)" : (shareMultiplier != 1.0 ? " (Adj)" : "")
    table.cell(fundamentalTable, 0, 1, "Market Cap" + mcSuffix, text_color=color.white,  text_size=size.small)
    table.cell(fundamentalTable, 1, 1, formatCurrency(finalMarketCap), text_color=color.yellow, text_size=size.small)
    table.cell(fundamentalTable, 2, 1, "-",             text_color=color.gray,   text_size=size.small)

    // Row 2: 50D Daily Liquidity (S)
    float minDollarVolAbs = minDailyDollarVol * 1e6
    color liqCol = na(avg50DollarVol) ? color.white : (avg50DollarVol >= minDollarVolAbs ? color.green : color.red)
    table.cell(fundamentalTable, 0, 2, "50D Dollar Vol", text_color=color.white,  text_size=size.small)
    table.cell(fundamentalTable, 1, 2, formatCurrency(avg50DollarVol), text_color=liqCol, text_size=size.small)
    table.cell(fundamentalTable, 2, 2, formatShares(avg50ShareVol), text_color=color.gray, text_size=size.small)

    // Row 3: Diluted EPS (C)
    color epsCol = na(yoyEpsGrowth) ? color.white : 
                   epsAnomaly       ? color.orange : 
                   (yoyEpsGrowth >= minEpsGrowth ? color.green : color.red)
    string epsStatus = epsAnomaly ? " [!]" : (isNegativeBase ? " [Turn]" : "")
    table.cell(fundamentalTable, 0, 3, "Diluted EPS",   text_color=color.white,  text_size=size.small)
    table.cell(fundamentalTable, 1, 3, na(latEpsVal) ? "N/A" : "$" + str.tostring(latEpsVal, "#.##"), text_color=color.white, text_size=size.small)
    table.cell(fundamentalTable, 2, 3, na(yoyEpsGrowth) ? "N/A" : str.tostring(yoyEpsGrowth, "+#.##;-#.##") + "%" + epsStatus, text_color=epsCol, text_size=size.small)

    // Row 4: Sales Confirmation (C)
    float latSales = array.size(salesHist) > 0 ? array.get(salesHist, 0) : na
    color salesCol = na(yoySalesGrowth) ? color.white : (yoySalesGrowth >= minSalesGrowth ? color.green : color.red)
    table.cell(fundamentalTable, 0, 4, "Revenue",       text_color=color.white,  text_size=size.small)
    table.cell(fundamentalTable, 1, 4, formatCurrency(latSales), text_color=color.white, text_size=size.small)
    table.cell(fundamentalTable, 2, 4, na(yoySalesGrowth) ? "N/A" : str.tostring(yoySalesGrowth, "+#.##;-#.##") + "%", text_color=salesCol, text_size=size.small)

    // Row 5: Net Margin Expansion
    float latMargin = array.size(marginHist) > 0 ? array.get(marginHist, 0) : na
    color marginCol = na(yoyMarginDelta) ? color.white : (yoyMarginDelta > 0 ? color.green : color.red)
    table.cell(fundamentalTable, 0, 5, "Net Margin",    text_color=color.white,  text_size=size.small)
    table.cell(fundamentalTable, 1, 5, na(latMargin) ? "N/A" : str.tostring(latMargin, "#.##") + "%", text_color=color.white, text_size=size.small)
    table.cell(fundamentalTable, 2, 5, na(yoyMarginDelta) ? "N/A" : str.tostring(yoyMarginDelta, "+#.##;-#.##") + " pp", text_color=marginCol, text_size=size.small)//@version=6
indicator("CS - Fundamentals", overlay=true)

// =============================================================================
// 1. INPUTS & CAN SLIM PARAMETERS
// =============================================================================
group_can = "CAN SLIM Thresholds"
float minEpsGrowth      = input.float(25.0, "Min YoY EPS Growth % (C)", minval=0.0, group=group_can)
float minSalesGrowth    = input.float(20.0, "Min YoY Sales Growth % (C)", minval=0.0, group=group_can)
float minDailyDollarVol = input.float(20.0, "Min 50D Dollar Vol ($M) (S)", minval=0.0, group=group_can,
                          tooltip="Institutional liquidity threshold in Millions USD (O'Neil/Minervini minimum)")

group_mc = "Market Capitalization & Share Structure"
float manualMcOverride  = input.float(0.0, "Manual MC Override ($B, 0 = Auto)", minval=0.0, group=group_mc,
                          tooltip="Manually set Market Cap (e.g. 7.64) if dual-class shares skew calculation")
float shareMultiplier   = input.float(1.0, "Float Multiplier (Dual-Class)", minval=0.1, step=0.05, group=group_mc,
                          tooltip="Factor up balance sheet shares if an unlisted voting class exists")

group_ui = "Display Settings"
bool   showTable        = input.bool(true, "Show Dashboard Table", group=group_ui)
string tblPos           = input.string("bottom_left", "Table Position", 
                          options=["top_right", "bottom_right", "top_left", "bottom_left"], group=group_ui)

// =============================================================================
// 2. TECHNICALS: SUPPLY & DEMAND (50-DAY LIQUIDITY)
// =============================================================================
float barDollarVol   = volume * close
float avg50DollarVol = ta.sma(barDollarVol, 50)
float avg50ShareVol  = ta.sma(volume, 50)

// =============================================================================
// 3. FUNDAMENTALS VIA REQUEST.FINANCIAL
// =============================================================================
float epsQ        = request.financial(syminfo.tickerid, "EARNINGS_PER_SHARE_DILUTED", "FQ")
float salesQ      = request.financial(syminfo.tickerid, "TOTAL_REVENUE", "FQ")
float netIncQ     = request.financial(syminfo.tickerid, "NET_INCOME", "FQ")
float sharesBasic = request.financial(syminfo.tickerid, "TOTAL_SHARES_OUTSTANDING", "FQ")
float sharesDil   = request.financial(syminfo.tickerid, "DILUTED_SHARES_OUTSTANDING", "FQ")

// Calculate consolidated float
float bestShares      = not na(sharesDil) and sharesDil > nz(sharesBasic) ? sharesDil : nz(sharesBasic)
float effectiveShares = bestShares * shareMultiplier

// Market Capitalization
float calculatedMC    = effectiveShares > 0 ? (effectiveShares * close) : na
float finalMarketCap  = (manualMcOverride > 0.0) ? (manualMcOverride * 1e9) : calculatedMC

// Net Margin
float marginQ         = (salesQ != 0 and not na(salesQ) and not na(netIncQ)) ? (netIncQ / salesQ) * 100.0 : na

// Detect earnings reporting event
bool isNewQuarter     = ta.change(epsQ) != 0 and not na(epsQ)

// =============================================================================
// 4. HISTORICAL QUARTER BUFFERS & YOY METRICS
// =============================================================================
var float[] epsHist    = array.new_float(0)
var float[] salesHist  = array.new_float(0)
var float[] marginHist = array.new_float(0)

if isNewQuarter
    array.unshift(epsHist, epsQ)
    array.unshift(salesHist, salesQ)
    array.unshift(marginHist, marginQ)
    
    if array.size(epsHist) > 8
        array.pop(epsHist)
        array.pop(salesHist)
        array.pop(marginHist)

// YoY Calculations (Comparing Q0 to Q4 - same quarter 1 year ago)
var float yoyEpsGrowth   = na
var float yoySalesGrowth = na
var float yoyMarginDelta = na
var bool  epsAnomaly     = false
var bool  isNegativeBase = false

if isNewQuarter and array.size(epsHist) >= 5
    float cEps = array.get(epsHist, 0)
    float pEps = array.get(epsHist, 4)
    
    if pEps != 0 and not na(pEps)
        yoyEpsGrowth   := ((cEps - pEps) / math.abs(pEps)) * 100.0
        isNegativeBase := (pEps < 0)

    float cSales = array.get(salesHist, 0)
    float pSales = array.get(salesHist, 4)
    if pSales != 0 and not na(pSales)
        yoySalesGrowth := ((cSales - pSales) / math.abs(pSales)) * 100.0

    float cMargin = array.get(marginHist, 0)
    float pMargin = array.get(marginHist, 4)
    if not na(cMargin) and not na(pMargin)
        yoyMarginDelta := cMargin - pMargin

    // Accounting Quality Check: Flag extreme GAAP EPS jumps unsupported by sales
    epsAnomaly := (yoyEpsGrowth >= 300.0 and yoySalesGrowth < 35.0) or (cEps <= 0.0)

// =============================================================================
// 5. CHART VISUALS & EVENT ANNOTATIONS
// =============================================================================
float latEpsVal = array.size(epsHist) > 0 ? array.get(epsHist, 0) : na

bool canSlimPass = (yoyEpsGrowth >= minEpsGrowth) and 
                   (yoySalesGrowth >= minSalesGrowth) and 
                   (yoyMarginDelta > 0) and 
                   (not epsAnomaly) and 
                   (latEpsVal > 0)

color signalColor = canSlimPass ? color.teal : epsAnomaly  ? color.orange : (yoyEpsGrowth >= minEpsGrowth ? color.blue : color.maroon)

plotshape(isNewQuarter, title="Filing Marker", style=shape.diamond, 
          location=location.belowbar, color=signalColor, size=size.tiny)

if isNewQuarter and not na(yoyEpsGrowth)
    string anomalyTag = epsAnomaly ? " [! Non-Op/GAAP]" : (isNegativeBase ? " [Turnaround]" : "")
    string reportTxt = "EPS: " + str.tostring(yoyEpsGrowth, "+#.##;-#.##") + "%" + anomalyTag + "\nRev: " + str.tostring(yoySalesGrowth, "+#.##;-#.##") + "%\nMargin Δ: " + str.tostring(yoyMarginDelta, "+#.##;-#.##") + " pp"
    label.new(bar_index, low * 0.99, text=reportTxt, style=label.style_label_up, color=signalColor, textcolor=color.white, size=size.small)

// =============================================================================
// 6. FORMATTING HELPERS
// =============================================================================
formatCurrency(float val) =>
    if na(val)
        "N/A"
    else if val >= 1e12
        "$" + str.tostring(val / 1e12, "#.##") + "T"
    else if val >= 1e9
        "$" + str.tostring(val / 1e9, "#.##") + "B"
    else if val >= 1e6
        "$" + str.tostring(val / 1e6, "#.##") + "M"
    else
        "$" + str.tostring(val, "#,###")

formatShares(float val) =>
    if na(val)
        "N/A"
    else if val >= 1e6
        str.tostring(val / 1e6, "#.##") + "M shs"
    else if val >= 1e3
        str.tostring(val / 1e3, "#.##") + "K shs"
    else
        str.tostring(val, "#,###") + " shs"

// =============================================================================
// 7. CAN SLIM DASHBOARD TABLE
// =============================================================================
var table fundamentalTable = na
if barstate.islast and showTable
    string p = tblPos == "top_right" ? position.top_right : 
               tblPos == "bottom_right" ? position.bottom_right : 
               tblPos == "top_left" ? position.top_left : position.bottom_left
               
    fundamentalTable := table.new(p, 3, 6, bgcolor=color.new(color.black, 20), border_color=color.gray, border_width=1)
    
    // Header
    table.cell(fundamentalTable, 0, 0, "CAN SLIM Metric", text_color=color.white, text_size=size.small)
    table.cell(fundamentalTable, 1, 0, "Current / Qtr",   text_color=color.white, text_size=size.small)
    table.cell(fundamentalTable, 2, 0, "Benchmark / Δ",   text_color=color.white, text_size=size.small)
    
    // Row 1: Market Cap (S)
    string mcSuffix = manualMcOverride > 0.0 ? " (Man)" : (shareMultiplier != 1.0 ? " (Adj)" : "")
    table.cell(fundamentalTable, 0, 1, "Market Cap" + mcSuffix, text_color=color.white,  text_size=size.small)
    table.cell(fundamentalTable, 1, 1, formatCurrency(finalMarketCap), text_color=color.yellow, text_size=size.small)
    table.cell(fundamentalTable, 2, 1, "-",             text_color=color.gray,   text_size=size.small)

    // Row 2: 50D Daily Liquidity (S)
    float minDollarVolAbs = minDailyDollarVol * 1e6
    color liqCol = na(avg50DollarVol) ? color.white : (avg50DollarVol >= minDollarVolAbs ? color.green : color.red)
    table.cell(fundamentalTable, 0, 2, "50D Dollar Vol", text_color=color.white,  text_size=size.small)
    table.cell(fundamentalTable, 1, 2, formatCurrency(avg50DollarVol), text_color=liqCol, text_size=size.small)
    table.cell(fundamentalTable, 2, 2, formatShares(avg50ShareVol), text_color=color.gray, text_size=size.small)

    // Row 3: Diluted EPS (C)
    color epsCol = na(yoyEpsGrowth) ? color.white : 
                   epsAnomaly       ? color.orange : 
                   (yoyEpsGrowth >= minEpsGrowth ? color.green : color.red)
    string epsStatus = epsAnomaly ? " [!]" : (isNegativeBase ? " [Turn]" : "")
    table.cell(fundamentalTable, 0, 3, "Diluted EPS",   text_color=color.white,  text_size=size.small)
    table.cell(fundamentalTable, 1, 3, na(latEpsVal) ? "N/A" : "$" + str.tostring(latEpsVal, "#.##"), text_color=color.white, text_size=size.small)
    table.cell(fundamentalTable, 2, 3, na(yoyEpsGrowth) ? "N/A" : str.tostring(yoyEpsGrowth, "+#.##;-#.##") + "%" + epsStatus, text_color=epsCol, text_size=size.small)

    // Row 4: Sales Confirmation (C)
    float latSales = array.size(salesHist) > 0 ? array.get(salesHist, 0) : na
    color salesCol = na(yoySalesGrowth) ? color.white : (yoySalesGrowth >= minSalesGrowth ? color.green : color.red)
    table.cell(fundamentalTable, 0, 4, "Revenue",       text_color=color.white,  text_size=size.small)
    table.cell(fundamentalTable, 1, 4, formatCurrency(latSales), text_color=color.white, text_size=size.small)
    table.cell(fundamentalTable, 2, 4, na(yoySalesGrowth) ? "N/A" : str.tostring(yoySalesGrowth, "+#.##;-#.##") + "%", text_color=salesCol, text_size=size.small)

    // Row 5: Net Margin Expansion
    float latMargin = array.size(marginHist) > 0 ? array.get(marginHist, 0) : na
    color marginCol = na(yoyMarginDelta) ? color.white : (yoyMarginDelta > 0 ? color.green : color.red)
    table.cell(fundamentalTable, 0, 5, "Net Margin",    text_color=color.white,  text_size=size.small)
    table.cell(fundamentalTable, 1, 5, na(latMargin) ? "N/A" : str.tostring(latMargin, "#.##") + "%", text_color=color.white, text_size=size.small)
    table.cell(fundamentalTable, 2, 5, na(yoyMarginDelta) ? "N/A" : str.tostring(yoyMarginDelta, "+#.##;-#.##") + " pp", text_color=marginCol, text_size=size.small)