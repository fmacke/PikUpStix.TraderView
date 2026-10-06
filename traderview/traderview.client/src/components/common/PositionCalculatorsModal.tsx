import { useEffect, useState } from 'react';
import { apiService } from '../../services/apiService';
import type { PositionCalculatorDto } from '../../types/api';
import './PositionCalculatorsModal.css';

interface Props {
    positionId: number;
    isOpen: boolean;
    onClose: () => void;
}

export default function PositionCalculatorsModal({ positionId, isOpen, onClose }: Props) {
    const [items, setItems] = useState<PositionCalculatorDto[]>([]);
    const [selected, setSelected] = useState<Set<number>>(new Set());
    const [loading, setLoading] = useState(false);

    useEffect(() => {
        if (!isOpen) return;
        const load = async () => {
            setLoading(true);
            try {
                const data = await apiService.getPositionCalculatorCandidates(positionId);
                setItems(data);
                // preselect those already linked
                const pre = new Set<number>(data.filter(d => d.positionId === positionId).map(d => d.id));
                setSelected(pre);
            } catch (e) {
                console.error('Failed to load position calculators', e);
            } finally {
                setLoading(false);
            }
        };
        load();
    }, [isOpen, positionId]);

    if (!isOpen) return null;

    const toggle = (id: number) => {
        const next = new Set(selected);
        if (next.has(id)) next.delete(id);
        else next.add(id);
        setSelected(next);
    };

    const handleLink = async () => {
        try {
            await apiService.linkPositionCalculators(positionId, Array.from(selected));
            onClose();
        } catch (e) {
            console.error('Link failed', e);
            alert('Failed to link calculators. See console.');
        }
    };

    const handleUnlink = async () => {
        try {
            await apiService.unlinkPositionCalculators(positionId, Array.from(selected));
            onClose();
        } catch (e) {
            console.error('Unlink failed', e);
            alert('Failed to unlink calculators. See console.');
        }
    };

    return (
        <div className="pc-modal-overlay">
            <div className="pc-modal-content">
                <div className="pc-modal-header">
                    <h3>Link Calculations for Position {positionId}</h3>
                    <button onClick={onClose} type="button">×</button>
                </div>
                <div className="pc-modal-body">
                    {loading ? (
                        <p>Loading...</p>
                    ) : (
                        <table className="pc-table">
                            <thead>
                                <tr>
                                    <th></th>
                                    <th>Date</th>
                                    <th>Symbol</th>
                                    <th>Strategy</th>
                                    <th>Shares</th>
                                    <th>Stop</th>
                                    <th>Target</th>
                                </tr>
                            </thead>
                            <tbody>
                                {items.map(item => (
                                    <tr key={item.id}>
                                        <td>
                                            <input type="checkbox" checked={selected.has(item.id)} onChange={() => toggle(item.id)} />
                                        </td>
                                        <td>{new Date(item.orderSetupDate).toLocaleDateString()}</td>
                                        <td>{item.symbol}</td>
                                        <td>{item.strategyId}</td>
                                        <td>{item.shareQuantity}</td>
                                        <td>{item.stopLossAt}</td>
                                        <td>{item.priceTarget}</td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    )}
                </div>
                <div className="pc-modal-footer">
                    <button onClick={handleLink} disabled={selected.size === 0}>Link selected</button>
                    <button onClick={handleUnlink} disabled={selected.size === 0}>Unlink selected</button>
                    <button onClick={onClose}>Close</button>
                </div>
            </div>
        </div>
    );
}
