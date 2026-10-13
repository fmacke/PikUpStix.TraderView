import api from './apiClient';
import type { Trade, TradeContext, RSIndicatorData, TradeExecution } from '../types/api';

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

    async getRSIndicator(
        tradeId: number,
        benchmarkSymbol: string = '^GSPC',
        daysBefore: number = 150,
        daysAfter: number = 150
    ): Promise<RSIndicatorData> {
        console.log(`Making API call to /tradeviewer/trades/${tradeId}/rs-indicator`);
        return api.get<RSIndicatorData>(`/tradeviewer/trades/${tradeId}/rs-indicator`, {
            params: { benchmarkSymbol, daysBefore, daysAfter },
            timeout: 30000,
        });
    },

    async getTradeExecutionsByPositionId(positionId: number): Promise<TradeExecution[]> {
        return api.get<TradeExecution[]>(`/tradeexecutions/position/${positionId}`);
    },
};

export default tradeService;
