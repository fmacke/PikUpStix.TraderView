import { useEffect, useRef } from 'react';
import type { Trade } from '../types/api';
import './TradeList.css';

interface TradeListProps {
    trades: Trade[];
    selectedTradeId: number | null;
    onTradeSelect: (trade: Trade) => void;
}

function TradeList({ trades, selectedTradeId, onTradeSelect }: TradeListProps) {
    const listRef = useRef<HTMLDivElement>(null);
    const selectedItemRef = useRef<HTMLDivElement>(null);

    // Focus the list container on mount and when selection changes
    useEffect(() => {
        if (listRef.current) {
            listRef.current.focus();
        }
    }, []);

    // Scroll selected item into view
    useEffect(() => {
        if (selectedItemRef.current) {
            selectedItemRef.current.scrollIntoView({
                behavior: 'smooth',
                block: 'nearest',
            });
        }
    }, [selectedTradeId]);

    const handleKeyDown = (event: React.KeyboardEvent) => {
        if (trades.length === 0) return;

        const currentIndex = trades.findIndex(trade => trade.id === selectedTradeId);

        if (event.key === 'ArrowDown') {
            event.preventDefault();
            const nextIndex = currentIndex < trades.length - 1 ? currentIndex + 1 : 0;
            onTradeSelect(trades[nextIndex]);
        } else if (event.key === 'ArrowUp') {
            event.preventDefault();
            const prevIndex = currentIndex > 0 ? currentIndex - 1 : trades.length - 1;
            onTradeSelect(trades[prevIndex]);
        }
    };

    return (
        <div className="trade-list">
            <h2>Trades</h2>
            <div 
                className="trade-list-items"
                ref={listRef}
                tabIndex={0}
                onKeyDown={handleKeyDown}
            >
                {trades.map((trade, index) => (
                    <div
                        key={`${trade.id}-${index}`}
                        ref={selectedTradeId === trade.id ? selectedItemRef : null}
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
