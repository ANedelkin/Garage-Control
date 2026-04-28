import React from 'react';
import { exportToPdf } from '../../Utilities/exportToExcel';

/**
 * Reusable button for PDF exports.
 * @param {string} endpoint - The API endpoint to call.
 * @param {string} className - Additional CSS classes.
 * @param {string} text - Button text (defaults to 'Export to PDF').
 * @param {boolean} disabled - Whether the button is disabled.
 */
const PdfExportButton = ({ endpoint, className = '', text = 'Export to PDF', disabled = false, onClick = null }) => {
    const handleClick = (e) => {
        e.stopPropagation();
        if (onClick) {
            onClick(e);
        } else {
            exportToPdf(endpoint);
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
            <i className="fa-solid fa-file-pdf" style={{ color: '#e74c3c', fontSize: '18px' }}></i>
        </button>
    );
};

export default PdfExportButton;
