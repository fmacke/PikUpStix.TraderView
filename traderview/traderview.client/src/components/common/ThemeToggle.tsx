import './ThemeToggle.css';
import { useTheme } from '../../hooks/useTheme';

export default function ThemeToggle() {
    const { theme, toggleTheme } = useTheme();

    const getIcon = () => {
        return theme === 'light' ? '☀️' : '🌙';
    };

    const getLabel = () => {
        return theme === 'light' ? 'Light Mode' : 'Dark Mode';
    };

    return (
        <button
            onClick={toggleTheme}
            className="theme-toggle"
            title={`Switch theme (current: ${getLabel()})`}
            aria-label={`Toggle theme mode, currently ${getLabel()}`}
        >
            <span className="theme-icon">{getIcon()}</span>
            <span className="theme-label">{getLabel()}</span>
        </button>
    );
}
