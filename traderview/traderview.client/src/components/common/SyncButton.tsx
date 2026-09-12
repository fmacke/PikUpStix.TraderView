import React, { useState } from 'react';

interface SyncButtonProps {
    label: string;
    syncingLabel?: string;
    title?: string;
    className?: string;
    onSync: () => Promise<{ message: string }>;
    onSuccess?: () => void | Promise<void>;
    onError?: (error: Error) => void;
}

export const SyncButton: React.FC<SyncButtonProps> = ({
    label,
    syncingLabel = 'Syncing...',
    title,
    className = 'sync-button',
    onSync,
    onSuccess,
    onError
}) => {
    const [isSyncing, setIsSyncing] = useState(false);
    const [statusMessage, setStatusMessage] = useState<string | null>(null);

    const handleClick = async () => {
        try {
            setIsSyncing(true);
            setStatusMessage(null);

            const result = await onSync();
            setStatusMessage(`✓ ${result.message}`);

            if (onSuccess) {
                await onSuccess();
            }
        } catch (err) {
            const errorObj = err instanceof Error ? err : new Error('Sync operation failed.');
            setStatusMessage(`✗ ${errorObj.message}`);
            if (onError) {
                onError(errorObj);
            }
            console.error(errorObj);
        } finally {
            setIsSyncing(false);
        }
    };

    return (
        <div className="sync-button-container" style={{ display: 'inline-flex', alignItems: 'center', gap: '8px' }}>
            <button
                onClick={handleClick}
                disabled={isSyncing}
                className={className}
                title={title ?? label}
            >
                {isSyncing ? syncingLabel : label}
            </button>
            {statusMessage && (
                <span className={statusMessage.startsWith('✓') ? 'sync-success' : 'sync-error'}>
                    {statusMessage}
                </span>
            )}
        </div>
    );
};

export default SyncButton;