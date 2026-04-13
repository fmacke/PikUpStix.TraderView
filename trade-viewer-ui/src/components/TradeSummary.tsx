import React from 'react';
import { Trade, Instrument } from '../types/api';
import './TradeSummary.css';

interface TradeSummaryProps {
  trade: Trade;
  instrument?: Instrument;
}

const TradeSummary: React.FC<TradeSummaryProps> = ({ trade, instrument }) => {
  const isProfitable = trade.pnl > 0;
  const formatCurrency = (value: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: instrument?.currency || 'USD',
    }).format(value);
  };

  const formatDate = (dateStr: string) => {
    return new Date(dateStr).toLocaleString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  return (
    <div className="trade-summary">
      <div className="summary-header">
        <h2>{trade.symbol}</h2>
        <div className={`pnl ${isProfitable ? 'profit' : 'loss'}`}>
          {isProfitable ? '+' : ''}{formatCurrency(trade.pnl)}
        </div>
      </div>

      <div className="summary-grid">
        <div className="summary-item">
          <span className="label">Trade ID:</span>
          <span className="value">{trade.id}</span>
        </div>

        <div className="summary-item">
          <span className="label">Direction:</span>
          <span className={`value ${trade.buySell?.toLowerCase() === 'buy' ? 'buy' : 'sell'}`}>
            {trade.buySell}
          </span>
        </div>

        <div className="summary-item">
          <span className="label">Quantity:</span>
          <span className="value">{trade.quantity}</span>
        </div>

        <div className="summary-item">
          <span className="label">Entry Date:</span>
          <span className="value">{formatDate(trade.entryDate)}</span>
        </div>

        <div className="summary-item">
          <span className="label">Entry Price:</span>
          <span className="value">{formatCurrency(trade.entryPrice)}</span>
        </div>

        <div className="summary-item">
          <span className="label">Exit Date:</span>
          <span className="value">{formatDate(trade.exitDate)}</span>
        </div>

        <div className="summary-item">
          <span className="label">Exit Price:</span>
          <span className="value">{formatCurrency(trade.exitPrice)}</span>
        </div>

        {instrument && (
          <>
            <div className="summary-item">
              <span className="label">Instrument:</span>
              <span className="value">{instrument.instrumentName}</span>
            </div>

            <div className="summary-item">
              <span className="label">Exchange:</span>
              <span className="value">{instrument.listingExchange}</span>
            </div>
          </>
        )}
      </div>
    </div>
  );
};

export default TradeSummary;
