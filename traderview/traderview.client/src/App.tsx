import { useState } from 'react';
import './App.css';
import { syncService } from './services';
import TradeDetailView from './components/features/views/TradeDetailView';
import OpenPositionsView from './components/features/views/OpenPositionsView';
import RiskCalculatorView from './components/features/views/RiskCalculatorView';
import StockScreenerView from './components/features/views/StockScreenerView';
import JournalView from './components/features/views/JournalView';
import ReportsView from './components/features/views/ReportsView';
import SyncButton from './components/common/SyncButton';
import ThemeToggle from './components/common/ThemeToggle';
import { useTheme } from './hooks/useTheme';

type ViewMode = 'trades' | 'positions' | 'riskcalculator' | 'stockscreener' | 'journal' | 'reports';

function App() {    
    const [viewMode, setViewMode] = useState<ViewMode>('trades');    
    const { theme } = useTheme();

    return (
        <div className="app-container">
            <div className="nav-header">
                <div className="logo-container" style={{ display: 'flex', alignItems: 'center' }}>
                    <img src={theme === 'dark' ? '/logo-dark.png' : '/logo.png'} alt="PikUpStix Logo" style={{ height: '200px', marginRight: '20px' }} />
                </div>
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
                    <button
                        onClick={() => setViewMode('riskcalculator')}
                        className={`nav-button ${viewMode === 'riskcalculator' ? 'active' : ''}`}
                    >
                        Risk Calculator
                    </button>
                    <button
                        onClick={() => setViewMode('stockscreener')}
                        className={`nav-button ${viewMode === 'stockscreener' ? 'active' : ''}`}
                    >
                        Screener
                    </button>
                    <button
                        onClick={() => setViewMode('journal')}
                        className={`nav-button ${viewMode === 'journal' ? 'active' : ''}`}
                    >
                        Journal
                    </button>
                    <button
                        onClick={() => setViewMode('reports')}
                        className={`nav-button ${viewMode === 'reports' ? 'active' : ''}`}
                    >
                        Reports
                    </button>
                </div>

                <div className="nav-actions" style={{ display: 'flex', gap: '10px', alignItems: 'center' }}>
                    {/* Theme Toggle Button */}
                    <ThemeToggle />

                    {/* Primary IBKR Sync Button */}
                    <SyncButton
                        label="Sync IBKR Data"
                        syncingLabel="Syncing IBKR..."
                        title="Sync data from Interactive Brokers"
                        onSync={() => syncService.syncIBKRData()}
                        onSuccess={() => window.location.reload()}
                    />

                    {/* Secondary Custom Sync Button */}
                    <SyncButton
                        label="Sync Screener Data"
                        syncingLabel="Syncing Screener..."
                        title="Sync screener data from FMP"
                        onSync={() => syncService.syncFMPData()}
                        onSuccess={() => window.location.reload()}
                    />
                </div>
            </div>

            {viewMode === 'trades' && <TradeDetailView />}
            {viewMode === 'positions' && <OpenPositionsView />}
            {viewMode === 'riskcalculator' && <RiskCalculatorView />}
            {viewMode === 'stockscreener' && <StockScreenerView />}
            {viewMode === 'journal' && <JournalView />}
            {viewMode === 'reports' && <ReportsView />}
        </div>
        
    );

    
}

export default App;