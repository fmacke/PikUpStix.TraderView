import api from './apiClient';
import type { CurrentPerformanceResult, RiskMatrixCalculationResultDto, TradeCalculationRequest, TradeCalculationResponse, CompoundPositions } from '../types/api';

export const riskService = {
    async getCurrentPerformance(): Promise<CurrentPerformanceResult> {
        return api.get<CurrentPerformanceResult>('/risk/currentperformance');
    },

    async getDesiredPerformance(portfolioSize: number, positionSizePercent: number, desiredReturnPercent: number): Promise<RiskMatrixCalculationResultDto> {
        return api.get<RiskMatrixCalculationResultDto>('/risk/desiredperformance', { params: { portfolioSize, positionSizePercent, desiredReturnPercent } });
    },

    async calculateTradePosition(request: TradeCalculationRequest): Promise<TradeCalculationResponse> {
        return api.post<TradeCalculationResponse>('/tradecalculator/calculate', request);
    },

    async calculateCompoundPositions(request: TradeCalculationRequest): Promise<CompoundPositions> {
        return api.post<CompoundPositions>('/tradecalculator/calculate-compound', request);
    },
};

export default riskService;
