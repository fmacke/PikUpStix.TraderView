import axios from 'axios';
import type {
    Trade,
    TradeContext,
    RSIndicatorData,
    OpenPosition,
    Note,
    CreateNoteRequest,
    ListItem,
    CanSlimCandidate,
    CurrentPerformanceResult,
    RiskMatrixCalculationResultDto,
    TradeCalculationRequest,
    TradeCalculationResponse,
    AssetValueChartData,
    PositionCalculatorCreateDto,
    PositionCalculatorDto
} from '../types/api';

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
        } catch (error: unknown) {
            console.error('API call failed:', error);
            throw error;
        }
    },

    // Get notes for a position
    async getNotesByPositionId(positionId: number): Promise<Note[]> {
        try {
            const response = await apiClient.get<Note[]>(`/notes/position/${positionId}`);
            return response.data;
        } catch (error) {
            console.error('getNotesByPositionId failed', error);
            throw error;
        }
    },

    // Update a note
    async updateNote(noteId: number, comment: string, entryDate: string, entryMethodId: number | null, errorTypeId: number | null, exitTypeId: number | null): Promise<Note> {
        try {
            const body = { comment, entryDate, tradeTypeId: entryMethodId, errorTypeId, exitTypeId };
            const response = await apiClient.put<Note>(`/notes/${noteId}`, body);
            return response.data;
        } catch (error) {
            console.error('updateNote failed', error);
            throw error;
        }
    },

    // Get current performance summary
    async getCurrentPerformance(): Promise<CurrentPerformanceResult> {
        try {
            const response = await apiClient.get<CurrentPerformanceResult>('/currentperformance');
            return response.data;
        } catch (error) {
            console.error('getCurrentPerformance failed', error);
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
        } catch (error: unknown) {
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
        } catch (error: unknown) {
            console.error('IBKR sync API call failed:', error);
            throw error;
        }
    },

    // Sync FMP Stock Screener data - fetches shortlisted stocks FMP and updates database
    async syncFMPData(): Promise<{ message: string; timestamp: string }> {
        console.log('Making API call to /stockscreener/RunStockScreener');
        try {
            const response = await apiClient.post<{ message: string; timestamp: string }>(
                '/stockscreener/RunStockScreener',
                {},
                {
                    timeout: 300000 // 5 minute timeout for long-running sync operation
                }
            );
            console.log('StockScreener sync API response received:', response.data);
            return response.data;
        } catch (error: unknown) {
            console.error('StockScreener sync API call failed:', error);
            throw error;
        }
    },
    // Get the latest screener results
    async getLatestScreenerResults(): Promise<CanSlimCandidate[]> {
        console.log('PPMaking API call to /stockscreener/GetLatestScreenerResults');
        try {
            const response = await apiClient.get<CanSlimCandidate[]>('/stockscreener/GetCanSlimCandidates');
            console.log('EEGetLatestScreenerResults API response received:', response.data);
            return response.data;
        } catch (error: unknown) {
            console.error('JJGetLatestScreenerResults API call failed:', error);
            throw error;
        }
    },
    // Get exchange rate from server (which proxies to market data service)
    async getExchangeRate(baseCurrency: string, quoteCurrency: string): Promise<number> {
        console.log(`Making API call to /marketdata/exchange-rate?base=${baseCurrency}&quote=${quoteCurrency}`);
        try {
            // match server query parameter names: baseCurrency and quote
            const response = await apiClient.get<number>('/marketdata/exchange-rate', { params: { baseCurrency: baseCurrency, quote: quoteCurrency } });
            console.log('GetExchangeRate API response received:', response.data);
            return response.data;
        } catch (error: unknown) {
            console.error('GetExchangeRate API call failed:', error);
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
        } catch (error: unknown) {
            console.error('Open positions API call failed:', error);
            throw error;
        }
    },

    // Create a new note
    async createNote(request: CreateNoteRequest): Promise<Note> {
        try {
            const response = await apiClient.post<Note>('/notes', request);
            return response.data;
        } catch (error) {
            console.error('createNote failed', error);
            throw error;
        }
    },

    // Get list items by category
    async getListItems(category: string): Promise<ListItem[]> {
        try {
            const response = await apiClient.get<ListItem[]>(`/list/${category}`);
            return response.data;
        } catch (error) {
            console.error('getListItems failed', error);
            throw error;
        }
    },

    // Get desired performance calculation from RiskController with input parameters
    async getDesiredPerformance(
        portfolioSize: number,
        positionSizePercent: number,
        desiredReturnPercent: number
    ): Promise<RiskMatrixCalculationResultDto> {
        console.log('Making API call to /risk/desiredperformance', { portfolioSize, positionSizePercent, desiredReturnPercent });
        try {
            const response = await apiClient.get<RiskMatrixCalculationResultDto>('/risk/desiredperformance', {
                params: { portfolioSize, positionSizePercent, desiredReturnPercent }
            });
            console.log('Desired performance API response received:', response.data);
            return response.data;
        } catch (error: unknown) {
            console.error('Desired performance API call failed:', error);
            throw error;
        }
    },
    // Calculate trade position based on calculator input
    async calculateTradePosition(request: TradeCalculationRequest): Promise<TradeCalculationResponse> {
        console.log('Making API call to /tradecalculator/calculate', request);
        try {
            const response = await apiClient.post<TradeCalculationResponse>('/tradecalculator/calculate', request);
            console.log('Trade calculator API response received:', response.data);
            return response.data;
        } catch (error: unknown) {
            console.error('Trade calculator API call failed:', error);
            throw error;
        }
    },

    // Save a PositionCalculator record
    async savePositionCalculator(dto: PositionCalculatorCreateDto): Promise<PositionCalculatorDto> {
        console.log('Making API call to /PositionCalculator', dto);
        try {
            const response = await apiClient.post<PositionCalculatorDto>('/PositionCalculator', dto);
            console.log('PositionCalculator saved:', response.data);
            return response.data;
        } catch (error: unknown) {
            console.error('savePositionCalculator API call failed:', error);
            throw error;
        }
    },

    // Get asset value over time for date range
    async getAssetValueOverTime(startDate: Date, endDate: Date): Promise<AssetValueChartData[]> {
        console.log('Making API call to /equities/asset-value-over-time');
        try {
            const response = await apiClient.get<AssetValueChartData[]>('/equities/asset-value-over-time', {
                params: {
                    startDate: startDate.toISOString().split('T')[0],
                    endDate: endDate.toISOString().split('T')[0]
                }
            });
            console.log('Asset value over time API response received:', response.data);
            return response.data;
        } catch (error: unknown) {
            console.error('Asset value over time API call failed:', error);
            throw error;
        }
    },

    // Get asset value over time for specific account
    async getAssetValueOverTimeByAccount(accountId: string, startDate: Date, endDate: Date): Promise<AssetValueChartData[]> {
        console.log(`Making API call to /equities/asset-value-over-time/${accountId}`);
        try {
            const response = await apiClient.get<AssetValueChartData[]>(`/equities/asset-value-over-time/${accountId}`, {
                params: {
                    startDate: startDate.toISOString().split('T')[0],
                    endDate: endDate.toISOString().split('T')[0]
                }
            });
            console.log('Asset value over time by account API response received:', response.data);
            return response.data;
        } catch (error) {
            console.error('Asset value over time by account API call failed:', error);
            throw error;
        }
    },

    // Get latest asset value
    async getLatestAssetValue(): Promise<AssetValueChartData> {
        console.log('Making API call to /equities/latest');
        try {
            const response = await apiClient.get<AssetValueChartData>('/equities/latest');
            console.log('Latest asset value API response received:', response.data);
            return response.data;
        } catch (error) {
            console.error('Latest asset value API call failed:', error);
            throw error;
        }
    },
};