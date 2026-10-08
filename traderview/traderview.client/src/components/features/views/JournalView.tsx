import { useEffect, useState } from 'react';
import NotesList from '../../common/NotesList';
import AddJournalEntryModal from '../../common/AddJournalEntryModal';
import EditNoteModal from '../../common/EditNoteModal';
import { apiService } from '../../../services/apiService';
import type { Note } from '../../../types/api';

function JournalView() {
    const [notes, setNotes] = useState<Note[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);
    const [isAddOpen, setIsAddOpen] = useState(false);
    const [isEditOpen, setIsEditOpen] = useState(false);
    const [selectedNote, setSelectedNote] = useState<Note | null>(null);

    useEffect(() => {
        const fetch = async () => {
            try {
                setLoading(true);
                setError(null);
                const data = await apiService.getJournalEntries();
                setNotes(data);
            } catch (err) {
                console.error('Error loading journal entries', err);
                setError('Failed to load journal entries');
            } finally {
                setLoading(false);
            }
        };

        fetch();
    }, []);

    // Exposed loader used by handlers (do not call this directly from useEffect to satisfy lint)
    const loadJournal = async () => {
        try {
            setLoading(true);
            setError(null);
            const data = await apiService.getJournalEntries();
            setNotes(data);
        } catch (err) {
            console.error('Error loading journal entries', err);
            setError('Failed to load journal entries');
        } finally {
            setLoading(false);
        }
    };

    const handleAddSubmit = async (comment: string, journalTaskTypeId: number | null, journalSubTaskTypeId: number | null, entryDateParam?: string) => {
        // Determine the JournalEntry list item id (should be a single item)
        const journalEntryItems = await apiService.getListItems('JournalEntry');
        const journalEntryId = journalEntryItems.length > 0 ? journalEntryItems[0].id : null;

        const request = {
            positionId: null,
            tradeExecutionId: null,
            comment,
            entryDate: entryDateParam ?? new Date().toISOString(),
            tradeTypeId: journalEntryId,
            // JournalTaskType stored in ErrorTypeId per spec
            errorTypeId: journalTaskTypeId,
            // JournalSubTaskType stored in ExitTypeId per spec
            exitTypeId: journalSubTaskTypeId
        };

        await apiService.createNote(request);
        await loadJournal();
    };

    const handleEditSubmit = async (noteId: number, _positionId: number, comment: string, entryDate: string, entryMethodId: number | null, errorTypeId: number | null, exitMethodId: number | null) => {
        await apiService.updateNote(noteId, comment, entryDate, entryMethodId, errorTypeId, exitMethodId);
        await loadJournal();
    };

    const handleEdit = (note: Note) => {
        setSelectedNote(note);
        setIsEditOpen(true);
    };

    const handleDelete = async (noteId: number) => {
        if (!confirm('Delete this journal entry?')) return;
        await apiService.deleteNote(noteId);
        await loadJournal();
    };

    return (
        <div style={{ padding: '10px' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '10px' }}>
                <h2>Journal</h2>
                <div>
                    <button onClick={() => setIsAddOpen(true)} className="nav-button">Add Journal Entry</button>
                </div>
            </div>

            {loading && <p>Loading journal...</p>}
            {error && <p style={{ color: 'red' }}>{error}</p>}

            {!loading && !error && (
                <NotesList notes={notes} loading={false} variant="simple" onEditNote={handleEdit} onDeleteNote={handleDelete} />
            )}

            <AddJournalEntryModal
                isOpen={isAddOpen}
                onClose={() => setIsAddOpen(false)}
                onSubmit={handleAddSubmit}
            />

            <EditNoteModal
                isOpen={isEditOpen}
                onClose={() => { setIsEditOpen(false); setSelectedNote(null); }}
                onSubmit={handleEditSubmit}
                note={selectedNote}
            />
        </div>
    );
}

export default JournalView;
