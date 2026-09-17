import { useState, useEffect } from 'react';
import './AddNoteModal.css';
import { apiService } from '../../services/apiService';
import type { ListItem, Note } from '../../types/api';

interface EditNoteModalProps {
    isOpen: boolean;
    onClose: () => void;
    onSubmit: (noteId: number, positionId: number, comment: string, entryDate: string, entryMethodId: number | null, errorTypeId: number | null) => Promise<void> | Promise<any>;
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

    useEffect(() => {
        if (isOpen && note) {
            setComment(note.comment);
            setEntryDate(note.entryDate);
            setSelectedEntryMethodId(note.tradeTypeId);
            setSelectedErrorTypeId(note.errorTypeId);
            fetchEntryMethods();
            fetchErrorTypes();
        }
    }, [isOpen, note]);

    const fetchEntryMethods = async () => {
        setIsLoadingEntryMethods(true);
        try {
            const methods = await apiService.getEntryMethods();
            setEntryMethods(methods);
        } catch (error) {
            console.error('Error fetching entry methods:', error);
        } finally {
            setIsLoadingEntryMethods(false);
        }
    };

    const fetchErrorTypes = async () => {
        setIsLoadingErrorTypes(true);
        try {
            const types = await apiService.getErrorTypes();
            setErrorTypes(types);
        } catch (error) {
            console.error('Error fetching error types:', error);
        } finally {
            setIsLoadingErrorTypes(false);
        }
    };

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

                        <div className="form-group">
                            <label htmlFor="entryMethod">Entry Method</label>
                            <select
                                id="entryMethod"
                                value={selectedEntryMethodId ?? ''}
                                onChange={(e) => setSelectedEntryMethodId(e.target.value ? parseInt(e.target.value) : null)}
                                disabled={isSubmitting || isLoadingEntryMethods}
                            >
                                <option value="">-- Select Entry Method (Optional) --</option>
                                {entryMethods.map((method) => (
                                    <option key={method.id} value={method.id}>
                                        {method.name}
                                    </option>
                                ))}
                            </select>
                        </div>

                        <div className="form-group">
                            <label htmlFor="errorType">Error Type</label>
                            <select
                                id="errorType"
                                value={selectedErrorTypeId ?? ''}
                                onChange={(e) => setSelectedErrorTypeId(e.target.value ? parseInt(e.target.value) : null)}
                                disabled={isSubmitting || isLoadingErrorTypes}
                            >
                                <option value="">-- Select Error Type (Optional) --</option>
                                {errorTypes.map((type) => (
                                    <option key={type.id} value={type.id}>
                                        {type.name}
                                    </option>
                                ))}
                            </select>
                        </div>

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
