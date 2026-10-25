import type { Position } from '../../types/api';
import TradeList from '../features/trade/TradeList';
import TradeRollerList from '../features/trade/TradeRollerList';
import './TradeListerCard.css';

interface TradeListerCardProps {
    positions: Position[];
    selectedTrade: Position | null;
    onTradeSelect: (trade: Position) => void;
    useCarousel: boolean;
    onToggleCarousel: (value: boolean) => void;
}

const TradeListerCard: React.FC<TradeListerCardProps> = ({
    positions,
    selectedTrade,
    onTradeSelect,
    useCarousel,
    onToggleCarousel,
}) => {
    return (
        <div className="trade-lister-card">
            <button
                onClick={() => onToggleCarousel(!useCarousel)}
                className={`trade-lister-toggle ${useCarousel ? 'active' : ''}`}
                title="Toggle carousel trade view"
            >
                {useCarousel ? 'Carousel View' : 'Full List'}
            </button>
            {useCarousel ? (
                <TradeRollerList
                    trades={positions}
                    // use a composite key of positionId and entryDate so items are uniquely identified
                    selectedPositionId={selectedTrade ? `${selectedTrade.positionId}-${selectedTrade.entryDate}` : null}
                    onTradeSelect={onTradeSelect}
                />
            ) : (
                <TradeList
                    trades={positions}
                    selectedPositionId={selectedTrade ? `${selectedTrade.positionId}-${selectedTrade.entryDate}` : null}
                    onTradeSelect={onTradeSelect}
                />
            )}
        </div>
    );
};

export default TradeListerCard;
