import { useEffect, useState } from 'react';
import type { AIReport } from '../../types/api';
import { aiReportService } from '../../services';
import AIReportsList from '../common/AIReportsList';
import AIReportModal from '../common/AIReportModal';
import './AIReportCard.css';

function AIReportCard() {
    const [reports, setReports] = useState<AIReport[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const [selectedReport, setSelectedReport] = useState<AIReport | null>(null);
    const [isModalOpen, setIsModalOpen] = useState<boolean>(false);

    // Fetch all AI reports on component mount
    useEffect(() => {
        const fetchReports = async () => {
            try {
                setLoading(true);
                const data = await aiReportService.getAll();
                // Sort by createdAt descending (most recent first)
                const sortedReports = data.sort(
                    (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
                );
                setReports(sortedReports);
            } catch (error) {
                console.error('Error fetching AI reports:', error);
            } finally {
                setLoading(false);
            }
        };

        fetchReports();
    }, []);

    const handleReportSelect = (report: AIReport) => {
        setSelectedReport(report);
        setIsModalOpen(true);
    };

    const handleCloseModal = () => {
        setIsModalOpen(false);
        // Keep selectedReport state for potential future use
    };

    const mostRecentReport = reports.length > 0 ? reports[0] : null;
    const otherReports = reports.slice(1);

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

    if (loading && reports.length === 0) {
        return (
            <div className="ai-report-card">
                <h2 className="ai-report-card-title">AI Reports</h2>
                <p className="ai-report-loading">Loading reports...</p>
            </div>
        );
    }

    if (reports.length === 0) {
        return (
            <div className="ai-report-card">
                <h2 className="ai-report-card-title">AI Reports</h2>
                <p className="ai-report-empty">No AI reports available.</p>
            </div>
        );
    }

    return (
        <div className="ai-report-card">
            <h2 className="ai-report-card-title">AI Reports</h2>

            {/* Most Recent Report Section */}
            {mostRecentReport && (
                <div className="ai-report-recent-section">
                    <h3 className="ai-report-recent-title">Most Recent Report</h3>
                    <div className="ai-report-recent-container">
                        <div className="ai-report-recent-meta">
                            <span className="ai-report-recent-date">
                                {formatDate(mostRecentReport.createdAt)}
                            </span>
                        </div>
                        <div className="ai-report-recent-content">
                            {mostRecentReport.reportDetail}
                        </div>
                    </div>
                </div>
            )}

            {/* Other Reports Section */}
            {otherReports.length > 0 && (
                <div className="ai-report-list-section">
                    <h3 className="ai-report-list-title">Other Reports</h3>
                    <p className="ai-report-list-hint">Click a report to view full details</p>
                    <AIReportsList
                        reports={otherReports}
                        loading={loading}
                        onReportSelect={handleReportSelect}
                    />
                </div>
            )}

            {/* Modal for viewing selected report */}
            <AIReportModal
                isOpen={isModalOpen}
                onClose={handleCloseModal}
                report={selectedReport}
            />
        </div>
    );
}

export default AIReportCard;
