import { useEffect, useState } from 'react';
import backtestService from '../../../services/backtestService';
import type { BacktestRequest, BacktestResult } from '../../../services/backtestService';
import { BacktestForm } from './BacktestForm';
import { BacktestResults } from './BacktestResults';
import { tradeService } from '../../../services';
import './BacktestView.css';

export default function BacktestView() {
    const [result, setResult] = useState<BacktestResult | null>(null);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [instruments, setInstruments] = useState<Array<{ id: number; name: string }>>([]);
    const [instrumentsLoading, setInstrumentsLoading] = useState(true);

    // Fetch available instruments from trades
    useEffect(() => {
        const fetchInstruments = async () => {
            try {
                setInstrumentsLoading(true);
                const trades = await tradeService.getTrades();
                const uniqueInstruments = Array.from(
                    new Map(
                        trades
                            .filter((t) => t.instrumentId)
                            .map((t) => [
                                t.instrumentId,
                                { id: t.instrumentId, name: t.symbol || `ID ${t.instrumentId}` },
                            ])
                    ).values()
                ) as Array<{ id: number; name: string }>;
                setInstruments(uniqueInstruments.sort((a, b) => a.name.localeCompare(b.name)));
            } catch (err) {
                console.error('Error fetching instruments:', err);
                // Provide fallback instruments
                setInstruments([
                    { id: 1, name: 'Instrument 1' },
                    { id: 2, name: 'Instrument 2' },
                ]);
            } finally {
                setInstrumentsLoading(false);
            }
        };

        fetchInstruments();
    }, []);

    const handleRunBacktest = async (request: BacktestRequest) => {
        try {
            setLoading(true);
            setError(null);
            setResult(null);

            const backtestResult = await backtestService.runBacktest(request);
            setResult(backtestResult);
        } catch (err) {
            console.error('Error running backtest:', err);
            const errorMessage = err instanceof Error ? err.message : 'Failed to run backtest';
            setError(errorMessage);
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="backtest-view">
            <div className="backtest-container">
                <div className="backtest-left-panel">
                    <div className="form-container">
                        <h2>Backtest Engine</h2>
                        {instrumentsLoading ? (
                            <div className="loading-message">Loading instruments...</div>
                        ) : (
                            <BacktestForm
                                onSubmit={handleRunBacktest}
                                loading={loading}
                                instruments={instruments}
                            />
                        )}

                        {error && (
                            <div className="error-message">
                                <strong>Error:</strong> {error}
                            </div>
                        )}
                    </div>
                </div>

                <div className="backtest-right-panel">
                    {result ? (
                        <BacktestResults result={result} />
                    ) : (
                        <div className="results-placeholder">
                            <div className="placeholder-content">
                                <h3>📊 No Backtest Results Yet</h3>
                                <p>Configure your strategy on the left and click "Run Backtest" to analyze historical performance.</p>
                                <div className="placeholder-tips">
                                    <h4>Tips:</h4>
                                    <ul>
                                        <li>Select an instrument with available historical data</li>
                                        <li>Choose a date range with sufficient price history</li>
                                        <li>Adjust strategy parameters to test different configurations</li>
                                        <li>Use realistic slippage and commission values</li>
                                    </ul>
                                </div>
                            </div>
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
}
