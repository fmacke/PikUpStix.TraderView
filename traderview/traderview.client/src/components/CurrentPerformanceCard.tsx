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
        <div className="bg-white shadow-md rounded-lg p-6 max-w-md mx-auto">
            <h3 className="text-lg font-semibold text-gray-800 mb-4">Position Review Risk Matrix</h3>

            <div className="grid grid-cols-2 gap-4 text-sm">
                <div className="bg-gray-50 p-3 rounded">
                    <span className="block text-gray-500">Win Rate:  </span>
                    <span className="text-base font-medium">{data.winRatePercentage}%</span>
                </div>
                <div className="bg-gray-50 p-3 rounded">
                    <span className="block text-gray-500">Loss Rate:  </span>
                    <span className="text-base font-medium">{data.lossRatePercentage}%</span>
                </div>

                <div className="bg-gray-50 p-3 rounded">
                    <span className="block text-gray-500">Gain / Loss Target:  </span>
                    <span className="text-base font-medium">+{data.gainPercentage}% / -{data.lossPercentage}%</span>
                </div>
                <div className="bg-gray-50 p-3 rounded">
                    <span className="block text-gray-500">Reward-to-Risk:  </span>
                    <span className="text-base font-medium">{data.rewardToRiskRatio.toFixed(2)}</span>
                </div>

                <div className="bg-gray-50 p-3 rounded">
                    <span className="block text-gray-500">Expected Return / Trade:  </span>
                    <span className={`text-base font-medium ${data.expectedReturnPerTrade >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                        {data.expectedReturnPerTrade.toFixed(2)}%
                    </span>
                </div>
                <div className="bg-gray-50 p-3 rounded">
                    <span className="block text-gray-500">Number of Trades:  </span>
                    <span className="text-base font-medium">{data.numberOfTrades}</span>
                </div>
            </div>

            <div className="mt-4 pt-4 border-t border-gray-100 grid grid-cols-2 gap-4">
                <div>
                    <span className="block text-xs text-gray-500">Simple ROI:  </span>
                    <span className={`text-lg font-bold ${data.simpleRoi >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                        {data.simpleRoi.toFixed(2)}%
                    </span>
                </div>
                <div>
                    <span className="block text-xs text-gray-500">Compounded ROI:  </span>
                    <span className={`text-lg font-bold ${data.compoundedRoi >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                        {data.compoundedRoi.toFixed(2)}%
                    </span>
                </div>
            </div>
        </div>
    );
};

export default CurrentPerformanceCard;