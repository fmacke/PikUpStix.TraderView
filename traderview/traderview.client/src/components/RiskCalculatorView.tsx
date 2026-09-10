import React from 'react';
import { CurrentPerformanceCard } from './CurrentPerformanceCard';
import { DesiredPerformanceCard } from './DesiredPerformanceCard';
import { TradeCalculatorCard } from './TradeCalculatorCard';

export const RiskAndTradingDashboard: React.FC = () => {
    return (
        <div className="p-6 max-w-[95%] mx-auto space-y-6">
            <h1 className="text-3xl font-bold text-gray-900">Trading & Risk Dashboard</h1>

            {/* Main Layout Grid - Using xl breakpoint to enforce side-by-side on wide screens */}
            <div className="grid grid-cols-1 xl:grid-cols-12 gap-6 items-start">

                {/* Left Column: Performance Cards & Forecast Stack (5 Columns) */}
                <div className="xl:col-span-5 space-y-6">
                    <div className="bg-white shadow-md rounded-xl p-4">
                        <CurrentPerformanceCard />
                    </div>
                    <div className="bg-white shadow-md rounded-xl p-4">
                        <DesiredPerformanceCard />
                    </div>
                </div>

                {/* Right Column: Trade Calculator (7 Columns) */}
                <div className="xl:col-span-7">
                    <div className="bg-white shadow-md rounded-xl p-4">
                        <TradeCalculatorCard />
                    </div>
                </div>

            </div>
        </div>
    );
};

export default RiskAndTradingDashboard;