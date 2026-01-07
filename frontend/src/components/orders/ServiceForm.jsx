import React, { useState, useEffect } from 'react';
import { partApi } from '../../services/partApi';

const ServiceForm = ({ service, index, updateService, removeService, jobTypes, workers }) => {
    // For part search
    const [partSearch, setPartSearch] = useState('');
    const [partSuggestions, setPartSuggestions] = useState([]);

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
        updateService(service.id, 'parts', [...service.parts, newPart]);
        setPartSearch('');
        setPartSuggestions([]);
    };

    const removePart = (partIndex) => {
        const newParts = [...service.parts];
        newParts.splice(partIndex, 1);
        updateService(service.id, 'parts', newParts);
    };

    const handlePartSearch = async (val) => {
        setPartSearch(val);
        if (val.length > 2) {
            // Mock search or implement search endpoint? use folder content recursive?
            // For now let's just use what we have or implementing a part search endpoint is better.
            // But to keep it simple, assume we have a search endpoint or just load all parts.
            // Let's rely on simple text input for now or just mock if API not ready.
            // Actually partApi doesn't have search.
            // Let's just implement a simple input for Part ID for now or skip suggestion.
        }
    };

    // Note: For real implementation, we need Part Search API.
    // For now, I will add a simple mechanic to add parts by ID or just pick from a small list if possible?
    // Or, let's just assume we can select from a dropdown of ALL parts (might be heavy but ok for prototype) 
    // OR: Just add a "Select Part" button that opens a mini stock browser?
    // Let's stick to the prototype Design: "Add Part" button adds a row.

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
                <div>
                    <label>Job Type</label>
                    <select value={service.jobTypeId} onChange={e => handleChange('jobTypeId', e.target.value)}>
                        <option value="">Select Type</option>
                        {jobTypes.map(jt => <option key={jt.id} value={jt.id}>{jt.name}</option>)}
                    </select>

                    <label>Mechanic</label>
                    <select value={service.workerId} onChange={e => handleChange('workerId', e.target.value)}>
                        <option value="">Select Mechanic</option>
                        {workers.map(w => <option key={w.id} value={w.id}>{w.firstName} {w.lastName}</option>)}
                    </select>

                    <label>Labor Cost</label>
                    <input type="number" step="0.01" value={service.laborCost} onChange={e => handleChange('laborCost', parseFloat(e.target.value))} />

                </div>

                <div>
                    <label>Start Time</label>
                    <input type="datetime-local" value={service.startTime} onChange={e => handleChange('startTime', e.target.value)} />

                    <label>End Time</label>
                    <input type="datetime-local" value={service.endTime} onChange={e => handleChange('endTime', e.target.value)} />

                    <label>Description</label>
                    <textarea value={service.description} onChange={e => handleChange('description', e.target.value)} placeholder="Describe..." />
                </div>
            </div>

            <div className="parts-table-wrapper">
                <h5>Parts</h5>
                <table className="parts-table">
                    <thead>
                        <tr>
                            <th>Part ID (Manual)</th>
                            <th>Qty</th>
                            <th>Action</th>
                        </tr>
                    </thead>
                    <tbody>
                        {service.parts.map((p, i) => (
                            <tr key={i}>
                                <td>
                                    <input
                                        type="text"
                                        value={p.partId}
                                        onChange={e => updatePartRow(i, 'partId', e.target.value)}
                                        placeholder="Paste Part ID"
                                    />
                                </td>
                                <td>
                                    <input
                                        type="number"
                                        value={p.quantity}
                                        onChange={e => updatePartRow(i, 'quantity', parseInt(e.target.value))}
                                        style={{ width: '60px' }}
                                    />
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
