import type { AIReport } from '../../types/api';
import './AIReportsList.css';

interface AIReportsListProps {
    reports: AIReport[];
    loading: boolean;
    onReportSelect: (report: AIReport) => void;
}

function AIReportsList({ reports, loading, onReportSelect }: AIReportsListProps) {
    if (loading) {
        return <p className="ai-reports-loading">Loading AI reports...</p>;
    }

    if (reports.length === 0) {
        return <p className="ai-reports-empty">No AI reports available.</p>;
    }

    const formatDate = (dateString: string): string => {
        try {
            const date = new Date(dateString);
            return date.toLocaleDateString('en-UK', {
                year: 'numeric',
                month: '2-digit',
                day: '2-digit',
                hour: '2-digit',
                minute: '2-digit',
            });
        } catch {
            return dateString;
        }
    };

    const truncateText = (text: string, maxLength: number = 100): string => {
        return text.length > maxLength ? text.substring(0, maxLength) + '...' : text;
    };

    return (
        <div className="ai-reports-list-container">
            <div className="ai-reports-list">
                {reports.map((report) => (
                    <div
                        key={report.id}
                        className="ai-report-item"
                        onClick={() => onReportSelect(report)}
                        role="button"
                        tabIndex={0}
                        onKeyDown={(e) => {
                            if (e.key === 'Enter' || e.key === ' ') {
                                onReportSelect(report);
                            }
                        }}
                    >
                        <div className="ai-report-item-date">
                            {formatDate(report.createdAt)}
                        </div>
                        <div className="ai-report-item-preview">
                            {truncateText(report.reportDetail)}
                        </div>
                    </div>
                ))}
            </div>
        </div>
    );
}

export default AIReportsList;
