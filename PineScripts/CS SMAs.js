//@version=6
indicator("CS SMAs", overlay = true)

// --- Inputs ---
src = input.source(close, title = "Source")

// Lengths (Standard CAN SLIM: 10, 21, 50, 200)
len10 = input.int(10, title = "SMA 10", group = "Lengths")
len21 = input.int(21, title = "SMA 21", group = "Lengths")
len50 = input.int(50, title = "SMA 50", group = "Lengths")
len200 = input.int(200, title = "SMA 200", group = "Lengths")

// Colors
col10 = input.color(#9c27b0, title = "SMA 10 Color", group = "Style") // Purple
col21 = input.color(#00bcd4, title = "SMA 21 Color", group = "Style") // Cyan
col50 = input.color(#ff9800, title = "SMA 50 Color", group = "Style") // Orange
col200 = input.color(#e91e63, title = "SMA 200 Color", group = "Style") // Pink

// --- Calculations ---
// Direct local calculations (exact match for current chart feed, e.g. IG)
sma10_local = ta.sma(src, len10)
sma21_local = ta.sma(src, len21)
sma50_local = ta.sma(src, len50)
sma200_local = ta.sma(src, len200)

// Higher-timeframe daily fallback (used automatically if you switch to 1h, 15m, etc.)
sma10_htf = request.security(syminfo.tickerid, "D", ta.sma(src, len10), gaps = barmerge.gaps_off, lookahead = barmerge.lookahead_off)
sma21_htf = request.security(syminfo.tickerid, "D", ta.sma(src, len21), gaps = barmerge.gaps_off, lookahead = barmerge.lookahead_off)
sma50_htf = request.security(syminfo.tickerid, "D", ta.sma(src, len50), gaps = barmerge.gaps_off, lookahead = barmerge.lookahead_off)
sma200_htf = request.security(syminfo.tickerid, "D", ta.sma(src, len200), gaps = barmerge.gaps_off, lookahead = barmerge.lookahead_off)

// Dynamic timeframe switch
is_daily_or_above = timeframe.isdaily or timeframe.isweekly or timeframe.ismonthly

ma10 = is_daily_or_above ? sma10_local : sma10_htf
ma21 = is_daily_or_above ? sma21_local : sma21_htf
ma50 = is_daily_or_above ? sma50_local : sma50_htf
ma200 = is_daily_or_above ? sma200_local : sma200_htf

// --- Plotting ---
plot(ma10, title = "SMA 10", color = col10, linewidth = 2)
plot(ma21, title = "SMA 21", color = col21, linewidth = 2)
plot(ma50, title = "SMA 50", color = col50, linewidth = 2)
plot(ma200, title = "SMA 200", color = col200, linewidth = 2)