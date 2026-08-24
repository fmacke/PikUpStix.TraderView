
import './SortableTableHeader.css';

export type SortDirection = 'asc' | 'desc';

export interface SortConfig<T> {
    key: keyof T;
    direction: SortDirection;
}

interface SortableTableHeaderProps<T> {
    columnKey: keyof T;
    title: string;
    sortConfig: SortConfig<T>;
    onSort: (key: keyof T) => void;
    className?: string;
    align?: 'left' | 'center' | 'right';
}

export function SortableTableHeader<T>({
    columnKey,
    title,
    sortConfig,
    onSort,
    className = '',
    align = 'left'
}: SortableTableHeaderProps<T>) {
    const isActive = sortConfig.key === columnKey;

    return (
        <th
            className={`sortable-th ${align} ${isActive ? 'active' : ''} ${className}`}
            onClick={() => onSort(columnKey)}
            aria-sort={isActive ? (sortConfig.direction === 'asc' ? 'ascending' : 'descending') : 'none'}
        >
            <div className="th-content">
                <span className="th-title">{title}</span>
                <span className={`sort-icon ${isActive ? sortConfig.direction : 'inactive'}`}>
                    <svg viewBox="0 0 10 14" width="8" height="11" aria-hidden="true">
                        <path
                            className="arrow-up"
                            d="M 5 0 L 10 6 L 0 6 Z"
                        />
                        <path
                            className="arrow-down"
                            d="M 0 8 L 10 8 L 5 14 Z"
                        />
                    </svg>
                </span>
            </div>
        </th>
    );
}