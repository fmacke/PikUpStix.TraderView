import api from './apiClient';
import type { Trade, TradeContext, TradeExecution } from '../types/api';

export const tradeService = {
    async getTrades(): Promise<Trade[]> {
        return api.get<Trade[]>('/tradeviewer/trades');
    },

    async getTradeCandlesticks(positionId: number, daysBefore: number = 150, daysAfter: number = 150): Promise<TradeContext> {
        console.log(`Making API call to /tradeviewer/trades/${positionId}/candlesticks`);
        return api.get<TradeContext>(`/tradeviewer/trades/${positionId}/candlesticks`, {
            params: { daysBefore, daysAfter },
            timeout: 30000,
        });
    },

    async getTradeExecutionsByPositionId(positionId: number): Promise<TradeExecution[]> {
        return api.get<TradeExecution[]>(`/tradeexecutions/position/${positionId}`);
    },
};

export default tradeService;
