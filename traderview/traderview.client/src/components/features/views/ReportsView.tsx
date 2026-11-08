import AIReportCard from '../../cards/AIReportCard';
import PositionReportCard from '../../cards/PositionReportCard';
import CompanyReportCard from '../../cards/CompanyReportCard';
import { useTheme } from '../../../hooks/useTheme';
import './ReportsView.css';


export default function ReportsView() {
    const { theme } = useTheme();

    return (
        <div className={`reports-view-container theme-${theme}`}>
            <h1>Reports</h1>
            <div className="reports-layout-wrapper">
                {/* Left Column - Company Report Card (25% width) */}
                <div className="reports-left-column">
                    <CompanyReportCard />
                </div>

                {/* Right Column - Position Report Card (left) and AI Report Card (right) */}
                <div className="reports-right-column">
                    <PositionReportCard />
                    <AIReportCard />
                </div>
            </div>
        </div>
    );
}
