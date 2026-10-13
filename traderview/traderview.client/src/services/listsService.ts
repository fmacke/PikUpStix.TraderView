import api from './apiClient';
import type { ListItem } from '../types/api';

export const listsService = {
    async getListItems(category: string): Promise<ListItem[]> {
        return api.get<ListItem[]>(`/lists/${category}`);
    },
};

export default listsService;
