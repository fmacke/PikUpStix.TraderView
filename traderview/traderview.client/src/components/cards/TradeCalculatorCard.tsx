import { useState, useEffect, useRef } from 'react';
import { apiService } from '../../services/apiService';
import type { TradeCalculationRequest, TradeCalculationResponse } from '../../types/api';
import type { ListItem } from '../../types/api';
import ListItemSelect from '../common/ListItemSelect';

export const TradeCalculatorCard: React.FC = () => {
    const [request, setRequest] = useState<TradeCalculationRequest>({
        tradeDate: new Date().toISOString().split('T')[0],
        instrument: 'TICKER-CODE',
        exchangeRate: 1.36,
        buyPrice: 520,
        tradingCapital: 100000,
        riskPerTrade: 5,
        maxExposure: 2.5,
        gainLossRatio: 200,
        calculationMode: 'LotSize',
        stopLossAtInput: 0,
    });

    const [currencyPair, setCurrencyPair] = useState<string>('GBP/USD');
    const [result, setResult] = useState<TradeCalculationResponse | null>(null);
    const [loading, setLoading] = useState<boolean>(false);
    const [error, setError] = useState<string | null>(null);
    const [isSubmitting] = useState(false);
    const [strategies, setStrategies] = useState<ListItem[]>([]);
    const [selectedStrategyId, setSelectedStrategyId] = useState<number | null>(null);
    const [isLoadingStrategies] = useState(false);

    // Validate that all required inputs have valid values
    const isValidRequest = (req: TradeCalculationRequest): boolean => {
        return !!(
            req.tradeDate &&
            req.tradeDate.length > 0 &&
            req.instrument &&
            req.instrument.trim().length > 0 &&
            req.exchangeRate > 0 &&
            req.buyPrice > 0 &&
            req.tradingCapital > 0 &&
            req.riskPerTrade > 0 &&
            req.maxExposure > 0 &&
            req.gainLossRatio > 0
        );
    };

    const calculateTrade = async () => {
        try {
            setLoading(true);
            setError(null);
            const data = await apiService.calculateTradePosition(request);
            setResult(data);
        } catch (error) {
            console.error(error);
            setError('Failed to calculate trade position. Please check your inputs or backend connection.');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        if (isValidRequest(request)) {
            // calling async setter that will update state; suppress linter about setState-in-effect for this intentional pattern
            // eslint-disable-next-line react-hooks/set-state-in-effect
            void calculateTrade();
        } else {        
            
            setResult(null);
            setError(null);
        }
    }, [request]);

    // Update exchange rate when currency pair changes
    // Track initialization to avoid firing calculateTrade while we're setting initial values
    const isInitializing = useRef(false);

    // Unified initialization on mount: fetch strategies, latest asset value and initial exchange rate
    useEffect(() => {
        isInitializing.current = true;

        const init = async () => {
            try {
                // Kick off requests in parallel
                const listPromise = apiService.getListItems('EntryMethod');
                const assetPromise = apiService.getLatestAssetValue();

                // compute exchange rate
                const parts = currencyPair.split('/').map(p => p.trim().toUpperCase());
                let ratePromise: Promise<number> | null = null;
                if (parts.length === 2) {
                    const [base, quote] = parts;
                    if (base === quote) {
                        ratePromise = Promise.resolve(1);
                    } else {
                        ratePromise = apiService.getExchangeRate(base, quote);
                    }
                }

                const [lists, assetValue, rate] = await Promise.all([listPromise, assetPromise, ratePromise]);

                if (lists) {
                    setStrategies(lists);
                    if (lists.length > 0 && selectedStrategyId == null) {
                        setSelectedStrategyId(lists[0].id);
                    }
                }

                if (assetValue) {
                    // set trading capital once
                    setRequest(prev => ({ ...prev, tradingCapital: assetValue.totalAssetValue }));
                }

                if (rate != null) {
                    // set exchange rate once
                    setRequest(prev => ({ ...prev, exchangeRate: rate }));
                }
            } catch (error) {
                console.error('Initialization failed for TradeCalculatorCard', error);
            } finally {
                // small timeout to ensure other state updates settle before enabling calculations
                setTimeout(() => { isInitializing.current = false; }, 0);
            }
        };

        void init();
    }, []);

    // Fetch latest asset value on component mount
    useEffect(() => {
        const loadLatestAssetValue = async () => {
            try {
                const assetValue = await apiService.getLatestAssetValue();
                setRequest(prev => ({ ...prev, tradingCapital: assetValue.totalAssetValue }));
            } catch (error) {
                console.error('Failed to fetch latest asset value:', error);
            }
        };

        void loadLatestAssetValue();
    }, []);

    const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
        // Support checkboxes (use checked) and numeric/text inputs (use value)
        const target = e.target as HTMLInputElement;
        const { name, value, type, checked } = target;

        setRequest(prev => {
            const newValue: string | number | boolean = type === 'checkbox'
                ? checked
                : ['instrument', 'tradeDate'].includes(name)
                    ? value
                    : value === ''
                        ? 0
                        : parseFloat(value);

            // eslint-disable-next-line @typescript-eslint/no-explicit-any
            return {
                ...prev,
                [name]: newValue as any,
            } as TradeCalculationRequest;
        });
    };

    const handlePairChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
        const val = e.target.value;
        setCurrencyPair(val);

        // Immediately fetch rate for the selected pair (avoid waiting for effect)
        (async () => {
            try {
                const parts = val.split('/').map(p => p.trim().toUpperCase());
                if (parts.length !== 2) return;

                const base = parts[0];
                const quote = parts[1];

                if (base === quote) {
                    setRequest(prev => ({ ...prev, exchangeRate: 1 }));
                    return;
                }

                const rate = await apiService.getExchangeRate(base, quote);
                setRequest(prev => ({ ...prev, exchangeRate: rate }));
            } catch (error) {
                console.error('Failed to update exchange rate on pair change for', val, error);
            }
        })();
    };

    return (
        <div className="dashboard-card">
            <div className="flex justify-between items-center">
                <h3>Trading Position Calculator</h3>
                {loading && <span className="text-sm text-blue-600 animate-pulse">Calculating...</span>}
            </div>

            {error && (
                <div className="p-3 bg-red-100 border border-red-400 text-red-700 rounded-md text-sm">
                    {error}
                </div>
            )}

            {/* Input Section */}
            <div>
                <div className="flex items-center gap-4 mb-4">
                    <label className="w-48 flex-shrink-0">Trade Date</label>
                    <input
                        type="date"
                        name="tradeDate"
                        value={request.tradeDate}
                        onChange={handleChange}
                        className="flex-1 rounded-md border-gray-300 shadow-sm p-2 bg-yellow-100 font-semibold text-xs focus:ring-yellow-500 focus:border-yellow-500"
                    />
                </div>
                <div className="flex items-center gap-4 mb-4">
                    <label className="w-48 flex-shrink-0">Instrument</label>
                    <input
                        type="text"
                        name="instrument"
                        value={request.instrument}
                        onChange={handleChange}
                        className="flex-1 rounded-md border-gray-300 shadow-sm p-2 bg-yellow-100 font-semibold text-xs focus:ring-yellow-500 focus:border-yellow-500"
                    />
                </div>
                <div className="flex items-center gap-4 mb-4">
                    <label className="w-48 flex-shrink-0">Currency Pair</label>
                    <select
                        name="currencyPair"
                        value={currencyPair}
                        onChange={handlePairChange}
                        className="w-40 rounded-md border-gray-300 shadow-sm p-2 bg-yellow-100 font-semibold text-xs focus:ring-yellow-500 focus:border-yellow-500"
                    >
                        <option>GBP/USD</option>
                        <option>GBP/GBP</option>
                        <option>GBP/EUR</option>
                    </select>

                    <label className="w-28 text-right">Exchange Rate</label>
                    <input
                        type="number"
                        step="0.0001"
                        name="exchangeRate"
                        value={request.exchangeRate}
                        onChange={handleChange}
                        className="flex-1 rounded-md border-gray-300 shadow-sm p-2 bg-yellow-100 font-semibold text-xs focus:ring-yellow-500 focus:border-yellow-500"
                    />
                </div>
                <div className="flex items-center gap-4 mb-4">
                    <label className="w-48 flex-shrink-0">Buy Price (in stock Currency)</label>
                    <input
                        type="number"
                        step="0.01"
                        name="buyPrice"
                        value={request.buyPrice}
                        onChange={handleChange}
                        className="flex-1 rounded-md border-gray-300 shadow-sm p-2 bg-yellow-100 font-semibold text-xs focus:ring-yellow-500 focus:border-yellow-500"
                    />
                </div>
                <div className="flex items-center gap-4 mb-4">
                    <label className="w-48 flex-shrink-0">Trading Capital (£)</label>
                    <input
                        type="number"
                        name="tradingCapital"
                        value={request.tradingCapital}
                        onChange={handleChange}
                        className="flex-1 rounded-md border-gray-300 shadow-sm p-2 bg-yellow-100 font-semibold text-xs focus:ring-yellow-500 focus:border-yellow-500"
                    />
                </div>
                <div className="flex items-center gap-4 mb-4">
                    <label className="w-48 flex-shrink-0">Risk Per Position(%)</label>
                    <input
                        type="number"
                        step="0.5"
                        name="riskPerTrade"
                        value={request.riskPerTrade}
                        onChange={handleChange}
                        className="flex-1 rounded-md border-gray-300 shadow-sm p-2 bg-yellow-100 font-semibold text-xs focus:ring-yellow-500 focus:border-yellow-500"
                    />
                </div>
                <div className="flex items-center gap-4 mb-4">
                    <label className="w-48 flex-shrink-0">Max Exposure On Position</label>
                    <input
                        type="number"
                        step="0.5"
                        name="maxExposure"
                        value={request.maxExposure}
                        onChange={handleChange}
                        className="flex-1 rounded-md border-gray-300 shadow-sm p-2 bg-yellow-100 font-semibold text-xs focus:ring-yellow-500 focus:border-yellow-500"
                    />
                </div>
                <div className="flex items-center gap-4 mb-4">
                    <label className="w-48 flex-shrink-0">Gain / Loss Ratio (% Winning)</label>
                    <input
                        type="number"
                        step="0.5"
                        name="gainLossRatio"
                        value={request.gainLossRatio}
                        onChange={handleChange}
                        className="flex-1 rounded-md border-gray-300 shadow-sm p-2 bg-yellow-100 font-semibold text-xs focus:ring-yellow-500 focus:border-yellow-500"
                    />
                </div>            
                <br />
                <div><p>STOPLOSS  - SET BY STOPLOSS POINT</p></div>
                <div className="flex items-center gap-4 mb-4">
                    <label className="w-48 flex-shrink-0">Stop Loss At</label>
                    <input
                        type="number"
                        step="0.01"
                        name="stopLossAtInput"
                        value={request.stopLossAtInput}
                        onChange={handleChange}
                        className="flex-1 rounded-md border-gray-300 shadow-sm p-2 bg-yellow-100 font-semibold text-xs focus:ring-yellow-500 focus:border-yellow-500"
                    />
                </div>

                <ListItemSelect
                    id="strategy"
                    items={strategies}
                    selectedId={selectedStrategyId}
                    label="Strategy"
                    onChange={setSelectedStrategyId}
                    isLoading={isLoadingStrategies}
                    isDisabled={isSubmitting}
                    placeholder="-- Select Strategy (Optional) --"
                    inline={true}
                    wrapperClassName="mb-4"
                    labelClassName="w-48 flex-shrink-0"
                    className="flex-1 rounded-md border-gray-300 shadow-sm p-2 bg-yellow-100 font-semibold text-xs focus:ring-yellow-500 focus:border-yellow-500"
                />
            </div>

            {/* Output Section */}
            {result && (
                <div className="grid grid-cols-1 md:grid-cols-3 gap-4 p-4 border rounded-lg bg-green-50 text-xs">
                    <div className="md:col-span-3">
                        <p>STOP LOSS DETAILS</p>
                    </div>
                    <div>
                        <label className="block font-medium text-gray-700">Lot Size (£) / ($)</label>
                        <div className="mt-1 p-2 bg-green-200 rounded-md font-semibold text-gray-800">
                            £{result.lotGbp.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })} / ${result.lotUsd.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
                        </div>
                    </div>
                    <div>
                        <label className="block font-medium text-gray-700">Lot Size (%)</label>
                        <div className="mt-1 p-2 bg-green-200 rounded-md font-semibold text-gray-800">
                            {(result.lotPercent * 100).toFixed(1)}%
                        </div>
                    </div>
                    <div>
                        <label className="block font-medium text-gray-700">Shares</label>
                        <div className="mt-1 p-2 bg-green-200 rounded-md font-semibold text-gray-800">
                            {result.shares.toFixed(2)}
                        </div>
                    </div>
                    <div>
                        <label className="block font-medium text-gray-700">Stop Loss At</label>
                        <div className="mt-1 p-2 bg-green-200 rounded-md font-semibold text-gray-800">
                            {result.stopLossAt.toFixed(2)}
                        </div>
                    </div>
                    <div>
                        <label className="block font-medium text-gray-700">Loss (£ / USD)</label>
                        <div className="mt-1 p-2 bg-green-200 rounded-md font-semibold text-gray-800">
                            £{result.lossGbp.toFixed(2)} / ${result.lossUsd.toFixed(2)}
                        </div>
                    </div>
                    <div>
                        <label className="block font-medium text-gray-700">Loss %</label>
                        <div className="mt-1 p-2 bg-green-200 rounded-md font-semibold text-gray-800">
                            {(result.lossPercentage * 100).toFixed(3)}%
                        </div>
                    </div>
                    <div className="md:col-span-3">
                        <p>TAKE PROFIT DETAILS</p>
                    </div>
                    <div>
                        <label className="block font-medium text-gray-700">Price Target</label>
                        <div className="mt-1 p-2 bg-green-200 rounded-md font-semibold text-gray-800">
                            ${result.priceTarget.toFixed(2)}
                        </div>
                    </div>
                    <div>
                        <label className="block font-medium text-gray-700">Take Profit At</label>
                        <div className="mt-1 p-2 bg-green-200 rounded-md font-semibold text-gray-800">
                            {result.takeProfitAt.toFixed(1)}%
                        </div>
                    </div>
                    <div className="md:col-span-3">
                        <label className="block font-medium text-gray-700">Overall Profit (£ / USD)</label>
                        <div className="mt-1 p-2 bg-green-200 rounded-md font-semibold text-gray-800">
                            £{result.overallProfitGbp.toFixed(2)} / ${result.overallProfitUsd.toFixed(2)}
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};

export default TradeCalculatorCard;