import React from 'react';
import { exportToExcel } from '../../Utilities/exportToExcel';

/**
 * Reusable button for Excel exports.
 * @param {string} endpoint - The API endpoint to call.
 * @param {string} className - Additional CSS classes.
 * @param {string} text - Button text (defaults to 'Export to Excel').
 * @param {boolean} disabled - Whether the button is disabled.
 */
const ExcelExportButton = ({ endpoint, className = '', text = 'Export to Excel', disabled = false, onClick = null }) => {
    const handleClick = (e) => {
        e.stopPropagation();
        if (onClick) {
            onClick(e);
        } else {
            exportToExcel(endpoint);
        }
    };

    return (
        <button
            className={`btn secondary ${className}`}
            onClick={handleClick}
            disabled={disabled}
            title={text}
            type="button"
        >
            <i className="fa-solid fa-file-excel" style={{ color: '#22a15bff', fontSize: '18px' }}></i>
        </button>
    );
};

export default ExcelExportButton;
