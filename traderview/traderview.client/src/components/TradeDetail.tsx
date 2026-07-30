import { useState, useEffect } from 'react';
import type { Trade, RSIndicatorData, CreateNoteRequest, Note } from '../types/api';
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
    const [notes, setNotes] = useState<Note[]>([]);
    const [notesLoading, setNotesLoading] = useState<boolean>(false);

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

    // Fetch notes when trade changes
    useEffect(() => {
        if (!trade) {
            setNotes([]);
            return;
        }

        const fetchNotes = async () => {
            try {
                setNotesLoading(true);
                const notesData = await apiService.getNotesByPositionId(trade.positionId);
                setNotes(notesData);
            } catch (error) {
                console.error('Error fetching notes:', error);
                setNotes([]);
            } finally {
                setNotesLoading(false);
            }
        };

        fetchNotes();
    }, [trade?.positionId]);

    const handleAddNoteClick = () => {
        setIsNoteModalOpen(true);
    };

    const handleCloseNoteModal = () => {
        setIsNoteModalOpen(false);
    };

    const handleSubmitNote = async (comment: string) => {
        if (!trade) {
            throw new Error('No trade selected');
        }

        const noteRequest: CreateNoteRequest = {
            positionId: trade.positionId,
            tradeExecutionId: null, // Can be extended later to link to specific executions
            comment: comment,
            entryDate: new Date().toISOString(),
            tradeTypeId: 1 // Default trade type, can be made configurable
        };

        const result = await apiService.createNote(noteRequest);
        console.log('Note created successfully', result);

        // Refresh notes list after adding a new note
        const notesData = await apiService.getNotesByPositionId(trade.positionId);
        setNotes(notesData);

        return result;
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
                <div className="detail-section compact">
                    <h3>Trade Summary & Performance</h3>
                    <table className="detail-table">
                        <tbody>
                            <tr>
                                <td className="label">Entry Date</td>
                                <td>{new Date(trade.entryDate).toLocaleDateString()}</td>
                            </tr>
                            <tr>
                                <td className="label">Exit Date</td>
                                <td>{new Date(trade.exitDate).toLocaleDateString()}</td>
                            </tr>
                            <tr>
                                <td className="label">Quantity</td>
                                <td>{trade.quantity.toFixed(2)}</td>
                            </tr>
                            <tr>
                                <td className="label">Entry Price</td>
                                <td>${trade.entryPrice.toFixed(2)}</td>
                            </tr>
                            <tr>
                                <td className="label">Exit Price</td>
                                <td>${trade.exitPrice.toFixed(2)}</td>
                            </tr>
                            <tr>
                                <td className="label">Instrument ID</td>
                                <td>{trade.instrumentId}</td>
                            </tr>
                            <tr>
                                <td className="label">P&L</td>
                                <td className={trade.pnl >= 0 ? 'positive' : 'negative'}>
                                    ${trade.pnl.toFixed(2)}
                                </td>
                            </tr>
                            <tr>
                                <td className="label">Price Change</td>
                                <td className={trade.exitPrice - trade.entryPrice >= 0 ? 'positive' : 'negative'}>
                                    ${(trade.exitPrice - trade.entryPrice).toFixed(2)} ({((trade.exitPrice - trade.entryPrice) / trade.entryPrice * 100).toFixed(2)}%)
                                </td>
                            </tr>
                            <tr>
                                <td className="label">Total Value</td>
                                <td>${(trade.quantity * trade.exitPrice).toFixed(2)}</td>
                            </tr>
                        </tbody>
                    </table>
                </div>
                <div id="notesHolder" className="detail-section compact">
                    <h3>Notes</h3>
                    {notesLoading ? (
                        <p>Loading notes...</p>
                    ) : notes.length === 0 ? (
                        <p>No notes available for this position.</p>
                    ) : (
                        <table className="detail-table">
                            <thead>
                                <tr>
                                    <th>Date</th>
                                    <th>Comment</th>
                                </tr>
                            </thead>
                            <tbody>
                                {notes.map((note) => (
                                    <tr key={note.id}>
                                        <td>{new Date(note.entryDate).toLocaleDateString()}</td>
                                        <td>{note.comment}</td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    )}
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
