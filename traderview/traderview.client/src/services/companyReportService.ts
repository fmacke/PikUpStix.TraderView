import { api } from './apiClient';
import type { CompanyChecklistReportDto } from '../types/companyReport';

class CompanyReportService {
    /**
     * Get company checklist report for a specific symbol
     */
    async getChecklistReport(symbol: string): Promise<CompanyChecklistReportDto> {
        try {
            console.log('CompanyReportService: Fetching report for symbol:', symbol);
            const data = await api.get<CompanyChecklistReportDto>(
                `/reports/company-checklist/${encodeURIComponent(symbol)}`
            );
            console.log('CompanyReportService: Data received:', data);

            // Ensure response data exists
            if (!data) {
                throw new Error('No data returned from API');
            }

            return data;
        } catch (error) {
            console.error('CompanyReportService: Error fetching report:', error);
            throw error;
        }
    }
}

export default new CompanyReportService();
