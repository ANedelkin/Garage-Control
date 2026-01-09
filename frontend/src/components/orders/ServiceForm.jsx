import React, { useState, useEffect } from 'react';
import { partApi } from '../../services/partApi';
import TimeSlotPicker from '../common/TimeSlotPicker';

const ServiceForm = ({ service, index, updateService, removeService, jobTypes, workers, allParts = [] }) => {
    // For part search
    const [partSearch, setPartSearch] = useState('');
    const [activePartIndex, setActivePartIndex] = useState(null);
    const [suggestions, setSuggestions] = useState([]);

    const handleChange = (field, value) => {
        updateService(service.id, field, value);
    };

    const addPart = (part) => {
        const newPart = {
            partId: part.id,
            name: part.name,
            quantity: 1,
            price: part.price
        };
        // If we were searching for a specific row, update that row
        if (activePartIndex !== null) {
            const newParts = [...service.parts];
            newParts[activePartIndex] = newPart;
            updateService(service.id, 'parts', newParts);
            setActivePartIndex(null);
        } else {
            // Otherwise add new
            updateService(service.id, 'parts', [...service.parts, newPart]);
        }
        setPartSearch('');
        setSuggestions([]);
    };

    const removePart = (partIndex) => {
        const newParts = [...service.parts];
        newParts.splice(partIndex, 1);
        updateService(service.id, 'parts', newParts);
    };

    const handlePartSearch = (val, rowIndex) => {
        setPartSearch(val);
        // Update the row's name field instantly for typing feeling
        const newParts = [...service.parts];
        newParts[rowIndex] = { ...newParts[rowIndex], name: val };
        updateService(service.id, 'parts', newParts);

        setActivePartIndex(rowIndex);

        if (!val.trim()) {
            setSuggestions([]);
            return;
        }

        const filtered = allParts.filter(p =>
            p.name.toLowerCase().includes(val.toLowerCase()) ||
            p.partNumber.toLowerCase().includes(val.toLowerCase())
        );
        setSuggestions(filtered);
    };

    const addNewRow = () => {
        updateService(service.id, 'parts', [...service.parts, { partId: '', name: '', quantity: 1, price: 0 }]);
    };

    const updatePartRow = (partIndex, field, val) => {
        const newParts = [...service.parts];
        newParts[partIndex] = { ...newParts[partIndex], [field]: val };
        updateService(service.id, 'parts', newParts);
    };

    return (
        <div className="service-tile">
            <div className="orders-header">
                <h4>Service #{index + 1}</h4>
                <button type="button" className="btn delete" onClick={() => removeService(service.id)}>
                    <i className="fa-solid fa-trash"></i>
                </button>
            </div>

            <div className="service-form">
                <div className="form-row-4">
                    <div className="form-group">
                        <label>Job Type</label>
                        <select value={service.jobTypeId} onChange={e => handleChange('jobTypeId', e.target.value)}>
                            <option value="">Select Type</option>
                            {jobTypes.map(jt => <option key={jt.id} value={jt.id}>{jt.name}</option>)}
                        </select>
                    </div>

                    <div className="form-group">
                        <label>Mechanic</label>
                        <select value={service.workerId} onChange={e => handleChange('workerId', e.target.value)}>
                            <option value="">Select Mechanic</option>
                            {workers
                                .filter(w => !service.jobTypeId || (w.jobTypeIds && w.jobTypeIds.includes(service.jobTypeId)))
                                .map(w => <option key={w.id} value={w.id}>{w.name}</option>)
                            }
                        </select>
                    </div>

                    <div className="form-group">
                        <label>Labor Cost</label>
                        <input type="number" step="0.01" value={service.laborCost} onChange={e => handleChange('laborCost', parseFloat(e.target.value))} />
                    </div>

                    <div className="form-group" style={{ display: 'flex', flexDirection: 'column' }}>
                        <label>Time Slot</label>
                        <TimeSlotPicker
                            worker={workers.find(w => w.id === service.workerId)}
                            initialStart={service.startTime}
                            initialEnd={service.endTime}
                            onTimeSelect={(start, end) => {
                                handleChange('startTime', start);
                                handleChange('endTime', end);
                            }}
                        />
                    </div>
                </div>

                <div className="form-group full-width">
                    <label>Description</label>
                    <textarea value={service.description} onChange={e => handleChange('description', e.target.value)} placeholder="Describe..." />
                </div>
            </div>

            <div className="parts-table-wrapper">
                <h5>Parts</h5>
                <table className="parts-table" style={{ overflow: 'visible' }}>
                    <thead>
                        <tr>
                            <th>Part Name / Number</th>
                            <th style={{ width: '80px' }}>Qty</th>
                            <th style={{ width: '100px' }}>Unit Price</th>
                            <th style={{ width: '100px' }}>Total</th>
                            <th style={{ width: '50px' }}>Action</th>
                        </tr>
                    </thead>
                    <tbody>
                        {service.parts.map((p, i) => (
                            <tr key={i} style={{ position: 'relative' }}>
                                <td>
                                    <input
                                        type="text"
                                        value={p.name}
                                        onChange={e => handlePartSearch(e.target.value, i)}
                                        placeholder="Search Part..."
                                        onFocus={() => setActivePartIndex(i)}
                                    />
                                    {activePartIndex === i && suggestions.length > 0 && (
                                        <ul className="car-suggestions" style={{ top: '100%', left: 0, width: '100%' }}>
                                            {suggestions.map(part => (
                                                <li key={part.id} onClick={() => addPart(part)}>
                                                    <b>{part.name}</b> ({part.partNumber}) - ${part.price}
                                                </li>
                                            ))}
                                        </ul>
                                    )}
                                </td>
                                <td>
                                    <input
                                        type="number"
                                        value={p.quantity}
                                        onChange={e => updatePartRow(i, 'quantity', parseInt(e.target.value))}
                                    />
                                </td>
                                <td>
                                    <input
                                        type="number"
                                        step="0.01"
                                        value={p.price}
                                        onChange={e => updatePartRow(i, 'price', parseFloat(e.target.value))}
                                    />
                                </td>
                                <td>
                                    {(p.quantity * p.price).toFixed(2)}
                                </td>
                                <td>
                                    <button type="button" className="btn icon-btn delete" onClick={() => removePart(i)}>
                                        <i className="fa-solid fa-trash"></i>
                                    </button>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
                <button type="button" className="btn add-part-btn" onClick={addNewRow}>+ Add Part Row</button>
            </div>
        </div>
    );
};

export default ServiceForm;
