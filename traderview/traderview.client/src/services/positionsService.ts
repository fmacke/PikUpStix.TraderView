import api from './apiClient';
import type { OpenPosition, PositionCalculatorCreateDto, PositionCalculatorDto, AssetValueChartData } from '../types/api';

export const positionsService = {
    async getPositionCalculators(positionId: number): Promise<PositionCalculatorDto[]> {
        return api.get<PositionCalculatorDto[]>(`/positions/${positionId}/calculators`);
    },

    async getPositionCalculatorCandidates(positionId: number): Promise<PositionCalculatorDto[]> {
        return api.get<PositionCalculatorDto[]>(`/positions/${positionId}/calculators/candidates`);
    },
    async getExchangeRate(baseCurrency: string, quoteCurrency: string): Promise<number> {
        return api.get<number>('/marketdata/exchange-rate', { params: { baseCurrency, quote: quoteCurrency } });
    },

    async getOpenPositions(): Promise<OpenPosition[]> {
        return api.get<OpenPosition[]>('/openpositions');
    },

    async savePositionCalculator(dto: PositionCalculatorCreateDto): Promise<PositionCalculatorDto> {
        return api.post<PositionCalculatorDto>('/PositionCalculator', dto);
    },

    async linkPositionCalculators(positionId: number, calculatorIds: number[]): Promise<void> {
        await api.post(`/positions/${positionId}/calculators/link`, { calculatorIds });
    },

    async unlinkPositionCalculators(positionId: number, calculatorIds: number[]): Promise<void> {
        await api.post(`/positions/${positionId}/calculators/unlink`, { calculatorIds });
    },

    async getAssetValueOverTime(startDate: Date, endDate: Date): Promise<AssetValueChartData[]> {
        return api.get<AssetValueChartData[]>('/equities/asset-value-over-time', {
            params: {
                startDate: startDate.toISOString().split('T')[0],
                endDate: endDate.toISOString().split('T')[0],
            },
        });
    },

    async getAssetValueOverTimeByAccount(accountId: string, startDate: Date, endDate: Date): Promise<AssetValueChartData[]> {
        return api.get<AssetValueChartData[]>(`/equities/asset-value-over-time/${accountId}`, {
            params: {
                startDate: startDate.toISOString().split('T')[0],
                endDate: endDate.toISOString().split('T')[0],
            },
        });
    },

    async getLatestAssetValue(): Promise<AssetValueChartData> {
        return api.get<AssetValueChartData>('/equities/latest');
    },

    async downloadCompoundPositionsReport(): Promise<Blob> {
        return api.getBlob('/tradecalculator/compound-report');
    },
};

export default positionsService;