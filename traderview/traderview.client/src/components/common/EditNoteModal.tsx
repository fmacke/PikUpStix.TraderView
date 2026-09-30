import { useState, useEffect } from 'react';
import './AddNoteModal.css';
import { apiService } from '../../services/apiService';
import type { ListItem, Note } from '../../types/api';
import ListItemSelect from './ListItemSelect';

interface EditNoteModalProps {
    isOpen: boolean;
    onClose: () => void;
    onSubmit: (noteId: number, positionId: number, comment: string, entryDate: string, entryMethodId: number | null, errorTypeId: number | null) => Promise<void> | Promise<unknown>;
    note: Note | null;
}

function EditNoteModal({ isOpen, onClose, onSubmit, note }: EditNoteModalProps) {
    const [comment, setComment] = useState('');
    const [entryDate, setEntryDate] = useState('');
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [entryMethods, setEntryMethods] = useState<ListItem[]>([]);
    const [selectedEntryMethodId, setSelectedEntryMethodId] = useState<number | null>(null);
    const [isLoadingEntryMethods, setIsLoadingEntryMethods] = useState(false);
    const [errorTypes, setErrorTypes] = useState<ListItem[]>([]);
    const [selectedErrorTypeId, setSelectedErrorTypeId] = useState<number | null>(null);
    const [isLoadingErrorTypes, setIsLoadingErrorTypes] = useState(false);
    const [exitMethods, setExitMethods] = useState<ListItem[]>([]);
    const [selectedExitMethodId, setSelectedExitMethodId] = useState<number | null>(null);
    const [isLoadingExitMethods, setIsLoadingExitMethods] = useState(false);

    const fetchListItems = async (
        category: string,
        setItems: React.Dispatch<React.SetStateAction<ListItem[]>>,
        setIsLoading: React.Dispatch<React.SetStateAction<boolean>>
    ) => {
        setIsLoading(true);
        try {
            const items = await apiService.getListItems(category);
            setItems(items);
        } catch (error) {
            console.error(`Error fetching ${category} list items:`, error);
        } finally {
            setIsLoading(false);
        }
    };

    useEffect(() => {
        if (isOpen && note) {
            setComment(note.comment);
            setEntryDate(note.entryDate);
            setSelectedEntryMethodId(note.tradeTypeId);
            setSelectedErrorTypeId(note.errorTypeId);
            setSelectedExitMethodId(note.exitTypeId)
            fetchListItems('EntryMethod', setEntryMethods, setIsLoadingEntryMethods);
            fetchListItems('ErrorType', setErrorTypes, setIsLoadingErrorTypes);
            fetchListItems('ExitMethod', setExitMethods, setIsLoadingExitMethods);
        }
    }, [isOpen, note]);



    if (!isOpen || !note) {
        return null;
    }

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!comment.trim()) {
            alert('Please enter a comment');
            return;
        }

        setIsSubmitting(true);
        try {
            console.log('EditNoteModal: About to call onSubmit with noteId:', note.id, 'positionId:', note.positionId, 'comment:', comment, 'entryDate:', entryDate, 'entryMethodId:', selectedEntryMethodId, 'and errorTypeId:', selectedErrorTypeId);
            const result = await onSubmit(note.id, note.positionId, comment, entryDate, selectedEntryMethodId, selectedErrorTypeId);
            console.log('EditNoteModal: onSubmit returned successfully:', result);
            onClose(); // Close the modal
        } catch (error) {
            console.error('EditNoteModal: Error submitting note:', error);
            if (error instanceof Error) {
                console.error('Error message:', error.message);
                console.error('Error stack:', error.stack);
            }
            alert('Failed to update note. Please try again.');
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleClose = () => {
        if (!isSubmitting) {
            onClose();
        }
    };

    const handleOverlayClick = (e: React.MouseEvent<HTMLDivElement>) => {
        if (e.target === e.currentTarget) {
            handleClose();
        }
    };

    return (
        <div className="modal-overlay" onClick={handleOverlayClick}>
            <div className="modal-content">
                <div className="modal-header">
                    <h2>Edit Note</h2>
                    <button 
                        className="modal-close-button" 
                        onClick={handleClose}
                        disabled={isSubmitting}
                        type="button"
                    >
                        &times;
                    </button>
                </div>

                <form onSubmit={handleSubmit}>
                    <div className="modal-body">
                        <div className="form-group">
                            <label htmlFor="entryDate">Entry Date</label>
                            <input
                                id="entryDate"
                                type="date"
                                value={entryDate.split('T')[0]}
                                onChange={(e) => setEntryDate(e.target.value + 'T00:00:00')}
                                disabled={isSubmitting}
                            />
                        </div>

                        <ListItemSelect
                            id="entryMethod"
                            label="Entry Method"
                            items={entryMethods}
                            selectedId={selectedEntryMethodId}
                            onChange={setSelectedEntryMethodId}
                            isLoading={isLoadingEntryMethods}
                            isDisabled={isSubmitting}
                            placeholder="-- Select Entry Method (Optional) --"
                        />

                        <ListItemSelect
                            id="errorType"
                            label="Error Type"
                            items={errorTypes}
                            selectedId={selectedErrorTypeId}
                            onChange={setSelectedErrorTypeId}
                            isLoading={isLoadingErrorTypes}
                            isDisabled={isSubmitting}
                            placeholder="-- Select Error Type (Optional) --"
                        />

                        <ListItemSelect
                            id="exitMethod"
                            label="Exit Method"
                            items={exitMethods}
                            selectedId={selectedExitMethodId}
                            onChange={setSelectedExitMethodId}
                            isLoading={isLoadingExitMethods}
                            isDisabled={isSubmitting}
                            placeholder="-- Select Exit Method (Optional) --"
                        />

                        <div className="form-group">
                            <label htmlFor="comment">Comment</label>
                            <textarea
                                id="comment"
                                value={comment}
                                onChange={(e) => setComment(e.target.value)}
                                placeholder="Enter your note here..."
                                rows={6}
                                disabled={isSubmitting}
                                required
                            />
                        </div>

                        <div className="form-info">
                            <small>Note ID: {note.id}</small>
                        </div>
                    </div>

                    <div className="modal-footer">
                        <button 
                            type="button" 
                            className="button-secondary" 
                            onClick={handleClose}
                            disabled={isSubmitting}
                        >
                            Cancel
                        </button>
                        <button 
                            type="submit" 
                            className="button-primary"
                            disabled={isSubmitting || !comment.trim()}
                        >
                            {isSubmitting ? 'Updating...' : 'Update Note'}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
}

export default EditNoteModal;
