//@version=6
indicator("Compound Position Calculator", overlay=true)

// --- User Inputs ---
startDate       = input.time(timestamp("2026-09-27 00:00"), title="Start Date & Time", tooltip="The lines will begin starting from this date on your chart")
buyPrice        = input.price(100.0, title="Buy Price / Average Cost Basis ($)")
stopLossPct     = input.float(4.0, title="Stop Loss on Position (%) [MaxExposure]")
tradingCapital  = input.float(132400, title="Trading Capital ($)")
baseRiskPct     = input.float(6.25, title="Base Risk Per Trade (%) [RiskPerTrade]")
showOverlay     = input.bool(true, title="Show Compound Overlay", tooltip="Uncheck to hide calculation lines")

// --- Calculations (Matching TradeCalculatorService.cs logic) ---
if showOverlay and buyPrice > 0
    const float pyramidStepPercent = 2.5

    buy1  = buyPrice
    buy2  = buy1 * (1.0 + pyramidStepPercent / 100.0)
    buy3  = buy2 * (1.0 + pyramidStepPercent / 100.0)

    sl1 = buy1 * (1.0 - stopLossPct / 100.0)

    // Capital & Share Sizing multipliers (1.0x pilot, 0.5x add 1, 0.5x add 2)
    risk1 = baseRiskPct * 1.0
    risk2 = baseRiskPct * 0.5
    risk3 = baseRiskPct * 0.5

    posSize1 = tradingCapital * (risk1 / 100.0)
    posSize2 = tradingCapital * (risk2 / 100.0)
    posSize3 = tradingCapital * (risk3 / 100.0)

    dist1 = buy1 - sl1
    shares14 = dist1 > 0 ? (posSize1 * (stopLossPct / 100.0)) / dist1 : 0.0

    dist2 = buy2 - (buy2 * (stopLossPct / 100.0))
    shares12Tranche = dist2 > 0 ? (posSize2 * (stopLossPct / 100.0)) / dist2 : 0.0

    dist3 = buy3 - (buy3 * (stopLossPct / 100.0))
    sharesFullTranche = dist3 > 0 ? (posSize3 * (stopLossPct / 100.0)) / dist3 : 0.0

    totalShares14   = shares14
    totalShares12   = totalShares14 + shares12Tranche
    totalSharesFull = totalShares12 + sharesFullTranche

    avgPrice12   = totalShares12 > 0 ? ((totalShares14 * buy1) + (shares12Tranche * buy2)) / totalShares12 : 0.0
    avgPriceFull = totalSharesFull > 0 ? ((totalShares14 * buy1) + (shares12Tranche * buy2) + (sharesFullTranche * buy3)) / totalSharesFull : 0.0

    sl2 = avgPrice12
    sl3 = buy2

    // --- Locate Start Bar Index ---
    var int startBar = na
    if time >= startDate and na(startBar)
        startBar := bar_index

    // --- Chart Visuals (Lines & Labels) ---
    var line line14 = na
    var line line12 = na
    var line lineFull = na

    var label lblSum14 = na
    var label lblSum12 = na
    var label lblSumFull = na

    if barstate.islast and not na(startBar)
        line.delete(line14)
        line.delete(line12)
        line.delete(lineFull)
        
        label.delete(lblSum14)
        label.delete(lblSum12)
        label.delete(lblSumFull)

        // Draw horizontal lines starting from startBar extending right
        line14   := line.new(startBar, buy1, bar_index + 15, buy1, color=color.blue, width=2, extend=extend.right)
        line12   := line.new(startBar, buy2, bar_index + 15, buy2, color=color.orange, width=2, extend=extend.right)
        lineFull := line.new(startBar, buy3, bar_index + 15, buy3, color=color.green, width=2, extend=extend.right)

        // Detailed summary notes on the right-side labels matching C# outputs
        txtSum14   = str.format("1/4 Pos: {0, number, #.##} shrs @ {1, number, #.##} (SL: {2, number, #.##})", totalShares14, buy1, sl1)
        txtSum12   = str.format("1/2 Pos: {0, number, #.##} shrs (Tranche: {1, number, #.##}) @ {2, number, #.##} | Avg: {3, number, #.##} (SL: {4, number, #.##})", totalShares12, shares12Tranche, buy2, avgPrice12, sl2)
        txtSumFull = str.format("Full Pos: {0, number, #.##} shrs (Tranche: {1, number, #.##}) @ {2, number, #.##} | Avg: {3, number, #.##} (SL: {4, number, #.##})", totalSharesFull, sharesFullTranche, buy3, avgPriceFull, sl3)

        lblSum14   := label.new(bar_index + 15, buy1, text=txtSum14, color=color.blue, textcolor=color.white, style=label.style_label_left)
        lblSum12   := label.new(bar_index + 15, buy2, text=txtSum12, color=color.orange, textcolor=color.white, style=label.style_label_left)
        lblSumFull := label.new(bar_index + 15, buy3, text=txtSumFull, color=color.green, textcolor=color.white, style=label.style_label_left)