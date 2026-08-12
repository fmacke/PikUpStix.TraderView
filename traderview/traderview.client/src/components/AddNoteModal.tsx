import { useState, useEffect } from 'react';
import './AddNoteModal.css';
import { apiService } from '../services/apiService';
import type { ListItem } from '../types/api';

interface AddNoteModalProps {
    isOpen: boolean;
    onClose: () => void;
    onSubmit: (comment: string, entryMethodId: number | null) => Promise<void> | Promise<any>;
    positionId: number;
}

function AddNoteModal({ isOpen, onClose, onSubmit, positionId }: AddNoteModalProps) {
    const [comment, setComment] = useState('');
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [entryMethods, setEntryMethods] = useState<ListItem[]>([]);
    const [selectedEntryMethodId, setSelectedEntryMethodId] = useState<number | null>(null);
    const [isLoadingEntryMethods, setIsLoadingEntryMethods] = useState(false);

    useEffect(() => {
        if (isOpen) {
            fetchEntryMethods();
        }
    }, [isOpen]);

    const fetchEntryMethods = async () => {
        setIsLoadingEntryMethods(true);
        try {
            const methods = await apiService.getEntryMethods();
            setEntryMethods(methods);
        } catch (error) {
            console.error('Error fetching entry methods:', error);
            // Continue without entry methods - the dropdown will just be empty
        } finally {
            setIsLoadingEntryMethods(false);
        }
    };

    if (!isOpen) {
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
            console.log('AddNoteModal: About to call onSubmit with comment:', comment, 'and entryMethodId:', selectedEntryMethodId);
            const result = await onSubmit(comment, selectedEntryMethodId);
            console.log('AddNoteModal: onSubmit returned successfully:', result);
            setComment(''); // Clear the form
            setSelectedEntryMethodId(null); // Clear the entry method selection
            onClose(); // Close the modal
        } catch (error) {
            console.error('AddNoteModal: Error submitting note:', error);
            if (error instanceof Error) {
                console.error('Error message:', error.message);
                console.error('Error stack:', error.stack);
            }
            alert('Failed to create note. Please try again.');
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleClose = () => {
        if (!isSubmitting) {
            setComment(''); // Clear the form when closing
            setSelectedEntryMethodId(null); // Clear the entry method selection
            onClose();
        }
    };

    const handleOverlayClick = (e: React.MouseEvent<HTMLDivElement>) => {
        // Only close if clicking on the overlay itself, not on the modal content
        if (e.target === e.currentTarget) {
            handleClose();
        }
    };

    return (
        <div className="modal-overlay" onClick={handleOverlayClick}>
            <div className="modal-content">
                <div className="modal-header">
                    <h2>Add Note</h2>
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
                            <small>Position ID: {positionId}</small>
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
                            {isSubmitting ? 'Saving...' : 'Save Note'}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
}

export default AddNoteModal;
