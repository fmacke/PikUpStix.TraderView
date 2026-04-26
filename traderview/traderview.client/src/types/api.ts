export interface Trade {
    id: number;
    instrumentId: number;
    symbol: string;
    entryDate: string;
    exitDate: string;
    entryPrice: number;
    exitPrice: number;
    quantity: number;
    pnl: number;
    buySell: string;
}

export interface TradeDetail {
    trade: Trade;
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
    trade: Trade;
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

export interface RSIndicatorData {
    rsData: RSDataPoint[];
    metrics: RSMetrics;
}

export interface RSDataPoint {
    date: string;
    rsRatio: number;
    rsma: number;
    isRS50High: boolean;
    isRSNewHigh: boolean;
    isBlueDot: boolean;
}

export interface RSMetrics {
    institutionalCount: number | null;
    institutionalPercent: number | null;
    institutionalCountDelta: number | null;
    isInstitutionalGrowing: boolean;
    isRSNewHigh: boolean;
    isStage2Trend: boolean;
    distanceFrom52WeekHigh: number;
    sma50: number;
    sma150: number;
    sma200: number;
}
