import { useState, useEffect } from 'react';
import './AddNoteModal.css';
import { apiService } from '../../services/apiService';
import type { ListItem } from '../../types/api';
import ListItemSelect from './ListItemSelect';

interface AddNoteModalProps {
    isOpen: boolean;
    onClose: () => void;
    onSubmit: (comment: string, entryMethodId: number | null, errorTypeId: number | null) => Promise<void> | Promise<any>;
    positionId: number;
}

function AddNoteModal({ isOpen, onClose, onSubmit, positionId }: AddNoteModalProps) {
    const [comment, setComment] = useState('');
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [entryMethods, setEntryMethods] = useState<ListItem[]>([]);
    const [selectedEntryMethodId, setSelectedEntryMethodId] = useState<number | null>(null);
    const [isLoadingEntryMethods, setIsLoadingEntryMethods] = useState(false);
    const [errorTypes, setErrorTypes] = useState<ListItem[]>([]);
    const [exitTypes, setExitTypes] = useState<ListItem[]>([]);
    const [selectedErrorTypeId, setSelectedErrorTypeId] = useState<number | null>(null);
    const [isLoadingErrorTypes, setIsLoadingErrorTypes] = useState(false);
    const [selectedExitTypeId, setSelectedExitTypeId] = useState<number | null>(null);
    const [isLoadingExitTypes, setIsLoadingExitTypes] = useState(false);

    useEffect(() => {
        if (isOpen) {
            fetchListItems('EntryMethod', setEntryMethods, setIsLoadingEntryMethods);
            fetchListItems('EntryError', setErrorTypes, setIsLoadingErrorTypes);
            fetchListItems('ExitMethod', setExitTypes, setIsLoadingExitTypes);
        }
    }, [isOpen]);

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
            // Continue without items - the dropdown will just be empty
        } finally {
            setIsLoading(false);
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
            console.log('AddNoteModal: About to call onSubmit with comment:', comment, 'entryMethodId:', selectedEntryMethodId, 'and errorTypeId:', selectedErrorTypeId);
            const result = await onSubmit(comment, selectedEntryMethodId, selectedErrorTypeId);
            console.log('AddNoteModal: onSubmit returned successfully:', result);
            setComment(''); // Clear the form
            setSelectedEntryMethodId(null); // Clear the entry method selection
            setSelectedErrorTypeId(null); // Clear the error type selection
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
            setSelectedErrorTypeId(null); // Clear the error type selection
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
                            id="exitType"
                            label="Exit Type"
                            items={exitTypes}
                            selectedId={selectedExitTypeId}
                            onChange={setSelectedExitTypeId}
                            isLoading={isLoadingExitTypes}
                            isDisabled={isSubmitting}
                            placeholder="-- Select Exit Type (Optional) --"
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
