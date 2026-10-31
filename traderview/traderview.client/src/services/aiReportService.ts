import api from './apiClient';
import type { AIReport } from '../types/api';

export const aiReportService = {
    async getAll(): Promise<AIReport[]> {
        return api.get<AIReport[]>('/aireports');
    },

    async getById(id: number): Promise<AIReport> {
        return api.get<AIReport>(`/aireports/${id}`);
    },
};

export default aiReportService;
