/**
 * Utility functions for formatting company report data
 */

export type RatingColor = 'green' | 'red' | 'yellow' | 'neutral';

/**
 * Determines the rating color based on a value and rating type
 */
export const getRating = (
    value: string | number | undefined | null,
    type: string
): RatingColor => {
    if (value === undefined || value === null) return 'neutral';

    // Numeric ratings (1-99 scale, higher is better)
    if (type === 'numeric-high') {
        const numVal = Number(value);
        if (numVal >= 70) return 'green';
        if (numVal >= 50) return 'yellow';
        return 'red';
    }

    // Reverse numeric (lower is better)
    if (type === 'numeric-low') {
        const numVal = Number(value);
        if (numVal <= 20) return 'green';
        if (numVal <= 50) return 'yellow';
        return 'red';
    }

    // Percentage-based (higher is better)
    if (type === 'percentage-high') {
        const numVal = Number(value);
        if (numVal >= 30) return 'green';
        if (numVal >= 10) return 'yellow';
        return 'red';
    }

    // Letter grades (A is best)
    if (type === 'grade') {
        const str = String(value).toUpperCase();
        if (str === 'A' || str === 'A+') return 'green';
        if (str === 'B' || str === 'B+') return 'yellow';
        return 'red';
    }

    return 'neutral';
};

/**
 * Formats a value for display (handles percentages, decimals, large numbers)
 */
export const formatValue = (value: string | number | undefined | null): string => {
    if (value === undefined || value === null) return 'N/A';
    if (typeof value === 'number') {
        // For percentages and decimals
        if (value < 1 && value !== 0) {
            return `${(value * 100).toFixed(2)}%`;
        }
        // For large numbers like volume
        if (value > 100000) {
            return `${(value / 1000000).toFixed(2)}M`;
        }
        // For decimals
        if (value % 1 !== 0) {
            return value.toFixed(2);
        }
        return value.toString();
    }
    return String(value);
};

/**
 * Formats a date string to MM/DD/YYYY format
 */
export const formatDate = (dateString: string | undefined | null): string => {
    if (!dateString) return 'N/A';
    try {
        const date = new Date(dateString);
        return date.toLocaleDateString('en-US', { year: 'numeric', month: '2-digit', day: '2-digit' });
    } catch {
        return dateString;
    }
};
