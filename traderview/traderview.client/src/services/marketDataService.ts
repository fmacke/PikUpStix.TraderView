import { api } from './apiClient';

const getMarketRegime = async (): Promise<string> => {
    // Calls server endpoint that uses server DateTime.Now as the openDate
    return await api.get<string>('/marketdata/regime');
};

export default {
    getMarketRegime,
};
