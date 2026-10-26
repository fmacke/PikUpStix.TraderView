import type { OpenPosition } from '../../types/api';
import { SortableTableHeader } from '../common/SortableTableHeader';
import type { SortConfig } from '../common/SortableTableHeader';
import './OpenPositionsTableCard.css';

interface OpenPositionsTableCardProps {
    positions: OpenPosition[];
    selectedPositionId: number | null;
    sortConfig: SortConfig<OpenPosition>;
    onSort: (key: keyof OpenPosition) => void;
    onRowClick: (position: OpenPosition) => void;
    onAddNoteClick: (positionId: number) => void;
    onLinkCalculationsClick: (position: OpenPosition) => void;
    totalValue: number;
    totalCurrentMargin: number;
}

function OpenPositionsTableCard({
    positions,
    selectedPositionId,
    sortConfig,
    onSort,
    onRowClick,
    onAddNoteClick,
    onLinkCalculationsClick,
    totalValue,
    totalCurrentMargin,
}: OpenPositionsTableCardProps) {
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

    return (
        <div className="open-positions-table-card">
            <div className="positions-table-container">
                <table className="positions-table">
                    <thead>
                        <tr>
                            <SortableTableHeader columnKey="symbol" title="Symbol" sortConfig={sortConfig} onSort={onSort} />
                            <SortableTableHeader columnKey="dateOpened" title="Date Opened" sortConfig={sortConfig} onSort={onSort} />
                            <SortableTableHeader columnKey="daysOpened" title="Days Opened" sortConfig={sortConfig} onSort={onSort} />
                            <SortableTableHeader columnKey="quantity" title="Quantity" sortConfig={sortConfig} onSort={onSort} />
                            <SortableTableHeader columnKey="costPrice" title="Cost Price" sortConfig={sortConfig} onSort={onSort} />
                            <SortableTableHeader columnKey="averagePrice" title="Average Price" sortConfig={sortConfig} onSort={onSort} />
                            <SortableTableHeader columnKey="currency" title="Currency" sortConfig={sortConfig} onSort={onSort} />
                            <SortableTableHeader columnKey="value" title="Value" sortConfig={sortConfig} onSort={onSort} />
                            <th>Value GBP</th>
                            <SortableTableHeader columnKey="unrealizedPnL" title="Unrealized P/L" sortConfig={sortConfig} onSort={onSort} />
                            <SortableTableHeader columnKey="percentChange" title="% Change" sortConfig={sortConfig} onSort={onSort} />
                            <SortableTableHeader columnKey="currentMargin" title="Current Margin" sortConfig={sortConfig} onSort={onSort} />
                            <th>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {positions.map((position, index) => (
                            <tr
                                key={`${position.symbol}-${position.accountId}-${index}`}
                                className={selectedPositionId === position.positionId ? 'selected-row' : ''}
                                onClick={() => onRowClick(position)}
                                style={{ cursor: 'pointer' }}
                            >
                                <td className="symbol-cell">{position.symbol}</td>
                                <td>{formatDate(position.dateOpened)}</td>
                                <td className="number-cell">{position.daysOpened ?? '-'}</td>
                                <td className="number-cell">{formatNumber(position.quantity, 4)}</td>
                                <td className="number-cell">{formatCurrency(position.costPrice)}</td>
                                <td className="number-cell">{formatCurrency(position.averagePrice)}</td>
                                <td>{position.currency || '-'}</td>
                                <td className="number-cell">{formatCurrency(position.value)}</td>
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
                                <td>
                                    <button
                                        className="add-note-button"
                                        onClick={(e) => {
                                            e.stopPropagation();
                                            onAddNoteClick(position.positionId);
                                        }}
                                    >
                                        Add Note
                                    </button>
                                    <button
                                        className="add-note-button"
                                        onClick={(e) => {
                                            e.stopPropagation();
                                            onLinkCalculationsClick(position);
                                        }}
                                    >
                                        Link Calculations
                                    </button>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                    <tfoot>
                        <tr className="summary-row">
                            <td colSpan={7} className="summary-label" style={{ textAlign: 'right', fontWeight: 'bold' }}>
                                Total Value:
                            </td>
                            <td
                                className="number-cell summary-value"
                                style={{ fontWeight: 'bold' }}
                            >
                                {formatCurrency(totalValue)}
                            </td>
                            <td colSpan={3}></td>
                        </tr>
                        <tr className="summary-row">
                            <td colSpan={11} className="summary-label" style={{ textAlign: 'right', fontWeight: 'bold' }}>
                                Total Margin:
                            </td>
                            <td
                                className={`number-cell summary-value ${totalCurrentMargin >= 0 ? 'positive' : 'negative'}`}
                                style={{ fontWeight: 'bold' }}
                            >
                                {formatCurrency(totalCurrentMargin)}
                            </td>
                            <td></td>
                        </tr>
                    </tfoot>
                </table>
            </div>
        </div>
    );
}

export default OpenPositionsTableCard;
