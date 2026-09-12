import { useEffect, useRef, useState } from 'react';
import { createChart, LineSeries } from 'lightweight-charts';
import type { IChartApi, LineData, Time } from 'lightweight-charts';
import type { AssetValueChartData } from '../../../types/api';
import './AssetValueChart.css';

interface AssetValueChartProps {
    data: AssetValueChartData[];
    title?: string;
    showLongShort?: boolean;
}

function AssetValueChart({ data, title = 'Asset Value Over Time', showLongShort = true }: AssetValueChartProps) {
    const chartContainerRef = useRef<HTMLDivElement>(null);
    const chartRef = useRef<IChartApi | null>(null);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        if (!chartContainerRef.current || data.length === 0) return;

        // Clean up existing chart
        if (chartRef.current) {
            chartRef.current.remove();
            chartRef.current = null;
        }

        try {
            setError(null);

            // Create chart
            const chart = createChart(chartContainerRef.current, {
                layout: {
                    background: { color: '#ffffff' },
                    textColor: '#333',
                },
                grid: {
                    vertLines: { color: '#e0e0e0' },
                    horzLines: { color: '#e0e0e0' },
                },
                width: chartContainerRef.current.clientWidth,
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

            // Convert data to line series format
            const totalAssetLineData: LineData<Time>[] = data.map((item) => ({
                time: Math.floor(new Date(item.date).getTime() / 1000) as Time,
                value: Number(item.totalAssetValue),
            }));

            const totalLongLineData: LineData<Time>[] = data.map((item) => ({
                time: Math.floor(new Date(item.date).getTime() / 1000) as Time,
                value: Number(item.totalLongValue),
            }));

            const totalShortLineData: LineData<Time>[] = data.map((item) => ({
                time: Math.floor(new Date(item.date).getTime() / 1000) as Time,
                value: Number(item.totalShortValue),
            }));

            // Create line series for total asset value
            const totalAssetSeries = chart.addSeries(LineSeries, {
                color: '#2196F3',
                lineWidth: 2,
                title: 'Total Asset Value',
            });
            totalAssetSeries.setData(totalAssetLineData);

            // Create line series for long value if requested
            if (showLongShort) {
                const longSeries = chart.addSeries(LineSeries, {
                    color: '#4CAF50',
                    lineWidth: 2,
                    title: 'Long Value',
                });
                longSeries.setData(totalLongLineData);

                const shortSeries = chart.addSeries(LineSeries, {
                    color: '#FF6B6B',
                    lineWidth: 2,
                    title: 'Short Value',
                });
                shortSeries.setData(totalShortLineData);
            }

            // Fit content
            chart.timeScale().fitContent();
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Failed to render chart');
            console.error('Chart rendering error:', err);
        }
    }, [data, showLongShort]);

    // Handle window resize
    useEffect(() => {
        const handleResize = () => {
            if (chartRef.current && chartContainerRef.current) {
                chartRef.current.applyOptions({
                    width: chartContainerRef.current.clientWidth,
                });
            }
        };

        window.addEventListener('resize', handleResize);
        return () => window.removeEventListener('resize', handleResize);
    }, []);

    if (error) {
        return <div className="asset-value-chart-error">Error: {error}</div>;
    }

    if (data.length === 0) {
        return <div className="asset-value-chart-empty">No data available</div>;
    }

    return (
        <div className="asset-value-chart-container">
            <h2 className="asset-value-chart-title">{title}</h2>
            <div ref={chartContainerRef} className="asset-value-chart" />
        </div>
    );
}

export default AssetValueChart;
