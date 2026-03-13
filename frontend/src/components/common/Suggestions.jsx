import React, { useState, useEffect, useRef, useImperativeHandle, forwardRef } from 'react';
import '../../assets/css/common/suggestions.css';

const Suggestions = forwardRef(({
    suggestions = [],
    isOpen = false,
    onSelect,
    onClose,
    renderItem,
    maxHeight = '200px',
    style = {}
}, ref) => {
    const [highlightedIndex, setHighlightedIndex] = useState(-1);

    useEffect(() => {
        if (isOpen && suggestions.length > 0) {
            setHighlightedIndex(0);
        } else {
            setHighlightedIndex(-1);
        }
    }, [isOpen, suggestions.length]);

    const handleKeyDown = (e) => {
        if (!isOpen || suggestions.length === 0) return;

        switch (e.key) {
            case 'ArrowDown':
                e.preventDefault();
                setHighlightedIndex(prev =>
                    prev < suggestions.length - 1 ? prev + 1 : prev
                );
                break;
            case 'ArrowUp':
                e.preventDefault();
                setHighlightedIndex(prev =>
                    prev > 0 ? prev - 1 : 0
                );
                break;
            case 'Enter':
                e.preventDefault();
                if (highlightedIndex >= 0 && highlightedIndex < suggestions.length) {
                    onSelect(suggestions[highlightedIndex]);
                }
                break;
            case 'Escape':
                e.preventDefault();
                if (highlightedIndex >= 0 && highlightedIndex < suggestions.length) {
                    onSelect(suggestions[highlightedIndex]);
                }
                if (onClose) {
                    onClose();
                }
                break;
            default:
                break;
        }
    };

    useImperativeHandle(ref, () => ({
        handleKeyDown
    }));

    if (!isOpen || suggestions.length === 0) {
        return null;
    }

    const defaultStyle = {
        position: 'absolute',
        top: '100%',
        left: 0,
        right: 0,
        zIndex: 9999,
        maxHeight: maxHeight,
        overflowY: 'auto',
        ...style
    };

    return (
        <ul
            className="suggestions-list"
            style={defaultStyle}
        >
            {suggestions.map((item, index) => (
                <li
                    key={`${item.id || index}`}
                    className={`suggestion-item ${highlightedIndex === index ? 'highlighted' : ''}`}
                    onClick={() => onSelect(item)}
                    onMouseEnter={() => setHighlightedIndex(index)}
                    onMouseLeave={() => setHighlightedIndex(-1)}
                >
                    {renderItem ? renderItem(item) : item.name || String(item)}
                </li>
            ))}
        </ul>
    );
});

Suggestions.displayName = 'Suggestions';

export default Suggestions;
