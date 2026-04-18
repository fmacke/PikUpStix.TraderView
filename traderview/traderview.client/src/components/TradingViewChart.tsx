import { useEffect, useRef, useState } from 'react';
import { createChart, CandlestickSeries } from 'lightweight-charts';
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

                // Fetch candlestick data
                const tradeContext = await apiService.getTradeCandlesticks(trade.id, 5, 5);

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
                const candlestickData = tradeContext.candlesticks.map(candle => ({
                    time: Math.floor(new Date(candle.date).getTime() / 1000) as any,
                    open: candle.open,
                    high: candle.high,
                    low: candle.low,
                    close: candle.close,
                }));

                console.log('Candlestick data for chart:', candlestickData);

                candlestickSeries.setData(candlestickData);

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

                // Fit content to visible range
                chart.timeScale().fitContent();

                setLoading(false);

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
            <div ref={chartContainerRef} className="chart-wrapper" style={{ display: loading || error ? 'none' : 'block' }} />
        </div>
    );
}

export default TradingViewChart;
