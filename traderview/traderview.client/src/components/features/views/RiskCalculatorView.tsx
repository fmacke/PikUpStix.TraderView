import React from 'react';
import CurrentPerformanceCard from '../../cards/CurrentPerformanceCard';
import DesiredPerformanceCard from '../../cards/DesiredPerformanceCard';
import TradeCalculatorCard from '../../cards/TradeCalculatorCard';

export const RiskCalculatorView: React.FC = () => {
    return (
        <div style={{ padding: '24px', maxWidth: '95%', margin: '0 auto' }}>
            <h1 style={{ fontSize: '30px', fontWeight: 'bold', marginBottom: '24px' }}>Trading & Risk Dashboard</h1>

            {/* Main Layout Container forcing side-by-side via Flexbox */}
            <div style={{ display: 'flex', flexDirection: 'row', gap: '24px', alignItems: 'flex-start', width: '100%' }}>

                {/* Left Column: Trade Calculator (Takes ~58% width) */}
                <div style={{ flex: '7', minWidth: '0' }}>
                    <TradeCalculatorCard />
                </div>

                {/* Right Column: Performance Cards & Forecast Stack (Takes ~42% width) */}
                <div style={{ flex: '5', display: 'flex', flexDirection: 'column', gap: '24px', minWidth: '0' }}>
                    <CurrentPerformanceCard />
                    <DesiredPerformanceCard />
                </div>

            </div>
        </div>
    );
};

export default RiskCalculatorView;
