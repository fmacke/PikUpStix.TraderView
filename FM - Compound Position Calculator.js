//@version=6
indicator("Compound Position Calculator", overlay=true)

// --- User Inputs ---
startDate       = input.time(timestamp("2026-09-27 00:00"), title="Start Date & Time", tooltip="The lines will begin starting from this date on your chart")
buyPrice        = input.price(100.0, title="Buy Price (1/4 Position / Buy1)")
stopLossPct     = input.float(4.0, title="Stop Loss on Position (%) [MaxExposure]")
takeProfitPct   = input.float(8.0, title="Profit Target / Pyramid Step (%) [TakeProfitAt]")
tradingCapital  = input.float(100000.0, title="Trading Capital ($)")
baseRiskPct     = input.float(6.25, title="Base Risk Per Trade (%) [RiskPerTrade]")

// --- Exact C# Price Level Calculations ---
buy1    = buyPrice
target1 = buy1 * (1.0 + takeProfitPct / 100.0)
buy2    = target1
target2 = buy2 * (1.0 + takeProfitPct / 100.0)
buy3    = target2

sl1 = buy1 * (1.0 - stopLossPct / 100.0)
sl2 = buy2 * (1.0 - stopLossPct / 100.0)
sl3 = buy3 * (1.0 - stopLossPct / 100.0)

// Capital & Share Sizing (Matching C# ScalePositionTier multipliers)
risk1 = baseRiskPct * 1.0
risk2 = baseRiskPct * 2.0
risk3 = baseRiskPct * 4.0

posSize1 = tradingCapital * (risk1 / 100.0)
posSize2 = tradingCapital * (risk2 / 100.0)
posSize3 = tradingCapital * (risk3 / 100.0)

totalShares14   = buy1 > 0 ? posSize1 / buy1 : 0.0
totalShares12   = buy2 > 0 ? posSize2 / buy2 : 0.0
totalSharesFull = buy3 > 0 ? posSize3 / buy3 : 0.0

shares12Tranche   = totalShares12 - totalShares14
sharesFullTranche = totalSharesFull - totalShares12

avgPrice14   = buy1
avgPrice12   = totalShares12 > 0 ? ((totalShares14 * buy1) + (shares12Tranche * buy2)) / totalShares12 : 0.0
avgPriceFull = totalSharesFull > 0 ? ((totalShares14 * buy1) + (shares12Tranche * buy2) + (sharesFullTranche * buy3)) / totalSharesFull : 0.0

// --- Locate Start Bar Index ---
var int startBar = na
if time >= startDate and na(startBar)
    startBar := bar_index

// --- Chart Visuals (Lines & Labels) ---
var line line14 = na
var line line12 = na
var line lineFull = na

var label lblLine14 = na
var label lblLine12 = na
var label lblLineFull = na

var label lblSum14 = na
var label lblSum12 = na
var label lblSumFull = na

if barstate.islast and not na(startBar)
    // Clean up old instances on redraw
    line.delete(line14)
    line.delete(line12)
    line.delete(lineFull)
    
    label.delete(lblLine14)
    label.delete(lblLine12)
    label.delete(lblLineFull)
    label.delete(lblSum14)
    label.delete(lblSum12)
    label.delete(lblSumFull)

    // 1. Draw horizontal lines starting from startBar and extending right
    line14   := line.new(startBar, buy1, bar_index + 15, buy1, color=color.blue, width=2, extend=extend.right)
    line12   := line.new(startBar, buy2, bar_index + 15, buy2, color=color.orange, width=2, extend=extend.right)
    lineFull := line.new(startBar, buy3, bar_index + 15, buy3, color=color.green, width=2, extend=extend.right)

    // 2. Extra share annotations directly on the lines using label.style_none
    midBar = math.floor((startBar + bar_index) / 2)
    txtLine14   = str.format(" 1/4 Pos: +{0, number, #.##} shrs ", totalShares14)
    txtLine12   = str.format(" 1/2 Pos Extra: +{0, number, #.##} shrs ", shares12Tranche)
    txtLineFull = str.format(" Full Pos Extra: +{0, number, #.##} shrs ", sharesFullTranche)

    lblLine14   := label.new(midBar, buy1, text=txtLine14, color=color.blue, textcolor=color.white, style=label.style_none)
    lblLine12   := label.new(midBar, buy2, text=txtLine12, color=color.orange, textcolor=color.white, style=label.style_none)
    lblLineFull := label.new(midBar, buy3, text=txtLineFull, color=color.green, textcolor=color.white, style=label.style_none)

    // 3. Detailed summary notes on the right-side labels
    txtSum14   = str.format("1/4 Pos: {0, number, #.##} shrs @ {1, number, #.##} (SL: {2, number, #.##})", totalShares14, buy1, sl1)
    txtSum12   = str.format("1/2 Pos: {0, number, #.##} shrs (Tranche: {1, number, #.##}) @ {2, number, #.##} | Avg: {3, number, #.##} (SL: {4, number, #.##})", totalShares12, shares12Tranche, buy2, avgPrice12, sl2)
    txtSumFull = str.format("Full Pos: {0, number, #.##} shrs (Tranche: {1, number, #.##}) @ {2, number, #.##} | Avg: {3, number, #.##} (SL: {4, number, #.##})", totalSharesFull, sharesFullTranche, buy3, avgPriceFull, sl3)

    lblSum14   := label.new(bar_index + 15, buy1, text=txtSum14, color=color.blue, textcolor=color.white, style=label.style_label_left)
    lblSum12   := label.new(bar_index + 15, buy2, text=txtSum12, color=color.orange, textcolor=color.white, style=label.style_label_left)
    lblSumFull := label.new(bar_index + 15, buy3, text=txtSumFull, color=color.green, textcolor=color.white, style=label.style_label_left)