import React, { useEffect, useState, useCallback } from 'react';
import { apiService } from '../services/apiService';
import { type RiskMatrixCalculationResultDto } from '../types/api.ts';

export const DesiredPerformanceCard: React.FC = () => {
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
        <div className="bg-white shadow-md rounded-lg p-3 max-w-xs">
            <h3 className="text-sm font-semibold text-gray-800 mb-2">Desired Performance & Risk Forecast</h3>

            {/* Input Form Controls */}
            <form onSubmit={handleSubmit} className="performance-form" style={{ marginBottom: '12px', display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '8px', alignItems: 'flex-end' }}>
                <div className="form-group">
                    <label style={{ display: 'block', marginBottom: '3px', fontSize: '0.75rem' }}>Portfolio:</label>
                    <input
                        type="number"
                        value={portfolioSize}
                        onChange={(e) => setPortfolioSize(Number(e.target.value))}
                        step="any"
                        required
                        style={{ padding: '4px', borderRadius: '3px', border: '1px solid #ccc', fontSize: '0.8rem', width: '100%' }}
                    />
                </div>

                <div className="form-group">
                    <label style={{ display: 'block', marginBottom: '3px', fontSize: '0.75rem' }}>Position %:</label>
                    <input
                        type="number"
                        value={positionSizePercent}
                        onChange={(e) => setPositionSizePercent(Number(e.target.value))}
                        step="0.1"
                        required
                        style={{ padding: '4px', borderRadius: '3px', border: '1px solid #ccc', fontSize: '0.8rem', width: '100%' }}
                    />
                </div>

                <div className="form-group">
                    <label style={{ display: 'block', marginBottom: '3px', fontSize: '0.75rem' }}>Return %:</label>
                    <input
                        type="number"
                        value={desiredReturnPercent}
                        onChange={(e) => setDesiredReturnPercent(Number(e.target.value))}
                        step="0.1"
                        required
                        style={{ padding: '4px', borderRadius: '3px', border: '1px solid #ccc', fontSize: '0.8rem', width: '100%' }}
                    />
                </div>

                <button type="submit" disabled={loading} style={{ padding: '6px 10px', cursor: 'pointer', backgroundColor: '#007bff', color: '#fff', border: 'none', borderRadius: '3px', fontSize: '0.75rem' }}>
                    {loading ? 'Calc...' : 'Calculate'}
                </button>
            </form>

            {loading && <div className="loading" style={{ fontSize: '0.8rem' }}>Calculating...</div>}
            {error && <div className="error-message" style={{ color: 'red', fontSize: '0.8rem' }}>Error: {error}</div>}

            {!loading && data && (
                <>
                    <div className="metrics-grid" style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(130px, 1fr))', gap: '8px', marginBottom: '10px' }}>
                        <div className="metric-card" style={{ padding: '8px', border: '1px solid #ddd', borderRadius: '4px' }}>
                            <span className="metric-label" style={{ display: 'block', color: '#666', fontSize: '0.7rem' }}>Expected Net Return</span>
                            <span className="metric-value" style={{ fontSize: '1rem', fontWeight: 'bold' }}>{data.expectedNetReturnPercent.toFixed(2)}%</span>
                            <span className="metric-sub" style={{ display: 'block', fontSize: '0.7rem', color: '#666' }}>({data.expectedNetReturnCurrency.toLocaleString()})</span>
                        </div>

                        <div className="metric-card" style={{ padding: '8px', border: '1px solid #ddd', borderRadius: '4px' }}>
                            <span className="metric-label" style={{ display: 'block', color: '#666', fontSize: '0.7rem' }}>Goal Target</span>
                            <span className="metric-value" style={{ fontSize: '1rem', fontWeight: 'bold' }}>{data.goalCurrency.toLocaleString()}</span>
                            <span className="metric-sub" style={{ display: 'block', fontSize: '0.7rem', color: '#666' }}>T: {Math.ceil(data.numberOfTradesNeededToReachGoal)}</span>
                        </div>

                        <div className="metric-card" style={{ padding: '8px', border: '1px solid #ddd', borderRadius: '4px' }}>
                            <span className="metric-label" style={{ display: 'block', color: '#666', fontSize: '0.7rem' }}>G/L Ratio</span>
                            <span className="metric-value" style={{ fontSize: '1rem', fontWeight: 'bold' }}>{data.gainLossRatio.toFixed(2)}</span>
                            <span className="metric-sub" style={{ display: 'block', fontSize: '0.7rem', color: '#666' }}>{data.adjustedGainLossRatio.toFixed(2)}</span>
                        </div>

                        <div className="metric-card" style={{ padding: '8px', border: '1px solid #ddd', borderRadius: '4px' }}>
                            <span className="metric-label" style={{ display: 'block', color: '#666', fontSize: '0.7rem' }}>Optimal F</span>
                            <span className="metric-value" style={{ fontSize: '1rem', fontWeight: 'bold' }}>{data.otpimalF.toFixed(4)}</span>
                            <span className="metric-sub" style={{ display: 'block', fontSize: '0.7rem', color: '#666' }}>{data.positionSize.toLocaleString()}</span>
                        </div>
                    </div>

                    <div className="trades-breakdown" style={{ fontSize: '0.75rem' }}>
                        <div style={{ fontWeight: 'bold', marginBottom: '4px' }}>Trades</div>
                        <div>Wins: <strong>{data.numberOfWinningTrades}</strong> ({data.averageCurrencyGainOnWinningTrade.toLocaleString()})</div>
                        <div>Losses: <strong>{data.numberOfLosingTrades}</strong> ({data.averageCurrencyLossOnLosingTrade.toLocaleString()})</div>
                    </div>
                </>
            )}
        </div>
    );
};

export default DesiredPerformanceCard;