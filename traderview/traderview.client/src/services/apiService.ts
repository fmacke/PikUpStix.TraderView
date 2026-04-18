import axios from 'axios';
import type { Trade } from '../types/api';

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
};
