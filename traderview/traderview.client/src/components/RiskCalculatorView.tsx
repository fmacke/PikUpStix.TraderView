import React from 'react';
import CurrentPerformanceCard from './CurrentPerformanceCard';
import DesiredPerformanceView from './DesiredPerformanceCard';

export const RiskCalculator: React.FC = () => {
    return (
        <div className="container mx-auto p-6">
            <h1 className="text-2xl font-bold mb-6">Risk & Performance Calculator</h1>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div>
                    <CurrentPerformanceCard />
                </div>
                <div>
                    <DesiredPerformanceView />
                </div>
            </div>
        </div>
    );
};

export default RiskCalculator;