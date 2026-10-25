import { useEffect, useState } from 'react';

type Theme = 'light' | 'dark';

export function useTheme() {
    const [theme, setTheme] = useState<Theme>(() => {
        // Get theme from localStorage, default to 'dark'
        const stored = localStorage.getItem('theme-preference') as Theme | null;
        return stored || 'dark';
    });

    useEffect(() => {
        // Save to localStorage whenever theme changes
        localStorage.setItem('theme-preference', theme);

        // Update document class and color-scheme
        const root = document.documentElement;
        root.setAttribute('data-theme', theme);
        root.style.colorScheme = theme;
    }, [theme]);

    useEffect(() => {
        // Listen for changes to data-theme attribute (for cross-component synchronization)
        const observer = new MutationObserver(() => {
            const root = document.documentElement;
            const newTheme = root.getAttribute('data-theme') as Theme | null;
            if (newTheme && newTheme !== theme) {
                setTheme(newTheme);
            }
        });

        observer.observe(document.documentElement, {
            attributes: true,
            attributeFilter: ['data-theme']
        });

        return () => observer.disconnect();
    }, [theme]);

    const toggleTheme = () => {
        setTheme(current => current === 'light' ? 'dark' : 'light');
    };

    return { theme, setTheme, toggleTheme };
}
