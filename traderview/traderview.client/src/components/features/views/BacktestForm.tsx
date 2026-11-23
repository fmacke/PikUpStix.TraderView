import { useState } from 'react';
import type { BacktestRequest } from '../../../services/backtestService';
import './BacktestForm.css';

interface BacktestFormProps {
    onSubmit: (request: BacktestRequest) => Promise<void>;
    loading: boolean;
    instruments: Array<{ id: number; name: string }>;
}

const STRATEGY_PRESETS = {
    SMACrossover: {
        strategyType: 'SMACrossover',
        fastPeriod: 12,
        slowPeriod: 26,
        atrPeriod: 14,
    },
    RSIOversold: {
        strategyType: 'RSIOversold',
        rsiPeriod: 14,
        atrPeriod: 14,
    },
};

export function BacktestForm({ onSubmit, loading, instruments }: BacktestFormProps) {
    const [formData, setFormData] = useState<BacktestRequest>({
        instrumentId: 1,
        startDate: new Date(new Date().setFullYear(new Date().getFullYear() - 1)).toISOString().split('T')[0],
        endDate: new Date().toISOString().split('T')[0],
        initialCapital: 10000,
        strategyType: 'SMACrossover',
        warmupBars: 50,
        slippagePercent: 0.01,
        commissionPerTrade: 10,
        fastPeriod: 12,
        slowPeriod: 26,
        rsiPeriod: 14,
        atrPeriod: 14,
    });

    const handleStrategyChange = (strategy: keyof typeof STRATEGY_PRESETS) => {
        const preset = STRATEGY_PRESETS[strategy];
        setFormData((prev) => ({
            ...prev,
            ...preset,
        }));
    };

    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
        const { name, value, type } = e.target;
        setFormData((prev) => ({
            ...prev,
            [name]: type === 'number' ? parseFloat(value) : value,
        }));
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        await onSubmit(formData);
    };

    return (
        <form className="backtest-form" onSubmit={handleSubmit}>
            <div className="form-section">
                <h3>Basic Settings</h3>

                <div className="form-group">
                    <label htmlFor="instrumentId">Instrument *</label>
                    <select
                        id="instrumentId"
                        name="instrumentId"
                        value={formData.instrumentId}
                        onChange={handleInputChange}
                        required
                    >
                        <option value="">Select an instrument</option>
                        {instruments.map((inst) => (
                            <option key={inst.id} value={inst.id}>
                                {inst.name}
                            </option>
                        ))}
                    </select>
                </div>

                <div className="form-row">
                    <div className="form-group">
                        <label htmlFor="startDate">Start Date *</label>
                        <input
                            id="startDate"
                            type="date"
                            name="startDate"
                            value={formData.startDate}
                            onChange={handleInputChange}
                            required
                        />
                    </div>
                    <div className="form-group">
                        <label htmlFor="endDate">End Date *</label>
                        <input
                            id="endDate"
                            type="date"
                            name="endDate"
                            value={formData.endDate}
                            onChange={handleInputChange}
                            required
                        />
                    </div>
                </div>

                <div className="form-group">
                    <label htmlFor="initialCapital">Initial Capital ($) *</label>
                    <input
                        id="initialCapital"
                        type="number"
                        name="initialCapital"
                        value={formData.initialCapital}
                        onChange={handleInputChange}
                        min="100"
                        step="100"
                        required
                    />
                </div>
            </div>

            <div className="form-section">
                <h3>Strategy</h3>

                <div className="strategy-presets">
                    {Object.keys(STRATEGY_PRESETS).map((key) => (
                        <button
                            key={key}
                            type="button"
                            className={`preset-btn ${formData.strategyType === key ? 'active' : ''}`}
                            onClick={() => handleStrategyChange(key as keyof typeof STRATEGY_PRESETS)}
                        >
                            {key}
                        </button>
                    ))}
                </div>

                {formData.strategyType === 'SMACrossover' && (
                    <div className="form-row">
                        <div className="form-group">
                            <label htmlFor="fastPeriod">Fast EMA Period</label>
                            <input
                                id="fastPeriod"
                                type="number"
                                name="fastPeriod"
                                value={formData.fastPeriod}
                                onChange={handleInputChange}
                                min="1"
                                step="1"
                            />
                        </div>
                        <div className="form-group">
                            <label htmlFor="slowPeriod">Slow EMA Period</label>
                            <input
                                id="slowPeriod"
                                type="number"
                                name="slowPeriod"
                                value={formData.slowPeriod}
                                onChange={handleInputChange}
                                min="1"
                                step="1"
                            />
                        </div>
                    </div>
                )}

                {formData.strategyType === 'RSIOversold' && (
                    <div className="form-group">
                        <label htmlFor="rsiPeriod">RSI Period</label>
                        <input
                            id="rsiPeriod"
                            type="number"
                            name="rsiPeriod"
                            value={formData.rsiPeriod}
                            onChange={handleInputChange}
                            min="1"
                            step="1"
                        />
                    </div>
                )}

                <div className="form-group">
                    <label htmlFor="atrPeriod">ATR Period</label>
                    <input
                        id="atrPeriod"
                        type="number"
                        name="atrPeriod"
                        value={formData.atrPeriod}
                        onChange={handleInputChange}
                        min="1"
                        step="1"
                    />
                </div>
            </div>

            <div className="form-section">
                <h3>Execution Settings</h3>

                <div className="form-row">
                    <div className="form-group">
                        <label htmlFor="slippagePercent">Slippage (%)</label>
                        <input
                            id="slippagePercent"
                            type="number"
                            name="slippagePercent"
                            value={formData.slippagePercent}
                            onChange={handleInputChange}
                            min="0"
                            step="0.001"
                        />
                    </div>
                    <div className="form-group">
                        <label htmlFor="commissionPerTrade">Commission ($)</label>
                        <input
                            id="commissionPerTrade"
                            type="number"
                            name="commissionPerTrade"
                            value={formData.commissionPerTrade}
                            onChange={handleInputChange}
                            min="0"
                            step="0.01"
                        />
                    </div>
                </div>

                <div className="form-group">
                    <label htmlFor="warmupBars">Warmup Bars</label>
                    <input
                        id="warmupBars"
                        type="number"
                        name="warmupBars"
                        value={formData.warmupBars}
                        onChange={handleInputChange}
                        min="1"
                        step="1"
                    />
                </div>
            </div>

            <button type="submit" className="submit-btn" disabled={loading}>
                {loading ? 'Running Backtest...' : 'Run Backtest'}
            </button>
        </form>
    );
}
