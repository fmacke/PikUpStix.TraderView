import React, { useEffect, useState } from 'react';
import CurrentPerformanceCard from '../../cards/CurrentPerformanceCard';
import DesiredPerformanceCard from '../../cards/DesiredPerformanceCard';
import TradeCalculatorCard from '../../cards/TradeCalculatorCard';
import AssetValueChart from '../charts/AssetValueChart';
import { apiService } from '../../../services/apiService';
import type { AssetValueChartData } from '../../../types/api';

export const RiskCalculatorView: React.FC = () => {
    const [chartData, setChartData] = useState<AssetValueChartData[]>([]);
    const [isLoadingChart, setIsLoadingChart] = useState(false);
    const [chartError, setChartError] = useState<string | null>(null);

    useEffect(() => {
        const fetchAssetValueData = async () => {
            setIsLoadingChart(true);
            setChartError(null);
            try {
                // Get data for the last 90 days
                const endDate = new Date();
                const startDate = new Date();
                startDate.setDate(startDate.getDate() - 90);

                const data = await apiService.getAssetValueOverTime(startDate, endDate);
                setChartData(data);
            } catch (error) {
                console.error('Failed to load asset value data:', error);
                setChartError(error instanceof Error ? error.message : 'Failed to load chart data');
            } finally {
                setIsLoadingChart(false);
            }
        };

        fetchAssetValueData();
    }, []);

    return (
        <div style={{ padding: '24px', maxWidth: '95%', margin: '0 auto' }}>
            <h1 style={{ fontSize: '30px', fontWeight: 'bold', marginBottom: '24px' }}>Trading & Risk Dashboard</h1>

            {/* Asset Value Over Time Chart */}
            {isLoadingChart && (
                <div style={{ padding: '20px', marginBottom: '24px', textAlign: 'center', color: '#999' }}>
                    Loading asset value chart...
                </div>
            )}
            {chartError && (
                <div style={{ padding: '20px', marginBottom: '24px', backgroundColor: '#ffebee', border: '1px solid #ef5350', borderRadius: '4px', color: '#c62828' }}>
                    Error loading chart: {chartError}
                </div>
            )}
            {!isLoadingChart && chartData.length > 0 && (
                <AssetValueChart data={chartData} title="Portfolio Asset Value (90 Days)" showLongShort={true} />
            )}

            {/* Main Layout Container forcing side-by-side via Flexbox */}
            <div style={{ display: 'flex', flexDirection: 'row', gap: '24px', alignItems: 'flex-start', width: '100%', marginTop: '24px' }}>

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
