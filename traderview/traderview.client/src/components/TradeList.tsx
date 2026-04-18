import type { Trade } from '../types/api';
import './TradeList.css';

interface TradeListProps {
    trades: Trade[];
    selectedTradeId: number | null;
    onTradeSelect: (trade: Trade) => void;
}

function TradeList({ trades, selectedTradeId, onTradeSelect }: TradeListProps) {
    return (
        <div className="trade-list">
            <h2>Trades</h2>
            <div className="trade-list-items">
                {trades.map(trade => (
                    <div
                        key={trade.id}
                        className={`trade-item ${selectedTradeId === trade.id ? 'selected' : ''}`}
                        onClick={() => onTradeSelect(trade)}
                    >
                        <div className="trade-symbol">{trade.symbol}</div>
                        <div className="trade-date">{new Date(trade.entryDate).toLocaleDateString()}</div>
                        <div className={`trade-pnl ${trade.pnl >= 0 ? 'positive' : 'negative'}`}>
                            ${trade.pnl.toFixed(2)}
                        </div>
                    </div>
                ))}
            </div>
        </div>
    );
}

export default TradeList;
