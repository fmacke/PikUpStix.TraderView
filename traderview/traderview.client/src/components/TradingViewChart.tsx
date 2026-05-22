import { useEffect, useRef, useState } from 'react';
import { createChart, CandlestickSeries, LineSeries } from 'lightweight-charts';
import type { IChartApi } from 'lightweight-charts';
import type { Trade } from '../types/api';
import { apiService } from '../services/apiService';
import './TradingViewChart.css';

interface TradingViewChartProps {
    trade: Trade;
}

function TradingViewChart({ trade }: TradingViewChartProps) {
    const chartContainerRef = useRef<HTMLDivElement>(null);
    const chartRef = useRef<IChartApi | null>(null);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        let isMounted = true;

        const initChart = async () => {
            try {
                setLoading(true);
                setError(null);

                console.log('Fetching candlesticks for trade:', trade.id);

                // Clean up existing chart before creating a new one
                if (chartRef.current) {
                    chartRef.current.remove();
                    chartRef.current = null;
                }

                // Fetch candlestick data with up to 100 bars before entry and 100 bars after exit
                // Using 150 calendar days to approximately get 100 trading days (accounting for weekends/holidays)
                const tradeContext = await apiService.getTradeCandlesticks(trade.id, 150, 150);

                console.log('Received candlestick data:', tradeContext);

                if (!isMounted || !chartContainerRef.current) return;

                // Check if we have candlestick data
                if (!tradeContext.candlesticks || tradeContext.candlesticks.length === 0) {
                    console.warn('No candlestick data available for trade:', trade.id);
                    setError(`No historical price data available for ${trade.symbol}. The HistoricalData table may not have data for this instrument (ID: ${trade.instrumentId}).`);
                    setLoading(false);
                    return;
                }

                console.log('Number of candlesticks:', tradeContext.candlesticks.length);

                // Create chart
                const chart = createChart(chartContainerRef.current!, {
                    layout: {
                        background: { color: '#ffffff' },
                        textColor: '#333',
                    },
                    grid: {
                        vertLines: { color: '#e0e0e0' },
                        horzLines: { color: '#e0e0e0' },
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
                        borderColor: '#cccccc',
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

                // Add a line series connecting entry and exit points in blue
                // Only add the line if entry and exit are on different days
                // Parse dates as date strings to avoid timezone shifts
                const entryDateStr = trade.entryDate.split('T')[0];
                const exitDateStr = trade.exitDate.split('T')[0];

                if (entryDateStr !== exitDateStr) {
                    const tradeLine = chart.addSeries(LineSeries, {
                        color: '#2196F3', // blue color for all trades
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

                // Add price lines for entry and exit points
                candlestickSeries.createPriceLine({
                    price: trade.entryPrice,
                    color: trade.buySell === 'BUY' ? '#26a69a' : '#ef5350',
                    lineWidth: 2,
                    lineStyle: 2, // dashed
                    axisLabelVisible: true,
                    title: `Entry: $${trade.entryPrice.toFixed(2)}`,
                });

                candlestickSeries.createPriceLine({
                    price: trade.exitPrice,
                    color: trade.buySell === 'BUY' ? '#ef5350' : '#26a69a',
                    lineWidth: 2,
                    lineStyle: 2, // dashed
                    axisLabelVisible: true,
                    title: `Exit: $${trade.exitPrice.toFixed(2)}`,
                });

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

                // Handle window resize
                const handleResize = () => {
                    if (chartContainerRef.current && chartRef.current) {
                        chartRef.current.applyOptions({
                            width: chartContainerRef.current.clientWidth,
                        });
                    }
                };

                window.addEventListener('resize', handleResize);

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
    }, [trade]);

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
