import { api } from './apiClient';

export interface BacktestRequest {
    instrumentId: number;
    startDate: string;
    endDate: string;
    initialCapital: number;
    strategyType: string;
    warmupBars?: number;
    slippagePercent?: number;
    commissionPerTrade?: number;
    fastPeriod?: number;
    slowPeriod?: number;
    rsiPeriod?: number;
    atrPeriod?: number;
}

export interface EquityPoint {
    date: string;
    equity: number;
    drawdownPercent: number;
}

export interface BacktestTrade {
    entryBarIndex: number;
    entryDate: string;
    entryPrice: number;
    direction: number;
    quantity: number;
    exitBarIndex?: number;
    exitDate?: string;
    exitPrice?: number;
    exitType?: number;
    grossProfit: number;
    commission: number;
    netProfit: number;
    returnPercent: number;
    maxFavorableExcursion: number;
    maxAdverseExcursion: number;
    barsHeld?: number;
}

export interface BacktestResult {
    instrumentName: string;
    initialCapital: number;
    finalCapital: number;
    totalReturn: number;
    totalReturnPercent: number;
    maxDrawdown: number;
    maxDrawdownPercent: number;
    totalTrades: number;
    winningTrades: number;
    losingTrades: number;
    winRate: number;
    avgWin: number;
    avgLoss: number;
    profitFactor: number;
    totalCommission: number;
    backtestStartDate: string;
    backtestEndDate: string;
    trades: BacktestTrade[];
    equityCurve: EquityPoint[];
    strategyMetadata: Record<string, string>;
}

class BacktestService {
    async runBacktest(request: BacktestRequest): Promise<BacktestResult> {
        return api.post<BacktestResult>('/backtests', request);
    }
}

const backtestService = new BacktestService();
export default backtestService;
