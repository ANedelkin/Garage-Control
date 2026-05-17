import React from 'react';
import { exportToExcel } from '../../Utilities/exportToExcel';
import { useStatus } from '../../context/StatusContext';

const ExcelExportButton = ({ endpoint, onClick, className = '', text = 'Export to Excel', disabled = false }) => {
    const { showStatus } = useStatus();

    const handleClick = async (e) => {
        if (onClick) {
            onClick(e);
            return;
        }
        e.stopPropagation();
        if (!endpoint) return;
        showStatus('Generating Excel file...', 'loading');
        try {
            await exportToExcel(endpoint);
            showStatus('Excel file generated!', 'success');
        } catch (error) {
            showStatus('Failed to generate Excel file', 'error');
        }
    };

    return (
        <button
            className={`btn ${className}`}
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
