import axios from 'axios';
import { Trade, TradeContext, TradeDetail } from '../types/api';

// In development, use relative path so the proxy in setupProxy.js handles the request
// In production, use environment variable or default to /api
const API_BASE_URL = process.env.REACT_APP_API_URL || '/api';

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

export const apiService = {
  // Get all trades
  async getTrades(): Promise<Trade[]> {
    const response = await apiClient.get<Trade[]>('/trades');
    return response.data;
  },

  // Get trade details by ID
  async getTradeDetail(tradeId: number): Promise<TradeDetail> {
    const response = await apiClient.get<TradeDetail>(`/trades/${tradeId}`);
    return response.data;
  },

  // Get trade context with candlestick data
  async getTradeContext(tradeId: number, daysBefore: number = 30, daysAfter: number = 30): Promise<TradeContext> {
    const response = await apiClient.get<TradeContext>(
      `/trades/${tradeId}/context?daysBefore=${daysBefore}&daysAfter=${daysAfter}`
    );
    return response.data;
  },
};
