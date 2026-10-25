import { useState, useEffect } from 'react';
import type { Trade, CreateNoteRequest, Note } from '../../../types/api';
import { notesService } from '../../../services';
import './TradeDetailView.css';
import TradingViewChart from '../charts/TradingViewChart';
import AddNoteModal from '../modals/AddNoteModal';
import EditNoteModal from '../modals/EditNoteModal';
import NotesList from '../../common/NotesList';
import TradeSummaryCard from '../../cards/TradeSummaryCard';

interface TradeDetailProps {
    trade: Trade | null;
}

function TradeDetailView({ trade }: TradeDetailProps) {
    const [isNoteModalOpen, setIsNoteModalOpen] = useState<boolean>(false);
    const [isEditNoteModalOpen, setIsEditNoteModalOpen] = useState<boolean>(false);
    const [selectedNoteForEdit, setSelectedNoteForEdit] = useState<Note | null>(null);
    const [notes, setNotes] = useState<Note[]>([]);
    const [notesLoading, setNotesLoading] = useState<boolean>(false);


    // Fetch notes when trade changes
    useEffect(() => {
        if (!trade) {
            setNotes([]);
            return;
        }

        const fetchNotes = async () => {
            try {
                setNotesLoading(true);
                const notesData = await notesService.getNotesByPositionId(trade.positionId);
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

    const handleSubmitNote = async (comment: string, entryMethodId: number | null, errorTypeId: number | null, exitTypeId: number | null) => {
        if (!trade) {
            throw new Error('No trade selected');
        }

        const noteRequest: CreateNoteRequest = {
            positionId: trade.positionId,
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
        const notesData = await notesService.getNotesByPositionId(trade.positionId);
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
                <TradingViewChart trade={trade} />
            </div>

            <div className="detail-sections">
                <div className="detail-section compact">
                    <TradeSummaryCard trade={trade} />
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
                positionId={trade.positionId}
            />

            {/* Edit Note Modal */}
            <EditNoteModal
                isOpen={isEditNoteModalOpen}
                onClose={handleCloseEditNoteModal}
                onSubmit={handleSubmitEditNote}
                note={selectedNoteForEdit}
            />
        </div>
    );
}

export default TradeDetailView;
