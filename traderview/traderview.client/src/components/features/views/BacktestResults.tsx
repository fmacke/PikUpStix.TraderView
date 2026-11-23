import type { BacktestResult } from '../../../services/backtestService';
import './BacktestResults.css';

interface BacktestResultsProps {
    result: BacktestResult;
}

export function BacktestResults({ result }: BacktestResultsProps) {

    const profitableTradesPercentage = result.totalTrades > 0 ? (result.winningTrades / result.totalTrades) * 100 : 0;

    return (
        <div className="backtest-results">
            <div className="results-header">
                <h2>{result.strategyMetadata.StrategyName || 'Backtest Results'}</h2>
                <div className="date-range">
                    {new Date(result.backtestStartDate).toLocaleDateString()} -{' '}
                    {new Date(result.backtestEndDate).toLocaleDateString()}
                </div>
            </div>

            {/* Key Metrics */}
            <div className="metrics-grid">
                <div className="metric-card">
                    <div className="metric-label">Total Return</div>
                    <div className={`metric-value ${result.totalReturn >= 0 ? 'positive' : 'negative'}`}>
                        {result.totalReturnPercent >= 0 ? '+' : ''}
                        {result.totalReturnPercent.toFixed(2)}%
                    </div>
                    <div className="metric-subtext">${result.totalReturn.toFixed(2)}</div>
                </div>

                <div className="metric-card">
                    <div className="metric-label">Total Trades</div>
                    <div className="metric-value">{result.totalTrades}</div>
                    <div className="metric-subtext">
                        {result.winningTrades} W / {result.losingTrades} L
                    </div>
                </div>

                <div className="metric-card">
                    <div className="metric-label">Win Rate</div>
                    <div className={`metric-value ${result.winRate > 0.5 ? 'positive' : 'negative'}`}>
                        {(result.winRate * 100).toFixed(1)}%
                    </div>
                    <div className="metric-subtext">{profitableTradesPercentage.toFixed(1)}% trades</div>
                </div>

                <div className="metric-card">
                    <div className="metric-label">Profit Factor</div>
                    <div className={`metric-value ${result.profitFactor > 1.5 ? 'positive' : result.profitFactor > 1 ? 'neutral' : 'negative'}`}>
                        {result.profitFactor.toFixed(2)}
                    </div>
                    <div className="metric-subtext">
                        Avg W: ${Math.abs(result.avgWin).toFixed(2)}
                    </div>
                </div>

                <div className="metric-card">
                    <div className="metric-label">Max Drawdown</div>
                    <div className={`metric-value ${result.maxDrawdownPercent < 20 ? 'positive' : result.maxDrawdownPercent < 40 ? 'neutral' : 'negative'}`}>
                        {result.maxDrawdownPercent.toFixed(2)}%
                    </div>
                    <div className="metric-subtext">${result.maxDrawdown.toFixed(2)}</div>
                </div>

                <div className="metric-card">
                    <div className="metric-label">Final Capital</div>
                    <div className={`metric-value ${result.finalCapital >= result.initialCapital ? 'positive' : 'negative'}`}>
                        ${result.finalCapital.toFixed(2)}
                    </div>
                    <div className="metric-subtext">
                        Initial: ${result.initialCapital.toFixed(2)}
                    </div>
                </div>
            </div>

            {/* Equity Curve Mini Chart */}
            {result.equityCurve.length > 0 && (
                <div className="chart-section">
                    <h3>Equity Curve</h3>
                    <div className="mini-chart">
                        <svg viewBox="0 0 1000 300" preserveAspectRatio="xMidYMid meet">
                            {/* Simple line chart */}
                            <polyline
                                points={result.equityCurve
                                    .map((point, i) => {
                                        const x = (i / (result.equityCurve.length - 1 || 1)) * 1000;
                                        const minEquity = Math.min(...result.equityCurve.map((p) => p.equity));
                                        const maxEquity = Math.max(...result.equityCurve.map((p) => p.equity));
                                        const range = maxEquity - minEquity || 1;
                                        const y = 300 - ((point.equity - minEquity) / range) * 280;
                                        return `${x},${y}`;
                                    })
                                    .join(' ')}
                                fill="none"
                                stroke="currentColor"
                                strokeWidth="2"
                            />
                        </svg>
                    </div>
                </div>
            )}

            {/* Trade Statistics Table */}
            <div className="trades-section">
                <h3>Trade History ({result.trades.length} trades)</h3>
                <div className="trades-table-container">
                    <table className="trades-table">
                        <thead>
                            <tr>
                                <th>Entry Date</th>
                                <th>Entry Price</th>
                                <th>Exit Date</th>
                                <th>Exit Price</th>
                                <th>Return %</th>
                                <th>Net P&L</th>
                                <th>Bars Held</th>
                                <th>MFE%</th>
                                <th>MAE%</th>
                            </tr>
                        </thead>
                        <tbody>
                            {result.trades.map((trade, idx) => (
                                <tr key={idx} className={trade.netProfit >= 0 ? 'positive' : 'negative'}>
                                    <td>{new Date(trade.entryDate).toLocaleDateString()}</td>
                                    <td>${trade.entryPrice.toFixed(2)}</td>
                                    <td>{trade.exitDate ? new Date(trade.exitDate).toLocaleDateString() : 'Open'}</td>
                                    <td>{trade.exitPrice ? `$${trade.exitPrice.toFixed(2)}` : '-'}</td>
                                    <td className={trade.returnPercent >= 0 ? 'positive' : 'negative'}>
                                        {trade.returnPercent >= 0 ? '+' : ''}
                                        {trade.returnPercent.toFixed(2)}%
                                    </td>
                                    <td className={trade.netProfit >= 0 ? 'positive' : 'negative'}>
                                        {trade.netProfit >= 0 ? '+' : ''}${Math.abs(trade.netProfit).toFixed(2)}
                                    </td>
                                    <td>{trade.barsHeld || '-'}</td>
                                    <td>{trade.maxFavorableExcursion.toFixed(2)}%</td>
                                    <td>{trade.maxAdverseExcursion.toFixed(2)}%</td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            </div>

            {/* Summary Statistics */}
            <div className="summary-section">
                <div className="summary-box">
                    <h4>Trade Statistics</h4>
                    <ul>
                        <li>
                            <span>Average Win:</span>
                            <span className="positive">${Math.abs(result.avgWin).toFixed(2)}</span>
                        </li>
                        <li>
                            <span>Average Loss:</span>
                            <span className="negative">${Math.abs(result.avgLoss).toFixed(2)}</span>
                        </li>
                        <li>
                            <span>Win/Loss Ratio:</span>
                            <span>{(result.avgWin / Math.abs(result.avgLoss) || 0).toFixed(2)}</span>
                        </li>
                        <li>
                            <span>Total Commission:</span>
                            <span>${result.totalCommission.toFixed(2)}</span>
                        </li>
                    </ul>
                </div>

                <div className="summary-box">
                    <h4>Risk Metrics</h4>
                    <ul>
                        <li>
                            <span>Max Drawdown %:</span>
                            <span className="negative">{result.maxDrawdownPercent.toFixed(2)}%</span>
                        </li>
                        <li>
                            <span>Max Drawdown $:</span>
                            <span className="negative">${result.maxDrawdown.toFixed(2)}</span>
                        </li>
                        <li>
                            <span>Profit Factor:</span>
                            <span>{result.profitFactor.toFixed(2)}</span>
                        </li>
                        <li>
                            <span>Period:</span>
                            <span>
                                {((new Date(result.backtestEndDate).getTime() - new Date(result.backtestStartDate).getTime()) / (1000 * 60 * 60 * 24)).toFixed(0)} days
                            </span>
                        </li>
                    </ul>
                </div>
            </div>
        </div>
    );
}
