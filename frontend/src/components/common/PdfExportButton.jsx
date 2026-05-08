import React from 'react';
import { exportToPdf } from '../../Utilities/exportToExcel';
import { useStatus } from '../../context/StatusContext';

const PdfExportButton = ({ endpoint, className = '', text = 'Export to PDF', disabled = false }) => {
    const { showStatus } = useStatus();

    const handleClick = async (e) => {
        e.stopPropagation();
        showStatus('Generating PDF file...', 'loading');
        try {
            await exportToPdf(endpoint);
            showStatus('PDF file generated!', 'success');
        } catch (error) {
            showStatus('Failed to generate PDF file', 'error');
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
