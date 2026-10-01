import type { ListItem } from '../../types/api';

interface ListItemSelectProps {
    id: string;
    label: string;
    items: ListItem[];
    selectedId: number | null;
    onChange: (value: number | null) => void;
    isLoading: boolean;
    isDisabled: boolean;
    placeholder: string;
    className?: string;
    // Optional styling hooks for the label so callers can inject custom styles
    labelClassName?: string;
    labelStyle?: React.CSSProperties;
    // Render label and select inline (label to left, select to right) to match form rows
    inline?: boolean;
    // Optional wrapper class for the outer container when inline is true
    wrapperClassName?: string;
}

function ListItemSelect({
    id,
    label,
    items,
    selectedId,
    onChange,
    isLoading,
    isDisabled,
    placeholder,
    className = '',
    labelClassName = '',
    labelStyle,
    inline = false,
    wrapperClassName = ''
}: ListItemSelectProps) {
    const handleChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
        onChange(e.target.value ? parseInt(e.target.value) : null);
    };

    if (inline) {
        return (
            <div className={"flex items-center gap-4 mb-4 " + wrapperClassName}>
                <label htmlFor={id} className={labelClassName} style={labelStyle}>{label}</label>
                <select
                    id={id}
                    value={selectedId ?? ''}
                    onChange={handleChange}
                    disabled={isDisabled || isLoading}
                    className={className}
                >
                    <option value="">{placeholder}</option>
                    {items.map((item) => (
                        <option key={item.id} value={item.id}>
                            {item.name}
                        </option>
                    ))}
                </select>
            </div>
        );
    }

    return (
        <div className="form-group">
            <label htmlFor={id} className={labelClassName} style={labelStyle}>{label}</label>
            <select
                id={id}
                value={selectedId ?? ''}
                onChange={handleChange}
                disabled={isDisabled || isLoading}
                className={className}
            >
                <option value="">{placeholder}</option>
                {items.map((item) => (
                    <option key={item.id} value={item.id}>
                        {item.name}
                    </option>
                ))}
            </select>
        </div>
    );
}

export default ListItemSelect;
