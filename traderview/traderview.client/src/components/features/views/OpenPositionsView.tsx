import { useEffect, useState, useMemo } from 'react';
import { notesService, tradeService, positionsService, syncService } from '../../../services';
import MarketRegime from '../trade/MarketRegime';
import type { OpenPosition, CreateNoteRequest, Note, TradeExecution } from '../../../types/api';
import type { SortConfig } from '../../common/SortableTableHeader';
import AddNoteModal from '../modals/AddNoteModal';
import EditNoteModal from '../modals/EditNoteModal';
import NotesList from '../../common/NotesList';
import TradeExecutionsList from '../../common/TradeExecutionsList';
import PositionCalculatorsModal from '../../common/PositionCalculatorsModal';
import OpenPositionsTableCard from '../../cards/OpenPositionsTableCard';
import './OpenPositionsView.css';

function OpenPositionsView() {
    const [openPositions, setOpenPositions] = useState<OpenPosition[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);
    const [isNoteModalOpen, setIsNoteModalOpen] = useState<boolean>(false);
    const [isEditNoteModalOpen, setIsEditNoteModalOpen] = useState<boolean>(false);
    const [selectedNoteForEdit, setSelectedNoteForEdit] = useState<Note | null>(null);
    const [selectedPositionId, setSelectedPositionId] = useState<number | null>(null);
    const [selectedPosition, setSelectedPosition] = useState<OpenPosition | null>(null);
    const [notes, setNotes] = useState<Note[]>([]);
    const [notesLoading, setNotesLoading] = useState<boolean>(false);
    const [tradeExecutions, setTradeExecutions] = useState<TradeExecution[]>([]);
    const [tradeExecutionsLoading, setTradeExecutionsLoading] = useState<boolean>(false);
    const [isCalcModalOpen, setIsCalcModalOpen] = useState<boolean>(false);
    const [isRunningTradeConfirmReport, setIsRunningTradeConfirmReport] = useState<boolean>(false);

    const loadOpenPositions = async () => {
        try {
            setLoading(true);
            setError(null);
            const positions = await positionsService.getOpenPositions();
            setOpenPositions(positions);
        } catch (err) {
            const errorMessage = err instanceof Error ? err.message : 'Failed to load open positions';
            setError(errorMessage);
            console.error('Error loading open positions:', err);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        // avoid calling setState synchronously inside effect — schedule in a microtask
        setTimeout(() => {
            void loadOpenPositions();
        }, 0);
    }, []);

    useEffect(() => {
        if (!selectedPosition) {
            // Avoid synchronous setState in effect
            setTimeout(() => setNotes([]), 0);
            return;
        }

        const fetchNotes = async () => {
            try {
                setNotesLoading(true);
                const notesData = await notesService.getNotesByPositionId(selectedPosition.positionId);
                setNotes(notesData);
            } catch (err) {
                console.error('Error fetching notes:', err);
                setNotes([]);
            } finally {
                setNotesLoading(false);
            }
        };

        fetchNotes();
    }, [selectedPosition?.positionId]);

    useEffect(() => {
        if (!selectedPosition) {
            // Avoid synchronous setState in effect
            setTimeout(() => setTradeExecutions([]), 0);
            return;
        }

        const fetchTradeExecutions = async () => {
            try {
                setTradeExecutionsLoading(true);
                const tradeExecutionsData = await tradeService.getTradeExecutionsByPositionId(selectedPosition.positionId);
                setTradeExecutions(tradeExecutionsData);
            } catch (err) {
                console.error('Error fetching trade executions:', err);
                setTradeExecutions([]);
            } finally {
                setTradeExecutionsLoading(false);
            }
        };

        fetchTradeExecutions();
    }, [selectedPosition?.positionId]);

    const handleAddNoteClick = (positionId: number) => {
        setSelectedPositionId(positionId);
        setIsNoteModalOpen(true);
    };

    const handleRowClick = (position: OpenPosition) => {
        setSelectedPosition(position);
    };

    const handleCloseNoteModal = () => {
        setIsNoteModalOpen(false);
        setSelectedPositionId(null);
    };

    const handleSubmitNote = async (comment: string, entryMethodId: number | null, errorTypeId: number | null, exitTypeId: number | null) => {
        if (!selectedPositionId) {
            throw new Error('No position selected');
        }

        const noteRequest: CreateNoteRequest = {
            positionId: selectedPositionId,
            tradeExecutionId: null,
            comment: comment,
            entryDate: new Date().toISOString(),
            tradeTypeId: entryMethodId ?? null,
            errorTypeId: errorTypeId ?? null,
            exitTypeId: exitTypeId ?? null,
            time: null,
        };

        const result = await notesService.createNote(noteRequest);

        if (selectedPosition && selectedPosition.positionId === selectedPositionId) {
            const notesData = await notesService.getNotesByPositionId(selectedPositionId);
            setNotes(notesData);
        }

        return result;
    };

    const handleEditNoteClick = (note: Note) => {
        setSelectedNoteForEdit(note);
        setIsEditNoteModalOpen(true);
    };

    const handleCloseEditNoteModal = () => {
        setIsEditNoteModalOpen(false);
        setSelectedNoteForEdit(null);
    };

    const handleSubmitEditNote = async (noteId: number, positionId: number, comment: string, entryDate: string, entryMethodId: number | null, errorTypeId: number | null, exitTypeId: number | null) => {

        const result = await notesService.updateNote(noteId, comment, entryDate, entryMethodId, errorTypeId, exitTypeId);

        // Refresh notes for the position that was edited
        const notesData = await notesService.getNotesByPositionId(positionId);

        // If the edited position is currently selected, update the notes list
        if (selectedPosition && selectedPosition.positionId === positionId) {
            setNotes(notesData);
        }

        return result;
    };

    const handleRunTradeConfirmReport = async () => {
        try {
            setIsRunningTradeConfirmReport(true);
        const result = await syncService.runTradeConfirmReport();
            console.log('Trade confirm report completed:', result);
            alert(`Trade confirm report completed successfully at ${new Date(result.timestamp).toLocaleString()}`);
            // Reload open positions after successful report completion
            await loadOpenPositions();
        } catch (err) {
            console.error('Failed to run trade confirm report', err);
            const errorMessage = err instanceof Error ? err.message : 'Failed to run trade confirm report. See console for details.';
            alert(errorMessage);
        } finally {
            setIsRunningTradeConfirmReport(false);
        }
    };

    const [sortConfig, setSortConfig] = useState<SortConfig<OpenPosition>>({
        key: 'symbol',
        direction: 'asc'
    });

    const sortedPositions = useMemo(() => {
        if (!openPositions || openPositions.length === 0) return [];

        return [...openPositions].sort((a, b) => {
            const aVal = a[sortConfig.key];
            const bVal = b[sortConfig.key];

            if (aVal == null && bVal == null) return 0;
            if (aVal == null) return 1;
            if (bVal == null) return -1;

            if (sortConfig.key === 'dateOpened') {
                const dateA = new Date(aVal as string).getTime();
                const dateB = new Date(bVal as string).getTime();
                return sortConfig.direction === 'asc' ? dateA - dateB : dateB - dateA;
            }

            if (typeof aVal === 'number' && typeof bVal === 'number') {
                return sortConfig.direction === 'asc' ? aVal - bVal : bVal - aVal;
            }

            const strA = String(aVal).toLowerCase();
            const strB = String(bVal).toLowerCase();
            return sortConfig.direction === 'asc'
                ? strA.localeCompare(strB)
                : strB.localeCompare(strA);
        });
    }, [openPositions, sortConfig]);

    const handleSort = (key: keyof OpenPosition) => {
        setSortConfig(prev => ({
            key,
            direction: prev.key === key && prev.direction === 'asc' ? 'desc' : 'asc'
        }));
    };

    const totalCurrentMargin = useMemo(() => {
        return openPositions?.reduce((sum, pos) => sum + (Number(pos.currentMargin) || 0), 0) ?? 0;
    }, [openPositions]);

    const totalValue = useMemo(() => {
        return openPositions?.reduce((sum, pos) => sum + (Number(pos.value) || 0), 0) ?? 0;
    }, [openPositions]);

    const totalValueGBP = useMemo(() => {
        return openPositions?.reduce((sum, pos) => sum + (Number(pos.valueInGBP) || 0), 0) ?? 0;
    }, [openPositions]);

    if (loading) {
        return (
            <div className="open-positions-container">
                <div className="loading-container">
                    <p><em>Loading open positions...</em></p>
                </div>
            <PositionCalculatorsModal isOpen={isCalcModalOpen} positionId={selectedPosition?.positionId ?? 0} onClose={() => setIsCalcModalOpen(false)} />
            </div>
        );
    }

    if (error) {
        return (
            <div className="open-positions-container">
                <div className="error-container">
                    <h2>Error Loading Open Positions</h2>
                    <p className="error">{error}</p>
                    <button onClick={loadOpenPositions}>Retry</button>
                </div>
            </div>
        );
    }

    if (openPositions.length === 0) {
        return (
            <div className="open-positions-container">
                <div className="empty-container">
                    <h2>Open Positions</h2>
                    <p><em>No open positions found.</em></p>
                </div>
            </div>
        );
    }

    return (
        <div className="open-positions-container">
            <div className="open-positions-header">
                <h1>Open Positions</h1>
                <div className="header-actions">
                    <div style={{ display: 'flex', alignItems: 'center', marginRight: 12 }}>
                        <MarketRegime />
                    </div>
                    <button onClick={async () => {
                        try {
                            const blob = await positionsService.downloadCompoundPositionsReport();
                            const url = window.URL.createObjectURL(blob);
                            const a = document.createElement('a');
                            a.href = url;
                            a.download = `CompoundPositions_${new Date().toISOString().replace(/[:.]/g, '-')}.xlsx`;
                            document.body.appendChild(a);
                            a.click();
                            a.remove();
                            window.URL.revokeObjectURL(url);
                        } catch (err) {
                            console.error('Failed to download compound positions report', err);
                            alert('Failed to download compound positions report. See console for details.');
                        }
                    }}>Download Compound Positions</button>
                    <button onClick={handleRunTradeConfirmReport} disabled={isRunningTradeConfirmReport}>
                        {isRunningTradeConfirmReport ? 'Running Trade Confirm Report...' : 'Run Trade Confirm Report'}
                    </button>
                </div>
            </div>
            <OpenPositionsTableCard
                positions={sortedPositions}
                selectedPositionId={selectedPosition?.positionId ?? null}
                sortConfig={sortConfig}
                onSort={handleSort}
                onRowClick={handleRowClick}
                onAddNoteClick={handleAddNoteClick}
                onLinkCalculationsClick={(position) => {
                    setSelectedPosition(position);
                    setIsCalcModalOpen(true);
                }}
                totalValue={totalValue}
                totalCurrentMargin={totalCurrentMargin}
                totalValueGBP={totalValueGBP}
            />

            {selectedPosition && (
                <div className="notes-section">
                    <h2>Notes for {selectedPosition.symbol}</h2>
                    <NotesList notes={notes} loading={notesLoading} variant="detailed" onEditNote={handleEditNoteClick} />
                </div>
            )}

            {selectedPosition && (
                <div className="trade-executions-section">
                    <TradeExecutionsList tradeExecutions={tradeExecutions} loading={tradeExecutionsLoading} />
                </div>
            )}

            <AddNoteModal
                isOpen={isNoteModalOpen}
                onClose={handleCloseNoteModal}
                onSubmit={handleSubmitNote}
                positionId={selectedPositionId ?? 0}
            />

            <EditNoteModal
                isOpen={isEditNoteModalOpen}
                onClose={handleCloseEditNoteModal}
                onSubmit={handleSubmitEditNote}
                note={selectedNoteForEdit}
            />
        </div>
    );
}

export default OpenPositionsView;