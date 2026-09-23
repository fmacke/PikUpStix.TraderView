import './TradeRollerList.css';
import type { Trade } from '../../../types/api';
import { useEffect, useRef, useState } from 'react';

interface TradeRollerListProps {
    trades: Trade[];
    selectedPositionId: number | null;
    onTradeSelect: (trade: Trade) => void;
}

export default function TradeRollerList({ trades, selectedPositionId, onTradeSelect }: TradeRollerListProps) {
    // Sort trades by exit date (most recent first)
    const sortedTrades = [...trades].sort((a, b) => {
        return new Date(b.exitDate).getTime() - new Date(a.exitDate).getTime();
    });

    // Duplicate items to allow continuous scrolling
    const loopItems = [...sortedTrades, ...sortedTrades];

    const containerRef = useRef<HTMLDivElement | null>(null);
    const itemRef = useRef<HTMLDivElement | null>(null);
    const rafRef = useRef<number | null>(null);
    const velocityRef = useRef<number>(0);
    const [spinning, setSpinning] = useState(false);

    useEffect(() => {
        return () => {
            if (rafRef.current) cancelAnimationFrame(rafRef.current);
        };
    }, []);

    function snapToNearest() {
        const container = containerRef.current;
        const item = itemRef.current;
        if (!container || !item || sortedTrades.length === 0) return;

        const itemHeight = item.getBoundingClientRect().height + parseFloat(getComputedStyle(item).marginBottom || '0');
        const scrollTop = container.scrollTop;
        const index = Math.round(scrollTop / itemHeight) % sortedTrades.length;
        const target = index * itemHeight;

        container.scrollTo({ top: target, behavior: 'smooth' });
        // select the trade that ended up centered
        const selected = sortedTrades[(index + sortedTrades.length) % sortedTrades.length];
        if (selected) onTradeSelect(selected);
    }

    function step() {
        const container = containerRef.current;
        if (!container) return;

        const v = velocityRef.current;
        if (Math.abs(v) < 0.1) {
            // stop
            setSpinning(false);
            velocityRef.current = 0;
            snapToNearest();
            return;
        }

        container.scrollTop += v;
        // wrap-around behavior if near edges (because items duplicated)
        const totalScrollHeight = container.scrollHeight / 2; // half corresponds to original list
        if (container.scrollTop >= totalScrollHeight) {
            container.scrollTop -= totalScrollHeight;
        }

        // apply friction
        velocityRef.current *= 0.985;

        rafRef.current = requestAnimationFrame(step);
    }

    function startSpin() {
        if (spinning) return;
        const initial = 50; // px per frame approx (vertical)
        velocityRef.current = initial;
        setSpinning(true);
        rafRef.current = requestAnimationFrame(step);
    }

    function stopSpin() {
        // reduce velocity quickly to stop
        velocityRef.current *= 0.2;
    }

    // When selectedPositionId changes externally, ensure the carousel centers that trade
    useEffect(() => {
        const container = containerRef.current;
        const item = itemRef.current;
        if (!container || !item || selectedPositionId == null) return;

        const itemHeight = item.getBoundingClientRect().height + parseFloat(getComputedStyle(item).marginBottom || '0');
        const index = sortedTrades.findIndex(t => t.positionId === selectedPositionId);
        if (index >= 0) {
            const target = index * itemHeight;
            container.scrollTo({ top: target, behavior: 'smooth' });
        }
    }, [selectedPositionId]);

    return (
        <div className="trade-roller">
            <div className="trade-roller-header">
                <h2>Trades</h2>
                <div className="roller-controls">
                    <button onClick={startSpin} disabled={spinning}>Spin</button>
                    <button onClick={stopSpin} disabled={!spinning}>Stop</button>
                </div>
            </div>

            <div
                className="trade-roller-window scroll-snap-container"
                ref={containerRef}
            >
                <div className="trade-roller-track">
                    {loopItems.map((trade, idx) => (
                        <div
                            key={`${trade.id}-${idx}`}
                            ref={idx === 0 ? itemRef : null}
                            className={`trade-roller-item ${selectedPositionId === trade.positionId ? 'selected' : ''}`}
                            onClick={() => onTradeSelect(trade)}
                        >
                            <div className="trade-symbol">{trade.symbol}</div>
                            <div className="trade-date">Closed: {new Date(trade.exitDate).toLocaleDateString()}</div>
                            <div className={`trade-pnl ${trade.pnl >= 0 ? 'positive' : 'negative'}`}>
                                ${trade.pnl.toFixed(2)}
                            </div>
                        </div>
                    ))}
                </div>
            </div>
        </div>
    );
}
