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

    // Sort trades by exit date (most recent first)
    const sortedTrades = [...trades].sort((a, b) => {
        return new Date(b.exitDate).getTime() - new Date(a.exitDate).getTime();
    });

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
        if (sortedTrades.length === 0) return;

        const currentIndex = sortedTrades.findIndex(trade => trade.id === selectedTradeId);

        if (event.key === 'ArrowDown') {
            event.preventDefault();
            const nextIndex = currentIndex < sortedTrades.length - 1 ? currentIndex + 1 : 0;
            onTradeSelect(sortedTrades[nextIndex]);
        } else if (event.key === 'ArrowUp') {
            event.preventDefault();
            const prevIndex = currentIndex > 0 ? currentIndex - 1 : sortedTrades.length - 1;
            onTradeSelect(sortedTrades[prevIndex]);
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
                {sortedTrades.map((trade, index) => (
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
