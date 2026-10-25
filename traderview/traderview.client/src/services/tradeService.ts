import api from './apiClient';
import type { Position, TradeContext, TradeExecution } from '../types/api';

export const tradeService = {
    async getTrades(): Promise<Position[]> {
        return api.get<Position[]>('/tradeviewer/trades');
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
