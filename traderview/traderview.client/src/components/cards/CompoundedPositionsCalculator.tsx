import { useEffect, useState } from 'react';
import { apiService } from '../../services/apiService';
import type { TradeCalculationRequest, TradeCalculationResponse, CompoundPositions, TradeCalculationCompoundedPosition } from '../../types/api';

interface Props {
    request: TradeCalculationRequest;
    trigger: TradeCalculationResponse | null;
}

const SmallStat: React.FC<{ label: string; value: string | number }> = ({ label, value }) => (
    <div className="mb-2">
        <div className="text-xs text-gray-600">{label}</div>
        <div className="mt-1 p-2 bg-green-200 rounded-md font-semibold text-gray-800">{value}</div>
    </div>
);

const ColumnBox: React.FC<{ title: string; pos: TradeCalculationCompoundedPosition }> = ({ title, pos }) => (
    <div className="p-3 border rounded bg-white shadow-sm">
        <div className="font-bold mb-2 text-sm">{title}</div>
        <SmallStat label="Trade Date" value={pos.tradeDate} />
        <SmallStat label="Instrument" value={pos.instrument} />
        <SmallStat label="Exchange Rate" value={pos.exchangeRate.toFixed(4)} />
        <SmallStat label="Trading Capital" value={`£${pos.tradingCapital.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`} />
        <SmallStat label="Risk Per Position" value={`${pos.riskPerPositionPercentage.toFixed(2)}%`} />
        <SmallStat label="Stop Loss On Position" value={`${pos.stopLossOnPositionPercentage.toFixed(2)}%`} />

        <SmallStat label="Position Size (USD)" value={`$${pos.positionSizeUsd.toFixed(2)}`} />
        <SmallStat label="Position Size (GBP)" value={`£${pos.positionSizeGbp.toFixed(2)}`} />
        <SmallStat label="Position Risk (USD)" value={`$${pos.positionRiskUsd.toFixed(2)}`} />
        <SmallStat label="Position Risk (GBP)" value={`£${pos.positionRiskGbp.toFixed(2)}`} />
        <SmallStat label="Account Risk" value={`${pos.accountRiskPercentage.toFixed(2)}%`} />

        <SmallStat label="Buy Price (USD)" value={`$${pos.buyPriceUsd.toFixed(2)}`} />
        <SmallStat label="Buy Price (GBP)" value={`£${pos.buyPriceGbp.toFixed(2)}`} />
        <SmallStat label="Shares" value={pos.shares.toFixed(2)} />
        <SmallStat label="Total Shares" value={pos.totalShares.toFixed(2)} />
        <SmallStat label="Avg Share Price (USD)" value={`$${pos.averageSharePriceUsd.toFixed(2)}`} />

        <SmallStat label="Stop Loss At (USD)" value={`$${pos.stopLossAtUsd.toFixed(2)}`} />
        <SmallStat label="Profit/Loss Target" value={`${pos.profitLossTargetPercentage.toFixed(2)}%`} />
        <SmallStat label="Profit Target (USD)" value={`$${pos.profitTargetUsd.toFixed(2)}`} />
        <SmallStat label="Take Profit / Pyramid At (USD)" value={`$${pos.takeProfitOrPyramidAtUsd.toFixed(2)}`} />
        <SmallStat label="Target Share Price (USD)" value={`$${pos.targetSharePriceUsd.toFixed(2)}`} />
        <SmallStat label="Target Share Price (%)" value={`${pos.targetSharePricePercentage.toFixed(2)}%`} />

        <SmallStat label="Win (USD)" value={`$${pos.winUsd.toFixed(2)}`} />
        <SmallStat label="Loss (USD)" value={`$${pos.lossUsd.toFixed(2)}`} />
        <SmallStat label="Win/Loss Ratio (%)" value={`${pos.winLossRatioPercentage.toFixed(2)}%`} />
    </div>
);

const CompoundedPositionsCalculator: React.FC<Props> = ({ request, trigger }) => {
    const [compound, setCompound] = useState<CompoundPositions | null>(null);
    const [loading, setLoading] = useState(false);

    useEffect(() => {
        // Recalculate compounds whenever the parent calculation result changes (trigger)
        const run = async () => {
            if (!request) return;
            setLoading(true);
            try {
                const data = await apiService.calculateCompoundPositions(request);
                setCompound(data as CompoundPositions);
            } catch (err) {
                console.error('Failed to calculate compound positions', err);
                setCompound(null);
            } finally {
                setLoading(false);
            }
        };

        // only run when trigger changes (parent result updated)
        run();
    }, [trigger]);

    if (loading) return <div className="p-4 text-sm">Updating compounded positions...</div>;
    if (!compound) return null;

    return (
        <div className="mt-4 grid grid-cols-1 md:grid-cols-3 gap-4 p-4 border rounded-lg bg-gray-50 text-xs">
            <ColumnBox title="TRADE 1 - 1/4 Position" pos={compound.quarterPosition} />
            <ColumnBox title="TRADE 2 - 1/2 Position" pos={compound.halfPosition} />
            <ColumnBox title="TRADE 3 - FULL Position" pos={compound.fullPosition} />
        </div>
    );
};

export default CompoundedPositionsCalculator;
