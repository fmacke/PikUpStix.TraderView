export function formatUtcDateTime(isoString?: string): string {
    if (!isoString) return '';
    return isoString.replace('T', ' ').substring(0, 16);
}