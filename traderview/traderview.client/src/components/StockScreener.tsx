import { useEffect, useState, useMemo } from 'react';
import { apiService } from '../services/apiService';
import type { CanSlimCandidate } from '../types/api';
import { SortableTableHeader } from './SortableTableHeader';
import type { SortConfig } from './SortableTableHeader';
import SyncButton from './SyncButton';
import './OpenPositionList.css';

function StockScreener() {
    const [candidates, setCandidates] = useState<CanSlimCandidate[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);

    const [sortConfig, setSortConfig] = useState<SortConfig<CanSlimCandidate>>({
        key: 'symbol',
        direction: 'asc'
    });

    const loadCandidates = async () => {
        try {
            setLoading(true);
            setError(null);
            const data = await apiService.getCanSlimCandidates();
            setCandidates(data);
        } catch (err) {
            const errorMessage = err instanceof Error ? err.message : 'Failed to load CAN SLIM candidates';
            setError(errorMessage);
            console.error('Error loading CAN SLIM candidates:', err);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadCandidates();
    }, []);

    const handleDownloadWatchList = async (): Promise<{ message: string; timestamp: string }> => {
        // Map candidates to TradingView format (e.g., "NASDAQ:WDC" or fallback to "WDC")
        const formattedSymbols = candidates
            .filter((c) => Boolean(c.symbol))
            .map((c) => (c.exchange ? `${c.exchange}:${c.symbol}` : c.symbol))
            .join(', ');

        const blob = new Blob([formattedSymbols], { type: 'text/plain;charset=utf-8' });
        const url = window.URL.createObjectURL(blob);

        const link = document.createElement('a');
        link.href = url;
        const dateStr = new Date().toISOString().split('T')[0];
        link.setAttribute('download', `tradingview-watchlist-${dateStr}.txt`);

        document.body.appendChild(link);
        link.click();
        link.parentNode?.removeChild(link);
        window.URL.revokeObjectURL(url);

        return {
            message: 'TradingView watch list downloaded successfully',
            timestamp: new Date().toISOString()
        };
    };

    const handleSort = (key: keyof CanSlimCandidate) => {
        setSortConfig(prev => ({
            key,
            direction: prev.key === key && prev.direction === 'asc' ? 'desc' : 'asc'
        }));
    };

    const sortedCandidates = useMemo(() => {
        if (!candidates || candidates.length === 0) return [];

        return [...candidates].sort((a, b) => {
            const aVal = a[sortConfig.key];
            const bVal = b[sortConfig.key];

            if (aVal == null && bVal == null) return 0;
            if (aVal == null) return 1;
            if (bVal == null) return -1;

            if (typeof aVal === 'boolean' && typeof bVal === 'boolean') {
                return sortConfig.direction === 'asc'
                    ? (aVal === bVal ? 0 : aVal ? -1 : 1)
                    : (aVal === bVal ? 0 : aVal ? 1 : -1);
            }

            if (typeof aVal === 'number' && typeof bVal === 'number') {
                return sortConfig.direction === 'asc' ? aVal - bVal : bVal - aVal;
            }

            const strA = String(aVal).toLowerCase();
            const strB = String(bVal).toLowerCase();
            return sortConfig.direction === 'asc'
                ? strA.localeCompare(strB)
                : strB.localeCompare(strA);
        });
    }, [candidates, sortConfig]);

    const formatCurrency = (value: number | null, decimals: number = 2) => {
        if (value === null || value === undefined) return '-';
        return value.toLocaleString('en-US', {
            style: 'currency',
            currency: 'USD',
            minimumFractionDigits: decimals,
            maximumFractionDigits: decimals,
        });
    };

    const formatCompactNumber = (value: number | null) => {
        if (value === null || value === undefined) return '-';
        return new Intl.NumberFormat('en-US', {
            notation: 'compact',
            maximumFractionDigits: 2
        }).format(value);
    };

    if (loading) {
        return (
            <div className="open-positions-container">
                <div className="loading-container">
                    <p><em>Loading CAN SLIM candidates...</em></p>
                </div>
            </div>
        );
    }

    if (error) {
        return (
            <div className="open-positions-container">
                <div className="error-container">
                    <h2>Error Loading Screener</h2>
                    <p className="error">{error}</p>
                    <button onClick={loadCandidates}>Retry</button>
                </div>
            </div>
        );
    }

    if (candidates.length === 0) {
        return (
            <div className="open-positions-container">
                <div className="empty-container">
                    <h2>CAN SLIM Screener</h2>
                    <p><em>No CAN SLIM candidates found.</em></p>
                </div>
            </div>
        );
    }

    return (
        <div className="open-positions-container">
            <h1>CAN SLIM Candidates</h1>
            <div className="positions-table-container">
                <SyncButton
                    label="Download Watch List"
                    syncingLabel="Downloading Watch List..."
                    title="Download watch list for TradingView"
                    onSync={handleDownloadWatchList}
                    onSuccess={() => { }}
                />
                <table className="positions-table">
                    <thead>
                        <tr>
                            <SortableTableHeader columnKey="symbol" title="Symbol" sortConfig={sortConfig} onSort={handleSort} />
                            <SortableTableHeader columnKey="companyName" title="Company" sortConfig={sortConfig} onSort={handleSort} />
                            <SortableTableHeader columnKey="exchange" title="Exchange" sortConfig={sortConfig} onSort={handleSort} />
                            <SortableTableHeader columnKey="sector" title="Sector" sortConfig={sortConfig} onSort={handleSort} />
                            <SortableTableHeader columnKey="industry" title="Industry" sortConfig={sortConfig} onSort={handleSort} />
                            <SortableTableHeader columnKey="passesBoth" title="Passes Both" sortConfig={sortConfig} onSort={handleSort} />
                            <SortableTableHeader columnKey="price" title="Price" sortConfig={sortConfig} onSort={handleSort} />
                            <SortableTableHeader columnKey="volume" title="Volume" sortConfig={sortConfig} onSort={handleSort} />
                            <SortableTableHeader columnKey="marketCap" title="Market Cap" sortConfig={sortConfig} onSort={handleSort} />
                        </tr>
                    </thead>
                    <tbody>
                        {sortedCandidates.map((candidate, index) => (
                            <tr key={candidate.id || `${candidate.symbol}-${index}`}>
                                <td className="symbol-cell">{candidate.symbol}</td>
                                <td>{candidate.companyName}</td>
                                <td>{candidate.exchange}</td>
                                <td>{candidate.sector}</td>
                                <td>{candidate.industry}</td>
                                <td>
                                    <span className={`status-pill ${candidate.passesBoth ? 'positive' : 'negative'}`}>
                                        {candidate.passesBoth ? 'Yes' : 'No'}
                                    </span>
                                </td>
                                <td className="number-cell">{formatCurrency(candidate.price)}</td>
                                <td className="number-cell">{formatCompactNumber(candidate.volume)}</td>
                                <td className="number-cell">{formatCompactNumber(candidate.marketCap)}</td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
        </div>
    );
}

export default StockScreener;