import React, { useState, useEffect, useCallback } from 'react';
import './App.css';
import TradingChart from './components/TradingChart';
import TradeSummary from './components/TradeSummary';
import { apiService } from './services/apiService';
import { Trade, TradeContext, TradeDetail } from './types/api';

function App() {
  const [trades, setTrades] = useState<Trade[]>([]);
  const [currentTradeIndex, setCurrentTradeIndex] = useState<number>(0);
  const [tradeContext, setTradeContext] = useState<TradeContext | null>(null);
  const [tradeDetail, setTradeDetail] = useState<TradeDetail | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  // Load all trades on mount
  useEffect(() => {
    const fetchTrades = async () => {
      try {
        setLoading(true);
        const tradesData = await apiService.getTrades();
        setTrades(tradesData);
        setError(null);
      } catch (err) {
        setError('Failed to load trades. Make sure the API is running.');
        console.error('Error fetching trades:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchTrades();
  }, []);

  // Load trade context when current trade changes
  useEffect(() => {
    if (trades.length === 0) return;

    const fetchTradeData = async () => {
      try {
        setLoading(true);
        const currentTrade = trades[currentTradeIndex];

        // Fetch trade context with candlestick data
        const context = await apiService.getTradeContext(currentTrade.id);
        setTradeContext(context);

        // Fetch trade details
        const detail = await apiService.getTradeDetail(currentTrade.id);
        setTradeDetail(detail);

        setError(null);
      } catch (err) {
        setError('Failed to load trade details.');
        console.error('Error fetching trade data:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchTradeData();
  }, [currentTradeIndex, trades]);

  // Handle keyboard navigation
  const handleKeyDown = useCallback((event: KeyboardEvent) => {
    if (event.key === 'ArrowRight') {
      setCurrentTradeIndex((prev) => Math.min(prev + 1, trades.length - 1));
    } else if (event.key === 'ArrowLeft') {
      setCurrentTradeIndex((prev) => Math.max(prev - 1, 0));
    }
  }, [trades.length]);

  useEffect(() => {
    window.addEventListener('keydown', handleKeyDown);
    return () => {
      window.removeEventListener('keydown', handleKeyDown);
    };
  }, [handleKeyDown]);

  const handlePreviousTrade = () => {
    setCurrentTradeIndex((prev) => Math.max(prev - 1, 0));
  };

  const handleNextTrade = () => {
    setCurrentTradeIndex((prev) => Math.min(prev + 1, trades.length - 1));
  };

  if (loading && trades.length === 0) {
    return (
      <div className="App">
        <div className="loading">Loading trades...</div>
      </div>
    );
  }

  if (error && trades.length === 0) {
    return (
      <div className="App">
        <div className="error">
          <h2>Error</h2>
          <p>{error}</p>
          <p>Make sure the TradeViewer.API is running on https://localhost:7001</p>
        </div>
      </div>
    );
  }

  if (trades.length === 0) {
    return (
      <div className="App">
        <div className="no-trades">No trades found</div>
      </div>
    );
  }

  const currentTrade = trades[currentTradeIndex];

  return (
    <div className="App">
      <header className="App-header">
        <h1>Trade Viewer</h1>
        <div className="trade-navigation">
          <button 
            onClick={handlePreviousTrade} 
            disabled={currentTradeIndex === 0}
            className="nav-button"
          >
            ← Previous
          </button>
          <span className="trade-counter">
            Trade {currentTradeIndex + 1} of {trades.length}
          </span>
          <button 
            onClick={handleNextTrade} 
            disabled={currentTradeIndex === trades.length - 1}
            className="nav-button"
          >
            Next →
          </button>
        </div>
      </header>

      <main className="App-main">
        {loading ? (
          <div className="loading">Loading trade data...</div>
        ) : (
          <>
            <TradeSummary 
              trade={currentTrade} 
              instrument={tradeDetail?.instrument}
            />

            {tradeContext && (
              <TradingChart
                candlesticks={tradeContext.candlesticks}
                entryDate={tradeContext.entryDate}
                exitDate={tradeContext.exitDate}
                entryPrice={currentTrade.entryPrice}
                exitPrice={currentTrade.exitPrice}
              />
            )}

            {error && (
              <div className="error-message">{error}</div>
            )}
          </>
        )}
      </main>

      <footer className="App-footer">
        <p>Use ← and → arrow keys to navigate between trades</p>
      </footer>
    </div>
  );
}

export default App;
