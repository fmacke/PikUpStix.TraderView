import './PaginationControl.css';

export interface PaginationConfig {
    currentPage: number;
    itemsPerPage: number;
    totalItems: number;
}

interface PaginationControlProps {
    config: PaginationConfig;
    onPageChange: (page: number) => void;
    onItemsPerPageChange: (itemsPerPage: number) => void;
    className?: string;
}

const PAGE_SIZE_OPTIONS = [20, 50, 100, 500];

export function PaginationControl({
    config,
    onPageChange,
    onItemsPerPageChange,
    className = ''
}: PaginationControlProps) {
    const { currentPage, itemsPerPage, totalItems } = config;

    // Calculate total pages
    const totalPages = itemsPerPage === 0 ? 1 : Math.ceil(totalItems / itemsPerPage);

    // Ensure current page doesn't exceed total pages
    const validCurrentPage = Math.max(1, Math.min(currentPage, totalPages));
    if (validCurrentPage !== currentPage && totalItems > 0) {
        onPageChange(validCurrentPage);
    }

    const handlePrevious = () => {
        if (validCurrentPage > 1) {
            onPageChange(validCurrentPage - 1);
        }
    };

    const handleNext = () => {
        if (validCurrentPage < totalPages) {
            onPageChange(validCurrentPage + 1);
        }
    };

    const handleItemsPerPageChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
        const value = e.target.value;
        if (value === 'all') {
            onItemsPerPageChange(totalItems);
        } else {
            onItemsPerPageChange(parseInt(value, 10));
        }
    };

    // Calculate displayed items range
    const startItem = totalItems === 0 ? 0 : (validCurrentPage - 1) * itemsPerPage + 1;
    const endItem = Math.min(validCurrentPage * itemsPerPage, totalItems);

    return (
        <div className={`pagination-control ${className}`}>
            <div className="pagination-left">
                <label htmlFor="items-per-page" className="items-per-page-label">
                    Items per page:
                </label>
                <select
                    id="items-per-page"
                    className="items-per-page-select"
                    value={itemsPerPage === totalItems && totalItems > 0 ? 'all' : itemsPerPage}
                    onChange={handleItemsPerPageChange}
                >
                    {PAGE_SIZE_OPTIONS.map(size => (
                        <option key={size} value={size}>
                            {size}
                        </option>
                    ))}
                    <option value="all">All</option>
                </select>
            </div>

            <div className="pagination-center">
                <span className="pagination-info">
                    Showing {startItem} to {endItem} of {totalItems} items
                </span>
            </div>

            <div className="pagination-right">
                <button
                    className="pagination-button"
                    onClick={handlePrevious}
                    disabled={validCurrentPage === 1}
                    type="button"
                    title="Previous page"
                >
                    ← Previous
                </button>

                <div className="pagination-pages">
                    <span className="current-page">{validCurrentPage}</span>
                    <span className="page-separator">/</span>
                    <span className="total-pages">{totalPages}</span>
                </div>

                <button
                    className="pagination-button"
                    onClick={handleNext}
                    disabled={validCurrentPage === totalPages}
                    type="button"
                    title="Next page"
                >
                    Next →
                </button>
            </div>
        </div>
    );
}
