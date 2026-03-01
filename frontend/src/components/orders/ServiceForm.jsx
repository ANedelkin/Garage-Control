import React, { useState } from 'react';
import { useAuth } from '../../context/AuthContext';
import PartTransferPopup from './PartTransferPopup';
import DropDown from '../common/Dropdown';
import TimeSlotPicker from '../common/TimeSlotPicker';
import '../../assets/css/common/status.css';

const ServiceForm = ({
    service,
    index,
    updateService,
    removeService,
    jobTypes,
    workers,
    allParts = [],
    mechanicView = false
}) => {

    const [partSearch, setPartSearch] = useState('');
    const [activePartIndex, setActivePartIndex] = useState(null);
    const [suggestions, setSuggestions] = useState([]);
    const [transferInfo, setTransferInfo] = useState({ isOpen: false, partIndex: null });

    const { user, accesses } = useAuth();
    const hasStockAccess = accesses.includes('Parts Stock');
    const isAssignedWorker = user && service.workerId === user.workerId;

    const handleChange = (field, value) => {
        updateService(service.id, field, value);

        if (field === 'status' && value === 3) {
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
            plannedQuantity: 1,
            sentQuantity: 1,
            usedQuantity: 0,
            requestedQuantity: 0,
            price: part.price
        };

        if (activePartIndex !== null) {
            const newParts = [...service.parts];
            newParts[activePartIndex] = newPart;
            updateService(service.id, 'parts', newParts);
            setActivePartIndex(null);
        } else {
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
        updateService(service.id, 'parts', [
            ...service.parts,
            { partId: '', name: '', plannedQuantity: 1, sentQuantity: 1, usedQuantity: 0, requestedQuantity: 0, price: 0 }
        ]);
    };

    const updatePartRow = (partIndex, field, val) => {
        const newParts = [...service.parts];
        let clampedVal = val;

        const p = newParts[partIndex];
        if (field === 'plannedQuantity') {
            clampedVal = Math.max(val, p.sentQuantity || 0);
        } else if (field === 'sentQuantity') {
            clampedVal = Math.max(Math.min(val, p.plannedQuantity || 0), p.usedQuantity || 0);
        } else if (field === 'usedQuantity') {
            clampedVal = Math.min(val, p.sentQuantity || 0);
        }

        newParts[partIndex] = { ...p, [field]: clampedVal };
        updateService(service.id, 'parts', newParts);
    };

    const handleTransfer = (partIndex, transferQty) => {
        const newParts = [...service.parts];
        const p = newParts[partIndex];
        const updatedPart = {
            ...p,
            plannedQuantity: (p.plannedQuantity || 0) + transferQty,
            requestedQuantity: (p.requestedQuantity || 0) - transferQty
        };
        newParts[partIndex] = updatedPart;
        updateService(service.id, 'parts', newParts);
    };

    return (
        <div className="tile">

            <div className="tile-header">
                <div className="header">
                    {/* Only render Job Type field if mechanicView is false */}
                    {!mechanicView && (
                        <div>
                            <label>Job Type</label>
                            <DropDown
                                value={service.jobTypeId}
                                onChange={e => handleChange('jobTypeId', e.target.value)}
                                disabled={mechanicView || service.status === 2}
                            >
                                <option value="">Select Type</option>
                                {jobTypes.map(jt =>
                                    <option key={jt.id} value={jt.id}>{jt.name}</option>
                                )}
                            </DropDown>
                        </div>
                    )}
                </div>

                {removeService && !mechanicView && service.status !== 2 && (
                    <button
                        type="button"
                        className="btn delete"
                        onClick={() => removeService(service.id)}
                    >
                        <i className="fa-solid fa-trash"></i>
                    </button>
                )}
            </div>


            <div className={`service-form ${mechanicView ? 'mechanic-layout' : ''}`}>

                {mechanicView ? (

                    <div className="mechanic-grid">

                        {/* LEFT COLUMN */}
                        <div className="mechanic-left">

                            <div className="form-section">
                                <label>Job Type</label>
                                <DropDown
                                    value={service.jobTypeId}
                                    onChange={() => { }}
                                    disabled={true}   // fully readonly
                                >
                                    {jobTypes.map(jt =>
                                        <option key={jt.id} value={jt.id}>{jt.name}</option>
                                    )}
                                </DropDown>
                            </div>

                            <div className="form-section">
                                <label>Job Status</label>
                                <DropDown
                                    className={`glow job-status-${service.status === 0
                                        ? 'pending'
                                        : service.status === 1
                                            ? 'inprogress'
                                            : 'finished'
                                        }`}
                                    value={service.status}
                                    onChange={e => handleChange('status', parseInt(e.target.value))}
                                >
                                    <option value={0}>Pending</option>
                                    <option value={1}>In Progress</option>
                                    <option value={2}>Finished</option>
                                </DropDown>
                            </div>

                            <div className="form-section">
                                <label>Time Slot</label>
                                <TimeSlotPicker
                                    worker={workers.find(w => w.id === service.workerId)}
                                    initialStart={service.startTime}
                                    initialEnd={service.endTime}
                                    readonly={true}      // prevents opening or changing
                                    onTimeSelect={null}   // ensure user cannot change
                                />
                            </div>

                        </div>

                        {/* RIGHT COLUMN */}
                        <div className="mechanic-right">
                            <div className="form-section grow">
                                <label>Description</label>
                                <textarea
                                    className="description"
                                    value={service.description}
                                    readOnly
                                />
                            </div>
                        </div>

                    </div>

                ) : (

                    <>
                        <div className="form-row-4">

                            <div className="form-section">
                                <label>Job Status</label>
                                <DropDown
                                    className={`glow job-status-${service.status === 0
                                        ? 'pending'
                                        : service.status === 1
                                            ? 'inprogress'
                                            : 'finished'
                                        }`}
                                    value={service.status}
                                    onChange={e => handleChange('status', parseInt(e.target.value))}
                                >
                                    <option value={0}>Pending</option>
                                    <option value={1}>In Progress</option>
                                    <option value={2}>Finished</option>
                                </DropDown>
                            </div>

                            {!mechanicView && (
                                <div className="form-section">
                                    <label>Mechanic</label>
                                    <DropDown
                                        value={service.workerId}
                                        onChange={e => handleChange('workerId', e.target.value)}
                                        disabled={service.status === 2}
                                    >
                                        <option value="">Select Mechanic</option>
                                        {workers
                                            .filter(w =>
                                                !service.jobTypeId ||
                                                (w.jobTypeIds && w.jobTypeIds.includes(service.jobTypeId))
                                            )
                                            .map(w =>
                                                <option key={w.id} value={w.id}>{w.name}</option>
                                            )
                                        }
                                    </DropDown>
                                </div>
                            )}

                            {!mechanicView && (
                                <div className="form-section">
                                    <label>Labor Cost</label>
                                    <input
                                        type="number"
                                        step="0.01"
                                        value={service.laborCost}
                                        onChange={e => handleChange('laborCost', parseFloat(e.target.value))}
                                        disabled={service.status === 2}
                                    />
                                </div>
                            )}

                            <div className="form-section" style={{ display: 'flex', flexDirection: 'column' }}>
                                <label>Time Slot</label>
                                <TimeSlotPicker
                                    worker={workers.find(w => w.id === service.workerId)}
                                    initialStart={service.startTime}
                                    initialEnd={service.endTime}
                                    readonly={service.status === 2}
                                    onTimeSelect={(start, end) => {
                                        handleChange('startTime', start);
                                        handleChange('endTime', end);
                                    }}
                                />
                            </div>

                        </div>

                        <div className="form-section">
                            <label>Description</label>
                            <textarea
                                className="description"
                                value={service.description}
                                onChange={e => handleChange('description', e.target.value)}
                                placeholder="Describe..."
                                disabled={service.status === 2}
                            />
                        </div>
                    </>

                )}

            </div>

            <div className="parts-table-wrapper">
                <label>Parts</label>
                <table className="table" style={{ overflow: 'visible' }}>
                    <thead>
                        <tr>
                            <th>Part Name / Number</th>
                            <th style={{ width: '90px' }}>Planned</th>
                            <th style={{ width: '90px' }}>Sent</th>
                            <th style={{ width: '90px' }}>Used</th>
                            <th style={{ width: '140px' }}>Req</th>
                            <th style={{ width: '100px' }}>Unit Price</th>
                            <th style={{ width: '130px' }}>Total Projected</th>
                            <th style={{ width: '130px' }}>Total Spent</th>
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
                                            disabled={service.status === 2}
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
                                            min={p.sentQuantity || 0}
                                            onChange={e => updatePartRow(i, 'plannedQuantity', parseFloat(e.target.value))}
                                            disabled={service.status === 2 || mechanicView || (!hasStockAccess && !isAssignedWorker)}
                                            title={(service.status === 2) ? "Job is finished" : (mechanicView || (!hasStockAccess && !isAssignedWorker)) ? "Only Parts Stock access or assigned worker can edit this" : ""}
                                        />
                                    </td>
                                    <td>
                                        <input
                                            type="number"
                                            className={usedError ? 'input-error' : ''}
                                            value={p.sentQuantity}
                                            min={p.usedQuantity || 0}
                                            max={p.plannedQuantity || 0}
                                            onChange={e => updatePartRow(i, 'sentQuantity', parseFloat(e.target.value))}
                                            disabled={service.status === 2 || mechanicView || !hasStockAccess}
                                            title={(service.status === 2) ? "Job is finished" : (mechanicView || !hasStockAccess) ? "Only Parts Stock access can edit this" : ""}
                                        />
                                    </td>
                                    <td>
                                        <input
                                            type="number"
                                            value={p.usedQuantity}
                                            min={0}
                                            max={p.sentQuantity || 0}
                                            onChange={e => updatePartRow(i, 'usedQuantity', parseFloat(e.target.value))}
                                            disabled={service.status === 2 || !isAssignedWorker}
                                            title={(service.status === 2) ? "Job is finished" : !isAssignedWorker ? "Only assigned worker can edit this" : ""}
                                        />
                                    </td>
                                    <td>
                                        <div style={{ display: 'flex', alignItems: 'center', gap: '5px' }}>
                                            <input
                                                type="number"
                                                value={p.requestedQuantity}
                                                min={0}
                                                onChange={e => updatePartRow(i, 'requestedQuantity', parseFloat(e.target.value))}
                                                disabled={service.status === 2 || !isAssignedWorker}
                                                title={(service.status === 2) ? "Job is finished" : !isAssignedWorker ? "Only assigned worker can edit this" : ""}
                                                style={{ flex: 1 }}
                                            />
                                            <button
                                                type="button"
                                                className="btn icon-btn"
                                                disabled={service.status === 2 || !p.requestedQuantity}
                                                onClick={() => setTransferInfo({ isOpen: true, partIndex: i })}
                                                title="Transfer to Planned"
                                            >
                                                <i className="fa-solid fa-plus"></i>
                                            </button>
                                        </div>
                                    </td>
                                    <td>
                                        {p.price.toFixed(2)}
                                    </td>
                                    <td>
                                        {(p.plannedQuantity * p.price).toFixed(2)}
                                    </td>
                                    <td>
                                        {(p.usedQuantity * p.price).toFixed(2)}
                                    </td>
                                    <td>
                                        <button type="button" className="btn icon-btn delete" onClick={() => removePart(i)} disabled={service.status === 2}>
                                            <i className="fa-solid fa-trash"></i>
                                        </button>
                                    </td>
                                </tr>
                            )
                        })}
                    </tbody>
                </table>
                <div className="form-footer">
                    <button type="button" className="btn" onClick={addNewRow} disabled={service.status === 2}>+ Add Part</button>
                </div>
            </div>

            <PartTransferPopup
                isOpen={transferInfo.isOpen}
                onClose={() => setTransferInfo({ isOpen: false, partIndex: null })}
                onConfirm={(qty) => handleTransfer(transferInfo.partIndex, qty)}
                maxQuantity={transferInfo.partIndex !== null ? service.parts[transferInfo.partIndex].requestedQuantity : 0}
                partName={transferInfo.partIndex !== null ? service.parts[transferInfo.partIndex].name : ''}
            />
        </div>
    );
};

export default ServiceForm;
