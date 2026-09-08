import React, { useEffect, useState, useCallback } from 'react';
import { apiService } from '../services/apiService';
import { type RiskMatrixCalculationResultDto } from '../types/api.ts';

export const DesiredPerformanceView: React.FC = () => {
    // Input parameters state with default initial values
    const [portfolioSize, setPortfolioSize] = useState<number>(100000);
    const [positionSizePercent, setPositionSizePercent] = useState<number>(2.0);
    const [desiredReturnPercent, setDesiredReturnPercent] = useState<number>(20.0);

    const [data, setData] = useState<RiskMatrixCalculationResultDto | null>(null);
    const [loading, setLoading] = useState<boolean>(false);
    const [error, setError] = useState<string | null>(null);

    const fetchDesiredPerformance = useCallback(async () => {
        try {
            setLoading(true);
            const result = await apiService.getDesiredPerformance(
                portfolioSize,
                positionSizePercent,
                desiredReturnPercent
            );
            setData(result);
            setError(null);
        } catch (err: any) {
            setError(err.response?.data?.message || 'Error fetching desired performance data.');
        } finally {
            setLoading(false);
        }
    }, [portfolioSize, positionSizePercent, desiredReturnPercent]);

    useEffect(() => {
        fetchDesiredPerformance();
    }, [fetchDesiredPerformance]);

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        fetchDesiredPerformance();
    };

    return (
        <div className="desired-performance-container">
            <h2>Desired Performance & Risk Forecast</h2>

            {/* Input Form Controls */}
            <form onSubmit={handleSubmit} className="performance-form" style={{ marginBottom: '20px', display: 'flex', gap: '15px', alignItems: 'flex-end', flexWrap: 'wrap' }}>
                <div className="form-group">
                    <label style={{ display: 'block', marginBottom: '5px', fontSize: '0.9rem' }}>Portfolio Size:</label>
                    <input
                        type="number"
                        value={portfolioSize}
                        onChange={(e) => setPortfolioSize(Number(e.target.value))}
                        step="any"
                        required
                        style={{ padding: '8px', borderRadius: '4px', border: '1px solid #ccc' }}
                    />
                </div>

                <div className="form-group">
                    <label style={{ display: 'block', marginBottom: '5px', fontSize: '0.9rem' }}>Position Size (%):</label>
                    <input
                        type="number"
                        value={positionSizePercent}
                        onChange={(e) => setPositionSizePercent(Number(e.target.value))}
                        step="0.1"
                        required
                        style={{ padding: '8px', borderRadius: '4px', border: '1px solid #ccc' }}
                    />
                </div>

                <div className="form-group">
                    <label style={{ display: 'block', marginBottom: '5px', fontSize: '0.9rem' }}>Desired Return (%):</label>
                    <input
                        type="number"
                        value={desiredReturnPercent}
                        onChange={(e) => setDesiredReturnPercent(Number(e.target.value))}
                        step="0.1"
                        required
                        style={{ padding: '8px', borderRadius: '4px', border: '1px solid #ccc' }}
                    />
                </div>

                <button type="submit" disabled={loading} style={{ padding: '9px 18px', cursor: 'pointer', backgroundColor: '#007bff', color: '#white', border: 'none', borderRadius: '4px' }}>
                    {loading ? 'Calculating...' : 'Calculate Forecast'}
                </button>
            </form>

            {loading && <div className="loading">Calculating performance forecast...</div>}
            {error && <div className="error-message" style={{ color: 'red' }}>Error: {error}</div>}

            {!loading && data && (
                <>
                    <div className="metrics-grid" style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))', gap: '15px', marginBottom: '20px' }}>
                        <div className="metric-card" style={{ padding: '15px', border: '1px solid #ddd', borderRadius: '6px' }}>
                            <span className="metric-label" style={{ display: 'block', color: '#666' }}>Expected Net Return</span>
                            <span className="metric-value" style={{ fontSize: '1.5rem', fontWeight: 'bold' }}>{data.expectedNetReturnPercent.toFixed(2)}%</span>
                            <span className="metric-sub" style={{ display: 'block', fontSize: '0.85rem', color: '#886' }}>({data.expectedNetReturnCurrency.toLocaleString()} Currency)</span>
                        </div>

                        <div className="metric-card" style={{ padding: '15px', border: '1px solid #ddd', borderRadius: '6px' }}>
                            <span className="metric-label" style={{ display: 'block', color: '#666' }}>Goal Target</span>
                            <span className="metric-value" style={{ fontSize: '1.5rem', fontWeight: 'bold' }}>{data.goalCurrency.toLocaleString()}</span>
                            <span className="metric-sub" style={{ display: 'block', fontSize: '0.85rem', color: '#886' }}>Trades needed: {Math.ceil(data.numberOfTradesNeededToReachGoal)}</span>
                        </div>

                        <div className="metric-card" style={{ padding: '15px', border: '1px solid #ddd', borderRadius: '6px' }}>
                            <span className="metric-label" style={{ display: 'block', color: '#666' }}>Gain / Loss Ratio</span>
                            <span className="metric-value" style={{ fontSize: '1.5rem', fontWeight: 'bold' }}>{data.gainLossRatio.toFixed(2)}</span>
                            <span className="metric-sub" style={{ display: 'block', fontSize: '0.85rem', color: '#886' }}>Adjusted: {data.adjustedGainLossRatio.toFixed(2)}</span>
                        </div>

                        <div className="metric-card" style={{ padding: '15px', border: '1px solid #ddd', borderRadius: '6px' }}>
                            <span className="metric-label" style={{ display: 'block', color: '#666' }}>Optimal F</span>
                            <span className="metric-value" style={{ fontSize: '1.5rem', fontWeight: 'bold' }}>{data.otpimalF.toFixed(4)}</span>
                            <span className="metric-sub" style={{ display: 'block', fontSize: '0.85rem', color: '#886' }}>Position Size: {data.positionSize.toLocaleString()}</span>
                        </div>
                    </div>

                    <div className="trades-breakdown">
                        <h3>Trade Breakdown</h3>
                        <ul>
                            <li>Winning Trades: <strong>{data.numberOfWinningTrades}</strong> (Avg Gain: {data.averageCurrencyGainOnWinningTrade.toLocaleString()})</li>
                            <li>Losing Trades: <strong>{data.numberOfLosingTrades}</strong> (Avg Loss: {data.averageCurrencyLossOnLosingTrade.toLocaleString()})</li>
                        </ul>
                    </div>
                </>
            )}
        </div>
    );
};

export default DesiredPerformanceView;