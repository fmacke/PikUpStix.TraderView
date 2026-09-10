import React, { useEffect, useState, useCallback } from 'react';
import { apiService } from '../services/apiService';
import { type RiskMatrixCalculationResultDto } from '../types/api.ts';

export const DesiredPerformanceCard: React.FC = () => {
    const [portfolioSize, setPortfolioSize] = useState<number>(100000);
    const [positionSizePercent, setPositionSizePercent] = useState<number>(2.0);
    const [desiredReturnPercent, setDesiredReturnPercent] = useState<number>(20.0);

    const [data, setData] = useState<RiskMatrixCalculationResultDto | null>(null);
    const [loading, setLoading] = useState<boolean>(false);
    const [error, setError] = useState<string | null>(null);

    const fetchDesiredPerformance = useCallback(async () => {
        try {
            setLoading(true);
            const result = await apiService.getDesiredPerformance(
                portfolioSize,
                positionSizePercent,
                desiredReturnPercent
            );
            setData(result);
            setError(null);
        } catch (err: any) {
            setError(err.response?.data?.message || 'Error fetching desired performance data.');
        } finally {
            setLoading(false);
        }
    }, [portfolioSize, positionSizePercent, desiredReturnPercent]);

    useEffect(() => {
        fetchDesiredPerformance();
    }, [fetchDesiredPerformance]);

    return (
        <div className="dashboard-card">
            <div className="flex justify-between items-center">
                <h3>Desired Performance & Risk</h3>
                {loading && <span className="text-xs text-blue-600 animate-pulse">Calculating...</span>}
            </div>

            {/* Compact 3-Column Input Row */}
            <div className="grid grid-cols-3 gap-2">
                <div>
                    <label className="block text-[11px] font-medium text-gray-600">Portfolio</label>
                    <input
                        type="number"
                        value={portfolioSize}
                        onChange={(e) => setPortfolioSize(Number(e.target.value))}
                        step="any"
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm p-1.5 text-xs bg-gray-50 focus:ring-blue-500 focus:border-blue-500 font-medium"
                    />
                </div>
                <div>
                    <label className="block text-[11px] font-medium text-gray-600">Position %</label>
                    <input
                        type="number"
                        value={positionSizePercent}
                        onChange={(e) => setPositionSizePercent(Number(e.target.value))}
                        step="0.1"
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm p-1.5 text-xs bg-gray-50 focus:ring-blue-500 focus:border-blue-500 font-medium"
                    />
                </div>
                <div>
                    <label className="block text-[11px] font-medium text-gray-600">Return %</label>
                    <input
                        type="number"
                        value={desiredReturnPercent}
                        onChange={(e) => setDesiredReturnPercent(Number(e.target.value))}
                        step="0.1"
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm p-1.5 text-xs bg-gray-50 focus:ring-blue-500 focus:border-blue-500 font-medium"
                    />
                </div>
            </div>

            {error && <div className="text-red-500 text-xs">Error: {error}</div>}

            {/* Compact Output Metrics Grid */}
            {data && (
                <div className="grid grid-cols-2 gap-2 text-xs pt-2 border-t border-gray-100">
                    <div className="bg-gray-50 p-2 rounded-md">
                        <span className="block text-gray-500 text-[11px]">Exp. Net Return:</span>
                        <span className="text-sm font-semibold text-gray-900">{data.expectedNetReturnPercent.toFixed(2)}%</span>
                        <span className="block text-[10px] text-gray-400">({data.expectedNetReturnCurrency.toLocaleString()})</span>
                    </div>

                    <div className="bg-gray-50 p-2 rounded-md">
                        <span className="block text-gray-500 text-[11px]">Goal Target:</span>
                        <span className="text-sm font-semibold text-gray-900">{data.goalCurrency.toLocaleString()}</span>
                        <span className="block text-[10px] text-gray-400">T: {Math.ceil(data.numberOfTradesNeededToReachGoal)}</span>
                    </div>

                    <div className="bg-gray-50 p-2 rounded-md">
                        <span className="block text-gray-500 text-[11px]">G/L Ratio:</span>
                        <span className="text-sm font-semibold text-gray-900">{data.gainLossRatio.toFixed(2)}</span>
                    </div>

                    <div className="bg-gray-50 p-2 rounded-md">
                        <span className="block text-gray-500 text-[11px]">Optimal F:</span>
                        <span className="text-sm font-semibold text-gray-900">{data.otpimalF.toFixed(4)}</span>
                    </div>
                </div>
            )}
        </div>
    );
};

export default DesiredPerformanceCard;