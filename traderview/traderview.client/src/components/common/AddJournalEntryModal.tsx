import { useState, useEffect } from 'react';
import './AddNoteModal.css';
import { apiService } from '../../services/apiService';
import type { ListItem } from '../../types/api';

interface AddJournalEntryModalProps {
    isOpen: boolean;
    onClose: () => void;
    onSubmit: (comment: string, journalTaskTypeId: number | null, journalSubTaskTypeId: number | null, entryDate?: string) => Promise<void>;
}

function AddJournalEntryModal({ isOpen, onClose, onSubmit }: AddJournalEntryModalProps) {
    const [comment, setComment] = useState('');
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [taskTypes, setTaskTypes] = useState<ListItem[]>([]);
    const [subTaskTypes, setSubTaskTypes] = useState<ListItem[]>([]);
    const [selectedTaskTypeId, setSelectedTaskTypeId] = useState<number | null>(null);
    const [selectedSubTaskTypeId, setSelectedSubTaskTypeId] = useState<number | null>(null);
    const [entryDate, setEntryDate] = useState<string>(new Date().toISOString());

    // Fetchers
    const fetchTaskTypes = async () => {
        try {
            const items = await apiService.getListItems('JournalTaskType');
            setTaskTypes(items);
        } catch (err) {
            console.error('Error fetching JournalTaskType items', err);
        }
    };

    const fetchSubTaskTypes = async () => {
        try {
            const items = await apiService.getListItems('JournalSubTaskType');
            setSubTaskTypes(items);
        } catch (err) {
            console.error('Error fetching JournalSubTaskType items', err);
        }
    };

    useEffect(() => {
        if (!isOpen) return;
        (async () => {
            try {
                await fetchTaskTypes();
                await fetchSubTaskTypes();
            } catch (err) {
                // errors logged in fetchers
            }
        })();
    }, [isOpen]);

    // Filter sub tasks based on selected task type (computed)
    const filtered = selectedTaskTypeId == null ? [] : subTaskTypes.filter(s => s.parentListId === selectedTaskTypeId);

    if (!isOpen) return null;

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!comment.trim()) {
            alert('Please enter a comment');
            return;
        }

        setIsSubmitting(true);
        try {
            await onSubmit(comment, selectedTaskTypeId, selectedSubTaskTypeId, entryDate);
            setComment('');
            setSelectedTaskTypeId(null);
            setSelectedSubTaskTypeId(null);
            onClose();
        } catch (err) {
            console.error('Failed to create journal entry', err);
            alert('Failed to create journal entry');
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleOverlayClick = (e: React.MouseEvent<HTMLDivElement>) => {
        if (e.target === e.currentTarget && !isSubmitting) onClose();
    };

    return (
        <div className="modal-overlay" onClick={handleOverlayClick}>
            <div className="modal-content">
                <div className="modal-header">
                    <h2>Add Journal Entry</h2>
                    <button className="modal-close-button" onClick={() => !isSubmitting && onClose()} type="button">&times;</button>
                </div>

                <form onSubmit={handleSubmit}>
                    <div className="modal-body">
                        <div className="form-group">
                            <label htmlFor="entryDate">Date</label>
                            <input
                                id="entryDate"
                                type="date"
                                value={entryDate.split('T')[0]}
                                onChange={(e) => setEntryDate(e.target.value + 'T00:00:00')}
                                disabled={isSubmitting}
                            />
                        </div>

                        <div className="form-group">
                            <label htmlFor="taskType">Task Type</label>
                            <select id="taskType" value={selectedTaskTypeId ?? ''} onChange={(e) => setSelectedTaskTypeId(e.target.value ? Number(e.target.value) : null)} disabled={isSubmitting}>
                                <option value="">-- Select Task Type (Optional) --</option>
                                {taskTypes.map(t => (
                                    <option key={t.id} value={t.id}>{t.name}</option>
                                ))}
                            </select>
                        </div>

                        <div className="form-group">
                            <label htmlFor="subTaskType">Sub Task Type</label>
                            <select id="subTaskType" value={selectedSubTaskTypeId ?? ''} onChange={(e) => setSelectedSubTaskTypeId(e.target.value ? Number(e.target.value) : null)} disabled={isSubmitting || filtered.length === 0}>
                                <option value="">-- Select Sub Task Type (Optional) --</option>
                                {filtered.map(s => (
                                    <option key={s.id} value={s.id}>{s.name}</option>
                                ))}
                            </select>
                        </div>

                        <div className="form-group">
                            <label htmlFor="comment">Comment</label>
                            <textarea id="comment" value={comment} onChange={(e) => setComment(e.target.value)} rows={6} disabled={isSubmitting}></textarea>
                        </div>
                    </div>

                    <div className="modal-footer">
                        <button type="button" className="modal-cancel-button" onClick={() => !isSubmitting && onClose()} disabled={isSubmitting}>Cancel</button>
                        <button type="submit" className="modal-submit-button" disabled={isSubmitting}>Add Entry</button>
                    </div>
                </form>
            </div>
        </div>
    );
}

export default AddJournalEntryModal;
