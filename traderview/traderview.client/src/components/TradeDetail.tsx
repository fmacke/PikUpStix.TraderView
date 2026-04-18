import type { Trade } from '../types/api';
import './TradeDetail.css';
import TradingViewChart from './TradingViewChart';

interface TradeDetailProps {
    trade: Trade | null;
}

function TradeDetail({ trade }: TradeDetailProps) {
    if (!trade) {
        return (
            <div className="trade-detail">
                <div className="no-selection">
                    <h2>No Trade Selected</h2>
                    <p>Select a trade from the list to view details</p>
                </div>
            </div>
        );
    }

    return (
        <div className="trade-detail">
            <div className="trade-header">
                <h1>{trade.symbol}</h1>
                <span className={`trade-side ${trade.buySell.toLowerCase()}`}>
                    {trade.buySell}
                </span>
            </div>

            <div className="chart-container">
                <TradingViewChart trade={trade} />
            </div>

            <div className="detail-sections">
                <div className="detail-section">
                    <h3>Trade Summary</h3>
                    <div className="detail-grid">
                        <div className="detail-item">
                            <label>Entry Date</label>
                            <span>{new Date(trade.entryDate).toLocaleDateString()}</span>
                        </div>
                        <div className="detail-item">
                            <label>Exit Date</label>
                            <span>{new Date(trade.exitDate).toLocaleDateString()}</span>
                        </div>
                        <div className="detail-item">
                            <label>Entry Price</label>
                            <span>${trade.entryPrice.toFixed(2)}</span>
                        </div>
                        <div className="detail-item">
                            <label>Exit Price</label>
                            <span>${trade.exitPrice.toFixed(2)}</span>
                        </div>
                        <div className="detail-item">
                            <label>Quantity</label>
                            <span>{trade.quantity.toFixed(2)}</span>
                        </div>
                        <div className="detail-item">
                            <label>Instrument ID</label>
                            <span>{trade.instrumentId}</span>
                        </div>
                    </div>
                </div>

                <div className="detail-section">
                    <h3>Performance</h3>
                    <div className="pnl-display">
                        <label>Profit & Loss</label>
                        <span className={`pnl-value ${trade.pnl >= 0 ? 'positive' : 'negative'}`}>
                            ${trade.pnl.toFixed(2)}
                        </span>
                    </div>
                    <div className="pnl-metrics">
                        <div className="metric">
                            <label>Price Change</label>
                            <span className={trade.exitPrice - trade.entryPrice >= 0 ? 'positive' : 'negative'}>
                                ${(trade.exitPrice - trade.entryPrice).toFixed(2)} 
                                ({((trade.exitPrice - trade.entryPrice) / trade.entryPrice * 100).toFixed(2)}%)
                            </span>
                        </div>
                        <div className="metric">
                            <label>Total Value</label>
                            <span>${(trade.quantity * trade.exitPrice).toFixed(2)}</span>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}

export default TradeDetail;
