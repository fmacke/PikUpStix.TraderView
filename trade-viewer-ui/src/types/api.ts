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
  executions: TradeExecution[];
}

export interface Instrument {
  id: number;
  instrumentName: string;
  provider: string;
  dataName: string;
  currency: string;
  listingExchange: string;
}

export interface TradeExecution {
  id: number;
  instrumentId: number;
  symbol: string;
  tradeID: number;
  dateTime: string;
  tradeDate: string;
  quantity: number;
  tradePrice: number;
  buySell: string;
  fifoPnlRealized: number;
  ibCommission: number;
}

export interface CandlestickData {
  date: string;
  open: number;
  high: number;
  low: number;
  close: number;
  volume: number;
}

export interface TradeContext {
  trade: Trade;
  candlesticks: CandlestickData[];
  entryDate: string;
  exitDate: string;
}
