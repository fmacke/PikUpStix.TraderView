import { useState } from 'react';
import type { CompanyChecklistReportDto } from '../../types/companyReport';
import { companyReportService } from '../../services';
import { useTheme } from '../../hooks/useTheme';
import { CompanyReportOutput } from './CompanyReportOutput';
import './cards-common.css';
import './CompanyReportCard.css';

function CompanyReportCard() {
    const [symbol, setSymbol] = useState<string>('');
    const [report, setReport] = useState<CompanyChecklistReportDto | null>(null);
    const [loading, setLoading] = useState<boolean>(false);
    const [error, setError] = useState<string | null>(null);
    const { theme } = useTheme();

    const handleSearch = async (e: React.FormEvent) => {
        e.preventDefault();
        console.log('Form submitted with symbol:', symbol);

        if (!symbol.trim()) {
            setError('Please enter a valid stock symbol');
            return;
        }

        try {
            setLoading(true);
            setError(null);
            console.log('Fetching report for symbol:', symbol.toUpperCase());
            const data = await companyReportService.getChecklistReport(symbol.toUpperCase());
            console.log('Report data received:', data);
            setReport(data);
            console.log('Report state updated, data:', data);
        } catch (err) {
            const errorMessage = err instanceof Error 
                ? err.message 
                : `Failed to fetch report for ${symbol}. Please check the symbol and try again.`;
            console.error('Error fetching report:', err, errorMessage);
            setError(errorMessage);
            setReport(null);
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className={`company-report-card theme-${theme}`}>
            <div className="company-report-header">
                <h2 className="card-heading">Company Metrics</h2>

                <form onSubmit={handleSearch} className="company-report-input-group">
                    <input
                        type="text"
                        className="company-report-input"
                        placeholder="Enter stock symbol (e.g., AAPL, MSFT)"
                        value={symbol}
                        onChange={(e) => setSymbol(e.target.value)}
                        disabled={loading}
                    />
                    <button
                        type="submit"
                        className="company-report-btn"
                        disabled={loading}
                    >
                        {loading ? 'Loading...' : 'Search'}
                    </button>
                </form>
            </div>

            {error && <div className="company-report-error">{error}</div>}

            {loading && !report && (
                <div className="company-report-loading">Loading company report...</div>
            )}

            <CompanyReportOutput report={report} />

            {!loading && !report && !error && (
                <div className="company-report-empty">
                    Enter a stock symbol above to view the company checklist report.
                </div>
            )}
        </div>
    );
}

export default CompanyReportCard;
