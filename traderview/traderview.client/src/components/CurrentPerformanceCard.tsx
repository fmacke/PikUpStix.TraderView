import React, { useEffect, useState } from 'react';
import { apiService } from '../services/apiService';
import { type CurrentPerformanceResult } from '../types/api';

export const CurrentPerformanceCard: React.FC = () => {
    const [data, setData] = useState<CurrentPerformanceResult | null>(null);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const fetchRiskMatrix = async () => {
            try {
                setLoading(true);
                const result = await apiService.getCurrentPerformance();
                setData(result);
            } catch (err: unknown) {
                if (err instanceof Error) {
                    setError(err.message);
                } else {
                    setError(String(err));          
                }
            } finally {
                setLoading(false);
            }
        };

        fetchRiskMatrix();
    }, []);

    if (loading) {
        return <div className="p-4 text-gray-500">Calculating risk matrix...</div>;
    }

    if (error) {
        return <div className="p-4 text-red-500">Error: {error}</div>;
    }

    if (!data) {
        return null;
    }

    return (
        <div className="bg-white shadow-md rounded-lg p-3 max-w-xs">
            <h3 className="text-sm font-semibold text-gray-800 mb-2">Position Review Risk Matrix</h3>

            <div className="grid grid-cols-2 gap-2 text-xs">
                <div className="bg-gray-50 p-2 rounded">
                    <span className="block text-gray-500 text-xs">Win Rate:</span>
                    <span className="text-sm font-medium">{data.winRatePercentage}%</span>
                </div>
                <div className="bg-gray-50 p-2 rounded">
                    <span className="block text-gray-500 text-xs">Loss Rate:</span>
                    <span className="text-sm font-medium">{data.lossRatePercentage}%</span>
                </div>

                <div className="bg-gray-50 p-2 rounded">
                    <span className="block text-gray-500 text-xs">Gain / Loss:</span>
                    <span className="text-sm font-medium">+{data.gainPercentage}% / -{data.lossPercentage}%</span>
                </div>
                <div className="bg-gray-50 p-2 rounded">
                    <span className="block text-gray-500 text-xs">Reward-to-Risk:</span>
                    <span className="text-sm font-medium">{data.rewardToRiskRatio.toFixed(2)}</span>
                </div>

                <div className="bg-gray-50 p-2 rounded col-span-2">
                    <span className="block text-gray-500 text-xs">Expected Return / Trade:</span>
                    <span className={`text-sm font-medium ${data.expectedReturnPerTrade >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                        {data.expectedReturnPerTrade.toFixed(2)}%
                    </span>
                </div>
                <div className="bg-gray-50 p-2 rounded">
                    <span className="block text-gray-500 text-xs">Num Trades:</span>
                    <span className="text-sm font-medium">{data.numberOfTrades}</span>
                </div>
            </div>

            <div className="mt-2 pt-2 border-t border-gray-100 grid grid-cols-2 gap-2">
                <div>
                    <span className="block text-xs text-gray-500">Simple ROI:</span>
                    <span className={`text-base font-bold ${data.simpleRoi >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                        {data.simpleRoi.toFixed(2)}%
                    </span>
                </div>
                <div>
                    <span className="block text-xs text-gray-500">Compound ROI:</span>
                    <span className={`text-base font-bold ${data.compoundedRoi >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                        {data.compoundedRoi.toFixed(2)}%
                    </span>
                </div>
            </div>
        </div>
    );
};

export default CurrentPerformanceCard;