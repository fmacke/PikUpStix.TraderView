import { useEffect, useState } from 'react';
import type { Note } from '../../types/api';
import { apiService } from '../../services/apiService';
import './NotesList.css';

interface NotesListProps {
    notes: Note[];
    loading: boolean;
    variant?: 'simple' | 'detailed';
    onEditNote?: (note: Note) => void;
}

function NotesList({ notes, loading, variant = 'simple', onEditNote }: NotesListProps) {
    const [tradeTypes, setTradeTypes] = useState<Map<number, string>>(new Map());
    const [errorTypes, setErrorTypes] = useState<Map<number, string>>(new Map());
    const [listItemsLoading, setListItemsLoading] = useState(false);

    // Fetch list items when component mounts (for detailed variant)
    useEffect(() => {
        if (variant !== 'detailed') {
            return;
        }

        const fetchListItems = async () => {
            try {
                setListItemsLoading(true);
                const [entryMethods, entryErrors] = await Promise.all([
                    apiService.getEntryMethods(),
                    apiService.getErrorTypes()
                ]);

                // Create maps for quick lookup
                const tradeTypeMap = new Map(entryMethods.map(item => [item.id, item.name]));
                const errorTypeMap = new Map(entryErrors.map(item => [item.id, item.name]));

                setTradeTypes(tradeTypeMap);
                setErrorTypes(errorTypeMap);
            } catch (error) {
                console.error('Error fetching list items:', error);
                // Continue without list items if fetch fails
            } finally {
                setListItemsLoading(false);
            }
        };

        fetchListItems();
    }, [variant]);

    if (loading || (variant === 'detailed' && listItemsLoading)) {
        return <p className="notes-loading">Loading notes...</p>;
    }

    if (notes.length === 0) {
        return <p className="notes-empty">No notes available for this position.</p>;
    }

    const getTradeTypeName = (tradeTypeId: number | null): string => {
        if (tradeTypeId === null || tradeTypeId === undefined) return '-';
        return tradeTypes.get(tradeTypeId) || `Unknown (${tradeTypeId})`;
    };

    const getErrorTypeName = (errorTypeId: number | null): string => {
        if (errorTypeId === null || errorTypeId === undefined) return '-';
        return errorTypes.get(errorTypeId) || `Unknown (${errorTypeId})`;
    };

    return (
        <div className="notes-table-container">
            <table className="notes-table">
                <thead>
                    <tr>
                        <th>Date</th>
                        <th>Comment</th>
                        {variant === 'detailed' && (
                            <>
                                <th>TradeType</th>
                                <th>ErrorType</th>
                            </>
                        )}
                        {onEditNote && <th>Actions</th>}
                    </tr>
                </thead>
                <tbody>
                    {notes.map((note) => (
                        <tr key={note.id}>
                            <td className="note-date">{new Date(note.entryDate).toLocaleDateString()}</td>
                            <td className="note-comment">{note.comment}</td>
                            {variant === 'detailed' && (
                                <>
                                    <td>{getTradeTypeName(note.tradeTypeId)}</td>
                                    <td>{getErrorTypeName(note.errorTypeId)}</td>
                                </>
                            )}
                            {onEditNote && (
                                <td className="note-actions">
                                    <button 
                                        className="edit-note-button"
                                        onClick={() => onEditNote(note)}
                                        type="button"
                                        title="Edit note"
                                    >
                                        Edit
                                    </button>
                                </td>
                            )}
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
}

export default NotesList;
