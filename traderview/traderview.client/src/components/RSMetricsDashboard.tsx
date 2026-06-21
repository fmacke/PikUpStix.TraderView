import type { RSMetrics } from '../types/api';
import './RSMetricsDashboard.css';

interface RSMetricsDashboardProps {
    metrics: RSMetrics;
}

function RSMetricsDashboard({ metrics }: RSMetricsDashboardProps) {
    const hasInstitutionalData = metrics.institutionalCount !== null && metrics.institutionalPercent !== null;

    const formatPercent = (value: number) => {
        return `${value.toFixed(2)}%`;
    };

    const formatCount = (count: number | null, delta: number | null) => {
        if (count === null) return 'N/A';
        if (delta === null || delta === 0) return count.toString();
        const sign = delta >= 0 ? '+' : '';
        return `${count} (${sign}${delta})`;
    };

    return (
        <div className="rs-metrics-dashboard">
            <h3>CAN SLIM Metrics Dashboard</h3>
            <div className="metrics-grid">
                {/* Institutional Count */}
                <div className="metric-card">
                    <div className="metric-header">Inst. Count</div>
                    <div className={`metric-value ${metrics.isInstitutionalGrowing ? 'positive' : hasInstitutionalData ? 'negative' : 'neutral'}`}>
                        {formatCount(metrics.institutionalCount, metrics.institutionalCountDelta)}
                    </div>
                    {!hasInstitutionalData && (
                        <div className="metric-note">Data requires paid API</div>
                    )}
                </div>

                {/* Institutional Held % */}
                <div className="metric-card">
                    <div className="metric-header">Inst. Held %</div>
                    <div className={`metric-value ${metrics.institutionalPercent !== null && metrics.institutionalPercent >= 30 ? 'positive' : hasInstitutionalData ? 'warning' : 'neutral'}`}>
                        {metrics.institutionalPercent !== null ? formatPercent(metrics.institutionalPercent) : 'N/A'}
                    </div>
                    {!hasInstitutionalData && (
                        <div className="metric-note">Data requires paid API</div>
                    )}
                </div>

                {/* RS New High */}
                <div className="metric-card">
                    <div className="metric-header">RS New High</div>
                    <div className={`metric-value ${metrics.isRSNewHigh ? 'highlight' : 'neutral'}`}>
                        {metrics.isRSNewHigh ? 'YES' : 'NO'}
                    </div>
                </div>

                {/* Stage 2 Trend */}
                <div className="metric-card">
                    <div className="metric-header">Stage 2 Trend</div>
                    <div className={`metric-value ${metrics.isStage2Trend ? 'positive' : 'negative'}`}>
                        {metrics.isStage2Trend ? 'YES' : 'NO'}
                    </div>
                    <div className="metric-detail">
                        SMA50: {metrics.sma50} | SMA150: {metrics.sma150} | SMA200: {metrics.sma200}
                    </div>
                </div>

                {/* Distance from 52W High */}
                <div className="metric-card">
                    <div className="metric-header">Off 52W High</div>
                    <div className={`metric-value ${metrics.distanceFrom52WeekHigh < 15 ? 'positive' : 'neutral'}`}>
                        {formatPercent(metrics.distanceFrom52WeekHigh)}
                    </div>
                </div>
            </div>

            <div className="dashboard-legend">
                <div className="legend-section">
                    <strong>Stage 2 Trend:</strong> Price &gt; SMA50 &gt; SMA150 &gt; SMA200 (Bullish setup)
                </div>
                <div className="legend-section">
                    <strong>RS New High:</strong> Relative Strength at 50-bar high vs benchmark
                </div>
                <div className="legend-section">
                    <strong>Note:</strong> Institutional ownership data requires integration with financial data APIs (e.g., Financial Modeling Prep, Alpha Vantage Pro)
                </div>
            </div>
        </div>
    );
}

export default RSMetricsDashboard;
