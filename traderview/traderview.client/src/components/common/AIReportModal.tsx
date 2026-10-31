import type { AIReport } from '../../types/api';
import './AIReportModal.css';

interface AIReportModalProps {
    isOpen: boolean;
    onClose: () => void;
    report: AIReport | null;
}

function AIReportModal({ isOpen, onClose, report }: AIReportModalProps) {
    if (!isOpen || !report) {
        return null;
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
                second: '2-digit',
            });
        } catch {
            return dateString;
        }
    };

    return (
        <div className="modal-overlay" onClick={onClose}>
            <div className="modal-content ai-report-modal" onClick={(e) => e.stopPropagation()}>
                <div className="modal-header">
                    <h2 className="modal-title">AI Report</h2>
                    <button
                        className="modal-close-button"
                        onClick={onClose}
                        aria-label="Close modal"
                    >
                        ×
                    </button>
                </div>
                <div className="modal-body ai-report-modal-body">
                    <div className="ai-report-meta">
                        <div className="ai-report-date">
                            <span className="label">Created:</span>
                            <span className="value">{formatDate(report.createdAt)}</span>
                        </div>
                        <div className="ai-report-id">
                            <span className="label">Report ID:</span>
                            <span className="value">{report.id}</span>
                        </div>
                    </div>
                    <div className="ai-report-content">
                        <h3>Report Details</h3>
                        <div className="ai-report-text">
                            {report.reportDetail}
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}

export default AIReportModal;
