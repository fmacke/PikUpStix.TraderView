import api from './apiClient';
import type { CanSlimCandidate } from '../types/api';

export const syncService = {
    async syncIBKRData(): Promise<{ message: string; timestamp: string }> {
        console.log('Making API call to /tradeviewer/sync');
        return api.post<{ message: string; timestamp: string }>(
            '/tradeviewer/sync',
            {},
            { timeout: 300000 }
        );
    },

    async syncFMPData(): Promise<{ message: string; timestamp: string }> {
        console.log('Making API call to /stockscreener/RunStockScreener');
        return api.post<{ message: string; timestamp: string }>('/stockscreener/RunStockScreener', {}, { timeout: 300000 });
    },

    async getLatestScreenerResults(): Promise<CanSlimCandidate[]> {
        console.log('Making API call to /stockscreener/GetCanSlimCandidates');
        return api.get<CanSlimCandidate[]>('/stockscreener/GetCanSlimCandidates');
    },

    async runTradeConfirmReport(): Promise<{ message: string; timestamp: string }> {
        console.log('Making API call to /openpositions/run-trade-confirm-report');
        return api.post<{ message: string; timestamp: string }>('/openpositions/run-trade-confirm-report', {}, { timeout: 300000 });
    },
};

export default syncService;
