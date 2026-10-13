import api from './apiClient';
import type { Note, CreateNoteRequest } from '../types/api';

export const notesService = {
    async getNotesByPositionId(positionId: number): Promise<Note[]> {
        return api.get<Note[]>(`/notes/position/${positionId}`);
    },

    async createNote(request: CreateNoteRequest): Promise<Note> {
        return api.post<Note>('/notes', request);
    },

    async updateNote(noteId: number, comment: string, entryDate: string, entryMethodId: number | null, errorTypeId: number | null, exitTypeId: number | null, time?: number | null): Promise<Note> {
        const body = {
            id: noteId,
            comment,
            entryDate: entryDate ?? new Date().toISOString(),
            updatedAt: new Date().toISOString(),
            tradeTypeId: entryMethodId,
            errorTypeId,
            exitTypeId,
            time: time ?? null,
        };
        return api.put<Note>('/notes', body);
    },

    async deleteNote(noteId: number): Promise<void> {
        await api.delete(`/notes/${noteId}`);
    },

    async getJournalEntries(): Promise<Note[]> {
        return api.get<Note[]>('/notes/journal');
    },
};

export default notesService;
