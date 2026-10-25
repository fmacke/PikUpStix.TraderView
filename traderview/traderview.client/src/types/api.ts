export interface Position {
    id: number;
    positionId: number;
    instrumentId: number;
    symbol: string;
    entryDate: string;
    exitDate: string;
    entryPrice: number;
    exitPrice: number;
    quantity: number;
    pnl: number;
    buySell: string;
    executions: Execution[];
}

export interface PositionOutcome {
    id: number;
    openDate?: string | null;
    closeDate?: string | null;
    status?: string | null;
    instrumentId: number;
    // Optional symbol name provided by the server for convenience
    symbolName?: string | null;
    lastReportedPrice?: number | null;
    lastReportedPriceUpdated?: string | null;
    marketRegime?: string | null;
    maximumFavourableExcursion: number;
    maximumAdverseExcursion: number;
    // Final margin for closed positions (if available)
    margin?: number | null;
}

export interface CompoundPositions {
    quarterPosition: TradeCalculationCompoundedPosition;
    halfPosition: TradeCalculationCompoundedPosition;
    fullPosition: TradeCalculationCompoundedPosition;
}

export interface TradeCalculationCompoundedPosition {
    tradeDate: string;
    instrument: string;
    exchangeRate: number;
    totalSizeGbp: number;

    tradingCapital: number;
    riskPerPositionPercentage: number;
    stopLossOnPositionPercentage: number;

    positionSizeUsd: number;
    positionSizeGbp: number;
    positionRiskUsd: number;
    positionRiskGbp: number;
    accountRiskPercentage: number;

    buyPriceUsd: number;
    buyPriceGbp: number;
    shares: number;
    totalShares: number;
    averageSharePriceUsd: number;

    stopLossAtUsd: number;
    profitLossTargetPercentage: number;
    profitTargetUsd: number;
    takeProfitOrPyramidAtUsd: number;
    targetSharePriceUsd: number;
    targetSharePricePercentage: number;

    winUsd: number;
    lossUsd: number;
    winGbp: number;
    lossGbp: number;
    winLossRatioPercentage: number;
}

export interface TradeDetail {
    trade: Position;
    instrument: Instrument;
    executions: Execution[];
}

export interface Instrument {
    id: number;
    instrumentName: string;
    exchange: string;
    instrumentType: string;
    currency: string;
}

export interface Execution {
    id: number;
    date: string;
    price: number;
    quantity: number;
    side: string;
    commission: number;
}

export interface TradeContext {
    trade: Position;
    candlesticks: Candlestick[];
    entryDate: string;
    exitDate: string;
}

export interface Candlestick {
    date: string;
    open: number;
    high: number;
    low: number;
    close: number;
    volume: number;
}

export interface OpenPosition {
    positionId: number;
    accountId: string;
    symbol: string;
    dateOpened: string | null;
    daysOpened: number | null;
    quantity: number | null;
    costPrice: number | null;
    averagePrice: number | null;
    value: number | null;
    unrealizedPnL: number | null;
    percentChange: number | null;
    currentMargin: number | null;

    // Keep for backward compatibility
    description: string;
    assetCategory: string;
    currency: string;
    position: number | null;
    markPrice: number | null;
    positionValue: number | null;
    costBasisPrice: number | null;
    costBasisMoney: number | null;
    fifoPnlUnrealized: number | null;
    percentOfNAV: number | null;
    reportDate: string | null;
    listingExchange: string;
}

export interface Note {
    id: number;
    positionId: number;
    tradeExecutionId: number | null;
    comment: string;
    entryDate: string;
    tradeTypeId: number | null;
    errorTypeId: number | null;
    exitTypeId: number | null;
    time: number | null;
    category: string;
    name: string;
}

export interface TradeExecution {
    id: number;
    positionId: number;
    instrumentId?: number;
    symbol?: string | null;
    tradeId?: number | null;
    dateTime: string;
    tradeDate?: string | null;
    quantity?: number | null;
    tradePrice?: number | null;
    buySell?: string | null;
    fifoPnlRealized?: number | null;
    ibCommission?: number | null;
    ibCommissionCurrency?: string | null;
    closePrice?: number | null;
    cost?: number | null;
    ibExecId?: string | null;
}

