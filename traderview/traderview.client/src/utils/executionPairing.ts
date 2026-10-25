import type { Execution } from '../types/api';

export interface ExecutionPair {
    entry: Execution;
    exit: Execution;
}

/**
 * Pairs executions based on trade direction.
 * For BUY trades: BUY executions are entries, SELL executions are exits
 * For SELL trades: SELL executions are entries, BUY executions are exits
 * Executions should already be sorted by date (FIFO)
 * 
 * @param executions - Array of executions sorted by date
 * @param tradeDirection - Direction of the trade ('BUY' or 'SELL')
 * @returns Array of execution pairs
 */
export function pairExecutions(executions: Execution[], tradeDirection: string): ExecutionPair[] {
    if (!executions || executions.length === 0) {
        return [];
    }

    // Determine which side is entry and which is exit based on trade direction
    const entrySide = tradeDirection;
    const exitSide = tradeDirection === 'BUY' ? 'SELL' : 'BUY';

    // Separate executions into entries and exits
    const entries = executions.filter(e => e.side === entrySide);
    const exits = executions.filter(e => e.side === exitSide);

    // Pair entries with exits on FIFO basis
    const pairs: ExecutionPair[] = [];

    // For each entry, pair it with an exit
    // This handles partial fills: multiple BUYs can be paired with first SELL, etc.
    let exitIndex = 0;
    let remainingExitQuantity = exits[exitIndex]?.quantity || 0;

    for (const entry of entries) {
        let remainingEntryQuantity = entry.quantity;

        while (remainingEntryQuantity > 0 && exitIndex < exits.length) {
            if (remainingExitQuantity <= 0) {
                exitIndex++;
                if (exitIndex >= exits.length) break;
                remainingExitQuantity = exits[exitIndex].quantity;
            }

            // Pair entry with current exit
            const exitQuantityUsed = Math.min(remainingEntryQuantity, remainingExitQuantity);

            pairs.push({
                entry: entry,
                exit: exits[exitIndex],
            });

            remainingEntryQuantity -= exitQuantityUsed;
            remainingExitQuantity -= exitQuantityUsed;
        }
    }

    return pairs;
}
