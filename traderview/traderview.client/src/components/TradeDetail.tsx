import { useState, useEffect } from 'react';
import type { Trade, RSIndicatorData, CreateNoteRequest } from '../types/api';
import { apiService } from '../services/apiService';
import './TradeDetail.css';
import TradingViewChart from './TradingViewChart';
import RSMetricsDashboard from './RSMetricsDashboard';
import AddNoteModal from './AddNoteModal';

interface TradeDetailProps {
    trade: Trade | null;
}

function TradeDetail({ trade }: TradeDetailProps) {
    const [rsData, setRsData] = useState<RSIndicatorData | null>(null);
    const [rsLoading, setRsLoading] = useState<boolean>(false);
    const [rsError, setRsError] = useState<string | null>(null);
    const [isNoteModalOpen, setIsNoteModalOpen] = useState<boolean>(false);

    // Fetch RS indicator data when trade changes
    useEffect(() => {
        if (!trade) {
            setRsData(null);
            return;
        }

        const fetchRSData = async () => {
            try {
                setRsLoading(true);
                setRsError(null);
                const data = await apiService.getRSIndicator(trade.id);
                setRsData(data);
            } catch (error) {
                console.error('Error fetching RS indicator data:', error);
                setRsError('RS indicator data not available. Ensure benchmark data (SPX) exists in the database.');
                setRsData(null);
            } finally {
                setRsLoading(false);
            }
        };

        fetchRSData();
    }, [trade?.id]);

    const handleAddNoteClick = () => {
        setIsNoteModalOpen(true);
    };

    const handleCloseNoteModal = () => {
        setIsNoteModalOpen(false);
    };

    const handleSubmitNote = async (comment: string) => {
        if (!trade) return;

        const noteRequest: CreateNoteRequest = {
            positionId: trade.positionId,
            tradeExecutionId: null, // Can be extended later to link to specific executions
            comment: comment,
            entryDate: new Date().toISOString(),
            tradeTypeId: 1 // Default trade type, can be made configurable
        };

        await apiService.createNote(noteRequest);
        console.log('Note created successfully');
    };

    if (!trade) {
        return (
            <div className="trade-detail">
                <div className="no-selection">
                    <h2>No Trade Selected</h2>
                    <p>Select a trade from the list to view details</p>
                </div>
            </div>
        );
    }

    return (
        <div className="trade-detail">
            <div className="trade-header">
                <h1>{trade.symbol}</h1>
                <span className={`trade-side ${trade.buySell.toLowerCase()}`}>
                    {trade.buySell}
                </span>
                <button className="add-note-button" onClick={handleAddNoteClick}>
                    Add Note
                </button>
            </div>

            <div className="chart-container">
                <TradingViewChart trade={trade} rsData={rsData?.rsData} />
            </div>

            <div className="detail-sections">
                <div className="detail-section">
                    <h3>Trade Summary</h3>
                    <div className="detail-grid">
                        <div className="detail-item">
                            <label>Entry Date</label>
                            <span>{new Date(trade.entryDate).toLocaleDateString()}</span>
                        </div>
                        <div className="detail-item">
                            <label>Exit Date</label>
                            <span>{new Date(trade.exitDate).toLocaleDateString()}</span>
                        </div>
                        <div className="detail-item">
                            <label>Entry Price</label>
                            <span>${trade.entryPrice.toFixed(2)}</span>
                        </div>
                        <div className="detail-item">
                            <label>Exit Price</label>
                            <span>${trade.exitPrice.toFixed(2)}</span>
                        </div>
                        <div className="detail-item">
                            <label>Quantity</label>
                            <span>{trade.quantity.toFixed(2)}</span>
                        </div>
                        <div className="detail-item">
                            <label>Instrument ID</label>
                            <span>{trade.instrumentId}</span>
                        </div>
                    </div>
                </div>

                <div className="detail-section">
                    <h3>Performance</h3>
                    <div className="pnl-display">
                        <label>Profit & Loss</label>
                        <span className={`pnl-value ${trade.pnl >= 0 ? 'positive' : 'negative'}`}>
                            ${trade.pnl.toFixed(2)}
                        </span>
                    </div>
                    <div className="pnl-metrics">
                        <div className="metric">
                            <label>Price Change</label>
                            <span className={trade.exitPrice - trade.entryPrice >= 0 ? 'positive' : 'negative'}>
                                ${(trade.exitPrice - trade.entryPrice).toFixed(2)} 
                                ({((trade.exitPrice - trade.entryPrice) / trade.entryPrice * 100).toFixed(2)}%)
                            </span>
                        </div>
                        <div className="metric">
                            <label>Total Value</label>
                            <span>${(trade.quantity * trade.exitPrice).toFixed(2)}</span>
                        </div>
                    </div>
                </div>
            </div>

            {/* RS Indicator Section */}
            <div className="rs-indicator-section">
                {rsLoading && (
                    <div className="rs-loading">
                        <p>Loading RS indicator data...</p>
                    </div>
                )}

                {rsError && (
                    <div className="rs-error">
                        <p>{rsError}</p>
                    </div>
                )}

                {!rsLoading && !rsError && rsData && (
                    <>
                        {/* RS Chart is now integrated in TradingViewChart above */}
                        <RSMetricsDashboard metrics={rsData.metrics} />
                    </>
                )}
            </div>

            {/* Add Note Modal */}
            <AddNoteModal
                isOpen={isNoteModalOpen}
                onClose={handleCloseNoteModal}
                onSubmit={handleSubmitNote}
                positionId={trade.positionId}
            />
        </div>
    );
}

export default TradeDetail;
