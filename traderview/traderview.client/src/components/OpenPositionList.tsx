import { useEffect, useState } from 'react';
import { apiService } from '../services/apiService';
import type { OpenPosition } from '../types/api';
import './OpenPositionList.css';

function OpenPositionList() {
    const [openPositions, setOpenPositions] = useState<OpenPosition[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        loadOpenPositions();
    }, []);

    const loadOpenPositions = async () => {
        try {
            setLoading(true);
            setError(null);
            const positions = await apiService.getOpenPositions();
            setOpenPositions(positions);
        } catch (err) {
            const errorMessage = err instanceof Error ? err.message : 'Failed to load open positions';
            setError(errorMessage);
            console.error('Error loading open positions:', err);
        } finally {
            setLoading(false);
        }
    };

    const formatCurrency = (value: number | null, decimals: number = 2) => {
        if (value === null || value === undefined) return '-';
        return value.toLocaleString('en-US', {
            minimumFractionDigits: decimals,
            maximumFractionDigits: decimals,
        });
    };

    const formatNumber = (value: number | null, decimals: number = 2) => {
        if (value === null || value === undefined) return '-';
        return value.toLocaleString('en-US', {
            minimumFractionDigits: decimals,
            maximumFractionDigits: decimals,
        });
    };

    const formatPercent = (value: number | null, decimals: number = 2) => {
        if (value === null || value === undefined) return '-';
        return (value * 100).toFixed(decimals) + '%';
    };

    const formatDate = (date: string | null) => {
        if (!date) return '-';
        return new Date(date).toLocaleDateString();
    };

    if (loading) {
        return (
            <div className="open-positions-container">
                <div className="loading-container">
                    <p><em>Loading open positions...</em></p>
                </div>
            </div>
        );
    }

    if (error) {
        return (
            <div className="open-positions-container">
                <div className="error-container">
                    <h2>Error Loading Open Positions</h2>
                    <p className="error">{error}</p>
                    <button onClick={loadOpenPositions}>Retry</button>
                </div>
            </div>
        );
    }

    if (openPositions.length === 0) {
        return (
            <div className="open-positions-container">
                <div className="empty-container">
                    <h2>Open Positions</h2>
                    <p><em>No open positions found.</em></p>
                </div>
            </div>
        );
    }

    return (
        <div className="open-positions-container">
            <h1>Open Positions</h1>
            <div className="positions-table-container">
                <table className="positions-table">
                    <thead>
                        <tr>
                            <th>Symbol</th>
                            <th>Date Opened</th>
                            <th>Days Opened</th>
                            <th>Quantity</th>
                            <th>Cost Price</th>
                            <th>Average Price</th>
                            <th>Value</th>
                            <th>Unrealized P/L</th>
                            <th>% Change</th>
                            <th>Current Margin</th>
                        </tr>
                    </thead>
                    <tbody>
                        {openPositions.map((position, index) => (
                            <tr key={`${position.symbol}-${position.accountId}-${index}`}>
                                <td className="symbol-cell">{position.symbol}</td>
                                <td>{formatDate(position.dateOpened)}</td>
                                <td className="number-cell">{position.daysOpened ?? '-'}</td>
                                <td className="number-cell">{formatNumber(position.quantity, 4)}</td>
                                <td className="number-cell">{formatCurrency(position.costPrice)}</td>
                                <td className="number-cell">{formatCurrency(position.averagePrice)}</td>
                                <td className="number-cell">{formatCurrency(position.value)}</td>
                                <td className={`number-cell ${(position.unrealizedPnL ?? 0) >= 0 ? 'positive' : 'negative'}`}>
                                    {formatCurrency(position.unrealizedPnL)}
                                </td>
                                <td className={`number-cell ${(position.percentChange ?? 0) >= 0 ? 'positive' : 'negative'}`}>
                                    {formatPercent(position.percentChange)}
                                </td>
                                <td className={`number-cell ${(position.currentMargin ?? 0) >= 0 ? 'positive' : 'negative'}`}>
                                    {formatCurrency(position.currentMargin)}
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
        </div>
    );
}

export default OpenPositionList;
