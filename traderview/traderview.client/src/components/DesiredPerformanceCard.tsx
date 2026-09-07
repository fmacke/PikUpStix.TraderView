// src/components/DesiredPerformanceView.tsx

import React, { useEffect, useState } from 'react';
import { apiService } from '../services/apiService';
import { type RiskMatrixCalculationResultDto } from '../types/api.ts';

export const DesiredPerformanceView: React.FC = () => {
    const [data, setData] = useState<RiskMatrixCalculationResultDto | null>(null);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const fetchDesiredPerformance = async () => {
            try {
                setLoading(true);
                const result = await apiService.getDesiredPerformance();
                setData(result);
                setError(null);
            } catch (err: any) {
                setError(err.response?.data?.message || 'Error fetching desired performance data.');
            } finally {
                setLoading(false);
            }
        };

        fetchDesiredPerformance();
    }, []);

    if (loading) return <div className="loading">Calculating performance forecast...</div>;
    if (error) return <div className="error-message">Error: {error}</div>;
    if (!data) return <div>No data available.</div>;

    return (
        <div className="desired-performance-container">
            <h2>Desired Performance & Risk Forecast</h2>

            <div className="metrics-grid">
                <div className="metric-card">
                    <span className="metric-label">Expected Net Return</span>
                    <span className="metric-value">{data.expectedNetReturnPercent.toFixed(2)}%</span>
                    <span className="metric-sub">({data.expectedNetReturnCurrency.toLocaleString()} Currency)</span>
                </div>

                <div className="metric-card">
                    <span className="metric-label">Goal Target</span>
                    <span className="metric-value">{data.goalCurrency.toLocaleString()}</span>
                    <span className="metric-sub">Trades needed: {Math.ceil(data.numberOfTradesNeededToReachGoal)}</span>
                </div>

                <div className="metric-card">
                    <span className="metric-label">Gain / Loss Ratio</span>
                    <span className="metric-value">{data.gainLossRatio.toFixed(2)}</span>
                    <span className="metric-sub">Adjusted: {data.adjustedGainLossRatio.toFixed(2)}</span>
                </div>

                <div className="metric-card">
                    <span className="metric-label">Optimal F</span>
                    <span className="metric-value">{data.otpimalF.toFixed(4)}</span>
                    <span className="metric-sub">Position Size: {data.positionSize.toLocaleString()}</span>
                </div>
            </div>

            <div className="trades-breakdown">
                <h3>Trade Breakdown</h3>
                <ul>
                    <li>Winning Trades: <strong>{data.numberOfWinningTrades}</strong> (Avg Gain: {data.averageCurrencyGainOnWinningTrade.toLocaleString()})</li>
                    <li>Losing Trades: <strong>{data.numberOfLosingTrades}</strong> (Avg Loss: {data.averageCurrencyLossOnLosingTrade.toLocaleString()})</li>
                </ul>
            </div>
        </div>
    );
};

export default DesiredPerformanceView;