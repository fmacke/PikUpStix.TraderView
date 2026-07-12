import { useEffect, useState } from 'react';
import './App.css';
import { apiService } from './services/apiService';
import type { Trade } from './types/api';
import TradeList from './components/TradeList';
import TradeDetail from './components/TradeDetail';
import OpenPositionList from './components/OpenPositionList';

type ViewMode = 'trades' | 'positions';

function App() {
    const [trades, setTrades] = useState<Trade[]>([]);
    const [selectedTrade, setSelectedTrade] = useState<Trade | null>(null);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);
    const [syncing, setSyncing] = useState<boolean>(false);
    const [syncMessage, setSyncMessage] = useState<string | null>(null);
    const [viewMode, setViewMode] = useState<ViewMode>('trades');

    useEffect(() => {
        populateTradeData();
    }, []);

    const handleTradeSelect = (trade: Trade) => {
        setSelectedTrade(trade);
    };

    const handleSync = async () => {
        try {
            setSyncing(true);
            setSyncMessage(null);
            setError(null);

            const result = await apiService.syncIBKRData();
            setSyncMessage(`✓ ${result.message}`);

            // Refresh trade data after successful sync
            await populateTradeData();
        } catch (err) {
            const errorMessage = err instanceof Error ? err.message : 'Failed to sync IBKR data. Please try again.';
            setError(errorMessage);
            console.error('Error syncing IBKR data:', err);
        } finally {
            setSyncing(false);
        }
    };

    if (loading) {
        return (
            <div className="app-container">
                <div className="loading-container">
                    <p><em>Loading trades... Please ensure the ASP.NET backend has started.</em></p>
                </div>
            </div>
        );
    }

    if (error) {
        return (
            <div className="app-container">
                <div className="error-container">
                    <h2>Error Loading Trades</h2>
                    <p className="error">{error}</p>
                    <button onClick={populateTradeData}>Retry</button>
                </div>
            </div>
        );
    }

    if (trades.length === 0) {
        return (
            <div className="app-container">
                <div className="empty-container">
                    <p><em>No trades found.</em></p>
                </div>
            </div>
        );
    }

    return (
        <div className="app-container">
            <div className="nav-header">
                <div className="nav-buttons">
                    <button 
                        onClick={() => setViewMode('trades')} 
                        className={`nav-button ${viewMode === 'trades' ? 'active' : ''}`}
                    >
                        Trades
                    </button>
                    <button 
                        onClick={() => setViewMode('positions')} 
                        className={`nav-button ${viewMode === 'positions' ? 'active' : ''}`}
                    >
                        Open Positions
                    </button>
                </div>
                <button 
                    onClick={handleSync} 
                    disabled={syncing}
                    className="sync-button"
                    title="Sync data from Interactive Brokers"
                >
                    {syncing ? 'Syncing...' : 'Sync IBKR Data'}
                </button>
                {syncMessage && <span className="sync-success">{syncMessage}</span>}
            </div>

            {viewMode === 'trades' ? (
                <div className="master-detail-layout">
                    <div className="detail-pane">
                        <TradeDetail trade={selectedTrade} />
                    </div>
                    <div className="list-pane">
                        <TradeList 
                            trades={trades} 
                            selectedPositionId={selectedTrade?.positionId ?? null}
                            onTradeSelect={handleTradeSelect}
                        />
                    </div>
                </div>
            ) : (
                <OpenPositionList />
            )}
        </div>
    );

    async function populateTradeData() {
        try {
            setLoading(true);
            setError(null);
            const data = await apiService.getTrades();
            setTrades(data);
            // Auto-select the most recently closed trade (sorted by exitDate descending)
            if (data.length > 0) {
                const sortedData = [...data].sort((a, b) => {
                    return new Date(b.exitDate).getTime() - new Date(a.exitDate).getTime();
                });
                setSelectedTrade(sortedData[0]);
            }
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Failed to load trades. Make sure the API is running.');
            console.error('Error fetching trades:', err);
        } finally {
            setLoading(false);
        }
    }
}

export default App;