import React, { useState, useEffect } from 'react';
import { useAuth } from '../../context/AuthContext';
import { partApi } from '../../services/partApi';
import DropDown from '../common/Dropdown';
import TimeSlotPicker from '../common/TimeSlotPicker';
import '../../assets/css/common/status.css';

const ServiceForm = ({ service, index, updateService, removeService, jobTypes, workers, allParts = [] }) => {
    // For part search
    const [partSearch, setPartSearch] = useState('');
    const [activePartIndex, setActivePartIndex] = useState(null);
    const [suggestions, setSuggestions] = useState([]);

    const { user, accesses } = useAuth();
    const hasStockAccess = accesses.includes('Parts Stock');
    const isAssignedWorker = user && service.workerId === user.workerId;

    const handleChange = (field, value) => {
        updateService(service.id, field, value);

        if (field === 'status' && value === 3) { // 3 = Finished
            const now = new Date();
            const pad = (n) => n.toString().padStart(2, '0');
            const localISO = `${now.getFullYear()}-${pad(now.getMonth() + 1)}-${pad(now.getDate())}T${pad(now.getHours())}:00:00`;
            updateService(service.id, 'endTime', localISO);
        }
    };

    const addPart = (part) => {
        const newPart = {
            partId: part.id,
            name: part.name,
            name: part.name,
            plannedQuantity: 1,
            sentQuantity: 1,
            usedQuantity: 0,
            requestedQuantity: 0,
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
        updateService(service.id, 'parts', [...service.parts, { partId: '', name: '', plannedQuantity: 1, sentQuantity: 1, usedQuantity: 0, requestedQuantity: 0, price: 0 }]);
    };

    const updatePartRow = (partIndex, field, val) => {
        const newParts = [...service.parts];
        newParts[partIndex] = { ...newParts[partIndex], [field]: val };
        updateService(service.id, 'parts', newParts);
    };

    return (
        <div className="tile">
            <div className="tile-header">
                <div className="header">
                    <label>Job Type</label>
                    <DropDown value={service.jobTypeId} onChange={e => handleChange('jobTypeId', e.target.value)}>
                        <option value="">Select Type</option>
                        {jobTypes.map(jt => <option key={jt.id} value={jt.id}>{jt.name}</option>)}
                    </DropDown>
                </div>
                {removeService && (
                    <button type="button" className="btn delete" onClick={() => removeService(service.id)}>
                        <i className="fa-solid fa-trash"></i>
                    </button>
                )}
            </div>

            <div className="service-form">
                <div className="form-row-4">
                    <div className="form-section">
                        <label>Job Status</label>
                        <DropDown
                            className={`status-glow job-status-${service.status === 0 ? 'pending' : service.status === 1 ? 'inprogress' : 'finished'}`}
                            value={service.status}
                            onChange={e => handleChange('status', parseInt(e.target.value))}
                        >
                            <option value={0}>Pending</option>
                            <option value={1}>In Progress</option>
                            <option value={2}>Finished</option>
                        </DropDown>
                    </div>

                    <div className="form-section">
                        <label>Mechanic</label>
                        <DropDown value={service.workerId} onChange={e => handleChange('workerId', e.target.value)}>
                            <option value="">Select Mechanic</option>
                            {workers
                                .filter(w => !service.jobTypeId || (w.jobTypeIds && w.jobTypeIds.includes(service.jobTypeId)))
                                .map(w => <option key={w.id} value={w.id}>{w.name}</option>)
                            }
                        </DropDown>
                    </div>

                    <div className="form-section">
                        <label>Labor Cost</label>
                        <input type="number" step="0.01" value={service.laborCost} onChange={e => handleChange('laborCost', parseFloat(e.target.value))} />
                    </div>

                    <div className="form-section" style={{ display: 'flex', flexDirection: 'column' }}>
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

                <div className="form-section">
                    <label>Description</label>
                    <textarea className="description" value={service.description} onChange={e => handleChange('description', e.target.value)} placeholder="Describe..." />
                </div>
            </div>

            <div className="parts-table-wrapper">
                <label>Parts</label>
                <table className="table" style={{ overflow: 'visible' }}>
                    <thead>
                        <tr>
                            <th>Part Name / Number</th>
                            <th style={{ width: '80px' }}>Planned</th>
                            <th style={{ width: '80px' }}>Sent</th>
                            <th style={{ width: '80px' }}>Used</th>
                            <th style={{ width: '80px' }}>Req</th>
                            <th style={{ width: '100px' }}>Unit Price</th>
                            <th style={{ width: '100px' }}>Total</th>
                            <th style={{ width: '50px' }}></th>
                        </tr>
                    </thead>
                    <tbody>
                        {service.parts.map((p, i) => {
                            // Validation/Error styling logic
                            const planned = p.plannedQuantity || 0;
                            const sent = p.sentQuantity || 0;
                            const used = p.usedQuantity || 0;

                            const sentError = sent > planned;
                            const usedError = used > sent;

                            return (
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
                                            className={sentError ? 'input-error' : ''}
                                            value={p.plannedQuantity}
                                            onChange={e => updatePartRow(i, 'plannedQuantity', parseFloat(e.target.value))}
                                            disabled={!hasStockAccess && !isAssignedWorker}
                                            title={(!hasStockAccess && !isAssignedWorker) ? "Only Parts Stock access or assigned worker can edit this" : ""}
                                        />
                                    </td>
                                    <td>
                                        <input
                                            type="number"
                                            className={usedError ? 'input-error' : ''}
                                            value={p.sentQuantity}
                                            onChange={e => updatePartRow(i, 'sentQuantity', parseFloat(e.target.value))}
                                            disabled={!hasStockAccess}
                                            title={!hasStockAccess ? "Only Parts Stock access can edit this" : ""}
                                        />
                                    </td>
                                    <td>
                                        <input
                                            type="number"
                                            value={p.usedQuantity}
                                            onChange={e => updatePartRow(i, 'usedQuantity', parseFloat(e.target.value))}
                                            disabled={!isAssignedWorker}
                                            title={!isAssignedWorker ? "Only assigned worker can edit this" : ""}
                                        />
                                    </td>
                                    <td>
                                        <input
                                            type="number"
                                            value={p.requestedQuantity}
                                            onChange={e => updatePartRow(i, 'requestedQuantity', parseFloat(e.target.value))}
                                            disabled={!isAssignedWorker}
                                            title={!isAssignedWorker ? "Only assigned worker can edit this" : ""}
                                        />
                                    </td>
                                    <td>
                                        {p.price.toFixed(2)}
                                    </td>
                                    <td>
                                        {(p.usedQuantity * p.price).toFixed(2)}
                                    </td>
                                    <td>
                                        <button type="button" className="btn icon-btn delete" onClick={() => removePart(i)}>
                                            <i className="fa-solid fa-trash"></i>
                                        </button>
                                    </td>
                                </tr>
                            )
                        })}
                    </tbody>
                </table>
                <div className="form-footer">
                    <button type="button" className="btn" onClick={addNewRow}>+ Add Part</button>
                    <div style={{ fontSize: '0.8em', color: '#888', marginTop: '5px' }}>
                        Planned: Stock or Worker. Sent: Stock only. Used/Req: Worker favored.
                    </div>
                </div>
            </div>
        </div>
    );
};

export default ServiceForm;
