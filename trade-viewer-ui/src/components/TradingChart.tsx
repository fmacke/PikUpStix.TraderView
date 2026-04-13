import React, { useEffect, useRef } from 'react';
import { createChart } from 'lightweight-charts';
import type { IChartApi, ISeriesApi, CandlestickData, Time } from 'lightweight-charts';
import { CandlestickData as ApiCandlestick } from '../types/api';
import './TradingChart.css';

interface TradingChartProps {
  candlesticks: ApiCandlestick[];
  entryDate: string;
  exitDate: string;
  entryPrice: number;
  exitPrice: number;
}

const TradingChart: React.FC<TradingChartProps> = ({
  candlesticks,
  entryDate,
  exitDate,
  entryPrice,
  exitPrice,
}) => {
  const chartContainerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!chartContainerRef.current) return;

    // Create chart
    const chart = createChart(chartContainerRef.current, {
      width: chartContainerRef.current.clientWidth,
      height: 600,
      layout: {
        background: { color: '#1e1e1e' },
        textColor: '#d1d4dc',
      },
      grid: {
        vertLines: { color: '#2b2b2b' },
        horzLines: { color: '#2b2b2b' },
      },
      crosshair: {
        mode: 1,
      },
      rightPriceScale: {
        borderColor: '#485c7b',
      },
      timeScale: {
        borderColor: '#485c7b',
        timeVisible: true,
        secondsVisible: false,
      },
    });

    // Add candlestick series - using v5 API
    const candlestickSeries = (chart as any).addCandlestickSeries({
      upColor: '#26a69a',
      downColor: '#ef5350',
      borderVisible: false,
      wickUpColor: '#26a69a',
      wickDownColor: '#ef5350',
    });

    // Convert API data to chart format
    const chartData = candlesticks.map((candle) => ({
      time: Math.floor(new Date(candle.date).getTime() / 1000) as Time,
      open: candle.open,
      high: candle.high,
      low: candle.low,
      close: candle.close,
    }));

    candlestickSeries.setData(chartData);

    // Add price lines for entry and exit
    candlestickSeries.createPriceLine({
      price: entryPrice,
      color: '#2196F3',
      lineWidth: 2,
      lineStyle: 2,
      axisLabelVisible: true,
      title: 'Entry',
    });

    candlestickSeries.createPriceLine({
      price: exitPrice,
      color: '#f44336',
      lineWidth: 2,
      lineStyle: 2,
      axisLabelVisible: true,
      title: 'Exit',
    });

    // Fit content
    chart.timeScale().fitContent();

    // Handle resize
    const handleResize = () => {
      if (chartContainerRef.current) {
        chart.applyOptions({
          width: chartContainerRef.current.clientWidth,
        });
      }
    };

    window.addEventListener('resize', handleResize);

    return () => {
      window.removeEventListener('resize', handleResize);
      chart.remove();
    };
  }, [candlesticks, entryDate, exitDate, entryPrice, exitPrice]);

  return (
    <div className="trading-chart-container">
      <div ref={chartContainerRef} className="chart" />
    </div>
  );
};

export default TradingChart;

