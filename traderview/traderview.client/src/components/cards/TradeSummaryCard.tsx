import type { Position } from '../../types/api';
import './TradeSummaryCard.css';

interface TradeSummaryCardProps {
    trade: Position;
}

const TradeSummaryCard: React.FC<TradeSummaryCardProps> = ({ trade }) => {
    return (
        <div className="trade-summary-card">
            <h3>Trade Summary & Performance</h3>
            <table className="detail-table">
                <tbody>
                    <tr>
                        <td className="label">Entry Date</td>
                        <td>{new Date(trade.entryDate).toLocaleDateString()}</td>
                    </tr>
                    <tr>
                        <td className="label">Exit Date</td>
                        <td>{new Date(trade.exitDate).toLocaleDateString()}</td>
                    </tr>
                    <tr>
                        <td className="label">Quantity</td>
                        <td>{trade.quantity.toFixed(2)}</td>
                    </tr>
                    <tr>
                        <td className="label">Entry Price</td>
                        <td>${trade.entryPrice.toFixed(2)}</td>
                    </tr>
                    <tr>
                        <td className="label">Exit Price</td>
                        <td>${trade.exitPrice.toFixed(2)}</td>
                    </tr>
                    <tr>
                        <td className="label">Direction</td>
                        <td>{trade.buySell  }</td>
                    </tr>
                    <tr>
                        <td className="label">P&L</td>
                        <td className={trade.pnl >= 0 ? 'positive' : 'negative'}>
                            ${trade.pnl.toFixed(2)}
                        </td>
                    </tr>
                    <tr>
                        <td className="label">Price Change</td>
                        <td className={trade.exitPrice - trade.entryPrice >= 0 ? 'positive' : 'negative'}>
                            ${(trade.exitPrice - trade.entryPrice).toFixed(2)} ({((trade.exitPrice - trade.entryPrice) / trade.entryPrice * 100).toFixed(2)}%)
                        </td>
                    </tr>
                    <tr>
                        <td className="label">Total Value</td>
                        <td>${(trade.quantity * trade.exitPrice).toFixed(2)}</td>
                    </tr>
                </tbody>
            </table>
        </div>
    );
};

export default TradeSummaryCard;
