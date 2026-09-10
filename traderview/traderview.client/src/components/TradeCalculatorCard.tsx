import { useState, useEffect } from 'react';
import { apiService } from '../services/apiService';
import type { TradeCalculationRequest, TradeCalculationResponse } from '../types/api';

export const TradeCalculatorCard: React.FC = () => {
    const [request, setRequest] = useState<TradeCalculationRequest>({
        tradeDate: new Date().toISOString().split('T')[0],
        instrument: 'mrx',
        exchangeRate: 1.36,
        buyPrice: 520,
        tradingCapital: 100000,
        lotSizePercentage: 0.05,
        lot: 1,
        maxExposure: 0.025,
        gainLossRatio: 2,
        calculationMode: 'LotSize',
        stopLossAtInput: 480,
    });

    const [result, setResult] = useState<TradeCalculationResponse | null>(null);
    const [loading, setLoading] = useState<boolean>(false);
    const [error, setError] = useState<string | null>(null);

    const calculateTrade = async () => {
        try {
            setLoading(true);
            setError(null);
            const data = await apiService.calculateTradePosition(request);
            setResult(data);
        } catch (err) {
            setError('Failed to calculate trade position. Please check your inputs or backend connection.');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        calculateTrade();
    }, [request]);

    const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
        const { name, value } = e.target;
        setRequest(prev => ({
            ...prev,
            [name]: ['instrument', 'calculationMode', 'tradeDate'].includes(name) ? value : parseFloat(value) || 0,
        }));
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
                <div>
                    <label>Trade Date</label>
                    <input
                        type="date"
                        name="tradeDate"
                        value={request.tradeDate}
                        onChange={handleChange}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm p-2 bg-yellow-100 font-semibold text-xs focus:ring-yellow-500 focus:border-yellow-500"
                    />
                </div>
                <div>
                    <label>Instrument</label>
                    <input
                        type="text"
                        name="instrument"
                        value={request.instrument}
                        onChange={handleChange}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm p-2 bg-yellow-100 font-semibold text-xs focus:ring-yellow-500 focus:border-yellow-500"
                    />
                </div>
                <div>
                    <label>Exchange Rate</label>
                    <input
                        type="number"
                        step="0.01"
                        name="exchangeRate"
                        value={request.exchangeRate}
                        onChange={handleChange}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm p-2 bg-yellow-100 font-semibold text-xs focus:ring-yellow-500 focus:border-yellow-500"
                    />
                </div>
                <div>
                    <label>Buy Price</label>
                    <input
                        type="number"
                        step="0.01"
                        name="buyPrice"
                        value={request.buyPrice}
                        onChange={handleChange}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm p-2 bg-yellow-100 font-semibold text-xs focus:ring-yellow-500 focus:border-yellow-500"
                    />
                </div>
                <div>
                    <label>Trading Capital (£)</label>
                    <input
                        type="number"
                        name="tradingCapital"
                        value={request.tradingCapital}
                        onChange={handleChange}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm p-2 bg-yellow-100 font-semibold text-xs focus:ring-yellow-500 focus:border-yellow-500"
                    />
                </div>
                <div>
                    <label>Lot Size (%)</label>
                    <input
                        type="number"
                        step="0.01"
                        name="lotSizePercentage"
                        value={request.lotSizePercentage}
                        onChange={handleChange}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm p-2 bg-yellow-100 font-semibold text-xs focus:ring-yellow-500 focus:border-yellow-500"
                    />
                </div>
                <div>
                    <label>Max Exposure</label>
                    <input
                        type="number"
                        step="0.001"
                        name="maxExposure"
                        value={request.maxExposure}
                        onChange={handleChange}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm p-2 bg-yellow-100 font-semibold text-xs focus:ring-yellow-500 focus:border-yellow-500"
                    />
                </div>
                <div>
                    <label>Gain / Loss Ratio</label>
                    <input
                        type="number"
                        step="0.1"
                        name="gainLossRatio"
                        value={request.gainLossRatio}
                        onChange={handleChange}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm p-2 bg-yellow-100 font-semibold text-xs focus:ring-yellow-500 focus:border-yellow-500"
                    />
                </div>
                <div>
                    <label>Lot</label>
                    <input
                        type="number"
                        step="0.01"
                        name="lot"
                        value={request.lot}
                        onChange={handleChange}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm p-2 bg-yellow-100 font-semibold text-xs focus:ring-yellow-500 focus:border-yellow-500"
                    />
                </div>
            </div>

            {/* Output Section */}
            {result && (
                <div className="grid grid-cols-1 md:grid-cols-3 gap-4 p-4 border rounded-lg bg-green-50 text-xs">
                    <div>
                        <label className="block font-medium text-gray-700">Lot Size (£)</label>
                        <div className="mt-1 p-2 bg-green-200 rounded-md font-semibold text-gray-800">
                            £{result.lotSizeGbp.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
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
                    <div>
                        <label className="block font-medium text-gray-700">Price Target</label>
                        <div className="mt-1 p-2 bg-green-200 rounded-md font-semibold text-gray-800">
                            {result.priceTarget.toFixed(2)}
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