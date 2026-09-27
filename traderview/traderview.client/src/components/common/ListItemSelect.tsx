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
}: ListItemSelectProps) {
    const handleChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
        onChange(e.target.value ? parseInt(e.target.value) : null);
    };

    return (
        <div className="form-group">
            <label htmlFor={id}>{label}</label>
            <select
                id={id}
                value={selectedId ?? ''}
                onChange={handleChange}
                disabled={isDisabled || isLoading}
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
