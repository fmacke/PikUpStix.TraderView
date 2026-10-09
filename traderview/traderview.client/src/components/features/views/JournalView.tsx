import { useEffect, useState, useMemo } from 'react';
import AddJournalEntryModal from '../../common/AddJournalEntryModal';
import EditJournalEntryModal from '../../common/EditJournalEntryModal';
import { SortableTableHeader } from '../../common/SortableTableHeader';
import type { SortConfig } from '../../common/SortableTableHeader';
import { apiService } from '../../../services/apiService';
import type { Note } from '../../../types/api';
import './JournalView.css';

function JournalView() {
    const [notes, setNotes] = useState<Note[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);
    const [isAddOpen, setIsAddOpen] = useState(false);
    const [isEditOpen, setIsEditOpen] = useState(false);
    const [selectedNote, setSelectedNote] = useState<Note | null>(null);
    const [sortConfig, setSortConfig] = useState<SortConfig<Note>>({
        key: 'entryDate',
        direction: 'desc'
    });
    const [taskTypes, setTaskTypes] = useState<Map<number, string>>(new Map());
    const [subtaskTypes, setSubtaskTypes] = useState<Map<number, string>>(new Map());

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

    // Fetch list items for type name mapping
    useEffect(() => {
        const fetchListItems = async () => {
            try {
                const taskAndSubtaskPromises = [
                    apiService.getListItems('JournalTaskType'),
                    apiService.getListItems('JournalSubTaskType')
                ];

                const results = await Promise.all(taskAndSubtaskPromises);

                const taskTypeMap = new Map(results[0].map((item: { id: number; name: string }) => [item.id, item.name]));
                const subtaskTypeMap = new Map(results[1].map((item: { id: number; name: string }) => [item.id, item.name]));

                setTaskTypes(taskTypeMap);
                setSubtaskTypes(subtaskTypeMap);
            } catch (error) {
                console.error('Error fetching list items:', error);
            }
        };

        fetchListItems();
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

    const handleAddSubmit = async (comment: string, journalTaskTypeId: number | null, journalSubTaskTypeId: number | null, entryDateParam?: string, timeSpent?: number | null) => {
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
            exitTypeId: journalSubTaskTypeId,
            time: timeSpent ?? null
        };

        await apiService.createNote(request);
        await loadJournal();
    };

    const handleEditSubmit = async (noteId: number, comment: string, journalTaskTypeId: number | null, journalSubTaskTypeId: number | null, entryDate: string, timeSpent: number | null) => {
        await apiService.updateNote(noteId, comment, entryDate, journalTaskTypeId, journalSubTaskTypeId, timeSpent);
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

    const handleSort = (key: keyof Note) => {
        setSortConfig(prev => ({
            key,
            direction: prev.key === key && prev.direction === 'asc' ? 'desc' : 'asc'
        }));
    };

    const sortedNotes = useMemo(() => {
        if (!notes || notes.length === 0) return [];

        return [...notes].sort((a, b) => {
            const aVal = a[sortConfig.key];
            const bVal = b[sortConfig.key];

            if (aVal == null && bVal == null) return 0;
            if (aVal == null) return 1;
            if (bVal == null) return -1;

            if (sortConfig.key === 'entryDate') {
                const dateA = new Date(aVal as string).getTime();
                const dateB = new Date(bVal as string).getTime();
                return sortConfig.direction === 'asc' ? dateA - dateB : dateB - dateA;
            }

            if (typeof aVal === 'number' && typeof bVal === 'number') {
                return sortConfig.direction === 'asc' ? aVal - bVal : bVal - aVal;
            }

            const strA = String(aVal).toLowerCase();
            const strB = String(bVal).toLowerCase();
            return sortConfig.direction === 'asc'
                ? strA.localeCompare(strB)
                : strB.localeCompare(strA);
        });
    }, [notes, sortConfig]);

    const getTaskTypeName = (taskTypeId: number | null): string => {
        if (taskTypeId === null || taskTypeId === undefined) return '-';
        return taskTypes.get(taskTypeId) || `Unknown (${taskTypeId})`;
    };

    const getSubtaskTypeName = (subtaskTypeId: number | null): string => {
        if (subtaskTypeId === null || subtaskTypeId === undefined) return '-';
        return subtaskTypes.get(subtaskTypeId) || `Unknown (${subtaskTypeId})`;
    };

    return (
        <div className="open-positions-container">
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
                <h1>Journal</h1>
                <div>
                    <button onClick={() => setIsAddOpen(true)} className="nav-button">Add Journal Entry</button>
                </div>
            </div>

            {loading && (
                <div className="loading-container">
                    <p><em>Loading journal...</em></p>
                </div>
            )}

            {error && (
                <div className="error-container">
                    <p className="error">{error}</p>
                </div>
            )}

            {!loading && !error && notes.length === 0 && (
                <div className="empty-container">
                    <p><em>No journal entries found.</em></p>
                </div>
            )}

            {!loading && !error && notes.length > 0 && (
                <div className="positions-table-container">
                    <table className="positions-table">
                        <thead>
                            <tr>
                                <SortableTableHeader columnKey="entryDate" title="Date" sortConfig={sortConfig} onSort={handleSort} />
                                <SortableTableHeader columnKey="comment" title="Comment" sortConfig={sortConfig} onSort={handleSort} />
                                <SortableTableHeader columnKey="time" title="Time Spent (hrs)" sortConfig={sortConfig} onSort={handleSort} />
                                <SortableTableHeader columnKey="errorTypeId" title="Task" sortConfig={sortConfig} onSort={handleSort} />
                                <SortableTableHeader columnKey="exitTypeId" title="Subtask" sortConfig={sortConfig} onSort={handleSort} />
                                <th>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            {sortedNotes.map((note) => (
                                <tr key={note.id}>
                                    <td className="note-date">{new Date(note.entryDate).toLocaleDateString()}</td>
                                    <td className="note-comment">{note.comment}</td>
                                    <td className="number-cell">{note.time !== null ? note.time.toFixed(2) : '-'}</td>
                                    <td>{getTaskTypeName(note.errorTypeId)}</td>
                                    <td>{getSubtaskTypeName(note.exitTypeId)}</td>
                                    <td className="note-actions">
                                        <button
                                            className="edit-note-button"
                                            onClick={() => handleEdit(note)}
                                            type="button"
                                            title="Edit note"
                                        >
                                            Edit
                                        </button>
                                        <button
                                            className="delete-note-button"
                                            onClick={() => handleDelete(note.id)}
                                            type="button"
                                            title="Delete note"
                                        >
                                            Delete
                                        </button>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            )}

            <AddJournalEntryModal
                isOpen={isAddOpen}
                onClose={() => setIsAddOpen(false)}
                onSubmit={handleAddSubmit}
            />

            <EditJournalEntryModal
                isOpen={isEditOpen}
                onClose={() => { setIsEditOpen(false); setSelectedNote(null); }}
                onSubmit={handleEditSubmit}
                note={selectedNote}
            />
        </div>
    );
}

export default JournalView;
