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

    const formatCurrency = (value: number | null, currency: string = 'USD') => {
        if (value === null || value === undefined) return '-';
        return new Intl.NumberFormat('en-US', {
            style: 'currency',
            currency: currency,
            minimumFractionDigits: 2,
            maximumFractionDigits: 2,
        }).format(value);
    };

    const formatNumber = (value: number | null, decimals: number = 2) => {
        if (value === null || value === undefined) return '-';
        return value.toFixed(decimals);
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
                            <th>Description</th>
                            <th>Category</th>
                            <th>Position</th>
                            <th>Mark Price</th>
                            <th>Position Value</th>
                            <th>Cost Basis</th>
                            <th>Unrealized P&L</th>
                            <th>% of NAV</th>
                            <th>Report Date</th>
                        </tr>
                    </thead>
                    <tbody>
                        {openPositions.map((position, index) => (
                            <tr key={`${position.symbol}-${position.accountId}-${index}`}>
                                <td className="symbol-cell">{position.symbol}</td>
                                <td className="description-cell">{position.description}</td>
                                <td>{position.assetCategory}</td>
                                <td className="number-cell">{formatNumber(position.position, 4)}</td>
                                <td className="number-cell">{formatCurrency(position.markPrice, position.currency)}</td>
                                <td className="number-cell">{formatCurrency(position.positionValue, position.currency)}</td>
                                <td className="number-cell">{formatCurrency(position.costBasisMoney, position.currency)}</td>
                                <td className={`number-cell ${(position.fifoPnlUnrealized ?? 0) >= 0 ? 'positive' : 'negative'}`}>
                                    {formatCurrency(position.fifoPnlUnrealized, position.currency)}
                                </td>
                                <td className="number-cell">{formatNumber(position.percentOfNAV, 2)}%</td>
                                <td>{formatDate(position.reportDate)}</td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
        </div>
    );
}

export default OpenPositionList;
