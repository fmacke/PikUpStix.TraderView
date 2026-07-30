import axios from 'axios';
import type { Trade, TradeContext, RSIndicatorData, OpenPosition, Note, CreateNoteRequest } from '../types/api';

// API base URL - will use the proxy configured in vite.config.ts in development
const API_BASE_URL = import.meta.env.VITE_API_URL || '/api';

const apiClient = axios.create({
    baseURL: API_BASE_URL,
    headers: {
        'Content-Type': 'application/json',
    },
    validateStatus: (status) => {
        // Accept any status code from 200-299
        return status >= 200 && status < 300;
    },
});

// Add response interceptor for debugging
apiClient.interceptors.response.use(
    (response) => {
        console.log('API Response interceptor - Success:', {
            status: response.status,
            statusText: response.statusText,
            url: response.config.url,
            data: response.data
        });
        return response;
    },
    (error) => {
        console.error('API Response interceptor - Error:', {
            message: error.message,
            response: error.response,
            status: error.response?.status,
            data: error.response?.data
        });
        return Promise.reject(error);
    }
);

export const apiService = {
    // Get all trades from the new controller endpoint
    async getTrades(): Promise<Trade[]> {
        const response = await apiClient.get<Trade[]>('/tradeviewer/trades');
        return response.data;
    },

    // Get candlestick data for a specific trade
    // Uses calendar days to fetch data - ~150 days typically provides ~100 trading days
    async getTradeCandlesticks(positionId: number, daysBefore: number = 150, daysAfter: number = 150): Promise<TradeContext> {
        console.log(`Making API call to /tradeviewer/trades/${positionId}/candlesticks`);
        try {
            const response = await apiClient.get<TradeContext>(
                `/tradeviewer/trades/${positionId}/candlesticks`,
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
        benchmarkSymbol: string = '^GSPC', 
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

    // Get all open positions
    async getOpenPositions(): Promise<OpenPosition[]> {
        console.log('Making API call to /openpositions');
        try {
            const response = await apiClient.get<OpenPosition[]>('/openpositions');
            console.log('Open positions API response received:', response.data);
            return response.data;
        } catch (error) {
            console.error('Open positions API call failed:', error);
            throw error;
        }
    },

    // Create a new note
    async createNote(noteRequest: CreateNoteRequest): Promise<Note> {
        console.log('Making API call to /notes', noteRequest);
        try {
            const response = await apiClient.post<Note>('/notes', noteRequest);
            console.log('Create note API response received:', response.data);
            return response.data;
        } catch (error) {
            console.error('Create note API call failed:', error);
            throw error;
        }
    },

    // Get notes for a specific position
    async getNotesByPositionId(positionId: number): Promise<Note[]> {
        console.log(`Making API call to /notes/position/${positionId}`);
        try {
            const response = await apiClient.get<Note[]>(`/notes/position/${positionId}`);
            console.log('Get notes API response received:', response.data);
            return response.data;
        } catch (error) {
            console.error('Get notes API call failed:', error);
            throw error;
        }
    },
};
