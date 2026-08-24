import React, { useEffect, useState, useMemo } from 'react';
import { apiService } from '../services/apiService';
import type { OpenPosition, CreateNoteRequest, Note } from '../types/api';
import { SortableTableHeader, SortConfig } from './SortableTableHeader';
import AddNoteModal from './AddNoteModal';
import './OpenPositionList.css';

function OpenPositionList() {
    const [openPositions, setOpenPositions] = useState<OpenPosition[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);
    const [isNoteModalOpen, setIsNoteModalOpen] = useState<boolean>(false);
    const [selectedPositionId, setSelectedPositionId] = useState<number | null>(null);
    const [selectedPosition, setSelectedPosition] = useState<OpenPosition | null>(null);
    const [notes, setNotes] = useState<Note[]>([]);
    const [notesLoading, setNotesLoading] = useState<boolean>(false);

    useEffect(() => {
        loadOpenPositions();
    }, []);

    useEffect(() => {
        if (!selectedPosition) {
            setNotes([]);
            return;
        }

        const fetchNotes = async () => {
            try {
                setNotesLoading(true);
                const notesData = await apiService.getNotesByPositionId(selectedPosition.positionId);
                setNotes(notesData);
            } catch (error) {
                console.error('Error fetching notes:', error);
                setNotes([]);
            } finally {
                setNotesLoading(false);
            }
        };

        fetchNotes();
    }, [selectedPosition?.positionId]);

    const loadOpenPositions = async () => {
        try {
            setLoading(true);
            setError(null);
            const positions = await apiService.getOpenPositions();
            setOpenPositions(positions);
        } catch (err) {
            const errorMessage = err instanceof Error ? err.message : 'Failed to load open positions';
            setError(errorMessage);
            console.error('Error loading open positions:', err);
        } finally {
            setLoading(false);
        }
    };

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

    const handleSubmitNote = async (comment: string, entryMethodId: number | null) => {
        if (!selectedPositionId) {
            throw new Error('No position selected');
        }

        const noteRequest: CreateNoteRequest = {
            positionId: selectedPositionId,
            tradeExecutionId: null,
            comment: comment,
            entryDate: new Date().toISOString(),
            tradeTypeId: entryMethodId ?? 1 // Use selected entry method or default to 1
        };

        const result = await apiService.createNote(noteRequest);
        console.log('Note created successfully', result);

        // Refresh notes list if the note was added for the currently selected position
        if (selectedPosition && selectedPosition.positionId === selectedPositionId) {
            const notesData = await apiService.getNotesByPositionId(selectedPositionId);
            setNotes(notesData);
        }

        return result;
    };

    const formatCurrency = (value: number | null, decimals: number = 2) => {
        if (value === null || value === undefined) return '-';
        return value.toLocaleString('en-US', {
            minimumFractionDigits: decimals,
            maximumFractionDigits: decimals,
        });
    };

    const formatNumber = (value: number | null, decimals: number = 2) => {
        if (value === null || value === undefined) return '-';
        return value.toLocaleString('en-US', {
            minimumFractionDigits: decimals,
            maximumFractionDigits: decimals,
        });
    };

    const formatPercent = (value: number | null, decimals: number = 2) => {
        if (value === null || value === undefined) return '-';
        return (value * 100).toFixed(decimals) + '%';
    };

    const formatDate = (date: string | null) => {
        if (!date) return '-';
        return new Date(date).toLocaleDateString();
    };

    const [sortConfig, setSortConfig] = useState<SortConfig<OpenPositionDto>>({
        key: 'symbol',
        direction: 'asc'
    });

    const handleSort = (key: keyof OpenPositionDto) => {
        setSortConfig(prev => ({
            key,
            direction: prev.key === key && prev.direction === 'asc' ? 'desc' : 'asc'
        }));
    };

    // Recalculate total current margin
    const totalCurrentMargin = useMemo(() => {
        return openPositions?.reduce((sum, pos) => sum + (Number(pos.currentMargin) || 0), 0) ?? 0;
    }, [openPositions]);


    if (loading) {
        return (
            <div className="open-positions-container">
                <div className="loading-container">
                    <p><em>Loading open positions...</em></p>
                </div>
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
            <h1>Open Positions</h1>
            <div className="positions-table-container">
                <table className="positions-table">
                    <thead>
                        <tr>
                            <SortableTableHeader
                                columnKey="symbol"
                                title="Symbol"
                                sortConfig={sortConfig}
                                onSort={handleSort}
                            />
                            <SortableTableHeader
                                columnKey="dateOpened"
                                title="Date Opened"
                                sortConfig={sortConfig}
                                onSort={handleSort}
                            />
                            <SortableTableHeader
                                columnKey="daysOpened"
                                title="Days Opened"
                                sortConfig={sortConfig}
                                onSort={handleSort}
                                align="right"
                            />
                            <SortableTableHeader
                                columnKey="quantity"
                                title="Quantity"
                                sortConfig={sortConfig}
                                onSort={handleSort}
                                align="right"
                            />
                            <SortableTableHeader
                                columnKey="costPrice"
                                title="Cost Price"
                                sortConfig={sortConfig}
                                onSort={handleSort}
                                align="right"
                            />
                            <SortableTableHeader
                                columnKey="averagePrice"
                                title="Average Price"
                                sortConfig={sortConfig}
                                onSort={handleSort}
                                align="right"
                            />
                            <SortableTableHeader
                                columnKey="value"
                                title="Value"
                                sortConfig={sortConfig}
                                onSort={handleSort}
                                align="right"
                            />
                            <SortableTableHeader
                                columnKey="unrealizedPnL"
                                title="Unrealized P/L"
                                sortConfig={sortConfig}
                                onSort={handleSort}
                                align="right"
                            />
                            <SortableTableHeader
                                columnKey="percentChange"
                                title="% Change"
                                sortConfig={sortConfig}
                                onSort={handleSort}
                                align="right"
                            />
                            <SortableTableHeader
                                columnKey="currentMargin"
                                title="Current Margin"
                                sortConfig={sortConfig}
                                onSort={handleSort}
                                align="right"
                            />
                            <th>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {sortedPositions.map((position, index) => (
                            <tr
                                key={`${position.symbol}-${position.accountId}-${index}`}
                                className={selectedPosition?.positionId === position.positionId ? 'selected-row' : ''}
                                onClick={() => handleRowClick(position)}
                                style={{ cursor: 'pointer' }}
                            >
                                <td className="symbol-cell">{position.symbol}</td>
                                <td>{formatDate(position.dateOpened)}</td>
                                <td className="number-cell">{position.daysOpened ?? '-'}</td>
                                <td className="number-cell">{formatNumber(position.quantity, 4)}</td>
                                <td className="number-cell">{formatCurrency(position.costPrice)}</td>
                                <td className="number-cell">{formatCurrency(position.averagePrice)}</td>
                                <td className="number-cell">{formatCurrency(position.value)}</td>
                                <td className={`number-cell ${(position.unrealizedPnL ?? 0) >= 0 ? 'positive' : 'negative'}`}>
                                    {formatCurrency(position.unrealizedPnL)}
                                </td>
                                <td className={`number-cell ${(position.percentChange ?? 0) >= 0 ? 'positive' : 'negative'}`}>
                                    {formatPercent(position.percentChange)}
                                </td>
                                <td className={`number-cell ${(position.currentMargin ?? 0) >= 0 ? 'positive' : 'negative'}`}>
                                    {formatCurrency(position.currentMargin)}
                                </td>
                                <td>
                                    <button
                                        className="add-note-button"
                                        onClick={(e) => {
                                            e.stopPropagation();
                                            handleAddNoteClick(position.positionId);
                                        }}
                                    >
                                        Add Note
                                    </button>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                    <tfoot>
                        <tr className="summary-row">
                            <td colSpan={9} className="summary-label" style={{ textAlign: 'right', fontWeight: 'bold' }}>
                                Total Margin:
                            </td>
                            <td
                                className={`number-cell summary-value ${totalCurrentMargin >= 0 ? 'positive' : 'negative'}`}
                                style={{ fontWeight: 'bold' }}
                            >
                                {formatCurrency(totalCurrentMargin)}
                            </td>
                            <td></td>
                        </tr>
                    </tfoot>
                </table>
            </div>

            {/* Notes Section */}
            {selectedPosition && (
                <div className="notes-section">
                    <h2>Notes for {selectedPosition.symbol}</h2>
                    {notesLoading ? (
                        <p className="notes-loading">Loading notes...</p>
                    ) : notes.length === 0 ? (
                        <p className="notes-empty">No notes available for this position.</p>
                    ) : (
                        <div className="notes-table-container">
                            <table className="notes-table">
                                <thead>
                                    <tr>
                                        <th>Date</th>
                                        <th>Comment</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {notes.map((note) => (
                                        <tr key={note.id}>
                                            <td className="note-date">{new Date(note.entryDate).toLocaleDateString()}</td>
                                            <td className="note-comment">{note.comment}</td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                    )}
                </div>
            )}

            {/* Add Note Modal */}
            <AddNoteModal
                isOpen={isNoteModalOpen}
                onClose={handleCloseNoteModal}
                onSubmit={handleSubmitNote}
                positionId={selectedPositionId ?? 0}
            />
        </div>
    );
}

export default OpenPositionList;
