import { useEffect, useState } from 'react';
import { riskService } from '../../services';
import type { TradeCalculationRequest, TradeCalculationResponse, CompoundPositions, TradeCalculationCompoundedPosition } from '../../types/api';
import './CompoundedPositionsCalculator.css';

interface Props {
    request: TradeCalculationRequest;
    trigger: TradeCalculationResponse | null;
}

const SmallStat: React.FC<{ label: string; value: string | number; highlight?: 'green' | 'yellow' }> = ({ label, value, highlight = 'green' }) => {
    const highlightClass = highlight === 'yellow' ? 'highlight-yellow' : 'highlight-green';
    return (
        <div className="compounded-stat">
            <div className="compounded-stat-label">{label}</div>
            <div className={`compounded-stat-value ${highlightClass}`}>{value}</div>
        </div>
    );
};

const ColumnBox: React.FC<{ title: string; pos: TradeCalculationCompoundedPosition }> = ({ title, pos }) => (
    <div className="compounded-position-column">
        <div className="compounded-position-title">{title}</div>

        <SmallStat label="Buy Price (USD)" value={`$${(pos.buyPriceUsd ?? 0).toFixed(2)}`} highlight="yellow" />
        <SmallStat label="Shares" value={(pos.shares ?? 0).toFixed(2)} highlight="yellow" />
        <SmallStat label="Stop Loss At (USD)" value={`$${(pos.stopLossAtUsd ?? 0).toFixed(2)}`} highlight="yellow" />
        <div className="compounded-stat-row">
            <div className="compounded-stat-row-flex">
                <SmallStat label="Target Share Price (USD)" value={`$${(pos.targetSharePriceUsd ?? 0).toFixed(2)}`} highlight="yellow" />
            </div>
            <div className="compounded-stat-row-small">
                <SmallStat label="%" value={`${(pos.targetSharePricePercentage ?? 0).toFixed(2)}%`} highlight="yellow" />
            </div>
        </div>
        <SmallStat label="Total Shares" value={(pos.totalShares ?? 0).toFixed(2)} />
        <SmallStat label="Avg Share Price (USD)" value={`$${(pos.averageSharePriceUsd ?? 0).toFixed(2)}`} />

        {/* Header block */}
        <SmallStat label="Risk Per Position" value={`${(pos.riskPerPositionPercentage ?? 0).toFixed(2)}%`} />

        {/* Position sizes */}
        <SmallStat label="Position Size" value={`$${(pos.positionSizeUsd ?? 0).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}` + ' / ' + `£${(pos.positionSizeGbp ?? 0).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`} />
        <SmallStat label="Cum. Size (GBP)" value={`£${(pos.totalSizeGbp ?? 0).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`} />
        <SmallStat label="Account Risk" value={`${(pos.accountRiskPercentage ?? 0).toFixed(2)}%`} />

        {/* Price and shares */}
        
        

        {/* Stop / targets */}
        
        <SmallStat label="Profit Target (USD)" value={`$${(pos.profitTargetUsd ?? 0).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`} />
        {/*<SmallStat label="Take Profit / Pyramid At (USD)" value={`$${(pos.takeProfitOrPyramidAtUsd ?? 0).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`} /> */}
        

        {/* Outcomes */}
        <SmallStat label="Win (USD)" value={`$${(pos.winUsd ?? 0).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })} / £${(pos.winGbp ?? 0).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 }) }`} />
      <SmallStat label="Loss (USD)" value={`$${(pos.lossUsd ?? 0).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })} / £${(pos.lossGbp ?? 0).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 }) }`} />
        <SmallStat label="Win/Loss Ratio (%)" value={`${(pos.winLossRatioPercentage ?? 0).toFixed(2)}%`} />
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
                const data = await riskService.calculateCompoundPositions(request);
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

    if (loading) return <div className="compounded-loading">Updating compounded positions...</div>;
    if (!compound) return null;

    return (
        <div className="compounded-positions-container">
            <ColumnBox title="TRADE 1 - 1/4 Position" pos={compound.quarterPosition} />
            <ColumnBox title="TRADE 2 - 1/2 Position" pos={compound.halfPosition} />
            <ColumnBox title="TRADE 3 - FULL Position" pos={compound.fullPosition} />
        </div>
    );
};

export default CompoundedPositionsCalculator;
