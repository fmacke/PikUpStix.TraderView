import type { Trade } from '../../types/api';
import TradeList from '../features/trade/TradeList';
import TradeRollerList from '../features/trade/TradeRollerList';
import './TradeListerCard.css';

interface TradeListerCardProps {
    trades: Trade[];
    selectedTrade: Trade | null;
    onTradeSelect: (trade: Trade) => void;
    useCarousel: boolean;
    onToggleCarousel: (value: boolean) => void;
}

const TradeListerCard: React.FC<TradeListerCardProps> = ({
    trades,
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
                    trades={trades}
                    // use a composite key of positionId and entryDate so items are uniquely identified
                    selectedPositionId={selectedTrade ? `${selectedTrade.positionId}-${selectedTrade.entryDate}` : null}
                    onTradeSelect={onTradeSelect}
                />
            ) : (
                <TradeList
                    trades={trades}
                    selectedPositionId={selectedTrade ? `${selectedTrade.positionId}-${selectedTrade.entryDate}` : null}
                    onTradeSelect={onTradeSelect}
                />
            )}
        </div>
    );
};

export default TradeListerCard;
