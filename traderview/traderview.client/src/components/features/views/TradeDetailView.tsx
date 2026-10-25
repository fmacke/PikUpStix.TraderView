import { useState, useEffect } from 'react';
import type { Trade, CreateNoteRequest, Note } from '../../../types/api';
import { notesService, tradeService } from '../../../services';
import './TradeDetailView.css';
import TradingViewChart from '../charts/TradingViewChart';
import AddNoteModal from '../modals/AddNoteModal';
import EditNoteModal from '../modals/EditNoteModal';
import NotesList from '../../common/NotesList';
import TradeSummaryCard from '../../cards/TradeSummaryCard';
import TradeListerCard from '../../cards/TradeListerCard';

function TradeDetailView() {
    const [trades, setTrades] = useState<Trade[]>([]);
    const [selectedTrade, setSelectedTrade] = useState<Trade | null>(null);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);
    const [useCarousel, setUseCarousel] = useState<boolean>(true);
    const [isNoteModalOpen, setIsNoteModalOpen] = useState<boolean>(false);
    const [isEditNoteModalOpen, setIsEditNoteModalOpen] = useState<boolean>(false);
    const [selectedNoteForEdit, setSelectedNoteForEdit] = useState<Note | null>(null);
    const [notes, setNotes] = useState<Note[]>([]);
    const [notesLoading, setNotesLoading] = useState<boolean>(false);

    useEffect(() => {
        populateTradeData();
    }, []);

    const handleTradeSelect = (trade: Trade) => {
        setSelectedTrade(trade);
    };

    // Fetch notes when trade changes
    useEffect(() => {
        if (!selectedTrade) {
            setNotes([]);
            return;
        }

        const fetchNotes = async () => {
            try {
                setNotesLoading(true);
                const notesData = await notesService.getNotesByPositionId(selectedTrade.positionId);
                setNotes(notesData);
            } catch (error) {
                console.error('Error fetching notes:', error);
                setNotes([]);
            } finally {
                setNotesLoading(false);
            }
        };

        fetchNotes();
    }, [selectedTrade?.positionId]);

    const handleAddNoteClick = () => {
        setIsNoteModalOpen(true);
    };

    const handleCloseNoteModal = () => {
        setIsNoteModalOpen(false);
    };

    const handleSubmitNote = async (comment: string, entryMethodId: number | null, errorTypeId: number | null, exitTypeId: number | null) => {
        if (!selectedTrade) {
            throw new Error('No trade selected');
        }

        const noteRequest: CreateNoteRequest = {
            positionId: selectedTrade.positionId,
            tradeExecutionId: null, // Can be extended later to link to specific executions
            comment: comment,
            entryDate: new Date().toISOString(),
            tradeTypeId: entryMethodId ?? null, 
            errorTypeId: errorTypeId ?? null,
            exitTypeId: exitTypeId ?? null,
            time: null,
        };

        const result = await notesService.createNote(noteRequest);
        console.log('Note created successfully', result);

        // Refresh notes list after adding a new note
        const notesData = await notesService.getNotesByPositionId(selectedTrade.positionId);
        setNotes(notesData);

        return result;
    };

    const handleEditNoteClick = (note: Note) => {
        setSelectedNoteForEdit(note);
        setIsEditNoteModalOpen(true);
    };

    const handleCloseEditNoteModal = () => {
        setIsEditNoteModalOpen(false);
        setSelectedNoteForEdit(null);
    };

    const handleSubmitEditNote = async (noteId: number, positionId: number, comment: string, entryDate: string, entryMethodId: number | null, errorTypeId: number | null, exitTypeId: number | null) => {
        const result = await notesService.updateNote(noteId, comment, entryDate, entryMethodId, errorTypeId, exitTypeId);
        console.log('Note updated successfully', result);

        // Refresh notes list after updating a note
        const notesData = await notesService.getNotesByPositionId(positionId);
        setNotes(notesData);

        return result;
    };

    if (!selectedTrade) {
        return (
            <div className="master-detail-layout">
                <div className="detail-pane">
                    <div className="trade-detail">
                        <div className="no-selection">
                            {loading && (
                                <>
                                    <h2>Loading</h2>
                                    <p><em>Loading trades... Please ensure the ASP.NET backend has started.</em></p>
                                </>
                            )}
                            {error && (
                                <>
                                    <h2>Error Loading Trades</h2>
                                    <p className="error">{error}</p>
                                    <button onClick={() => populateTradeData()}>Retry</button>
                                </>
                            )}
                            {!loading && !error && (
                                <>
                                    <h2>No Trade Selected</h2>
                                    <p>Select a trade from the list to view details</p>
                                </>
                            )}
                        </div>
                    </div>
                </div>
                {trades.length > 0 && (
                    <TradeListerCard
                        trades={trades}
                        selectedTrade={selectedTrade}
                        onTradeSelect={handleTradeSelect}
                        useCarousel={useCarousel}
                        onToggleCarousel={setUseCarousel}
                    />
                )}
            </div>
        );
    }

    return (
        <div className="master-detail-layout">
            <div className="detail-pane">
                <div className="trade-detail">
                    <div className="trade-header">
                        <h1>{selectedTrade.symbol}</h1>
                        <span className={`trade-side ${selectedTrade.buySell.toLowerCase()}`}>
                            {selectedTrade.buySell}
                        </span>
                        <button className="add-note-button" onClick={handleAddNoteClick}>
                            Add Note
                        </button>
                    </div>

                    <div className="chart-container">
                        <TradingViewChart trade={selectedTrade} />
                    </div>

                    <div className="detail-sections">
                        <div className="detail-section compact">
                            <TradeSummaryCard trade={selectedTrade} />
                        </div>
                        <div id="notesHolder" className="detail-section compact">
                            <h3>Notes</h3>
                            <NotesList notes={notes} loading={notesLoading} variant="detailed" onEditNote={handleEditNoteClick} />
                        </div>
                    </div>

                    {/* Add Note Modal */}
                    <AddNoteModal
                        isOpen={isNoteModalOpen}
                        onClose={handleCloseNoteModal}
                        onSubmit={handleSubmitNote}
                        positionId={selectedTrade.positionId}
                    />

                    {/* Edit Note Modal */}
                    <EditNoteModal
                        isOpen={isEditNoteModalOpen}
                        onClose={handleCloseEditNoteModal}
                        onSubmit={handleSubmitEditNote}
                        note={selectedNoteForEdit}
                    />
                </div>
            </div>
            <TradeListerCard
                trades={trades}
                selectedTrade={selectedTrade}
                onTradeSelect={handleTradeSelect}
                useCarousel={useCarousel}
                onToggleCarousel={setUseCarousel}
            />
        </div>
    );
    async function populateTradeData() {
        try {
            setLoading(true);
            setError(null);
            console.log('[App] populateTradeData - calling tradeService.getTrades');
            const data = await tradeService.getTrades();
            console.log('[App] populateTradeData - received data', data?.length);
            setTrades(data);
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

export default TradeDetailView;
