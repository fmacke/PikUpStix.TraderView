import axios from 'axios';
import type { Trade, TradeContext, RSIndicatorData } from '../types/api';

// API base URL - will use the proxy configured in vite.config.ts in development
const API_BASE_URL = import.meta.env.VITE_API_URL || '/api';

const apiClient = axios.create({
    baseURL: API_BASE_URL,
    headers: {
        'Content-Type': 'application/json',
    },
});

export const apiService = {
    // Get all trades from the new controller endpoint
    async getTrades(): Promise<Trade[]> {
        const response = await apiClient.get<Trade[]>('/tradeviewer/trades');
        return response.data;
    },

    // Get candlestick data for a specific trade
    // Uses calendar days to fetch data - ~150 days typically provides ~100 trading days
    async getTradeCandlesticks(tradeId: number, daysBefore: number = 150, daysAfter: number = 150): Promise<TradeContext> {
        console.log(`Making API call to /tradeviewer/trades/${tradeId}/candlesticks`);
        try {
            const response = await apiClient.get<TradeContext>(
                `/tradeviewer/trades/${tradeId}/candlesticks`,
                {
                    params: { daysBefore, daysAfter },
                    timeout: 30000 // 30 second timeout
                }
            );
            console.log('API response received:', response.data);
            return response.data;
        } catch (error) {
            console.error('API call failed:', error);
            throw error;
        }
    },

    // Get RS indicator data for a specific trade
    async getRSIndicator(
        tradeId: number, 
        benchmarkSymbol: string = 'SPX', 
        daysBefore: number = 150, 
        daysAfter: number = 150
    ): Promise<RSIndicatorData> {
        console.log(`Making API call to /tradeviewer/trades/${tradeId}/rs-indicator`);
        try {
            const response = await apiClient.get<RSIndicatorData>(
                `/tradeviewer/trades/${tradeId}/rs-indicator`,
                {
                    params: { benchmarkSymbol, daysBefore, daysAfter },
                    timeout: 30000 // 30 second timeout
                }
            );
            console.log('RS indicator API response received:', response.data);
            return response.data;
        } catch (error) {
            console.error('RS indicator API call failed:', error);
            throw error;
        }
    },

    // Sync IBKR data - fetches reports from Interactive Brokers and updates database
    async syncIBKRData(): Promise<{ message: string; timestamp: string }> {
        console.log('Making API call to /tradeviewer/sync');
        try {
            const response = await apiClient.post<{ message: string; timestamp: string }>(
                '/tradeviewer/sync',
                {},
                {
                    timeout: 300000 // 5 minute timeout for long-running sync operation
                }
            );
            console.log('IBKR sync API response received:', response.data);
            return response.data;
        } catch (error) {
            console.error('IBKR sync API call failed:', error);
            throw error;
        }
    },
};

