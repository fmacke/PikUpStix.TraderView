import type { TradeExecution } from '../../types/api';
import './TradeExecutionsList.css';

interface TradeExecutionsListProps {
    tradeExecutions: TradeExecution[];
    loading: boolean;
}

function TradeExecutionsList({ tradeExecutions, loading }: TradeExecutionsListProps) {
    if (loading) {
        return <p className="trade-executions-loading">Loading trade executions...</p>;
    }

    if (tradeExecutions.length === 0) {
        return <p className="trade-executions-empty">No trade executions available for this position.</p>;
    }

    const formatDate = (dateTimeString: string): string => {
        try {
            const date = new Date(dateTimeString);
            return date.toLocaleDateString('en-UK', { 
                year: 'numeric', 
                month: '2-digit', 
                day: '2-digit' 
            });
        } catch {
            return dateTimeString;
        }
    };

    const formatTime = (dateTimeString: string): string => {
        try {
            const date = new Date(dateTimeString);
            return date.toLocaleTimeString('en-US', { 
                hour: '2-digit', 
                minute: '2-digit', 
                second: '2-digit',
                hour12: false 
            });
        } catch {
            return '';
        }
    };

    const formatNumber = (value: number | null | undefined, decimals: number = 2): string => {
        if (value === null || value === undefined) return '-';
        return typeof value === 'number' ? value.toFixed(decimals) : '-';
    };

    return (
        <div className="trade-executions-container">
            <h3 className="trade-executions-title">Trade Executions</h3>
            <table className="trade-executions-table">
                <thead>
                    <tr>
                        <th>ID</th>
                        <th>Date</th>
                        <th>Time</th>
                        <th>Quantity</th>
                        <th>Trade Price</th>
                        <th>IB Commission</th>
                        <th>Commission Currency</th>
                        <th>Close Price</th>
                        <th>Cost</th>
                        <th>FIFO PnL Realized</th>
                        <th>IB Exec ID</th>
                    </tr>
                </thead>
                <tbody>
                    {tradeExecutions.map((execution) => (
                        <tr key={execution.id}>
                            <td className="execution-id">{execution.id}</td>
                            <td className="execution-date">{formatDate(execution.dateTime)}</td>
                            <td className="execution-time">{formatTime(execution.dateTime)}</td>
                            <td className="number-cell">{formatNumber(execution.quantity)}</td>
                            <td className="number-cell">{formatNumber(execution.tradePrice, 4)}</td>
                            <td className="number-cell">{formatNumber(execution.ibCommission, 2)}</td>
                            <td className="currency-cell">{execution.ibCommissionCurrency || '-'}</td>
                            <td className="number-cell">{formatNumber(execution.closePrice, 4)}</td>
                            <td className="number-cell">{formatNumber(execution.cost, 2)}</td>
                            <td className="number-cell">{formatNumber(execution.fifoPnlRealized, 2)}</td>
                            <td className="execution-id">{execution.ibExecId || '-'}</td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
}

export default TradeExecutionsList;
