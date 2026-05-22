import { useEffect, useRef } from 'react';
import { createChart, LineSeries } from 'lightweight-charts';
import type { IChartApi, LineData, Time } from 'lightweight-charts';
import type { RSDataPoint } from '../types/api';
import './RSIndicatorChart.css';

interface RSIndicatorChartProps {
    rsData: RSDataPoint[];
}

function RSIndicatorChart({ rsData }: RSIndicatorChartProps) {
    const chartContainerRef = useRef<HTMLDivElement>(null);
    const chartRef = useRef<IChartApi | null>(null);

    useEffect(() => {
        if (!chartContainerRef.current || rsData.length === 0) return;

        // Clean up existing chart
        if (chartRef.current) {
            chartRef.current.remove();
            chartRef.current = null;
        }

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
            height: 250,
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

        // Create RS Line series
        const rsLineSeries = chart.addSeries(LineSeries, {
            color: '#2962FF',
            lineWidth: 2,
            title: 'RS Line',
            priceLineVisible: false,
        });

        // Create RS MA series
        const rsMASeries = chart.addSeries(LineSeries, {
            color: '#757575',
            lineWidth: 1,
            lineStyle: 2, // Dashed
            title: 'RS MA (21)',
            priceLineVisible: false,
        });

        // Convert RS data to chart format
        // Parse dates as date strings to avoid timezone shifts
        const rsLineData: LineData[] = rsData.map(point => ({
            time: point.date.split('T')[0] as Time,
            value: point.rsRatio,
        }));

        const rsMAData: LineData[] = rsData.map(point => ({
            time: point.date.split('T')[0] as Time,
            value: point.rsma,
        }));

        rsLineSeries.setData(rsLineData);
        rsMASeries.setData(rsMAData);

        // Note: Markers (setMarkers) are not available in this version of Lightweight Charts
        // RS new highs can be visualized through the dashboard metrics instead

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
    }, [rsData]);

    if (rsData.length === 0) {
        return (
            <div className="rs-indicator-chart">
                <div className="no-data">
                    <p>No RS indicator data available</p>
                </div>
            </div>
        );
    }

    return (
        <div className="rs-indicator-chart">
            <h3>Relative Strength Indicator</h3>
            <div className="chart-legend">
                <span className="legend-item">
                    <span className="legend-color" style={{ backgroundColor: '#2962FF' }}></span>
                    RS Line
                </span>
                <span className="legend-item">
                    <span className="legend-color" style={{ backgroundColor: '#757575' }}></span>
                    RS MA (21)
                </span>
            </div>
            <div ref={chartContainerRef} className="chart-wrapper" />
        </div>
    );
}

export default RSIndicatorChart;
