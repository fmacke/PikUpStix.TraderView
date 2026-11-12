import { useEffect, useState } from 'react';
import type { CompanyChecklistReportDto } from '../../types/companyReport';
import { companyReportService } from '../../services';
import { useTheme } from '../../hooks/useTheme';
import { getRating, formatValue, formatDate } from '../../utils/reportFormatters';
import './cards-common.css';
import './CompanyReportCard.css';

interface RatingBadgeProps {
    rating: string | 'green' | 'red' | 'yellow' | 'neutral' | undefined | null;
}

const RatingBadge = ({ rating }: RatingBadgeProps) => {
    if (!rating || rating === 'neutral') return null;
    return <span className={`company-report-rating-indicator ${rating}`}></span>;
};

interface CompanyReportOutputProps {
    report?: CompanyChecklistReportDto | null;
    instrumentCode?: string;
    loading?: boolean;
    error?: string | null;
}

export function CompanyReportOutput({
    report: initialReport,
    instrumentCode,
    loading: initialLoading = false,
    error: initialError = null,
}: CompanyReportOutputProps) {
    const [report, setReport] = useState<CompanyChecklistReportDto | null>(initialReport || null);
    const [loading, setLoading] = useState(initialLoading);
    const [error, setError] = useState<string | null>(initialError);
    const { theme } = useTheme();

    // Update report when initialReport prop changes
    useEffect(() => {
        if (initialReport) {
            setReport(initialReport);
        }
    }, [initialReport]);

    // Fetch report if instrumentCode is provided
    useEffect(() => {
        if (instrumentCode && !initialReport) {
            const fetchReport = async () => {
                try {
                    setLoading(true);
                    setError(null);
                    const data = await companyReportService.getChecklistReport(instrumentCode);
                    setReport(data);
                } catch (err) {
                    const errorMessage = err instanceof Error 
                        ? err.message 
                        : `Failed to fetch report for ${instrumentCode}. Please check the symbol and try again.`;
                    setError(errorMessage);
                    setReport(null);
                } finally {
                    setLoading(false);
                }
            };

            fetchReport();
        }
    }, [instrumentCode, initialReport]);

    if (loading) {
        return <div className="company-report-loading">Loading company report...</div>;
    }

    if (error) {
        return <div className="company-report-error">{error}</div>;
    }

    if (!report) {
        return null;
    }

    return (
        <div className={`company-report-output theme-${theme}`}>
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
                            <span className="company-report-metric-label">Estimate Revisions Trend</span>
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
                            <span className="company-report-metric-label">Relative Strength Rating</span>
                            <span className="company-report-metric-value">
                                {report.relativeStrengthRating}
                                <RatingBadge rating={getRating(report.relativeStrengthRating, 'numeric-high')} />
                            </span>
                        </div>
                        <div className="company-report-metric-row">
                            <span className="company-report-metric-label">% Off 52 Week High</span>
                            <span className="company-report-metric-value">
                                {formatValue(report.percentOff52WeekHigh)}%
                                <RatingBadge rating={getRating(report.percentOff52WeekHigh, 'percentage-high')} />
                            </span>
                        </div>
                        <div className="company-report-metric-row">
                            <span className="company-report-metric-label">Price vs 50 Day SMA %</span>
                            <span className="company-report-metric-value">
                                {formatValue(report.priceVs50DaySmaPercent)}%
                                <RatingBadge rating={getRating(report.priceVs50DaySmaPercent, 'percentage-high')} />
                            </span>
                        </div>
                        <div className="company-report-metric-row">
                            <span className="company-report-metric-label">Average Volume (50 Day)</span>
                            <span className="company-report-metric-value">
                                {formatValue(report.volume50DayAverage)}
                            </span>
                        </div>
                    </div>

                    {/* Supply and Demand */}
                    <h3 className="company-report-section-subtitle">Supply and Demand</h3>
                    <div className="company-report-metrics">
                        <div className="company-report-metric-row">
                            <span className="company-report-metric-label">Market Cap (Billions)</span>
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
                            <span className="company-report-metric-label">Up/Down Volume Ratio</span>
                            <span className="company-report-metric-value">
                                {formatValue(report.upDownVolumeRatio)}
                                <RatingBadge rating={getRating(report.upDownVolumeRatio, 'percentage-high')} />
                            </span>
                        </div>
                        <div className="company-report-metric-row">
                            <span className="company-report-metric-label">Fund Ownership Change %</span>
                            <span className="company-report-metric-value">
                                {formatValue(report.fundOwnershipChangePercent)}%
                                <RatingBadge rating={getRating(report.fundOwnershipChangePercent, 'percentage-high')} />
                            </span>
                        </div>
                        <div className="company-report-metric-row">
                            <span className="company-report-metric-label">Qtrs Increasing Fund Ownership</span>
                            <span className="company-report-metric-value">
                                {report.qtrsIncreasingFundOwnership}
                                <RatingBadge rating={getRating(report.qtrsIncreasingFundOwnership, 'numeric-high')} />
                            </span>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}