export interface CreateNoteRequest {
    positionId: number | null;
    tradeExecutionId: number | null;
    comment: string;
    entryDate: string;
    tradeTypeId: number | null;
    errorTypeId: number | null;
    exitTypeId: number | null;
    time: number | null;
}

export interface ListItem {
    id: number;
    category: string;
    name: string;
    parentListId?: number | null;
    description?: string | null;
}

export interface CanSlimCandidate {
    id: number;
    canSlimScreenerSnapshotId: number;
    symbol: string;
    exchange: string;
    companyName: string;
    sector: string;
    passesBoth: boolean;
    industry: string;
    price: number;
    volume: number;
    marketCap: number;
    createdAtUtc: string;
    evaluationDateUtc: string;
}
export interface RiskMatrixCalculationResultDto {
    averageCurrencyGainOnWinningTrade: number;
    numberOfWinningTrades: number;
    averageCurrencyLossOnLosingTrade: number;
    numberOfLosingTrades: number;
    gainLossRatio: number;
    positionSize: number;
    expectedNetReturnPercent: number;
    expectedNetReturnCurrency: number;
    goalCurrency: number;
    numberOfTradesNeededToReachGoal: number;
    adjustedGainLossRatio: number;
    otpimalF: number; // Note: maintaining spelling from backend DTO
}
export interface CurrentPerformanceResult {
    gainPercentage: number;
    lossPercentage: number;
    rewardToRiskRatio: number;
    winRatePercentage: number;
    lossRatePercentage: number;
    numberOfTrades: number;
    expectedReturnPerTrade: number;
    simpleRoi: number;
    compoundedRoi: number;
}
export interface TradeCalculationRequest {
    tradeDate: string;
    instrument: string;
    exchangeRate: number;
    buyPrice: number;
    tradingCapital: number;
    riskPerTrade: number;
    maxExposure: number;
    gainLossRatio: number;
    calculationMode: string;
    stopLossAtInput: number;
    comment: string;
}
export interface TradeCalculationResponse {
    lotSizeGbp: number;
    lotGbp: number;
    lotUsd: number;
    lotPercent: number;
    shares: number;
    stopLossAt: number;
    lossGbp: number;
    lossUsd: number;
    lossPercentage: number;
    takeProfitAt: number;
    priceTarget: number;
    overallProfitGbp: number;
    overallProfitUsd: number;
    comment: string;
}

export interface AssetValueChartData {
    date: string;
    totalAssetValue: number;
    totalLongValue: number;
    totalShortValue: number;
}

export interface PositionCalculatorDto {
    id: number;
    positionId: number | null;
    orderSetupDate: string;
    symbol: string;
    currencyPair: string;
    exchangeRate: number;
    proposedPurchasePrice: number;
    tradingCapital: number;
    riskPerPosition: number;
    maxExposureOnPosition: number;
    gainLossRatioPercent: number;
    stopLossAtOverride: number | null;
    strategyId: number;
    lotSizeAccountCurrency: number;
    lotSizeStockCurrency: number;
    lotSizePercent: number;
    shareQuantity: number;
    stopLossAt: number;
    lossCurrency: number;
    lossPercent: number;
    priceTarget: number;
    takeProfitAtPercent: number;
    overallProfitAccountCurrency: number;
    overallProfitStockCurrency: number;
    comment: string
}

export interface PositionCalculatorCreateDto {
    positionId: number;
    orderSetupDate: string;
    symbol: string;
    currencyPair: string;
    exchangeRate: number;
    proposedPurchasePrice: number;
    tradingCapital: number;
    riskPerPosition: number;
    maxExposureOnPosition: number;
    gainLossRatioPercent: number;
    stopLossAtOverride?: number | null;
    strategyId: number;
    lotSizeAccountCurrency: number;
    lotSizeStockCurrency: number;
    lotSizePercent: number;
    shareQuantity: number;
    stopLossAt: number;
    lossCurrency: number;
    lossPercent: number;
    priceTarget: number;
    takeProfitAtPercent: number;
    overallProfitAccountCurrency: number;
    overallProfitStockCurrency: number;
    comment: string;
    compoundPositions?: CompoundPositions;
}