import { useEffect, useState } from 'react';
import { marketDataService } from '../../services';

type Regime = 'Choppy/Pressure' | 'Bull' | 'Correction' | 'Unknown';

function statusColor(regime: Regime) {
    switch (regime) {
        case 'Bull':
            return '#2ecc71'; // green
        case 'Choppy/Pressure':
            return '#FFBF00'; // amber
        case 'Correction':
            return '#e74c3c'; // red
        default:
            return '#95a5a6'; // grey
    }
}

export default function MarketRegime() {
    const [regime, setRegime] = useState<Regime>('Unknown');
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        let mounted = true;
        const load = async () => {
            try {
                setLoading(true);
                setError(null);
                const r = await marketDataService.getMarketRegime();
                if (!mounted) return;
                if (r === 'Bull' || r === 'Correction' || r === 'Choppy/Pressure') {
                    setRegime(r as Regime);
                } else {
                    setRegime('Unknown');
                }
            } catch (err) {
                console.error('Failed to load market regime', err);
                if (!mounted) return;
                setError(err instanceof Error ? err.message : 'Failed to load regime');
                setRegime('Unknown');
            } finally {
                if (!mounted) return;
                setLoading(false);
            }
        };

        void load();

        return () => { mounted = false; };
    }, []);

    return (
        <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
            <div
                aria-label={`market-regime-${regime}`}
                title={loading ? 'Loading market regime...' : error ?? regime}
                style={{
                    width: 14,
                    height: 14,
                    borderRadius: '50%',
                    background: statusColor(regime),
                    boxShadow: '0 0 6px rgba(0,0,0,0.15)'
                }}
            />
            <div style={{ fontSize: 14, color: '#333' }}>
                {loading ? 'Loading...' : (error ? 'Error' : regime)}
            </div>
        </div>
    );
}
