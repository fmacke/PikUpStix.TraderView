import { useEffect, useMemo, useState } from 'react';
import { positionsService } from '../../services';
import type { PositionOutcome } from '../../types/api';
import { SortableTableHeader } from '../common/SortableTableHeader';
import { PaginationControl } from '../common/PaginationControl';
import type { SortConfig } from '../common/SortableTableHeader';


const PAGE_SIZE = 50;

export default function PositionReportCard() {
    const [items, setItems] = useState<PositionOutcome[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [page, setPage] = useState(1);
    const [sortConfig, setSortConfig] = useState<SortConfig<PositionOutcome>>({ key: 'id' as keyof PositionOutcome, direction: 'asc' });

    useEffect(() => {
        const load = async () => {
            try {
                setLoading(true);
                const data = await positionsService.getPositionOutcomes();
                setItems(data);
            } catch (err) {
                console.error('Failed to load position outcomes', err);
                setError(err instanceof Error ? err.message : 'Failed to load reports');
            } finally {
                setLoading(false);
            }
        };

        void load();
    }, []);

    const sorted = useMemo(() => {
        if (!items) return [] as PositionOutcome[];
        return [...items].sort((a, b) => {
            const key = sortConfig.key as keyof PositionOutcome;
            const aVal = a[key];
            const bVal = b[key];
            if (aVal == null && bVal == null) return 0;
            if (aVal == null) return 1;
            if (bVal == null) return -1;

            if (typeof aVal === 'number' && typeof bVal === 'number') {
                return sortConfig.direction === 'asc' ? aVal - bVal : bVal - aVal;
            }

            const sa = String(aVal).toLowerCase();
            const sb = String(bVal).toLowerCase();
            return sortConfig.direction === 'asc' ? sa.localeCompare(sb) : sb.localeCompare(sa);
        });
    }, [items, sortConfig]);

    const [itemsPerPage, setItemsPerPage] = useState(PAGE_SIZE);
    // pageCount intentionally not used directly; PaginationControl derives pages from config
    const paged = sorted.slice((page - 1) * itemsPerPage, page * itemsPerPage);

    const handleSort = (key: keyof PositionOutcome) => {
        setSortConfig(prev => ({ key, direction: prev.key === key && prev.direction === 'asc' ? 'desc' : 'asc' }));
    };

    if (loading) return <div className="position-report-card"><em>Loading reports...</em></div>;
    if (error) return <div className="position-report-card error">Error: {error}</div>;

    return (
        <div className="position-report-card">
            <h2>Position Outcomes (MFE / MAE)</h2>
            <table className="positions-report-table">
                <thead>
                    <tr>
                        <SortableTableHeader columnKey="id" title="Id" sortConfig={sortConfig} onSort={handleSort} />
                        <SortableTableHeader columnKey="instrumentId" title="Instrument" sortConfig={sortConfig} onSort={handleSort} />
                        <SortableTableHeader columnKey="symbolName" title="Symbol" sortConfig={sortConfig} onSort={handleSort} />
                        <SortableTableHeader columnKey="openDate" title="Open Date" sortConfig={sortConfig} onSort={handleSort} />
                        <SortableTableHeader columnKey="closeDate" title="Close Date" sortConfig={sortConfig} onSort={handleSort} />
                        <SortableTableHeader columnKey="marketRegime" title="Regime" sortConfig={sortConfig} onSort={handleSort} />
                        <SortableTableHeader columnKey="maximumFavourableExcursion" title="MFE" sortConfig={sortConfig} onSort={handleSort} />
                        <SortableTableHeader columnKey="maximumAdverseExcursion" title="MAE" sortConfig={sortConfig} onSort={handleSort} />
                        <SortableTableHeader columnKey="margin" title="Margin" sortConfig={sortConfig} onSort={handleSort} />
                    </tr>
                </thead>
                <tbody>
                    {paged.map(item => (
                        <tr
                            key={item.id}
                            onClick={() => { window.location.href = `/TraderViewDetails?positionId=${item.id}`; }}
                            onKeyDown={(e) => { if (e.key === 'Enter') window.location.href = `/TraderViewDetails?positionId=${item.id}`; }}
                            role="link"
                            tabIndex={0}
                            style={{ cursor: 'pointer' }}
                        >
                            <td>{item.id}</td>
                            <td>{item.instrumentId}</td>
                            <td>{item.symbolName ?? '-'}</td>
                            <td>{item.openDate ? new Date(item.openDate).toLocaleDateString() : '-'}</td>
                            <td>{item.closeDate ? new Date(item.closeDate).toLocaleDateString() : '-'}</td>
                            <td>{item.marketRegime ?? '-'}</td>
                            <td className="number-cell">{item.maximumFavourableExcursion.toFixed(2)}</td>
                            <td className="number-cell">{item.maximumAdverseExcursion.toFixed(2)}</td>
                            <td className="number-cell">{typeof item.margin === 'number' ? item.margin.toFixed(2) : '-'}</td>
                        </tr>
                    ))}
                </tbody>
            </table>

            <div style={{ marginTop: 12 }}>
                <PaginationControl
                    config={{ currentPage: page, itemsPerPage: itemsPerPage, totalItems: sorted.length }}
                    onPageChange={(p) => setPage(p)}
                    onItemsPerPageChange={(n) => { setItemsPerPage(n); setPage(1); }}
                />
            </div>
        </div>
    );
}
