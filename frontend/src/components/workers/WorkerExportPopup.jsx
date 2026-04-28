import React, { useState } from 'react';
import { exportFile } from '../../Utilities/exportToExcel';

const WorkerExportPopup = ({ onClose }) => {
    const [selectedTypes, setSelectedTypes] = useState(['details']);

    const options = [
        { id: 'details', label: "Workers' Details", icon: 'fa-user-gear' },
        { id: 'schedules', label: "Working Schedules", icon: 'fa-calendar-days' },
        { id: 'leaves', label: "Worker Leaves", icon: 'fa-calendar-minus' }
    ];

    const toggleType = (id) => {
        setSelectedTypes(prev => 
            prev.includes(id) 
                ? (prev.length > 1 ? prev.filter(t => t !== id) : prev) 
                : [...prev, id]
        );
    };

    const handleExport = (format) => {
        const typesParam = selectedTypes.join(',');
        exportFile(`export/workers?types=${typesParam}`, format);
        onClose();
    };

    return (
        <div className="worker-export-popup" style={{ width: '420px' }}>
            <p>
                Select the information you want to include in the export. 
            </p>
            
            <div className="form-section">
                <label>Export Options</label>
                <div className="list-container">
                    {options.map(option => (
                        <div className="list-item" key={option.id}>
                            <label className="checkbox-item">
                                <input 
                                    type="checkbox" 
                                    checked={selectedTypes.includes(option.id)}
                                    onChange={() => toggleType(option.id)}
                                />
                                {option.label}
                            </label>
                        </div>
                    ))}
                </div>
            </div>

            <div className="form-footer">
                <button className="btn secondary" onClick={onClose}>Cancel</button>
                <div style={{ display: 'flex', gap: '8px' }}>
                    <button className="btn primary" onClick={() => handleExport('excel')}>
                        <i className="fa-solid fa-file-excel"></i>
                        Excel
                    </button>
                    <button className="btn primary" onClick={() => handleExport('pdf')} style={{ backgroundColor: '#e74c3c' }}>
                        <i className="fa-solid fa-file-pdf"></i>
                        PDF
                    </button>
                </div>
            </div>
        </div>
    );
};

export default WorkerExportPopup;
