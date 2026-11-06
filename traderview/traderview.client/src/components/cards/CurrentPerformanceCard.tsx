import React, { useEffect, useState } from 'react';
import { riskService } from '../../services';
import { type CurrentPerformanceResult } from '../../types/api';

export const CurrentPerformanceCard: React.FC = () => {
    const [data, setData] = useState<CurrentPerformanceResult | null>(null);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const fetchRiskMatrix = async () => {
            try {
                setLoading(true);
                const result = await riskService.getCurrentPerformance();
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
        return <div className="p-4 text-gray-500 bg-white shadow-md rounded-xl">Calculating risk matrix...</div>;
    }

    if (error) {
        return <div className="p-4 text-red-500 bg-white shadow-md rounded-xl">Error: {error}</div>;
    }

    if (!data) {
        return null;
    }

    return (
        <div className="dashboard-card">
            <h3>Position Review Risk Matrix</h3>

            <div className="mt-3 pt-3 border-t border-gray-100 grid grid-cols-2 gap-2">
                <div>
                    <span className="block text-xs text-gray-500">Win Rate:</span>
                    <span className="text-sm font-medium">{data.winRatePercentage}%</span>
                </div>
                <div>
                    <span className="block text-gray-500 text-xs">Loss Rate:</span>
                    <span className="text-sm font-medium">{data.lossRatePercentage}%</span>
                </div>

                <div>
                    <span className="block text-gray-500 text-xs">Gain / Loss:</span>
                    <span className={`text-sm font-medium ${data.gainPercentage >= 0 ? 'text-green-600' : 'text-red-600'}`}>{data.gainPercentage}%</span> / <span className={`text-sm font-medium ${data.lossPercentage >= 0 ? 'text-green-600' : 'text-red-600'}`}>{data.lossPercentage}%</span>
                </div>
                <div>
                    <span className="block text-gray-500 text-xs">Reward-to-Risk:</span>
                    <span className="text-sm font-medium">{data.rewardToRiskRatio.toFixed(2)}</span>
                </div>

                <div>
                    <span className="block text-gray-500 text-xs">Expected Return / Trade:</span>
                    <span className={`text-sm font-medium ${data.expectedReturnPerTrade >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                        {data.expectedReturnPerTrade.toFixed(2)}%
                    </span>
                </div>
                <div>
                    <span className="block text-gray-500 text-xs">Num Trades:</span>
                    <span className="text-sm font-medium">{data.numberOfTrades}</span>
                </div>
            </div>

            <div className="mt-3 pt-3 border-t border-gray-100 grid grid-cols-2 gap-2">
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
            <div className="mt-3 pt-3 border-t border-gray-100 grid grid-cols-3 gap-2">
                <div>
                    <span className="block text-xs text-gray-500">Average Win:</span>
                    <span className={`text-base font-bold ${data.averageWin >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                        {data.averageWin.toFixed(2)}
                    </span>
                </div>
                <div>
                    <span className="block text-xs text-gray-500">Largest Win:</span>
                    <span className={`text-base font-bold text-green-600`}>
                        {data.largestWin.toFixed(2)}</span>
                    <span className="text-xs"> (PosId: {data.largestWinPositionId})</span>
                </div>
                 <div>
                    <span className="block text-xs text-gray-500">Avg Days Win Held:</span>
                    <span className={`text-base font-bold ${data.averageDaysHeldGains >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                        {data.averageDaysHeldGains.toFixed(2)}
                    </span>
                </div>
                
            </div>
            <div className="mt-3 pt-3 border-t border-gray-100 grid grid-cols-3 gap-2">
                <div>
                    <span className="block text-xs text-gray-500">Average Loss:</span>
                    <span className={`text-base font-bold text-red-600`}>
                        {data.averageLoss.toFixed(2)}
                    </span>
                </div>                
                <div>
                    <span className="block text-xs text-gray-500">Largest Loss:</span>
                    <span className={`text-base font-bold text-red-600`}>
                        {data.largestLoss.toFixed(2)} 
                    </span>
                    <span className="text-xs"> (PosId: {data.largestLossPositionId})</span>
                </div>
                <div>
                    <span className="block text-xs text-gray-500">Avg Days Loss Held:</span>
                    <span className={`text-base font-bold text-red-600`}>
                        {data.averageDaysHeldLosses.toFixed(2)}
                    </span>
                </div>
            </div>
        </div>
    );
};

export default CurrentPerformanceCard;
