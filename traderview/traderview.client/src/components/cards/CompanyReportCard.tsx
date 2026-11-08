import { useState } from 'react';
import type { CompanyChecklistReportDto, RatingIndicator } from '../../types/companyReport';
import { companyReportService } from '../../services';
import { useTheme } from '../../hooks/useTheme';
import './cards-common.css';
import './CompanyReportCard.css';

interface MetricDisplay {
    label: string;
    value: string | number;
    rating?: 'green' | 'red' | 'yellow' | 'neutral';
}

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

    const getRating = (value: string | number | undefined, type: string): 'green' | 'red' | 'yellow' | 'neutral' => {
        if (value === undefined || value === null) return 'neutral';

        // Numeric ratings (1-99 scale, higher is better)
        if (type === 'numeric-high') {
            const numVal = Number(value);
            if (numVal >= 70) return 'green';
            if (numVal >= 50) return 'yellow';
            return 'red';
        }

        // Reverse numeric (lower is better)
        if (type === 'numeric-low') {
            const numVal = Number(value);
            if (numVal <= 20) return 'green';
            if (numVal <= 50) return 'yellow';
            return 'red';
        }

        // Percentage-based (higher is better)
        if (type === 'percentage-high') {
            const numVal = Number(value);
            if (numVal >= 30) return 'green';
            if (numVal >= 10) return 'yellow';
            return 'red';
        }

        // Letter grades (A is best)
        if (type === 'grade') {
            const str = String(value).toUpperCase();
            if (str === 'A' || str === 'A+') return 'green';
            if (str === 'B' || str === 'B+') return 'yellow';
            return 'red';
        }

        return 'neutral';
    };

    const formatValue = (value: string | number | undefined | null): string => {
        if (value === undefined || value === null) return 'N/A';
        if (typeof value === 'number') {
            // For percentages and decimals
            if (value < 1 && value !== 0) {
                return `${(value * 100).toFixed(2)}%`;
            }
            // For large numbers like volume
            if (value > 100000) {
                return `${(value / 1000000).toFixed(2)}M`;
            }
            // For decimals
            if (value % 1 !== 0) {
                return value.toFixed(2);
            }
            return value.toString();
        }
        return String(value);
    };

    const formatDate = (dateString: string | undefined | null): string => {
        if (!dateString) return 'N/A';
        try {
            const date = new Date(dateString);
            return date.toLocaleDateString('en-US', { year: 'numeric', month: '2-digit', day: '2-digit' });
        } catch {
            return dateString;
        }
    };

    const RatingBadge = ({ rating }: { rating: string | 'green' | 'red' | 'yellow' | 'neutral' | undefined }) => {
        if (!rating || rating === 'neutral') return null;
        return <span className={`company-report-rating-indicator ${rating}`}></span>;
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

            {report && (
                <div className="company-report-content">
                    {/* Metadata */}
                    <div className="company-report-meta">
                        <div>
                            <span className="company-report-company-name">{report.companyName}</span>
                            {' '}
                            <span>({report.symbol})</span>
                        </div>
                        <span>{formatDate(report.generatedAtUtc)}</span>
                    </div>

                    {/* Market & Industry Performance */}
                    <div className="company-report-section">
                        <h2 className="company-report-section-title">Market & Industry Performance</h2>
                        <div className="company-report-metrics">
                            <div className="company-report-metric-row">
                                <span className="company-report-metric-label">Composite Rating</span>
                                <span className="company-report-metric-value">
                                    {report.compositeRating}
                                    <RatingBadge rating={getRating(report.compositeRating, 'numeric-high')} />
                                </span>
                            </div>
                            <div className="company-report-metric-row">
                                <span className="company-report-metric-label">Stock Market Exposure</span>
                                <span className="company-report-metric-value">
                                    {report.marketExposureStatus}
                                    <RatingBadge rating={getRating(report.marketExposureStatus, 'neutral')} />
                                </span>
                            </div>
                            <div className="company-report-metric-row">
                                <span className="company-report-metric-label">Industry Group Rank (1 to 142)</span>
                                <span className="company-report-metric-value">
                                    {report.industryGroupRank}
                                    <RatingBadge rating={getRating(report.industryGroupRank, 'numeric-high')} />
                                </span>
                            </div>
                            <div className="company-report-metric-row">
                                <span className="company-report-metric-label">Group RS Rating</span>
                                <span className="company-report-metric-value">
                                    {report.groupRsRating}
                                    <RatingBadge rating={getRating(report.groupRsRating, 'grade')} />
                                </span>
                            </div>
                        </div>
                    </div>

                    {/* Fundamental Performance */}
                    <div className="company-report-section">
                        <h2 className="company-report-section-title">Fundamental Performance</h2>

                        {/* Current Earnings */}
                        <h3 className="company-report-section-subtitle">Current Earnings</h3>
                        <div className="company-report-metrics">
                            <div className="company-report-metric-row">
                                <span className="company-report-metric-label">EPS Due Date</span>
                                <span className="company-report-metric-value">
                                    {formatDate(report.epsDueDate)}
                                </span>
                            </div>
                            <div className="company-report-metric-row">
                                <span className="company-report-metric-label">EPS Rating</span>
                                <span className="company-report-metric-value">
                                    {report.epsRating}
                                    <RatingBadge rating={getRating(report.epsRating, 'numeric-high')} />
                                </span>
                            </div>
                            <div className="company-report-metric-row">
                                <span className="company-report-metric-label">EPS % Chg (Last Qtr)</span>
                                <span className="company-report-metric-value">
                                    {formatValue(report.lastQtrEpsGrowthYoY)}%
                                    <RatingBadge rating={getRating(report.lastQtrEpsGrowthYoY, 'percentage-high')} />
                                </span>
                            </div>
                            <div className="company-report-metric-row">
                                <span className="company-report-metric-label">Last 3 Qtrs Avg EPS Growth</span>
                                <span className="company-report-metric-value">
                                    {formatValue(report.last3QtrsAvgEpsGrowth)}%
                                    <RatingBadge rating={getRating(report.last3QtrsAvgEpsGrowth, 'percentage-high')} />
                                </span>
                            </div>
                            <div className="company-report-metric-row">
                                <span className="company-report-metric-label"># Qtrs of EPS Acceleration</span>
                                <span className="company-report-metric-value">
                                    {report.qtrsEpsAccelerationCount}
                                    <RatingBadge rating={getRating(report.qtrsEpsAccelerationCount, 'numeric-high')} />
                                </span>
                            </div>
                            <div className="company-report-metric-row">
                                <span className="company-report-metric-label">EPS Est % Chg (Current Qtr)</span>
                                <span className="company-report-metric-value">
                                    {formatValue(report.currentQtrEpsEstChg)}%
                                    <RatingBadge rating={getRating(report.currentQtrEpsEstChg, 'percentage-high')} />
                                </span>
                            </div>
                            <div className="company-report-metric-row">
                                <span className="company-report-metric-label">Estimate Revisions</span>
                                <span className="company-report-metric-value">
                                    {report.estimateRevisionsTrend}
                                    <RatingBadge rating={
                                        report.estimateRevisionsTrend === 'Up' ? 'green' :
                                        report.estimateRevisionsTrend === 'Down' ? 'red' : 'neutral'
                                    } />
                                </span>
                            </div>
                            <div className="company-report-metric-row">
                                <span className="company-report-metric-label">Last Quarter % Earnings Surprise</span>
                                <span className="company-report-metric-value">
                                    {formatValue(report.lastQtrEarningsSurprisePercent)}%
                                    <RatingBadge rating={getRating(report.lastQtrEarningsSurprisePercent, 'percentage-high')} />
                                </span>
                            </div>
                        </div>

                        {/* Annual Earnings */}
                        <h3 className="company-report-section-subtitle">Annual Earnings</h3>
                        <div className="company-report-metrics">
                            <div className="company-report-metric-row">
                                <span className="company-report-metric-label">3 Yr EPS Growth Rate</span>
                                <span className="company-report-metric-value">
                                    {formatValue(report.eps3YrCagr)}%
                                    <RatingBadge rating={getRating(report.eps3YrCagr, 'percentage-high')} />
                                </span>
                            </div>
                            <div className="company-report-metric-row">
                                <span className="company-report-metric-label">Consecutive Yrs of Annual EPS Growth</span>
                                <span className="company-report-metric-value">
                                    {report.consecutiveYrsAnnualEpsGrowth}
                                    <RatingBadge rating={getRating(report.consecutiveYrsAnnualEpsGrowth, 'numeric-high')} />
                                </span>
                            </div>
                            <div className="company-report-metric-row">
                                <span className="company-report-metric-label">EPS Est % Chg for Current Year</span>
                                <span className="company-report-metric-value">
                                    {formatValue(report.currentYearEpsEstChg)}%
                                    <RatingBadge rating={getRating(report.currentYearEpsEstChg, 'percentage-high')} />
                                </span>
                            </div>
                        </div>

                        {/* Sales, Margin, ROE */}
                        <h3 className="company-report-section-subtitle">Sales, Margin, ROE</h3>
                        <div className="company-report-metrics">
                            <div className="company-report-metric-row">
                                <span className="company-report-metric-label">SMR Rating</span>
                                <span className="company-report-metric-value">
                                    {report.smrRating}
                                    <RatingBadge rating={getRating(report.smrRating, 'grade')} />
                                </span>
                            </div>
                            <div className="company-report-metric-row">
                                <span className="company-report-metric-label">Sales % Chg (Last Qtr)</span>
                                <span className="company-report-metric-value">
                                    {formatValue(report.lastQtrSalesGrowthYoY)}%
                                    <RatingBadge rating={getRating(report.lastQtrSalesGrowthYoY, 'percentage-high')} />
                                </span>
                            </div>
                            <div className="company-report-metric-row">
                                <span className="company-report-metric-label">3 Yr Sales Growth Rate</span>
                                <span className="company-report-metric-value">
                                    {formatValue(report.sales3YrCagr)}%
                                    <RatingBadge rating={getRating(report.sales3YrCagr, 'percentage-high')} />
                                </span>
                            </div>
                            <div className="company-report-metric-row">
                                <span className="company-report-metric-label">Annual Pre-Tax Margin</span>
                                <span className="company-report-metric-value">
                                    {formatValue(report.annualPreTaxMarginPercent)}%
                                    <RatingBadge rating={getRating(report.annualPreTaxMarginPercent, 'percentage-high')} />
                                </span>
                            </div>
                            <div className="company-report-metric-row">
                                <span className="company-report-metric-label">Annual ROE</span>
                                <span className="company-report-metric-value">
                                    {formatValue(report.annualRoePercent)}%
                                    <RatingBadge rating={getRating(report.annualRoePercent, 'percentage-high')} />
                                </span>
                            </div>
                            <div className="company-report-metric-row">
                                <span className="company-report-metric-label">Debt/Equity Ratio</span>
                                <span className="company-report-metric-value">
                                    {formatValue(report.debtToEquityRatio)}%
                                    <RatingBadge rating={getRating(report.debtToEquityRatio, 'numeric-low')} />
                                </span>
                            </div>
                        </div>
                    </div>

                    {/* Technical Performance */}
                    <div className="company-report-section">
                        <h2 className="company-report-section-title">Technical Performance</h2>

                        {/* Price and Volume */}
                        <h3 className="company-report-section-subtitle">Price and Volume</h3>
                        <div className="company-report-metrics">
                            <div className="company-report-metric-row">
                                <span className="company-report-metric-label">Price</span>
                                <span className="company-report-metric-value">
                                    ${formatValue(report.price)}
                                </span>
                            </div>
                            <div className="company-report-metric-row">
                                <span className="company-report-metric-label">RS Rating</span>
                                <span className="company-report-metric-value">
                                    {report.relativeStrengthRating}
                                    <RatingBadge rating={getRating(report.relativeStrengthRating, 'numeric-high')} />
                                </span>
                            </div>
                            <div className="company-report-metric-row">
                                <span className="company-report-metric-label">% Off 52 Week High</span>
                                <span className="company-report-metric-value">
                                    {formatValue(report.percentOff52WeekHigh)}%
                                    <RatingBadge rating={getRating(report.percentOff52WeekHigh, 'numeric-low')} />
                                </span>
                            </div>
                            <div className="company-report-metric-row">
                                <span className="company-report-metric-label">Price vs. 50-Day Moving Average</span>
                                <span className="company-report-metric-value">
                                    {formatValue(report.priceVs50DaySmaPercent)}%
                                    <RatingBadge rating={getRating(report.priceVs50DaySmaPercent, 'percentage-high')} />
                                </span>
                            </div>
                            <div className="company-report-metric-row">
                                <span className="company-report-metric-label">50-Day Average Volume</span>
                                <span className="company-report-metric-value">
                                    {formatValue(report.volume50DayAverage)}
                                </span>
                            </div>
                        </div>

                        {/* Supply and Demand */}
                        <h3 className="company-report-section-subtitle">Supply and Demand</h3>
                        <div className="company-report-metrics">
                            <div className="company-report-metric-row">
                                <span className="company-report-metric-label">Market Capitalization</span>
                                <span className="company-report-metric-value">
                                    ${formatValue(report.marketCapBillions)}B
                                </span>
                            </div>
                            <div className="company-report-metric-row">
                                <span className="company-report-metric-label">Accumulation/Distribution Rating</span>
                                <span className="company-report-metric-value">
                                    {report.accumulationDistributionRating}
                                    <RatingBadge rating={getRating(report.accumulationDistributionRating, 'grade')} />
                                </span>
                            </div>
                            <div className="company-report-metric-row">
                                <span className="company-report-metric-label">Up/Down Volume</span>
                                <span className="company-report-metric-value">
                                    {formatValue(report.upDownVolumeRatio)}
                                    <RatingBadge rating={getRating(report.upDownVolumeRatio, 'percentage-high')} />
                                </span>
                            </div>
                            <div className="company-report-metric-row">
                                <span className="company-report-metric-label">% Change In Funds Owning Stock</span>
                                <span className="company-report-metric-value">
                                    {formatValue(report.fundOwnershipChangePercent)}%
                                    <RatingBadge rating={getRating(report.fundOwnershipChangePercent, 'percentage-high')} />
                                </span>
                            </div>
                            <div className="company-report-metric-row">
                                <span className="company-report-metric-label">Qtrs Of Increasing Fund Ownership</span>
                                <span className="company-report-metric-value">
                                    {report.qtrsIncreasingFundOwnership}
                                    <RatingBadge rating={getRating(report.qtrsIncreasingFundOwnership, 'numeric-high')} />
                                </span>
                            </div>
                        </div>
                    </div>
                </div>
            )}

            {!loading && !report && !error && (
                <div className="company-report-empty">
                    Enter a stock symbol above to view the company checklist report.
                </div>
            )}
        </div>
    );
}

export default CompanyReportCard;
