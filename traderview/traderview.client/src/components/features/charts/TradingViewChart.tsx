import { useEffect, useRef, useState } from 'react';
import { createChart, CandlestickSeries, LineSeries } from 'lightweight-charts';
import type { IChartApi } from 'lightweight-charts';
import type { Position } from '../../../types/api';
import { tradeService } from '../../../services';
import { useTheme } from '../../../hooks/useTheme';
import './TradingViewChart.css';

interface TradingViewChartProps {
    trade: Position;
}

function TradingViewChart({ trade }: TradingViewChartProps) {
    const chartContainerRef = useRef<HTMLDivElement>(null);
    const chartRef = useRef<IChartApi | null>(null);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);
    const { theme } = useTheme();

    useEffect(() => {
        let isMounted = true;

        const initChart = async () => {
            try {
                setLoading(true);
                setError(null);

                console.log('Fetching candlesticks for trade with positionId:', trade.positionId);

                // Clean up existing chart before creating a new one
                if (chartRef.current) {
                    chartRef.current.remove();
                    chartRef.current = null;
                }

                // Fetch candlestick data with up to 100 bars before entry and 100 bars after exit
                // Using 150 calendar days to approximately get 100 trading days (accounting for weekends/holidays)
                const tradeContext = await tradeService.getTradeCandlesticks(trade.positionId, 150, 150);

                console.log('Received candlestick data:', tradeContext);
                console.log('Trade object from context:', tradeContext.trade);
                console.log('Executions from context:', tradeContext.trade?.executions);
                console.log('Executions count:', tradeContext.trade?.executions?.length);

                if (!isMounted || !chartContainerRef.current) return;

                // Check if we have candlestick data
                if (!tradeContext.candlesticks || tradeContext.candlesticks.length === 0) {
                    console.warn('No candlestick data available for trade:', trade.id);
                    setError(`No historical price data available for ${trade.symbol}. The HistoricalData table may not have data for this instrument (ID: ${trade.instrumentId}).`);
                    setLoading(false);
                    return;
                }

                console.log('Number of candlesticks:', tradeContext.candlesticks.length);

                // Define theme-aware colors
                const isDarkMode = theme === 'dark';
                const chartColors = {
                    backgroundColor: isDarkMode ? '#1e1e1e' : '#ffffff',
                    textColor: isDarkMode ? '#d4d4d4' : '#333333',
                    gridColor: isDarkMode ? '#3a3a3a' : '#e0e0e0',
                    borderColor: isDarkMode ? '#555555' : '#cccccc',
                };

                // Create chart
                const chart = createChart(chartContainerRef.current!, {
                    layout: {
                        background: { color: chartColors.backgroundColor },
                        textColor: chartColors.textColor,
                    },
                    grid: {
                        vertLines: { color: chartColors.gridColor },
                        horzLines: { color: chartColors.gridColor },
                    },
                    width: chartContainerRef.current!.clientWidth,
                    height: 400,
                    timeScale: {
                        timeVisible: true,
                        secondsVisible: false,
                        rightOffset: 5,
                        barSpacing: 10,
                    },
                    rightPriceScale: {
                        borderColor: chartColors.borderColor,
                    },
                });

                chartRef.current = chart;

                // Create candlestick series
                const candlestickSeries = chart.addSeries(CandlestickSeries, {
                    upColor: '#26a69a',
                    downColor: '#ef5350',
                    borderVisible: false,
                    wickUpColor: '#26a69a',
                    wickDownColor: '#ef5350',
                });

                // Convert candlestick data to chart format
                // Parse date as UTC to avoid timezone shifts (dates come as YYYY-MM-DD from database)
                const candlestickData = tradeContext.candlesticks.map(candle => {
                    const dateStr = candle.date.split('T')[0]; // Get just the date part (YYYY-MM-DD)
                    return {
                        time: dateStr as any, // Use date string directly for lightweight-charts
                        open: candle.open,
                        high: candle.high,
                        low: candle.low,
                        close: candle.close,
                    };
                });

                console.log('Candlestick data for chart:', candlestickData);

                candlestickSeries.setData(candlestickData);

                // Add the overall trade line from entry to exit (blue line)
                const entryDateStr = trade.entryDate.split('T')[0];
                const exitDateStr = trade.exitDate.split('T')[0];

                if (entryDateStr !== exitDateStr) {
                    const tradeLine = chart.addSeries(LineSeries, {
                        color: '#2196F3', // blue color for overall trade line
                        lineWidth: 3,
                        lineStyle: 0, // solid line
                        crosshairMarkerVisible: true,
                        crosshairMarkerRadius: 6,
                        lastValueVisible: false,
                        priceLineVisible: false,
                    });

                    // Create line data points from entry to exit
                    tradeLine.setData([
                        { time: entryDateStr as any, value: trade.entryPrice },
                        { time: exitDateStr as any, value: trade.exitPrice },
                    ]);
                }

                // Add individual execution lines for each buy/sell execution
                const tradeFromContext = tradeContext.trade;
                console.log('Trade data from context:', tradeFromContext);
                console.log('Trade executions from context:', tradeFromContext.executions);
                console.log('Trade executions length:', tradeFromContext.executions?.length);

                if (tradeFromContext.executions && tradeFromContext.executions.length > 0) {
                    console.log('Adding execution markers...');

                    // Helper function to remove duplicate times (keep first occurrence)
                    const removeDuplicateTimes = (executions: any[]) => {
                        const seen = new Set<string>();
                        return executions.filter(exec => {
                            const dateStr = exec.date.split('T')[0];
                            if (seen.has(dateStr)) {
                                return false;
                            }
                            seen.add(dateStr);
                            return true;
                        });
                    };

                    // Create line series for BUY executions (green)
                    const buyExecutions = tradeFromContext.executions.filter(e => e.side === 'BUY');
                    if (buyExecutions.length > 0) {
                        const uniqueBuyExecutions = removeDuplicateTimes(buyExecutions);
                        const buyLineData = uniqueBuyExecutions.map(exec => ({
                            time: exec.date.split('T')[0] as any,
                            value: exec.price,
                        }));

                        console.log('BUY executions (filtered):', buyLineData);

                        if (buyLineData.length > 0) {
                            const buyLine = chart.addSeries(LineSeries, {
                                color: '#A52A2A', // bright green for BUY executions
                                lineWidth: 4, // thicker line
                                lineStyle: 1, // dotted line
                                crosshairMarkerVisible: true,
                                crosshairMarkerRadius: 8,
                                lastValueVisible: false,
                                priceLineVisible: true, // show horizontal price line
                                priceLineSource: 'LastBar' as any,
                            });

                            buyLine.setData(buyLineData);
                            console.log('Added BUY execution markers:', buyLineData);

                            // Add price lines with labels for each BUY execution
                            for (const exec of uniqueBuyExecutions) {
                                candlestickSeries.createPriceLine({
                                    price: exec.price,
                                    color: '#B8860B',
                                    lineWidth: 1,
                                    lineStyle: 1, // dotted
                                    axisLabelVisible: true,
                                    title: `BUY: ${exec.price.toFixed(2)}`,
                                });
                            }
                        }
                    }

                    // Create line series for SELL executions (red)
                    const sellExecutions = tradeFromContext.executions.filter(e => e.side === 'SELL');
                    if (sellExecutions.length > 0) {
                        const uniqueSellExecutions = removeDuplicateTimes(sellExecutions);
                        const sellLineData = uniqueSellExecutions.map(exec => ({
                            time: exec.date.split('T')[0] as any,
                            value: exec.price,
                        }));

                        console.log('SELL executions (filtered):', sellLineData);

                        if (sellLineData.length > 0) {
                            const sellLine = chart.addSeries(LineSeries, {
                                color: '#ff0000', // bright red for SELL executions
                                lineWidth: 4, // thicker line
                                lineStyle: 1, // dotted line
                                crosshairMarkerVisible: true,
                                crosshairMarkerRadius: 8,
                                lastValueVisible: false,
                                priceLineVisible: true, // show horizontal price line
                                priceLineSource: 'LastBar' as any,
                            });

                            sellLine.setData(sellLineData);
                            console.log('Added SELL execution markers:', sellLineData);

                            // Add price lines with labels for each SELL execution
                            for (const exec of uniqueSellExecutions) {
                                candlestickSeries.createPriceLine({
                                    price: exec.price,
                                    color: '#ff0000',
                                    lineWidth: 1,
                                    lineStyle: 1, // dotted
                                    axisLabelVisible: true,
                                    title: `SELL: ${exec.price.toFixed(2)}`,
                                });
                            }
                        }
                    }
                } else {
                    console.log('No executions available');
                }

                // Add price lines for entry and exit points (without titles on the lines)
                candlestickSeries.createPriceLine({
                    price: trade.entryPrice,
                    color: trade.buySell === 'BUY' ? '#26a69a' : '#ef5350',
                    lineWidth: 2,
                    lineStyle: 2, // dashed
                    axisLabelVisible: true,
                    title: '', // Remove title from line
                });

                candlestickSeries.createPriceLine({
                    price: trade.exitPrice,
                    color: trade.buySell === 'BUY' ? '#ef5350' : '#26a69a',
                    lineWidth: 2,
                    lineStyle: 2, // dashed
                    axisLabelVisible: true,
                    title: '', // Remove title from line
                });

                // Add custom labels on the left side of the chart
                // Wait for chart to render, then position labels
                setTimeout(() => {
                    if (chartRef.current && chartContainerRef.current) {
                        // Remove any existing labels
                        const existingLabels = chartContainerRef.current.querySelectorAll('.custom-price-label');
                        existingLabels.forEach(label => label.remove());

                        // Get the y-coordinate for each price
                        const entryY = candlestickSeries.priceToCoordinate(trade.entryPrice);
                        const exitY = candlestickSeries.priceToCoordinate(trade.exitPrice);

                        if (entryY !== null) {
                            const entryLabel = document.createElement('div');
                            entryLabel.className = 'custom-price-label';
                            entryLabel.textContent = `Entry: $${trade.entryPrice.toFixed(2)}`;
                            entryLabel.style.position = 'absolute';
                            entryLabel.style.left = '10px';
                            entryLabel.style.top = `${entryY - 25}px`;
                            entryLabel.style.color = trade.buySell === 'BUY' ? '#26a69a' : '#ef5350';
                            entryLabel.style.fontSize = '12px';
                            entryLabel.style.fontWeight = 'bold';
                            entryLabel.style.padding = '2px 6px';
                            entryLabel.style.zIndex = '10';
                            chartContainerRef.current.appendChild(entryLabel);
                        }

                        if (exitY !== null) {
                            const exitLabel = document.createElement('div');
                            exitLabel.className = 'custom-price-label';
                            exitLabel.textContent = `Exit: $${trade.exitPrice.toFixed(2)}`;
                            exitLabel.style.position = 'absolute';
                            exitLabel.style.left = '10px';
                            exitLabel.style.top = `${exitY - 10}px`;
                            exitLabel.style.color = trade.buySell === 'BUY' ? '#ef5350' : '#26a69a';
                            exitLabel.style.fontSize = '12px';
                            exitLabel.style.fontWeight = 'bold';
                            exitLabel.style.padding = '2px 6px';
                            exitLabel.style.zIndex = '10';
                            chartContainerRef.current.appendChild(exitLabel);
                        }
                    }
                }, 100);

                // Calculate price range including entry/exit points
                const candleHighs = candlestickData.map(c => c.high);
                const candleLows = candlestickData.map(c => c.low);
                const maxCandlePrice = Math.max(...candleHighs);
                const minCandlePrice = Math.min(...candleLows);

                // Include entry and exit prices in the range calculation
                const maxPrice = Math.max(maxCandlePrice, trade.entryPrice, trade.exitPrice);
                const minPrice = Math.min(minCandlePrice, trade.entryPrice, trade.exitPrice);

                // Add 5% padding to ensure entry/exit lines are visible
                const priceRange = maxPrice - minPrice;
                const pricePadding = priceRange * 0.05;
                const visibleMinPrice = minPrice - pricePadding;
                const visibleMaxPrice = maxPrice + pricePadding;

                // Reset and fit the chart to show all data properly
                // This multi-step approach ensures the chart renders correctly
                if (candlestickData.length > 0) {
                    const firstTime = candlestickData[0].time;
                    const lastTime = candlestickData[candlestickData.length - 1].time;

                    // Calculate time padding (add 10 days on each side for visibility)
                    const paddingDays = 10;

                    // Set loading to false first so chart container is visible
                    setLoading(false);

                    // Use setTimeout to ensure the chart container is rendered and has dimensions
                    setTimeout(() => {
                        if (chartRef.current) {
                            // Reset any previous scroll/zoom state
                            chartRef.current.timeScale().resetTimeScale();

                            // Calculate visible range with padding (date strings in YYYY-MM-DD format)
                            const firstDate = new Date(firstTime as string);
                            const lastDate = new Date(lastTime as string);
                            firstDate.setDate(firstDate.getDate() - paddingDays);
                            lastDate.setDate(lastDate.getDate() + paddingDays);

                            // Set visible range explicitly with padding
                            chartRef.current.timeScale().setVisibleRange({
                                from: firstDate.toISOString().split('T')[0] as any,
                                to: lastDate.toISOString().split('T')[0] as any,
                            });

                            // Set the price scale to include entry/exit prices with padding
                            chartRef.current.priceScale('right').applyOptions({
                                autoScale: false,
                            });

                            candlestickSeries.applyOptions({
                                autoscaleInfoProvider: () => ({
                                    priceRange: {
                                        minValue: visibleMinPrice,
                                        maxValue: visibleMaxPrice,
                                    },
                                }),
                            });

                            // Follow up with fitContent to optimize the view
                            setTimeout(() => {
                                if (chartRef.current) {
                                    chartRef.current.timeScale().fitContent();
                                }
                            }, 50);
                        }
                    }, 0);
                } else {
                    setLoading(false);
                }

                // Handle window resize and update label positions
                const updateLabelPositions = () => {
                    if (chartRef.current && chartContainerRef.current) {
                        // Remove any existing labels
                        const existingLabels = chartContainerRef.current.querySelectorAll('.custom-price-label');
                        existingLabels.forEach(label => label.remove());

                        // Get the y-coordinate for each price
                        const entryY = candlestickSeries.priceToCoordinate(trade.entryPrice);
                        const exitY = candlestickSeries.priceToCoordinate(trade.exitPrice);

                        if (entryY !== null) {
                            const entryLabel = document.createElement('div');
                            entryLabel.className = 'custom-price-label';
                            entryLabel.textContent = `Entry: $${trade.entryPrice.toFixed(2)}`;
                            entryLabel.style.position = 'absolute';
                            entryLabel.style.left = '10px';
                            entryLabel.style.top = `${entryY - 25}px`;
                            entryLabel.style.color = trade.buySell === 'BUY' ? '#26a69a' : '#ef5350';
                            entryLabel.style.fontSize = '12px';
                            entryLabel.style.fontWeight = 'bold';
                            entryLabel.style.padding = '2px 6px';
                            entryLabel.style.zIndex = '10';
                            chartContainerRef.current.appendChild(entryLabel);
                        }

                        if (exitY !== null) {
                            const exitLabel = document.createElement('div');
                            exitLabel.className = 'custom-price-label';
                            exitLabel.textContent = `Exit: $${trade.exitPrice.toFixed(2)}`;
                            exitLabel.style.position = 'absolute';
                            exitLabel.style.left = '10px';
                            exitLabel.style.top = `${exitY}px`;
                            exitLabel.style.color = trade.buySell === 'BUY' ? '#ef5350' : '#26a69a';
                            exitLabel.style.fontSize = '12px';
                            exitLabel.style.fontWeight = 'bold';
                            exitLabel.style.padding = '2px 6px';
                            exitLabel.style.zIndex = '10';
                            chartContainerRef.current.appendChild(exitLabel);
                        }
                    }
                };

                const handleResize = () => {
                    if (chartContainerRef.current && chartRef.current) {
                        chartRef.current.applyOptions({
                            width: chartContainerRef.current.clientWidth,
                        });
                        updateLabelPositions();
                    }
                };

                window.addEventListener('resize', handleResize);

                // Subscribe to price scale changes to update label positions
                chart.timeScale().subscribeVisibleLogicalRangeChange(() => {
                    updateLabelPositions();
                });

                // Cleanup
                return () => {
                    window.removeEventListener('resize', handleResize);
                    if (chartRef.current) {
                        chartRef.current.remove();
                        chartRef.current = null;
                    }
                };
            } catch (err) {
                console.error('Chart initialization error:', err);
                if (isMounted) {
                    setError(err instanceof Error ? err.message : 'Failed to load chart data');
                    setLoading(false);
                }
            }
        };

        initChart();

        return () => {
            isMounted = false;
            if (chartRef.current) {
                chartRef.current.remove();
                chartRef.current = null;
            }
        };
    }, [trade.id, trade.positionId, trade.symbol, trade.entryDate, trade.exitDate, trade.entryPrice, trade.exitPrice, trade.buySell, trade.pnl, trade.instrumentId, theme]);

    return (
        <div className="tradingview-chart-container">
            <div className="chart-header">
                <h3>Trade Chart - {trade.symbol}</h3>
                {!loading && !error && (
                    <div className="chart-info">
                        <span className="chart-period">
                            {new Date(trade.entryDate).toLocaleDateString()} → {new Date(trade.exitDate).toLocaleDateString()}
                        </span>
                        <span className={`chart-pnl ${trade.pnl >= 0 ? 'positive' : 'negative'}`}>
                            P&L: ${trade.pnl.toFixed(2)}
                        </span>
                    </div>
                )}
            </div>
            {loading && <div className="chart-loading">Loading chart data...</div>}
            {error && <div className="chart-error">Error: {error}</div>}
            <div ref={chartContainerRef} className="chart-wrapper" style={{ opacity: loading || error ? 0 : 1, visibility: loading || error ? 'hidden' : 'visible' }} />
        </div>
    );
}

export default TradingViewChart;
