import React, { useState, useRef } from 'react';
import { useAuth } from '../../context/AuthContext';
import { usePopup } from '../../context/PopupContext';
import PartTransferPopup from './PartTransferPopup';
import DropDown from '../common/Dropdown';
import TimeSlotPicker from '../common/TimeSlotPicker';
import Popup from '../common/Popup';
import Suggestions from '../common/Suggestions';
import '../../assets/css/common/status.css';
import FieldError from '../common/FieldError.jsx';

const JobForm = ({
    job,
    index,
    updateJob,
    removeJob,
    jobTypes,
    workers,
    allParts = [],
    mechanicView = false,
    errors = {} // Ensure errors is passed
}) => {

    const [partSearch, setPartSearch] = useState('');
    const [activePartIndex, setActivePartIndex] = useState(null);
    const [suggestions, setSuggestions] = useState([]);
    const [expandedParts, setExpandedParts] = useState({});
    const suggestionsRef = useRef(null);
    const { addPopup, removeLastPopup } = usePopup();

    const { user, accesses } = useAuth();
    const hasStockAccess = accesses.includes('Parts Stock');
    const isAssignedWorker = user && job.workerId === user.workerId;

    const handleChange = (field, value) => {
        let finalValue = value;
        if (field === 'laborCost') {
            finalValue = value;
        }

        updateJob(job.id, field, finalValue);

        if (field === 'status' && finalValue === 3) {
            const now = new Date();
            const pad = (n) => n.toString().padStart(2, '0');
            const localISO = `${now.getFullYear()}-${pad(now.getMonth() + 1)}-${pad(now.getDate())}T${pad(now.getHours())}:00:00`;
            updateJob(job.id, 'endTime', localISO);
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
            const newParts = [...job.parts];
            newParts[activePartIndex] = {
                ...newParts[activePartIndex],
                partId: part.id,
                name: part.name,
                price: part.price
            };
            updateJob(job.id, 'parts', newParts);
            setActivePartIndex(null);
        } else {
            updateJob(job.id, 'parts', [...job.parts, newPart]);
        }

        setPartSearch('');
        setSuggestions([]);
    };

    const removePart = (partIndex) => {
        const newParts = [...job.parts];
        newParts.splice(partIndex, 1);
        updateJob(job.id, 'parts', newParts);
    };

    const handlePartSearch = (val, rowIndex) => {
        setPartSearch(val);

        const newParts = [...job.parts];
        newParts[rowIndex] = { ...newParts[rowIndex], name: val, partId: '' }; // Clear Id when name is changed
        updateJob(job.id, 'parts', newParts);

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
        updateJob(job.id, 'parts', [
            ...job.parts,
            { partId: '', name: '', plannedQuantity: 1, sentQuantity: 1, usedQuantity: 0, requestedQuantity: 0, price: 0 }
        ]);
    };

    const updatePartRow = (partIndex, field, val) => {
        const newParts = [...job.parts];
        const p = newParts[partIndex];
        newParts[partIndex] = { ...p, [field]: val };
        updateJob(job.id, 'parts', newParts);
    };

    const handleTransfer = (partIndex, transferQty) => {
        const newParts = [...job.parts];
        const p = newParts[partIndex];
        const updatedPart = {
            ...p,
            plannedQuantity: (p.plannedQuantity || 0) + transferQty,
            requestedQuantity: (p.requestedQuantity || 0) - transferQty
        };
        newParts[partIndex] = updatedPart;
        updateJob(job.id, 'parts', newParts);
        removeLastPopup();
    };

    const togglePartExpand = (index) => {
        setExpandedParts(prev => ({
            ...prev,
            [index]: !prev[index]
        }));
    };

    const getPartErrors = (index) => {
        if (!errors) return null;
        const prefix = `parts[${index}].`.toLowerCase();
        const partErrors = Object.keys(errors)
            .filter(key => key.toLowerCase().startsWith(prefix))
            .map(key => errors[key]);

        const flattened = partErrors.flat();
        if (flattened.length === 0) return null;
        return flattened.join(', ');
    };

    const openTransferPopup = (partIndex) => {
        const part = job.parts[partIndex];
        console.log("e");
        addPopup(
            'Transfer to Planned',
            <PartTransferPopup
                onClose={removeLastPopup}
                onConfirm={(qty) => handleTransfer(partIndex, qty)}
                maxQuantity={part.requestedQuantity}
                partName={part.name}
            />
        );
    };

    return (
        <form>
            <div className="tile">

                <div className="tile-header">
                    <div className="header">
                        {!mechanicView && (
                            <>
                                <label>Job Type</label>
                                <DropDown
                                    name="JobTypeId"
                                    value={job.jobTypeId}
                                    onChange={e => handleChange('jobTypeId', e.target.value)}
                                    disabled={mechanicView || job.status === 2}
                                >
                                    <option value="">Select Type</option>
                                    {jobTypes.map(jt =>
                                        <option key={jt.id} value={jt.id}>{jt.name}</option>
                                    )}
                                </DropDown>
                                <FieldError name="JobTypeId" errors={errors} />
                            </>
                        )}
                    </div>

                    {removeJob && !mechanicView && job.status !== 2 && (
                        <button
                            type="button"
                            className="btn delete"
                            onClick={() => removeJob(job.id)}
                        >
                            <i className="fa-solid fa-trash"></i>
                        </button>
                    )}
                </div>


                <div className={`job-form ${mechanicView ? 'mechanic-layout' : ''}`}>

                    {mechanicView ? (

                        <div className="mechanic-grid">

                            {/* LEFT COLUMN */}
                            <div className="mechanic-left">

                                <div className="form-section">
                                    <label>Job Type</label>
                                    <DropDown
                                        value={job.jobTypeId}
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
                                        className={`glow job-status-${job.status === 0
                                            ? 'pending'
                                            : job.status === 1
                                                ? 'inprogress'
                                                : 'done'
                                            }`}
                                        name="Status"
                                        value={job.status}
                                        onChange={e => handleChange('status', parseInt(e.target.value))}
                                    >
                                        <option value={0}>Pending</option>
                                        <option value={1}>In Progress</option>
                                        <option value={2}>Done</option>
                                    </DropDown>
                                    <FieldError name="Status" errors={errors} />
                                </div>

                                <div className="form-section">
                                    <label>Time Slot</label>
                                    <TimeSlotPicker
                                        worker={workers.find(w => w.id === job.workerId)}
                                        initialStart={job.startTime}
                                        initialEnd={job.endTime}
                                        readonly={true}      // prevents opening or changing
                                        excludeId={job.id}
                                        onTimeSelect={null}   // ensure user cannot change
                                    />
                                    <FieldError name="StartTime" errors={errors} />
                                </div>

                            </div>

                            {/* RIGHT COLUMN */}
                            <div className="mechanic-right">
                                <div className="form-section grow">
                                    <label>Description</label>
                                    <textarea
                                        className="description"
                                        name="Description"
                                        value={job.description}
                                        readOnly
                                    />
                                    <FieldError name="Description" errors={errors} />
                                </div>
                            </div>

                        </div>

                    ) : (

                        <>
                            <div className="form-row-4">

                                <div className="form-section">
                                    <label>Job Status</label>
                                    <DropDown
                                        className={`glow job-status-${job.status === 0
                                            ? 'pending'
                                            : job.status === 1
                                                ? 'inprogress'
                                                : 'done'
                                            }`}
                                        name="Status"
                                        value={job.status}
                                        onChange={e => handleChange('status', parseInt(e.target.value))}
                                    >
                                        <option value={0}>Pending</option>
                                        <option value={1}>In Progress</option>
                                        <option value={2}>Done</option>
                                    </DropDown>
                                    <FieldError name="Status" errors={errors} />
                                </div>

                                {!mechanicView && (
                                    <div className="form-section">
                                        <label>Mechanic</label>
                                        <DropDown
                                            name="WorkerId"
                                            value={job.workerId}
                                            onChange={e => handleChange('workerId', e.target.value)}
                                            disabled={job.status === 2 || !job.jobTypeId}
                                        >
                                            <option value="">Select Mechanic</option>
                                            {workers
                                                .filter(w =>
                                                    !job.jobTypeId ||
                                                    (w.jobTypeIds && w.jobTypeIds.includes(job.jobTypeId))
                                                )
                                                .map(w =>
                                                    <option key={w.id} value={w.id}>{w.name}</option>
                                                )
                                            }
                                        </DropDown>
                                        <FieldError name="WorkerId" errors={errors} />
                                    </div>
                                )}

                                {!mechanicView && (
                                    <div className="form-section">
                                        <label>Labor Cost</label>
                                        <input
                                            type="number"
                                            name="LaborCost"
                                            step="0.01"
                                            min="0"
                                            value={job.laborCost}
                                            onChange={e => handleChange('laborCost', e.target.value)}
                                            disabled={job.status === 2}
                                            required
                                        />
                                        <FieldError name="LaborCost" errors={errors} />
                                    </div>
                                )}

                                <div className="form-section" style={{ display: 'flex', flexDirection: 'column' }}>
                                    <label>Time Slot</label>
                                    <TimeSlotPicker
                                        worker={workers.find(w => w.id === job.workerId)}
                                        initialStart={job.startTime}
                                        initialEnd={job.endTime}
                                        readonly={job.status === 2 || !job.workerId}
                                        excludeId={job.id}
                                        onTimeSelect={(start, end) => {
                                            handleChange('startTime', start);
                                            handleChange('endTime', end);
                                        }}
                                    />
                                    <FieldError name="StartTime" errors={errors} />
                                </div>

                            </div>

                            <div className="form-section">
                                <label>Description</label>
                                <textarea
                                    className="description"
                                    name="Description"
                                    value={job.description}
                                    onChange={e => handleChange('description', e.target.value)}
                                    placeholder="Describe..."
                                    disabled={job.status === 2}
                                />
                                <FieldError name="Description" errors={errors} />
                            </div>
                        </>

                    )}

                </div>

                <div className="form-section parts-table-section">
                    <label>Parts</label>
                    <div className="parts-table-wrapper">
                        <table className="table" style={{ overflow: 'visible', marginBottom: '18px' }}>
                            <thead>
                                <tr>
                                    <th>Part Name / Number</th>
                                    <th style={{ width: '120px' }}>Planned</th>
                                    <th style={{ width: '120px' }}>Sent</th>
                                    <th style={{ width: '120px' }}>Used</th>
                                    <th style={{ width: '120px' }}>Req</th>
                                    <th style={{ width: '100px' }}>Unit Price</th>
                                    <th style={{ width: '130px' }}>Total Projected</th>
                                    <th style={{ width: '130px' }}>Total Spent</th>
                                    <th style={{ width: '50px' }}></th>
                                </tr>
                            </thead>
                            <tbody>
                                {job.parts.map((p, i) => {
                                    // Validation/Error styling logic
                                    const planned = p.plannedQuantity || 0;
                                    const sent = p.sentQuantity || 0;
                                    const used = p.usedQuantity || 0;

                                    const sentError = sent > planned;
                                    const usedError = used > sent;

                                    const isExpanded = !!expandedParts[i];

                                    const partErrors = getPartErrors(i);
                                    return (
                                        <React.Fragment key={p.id || i}>
                                            <tr className={isExpanded ? 'expanded-row' : ''}>
                                                <td>
                                                    <div style={{ display: 'flex', flexDirection: 'column', gap: '5px', width: '100%' }}>
                                                        <div style={{ display: 'flex', alignItems: 'center', gap: '8px', width: '100%' }}>
                                                            <button
                                                                type="button"
                                                                className="btn icon-btn expand-btn mobile-only"
                                                                onClick={() => togglePartExpand(i)}
                                                                style={{ padding: '4px', minWidth: '24px' }}
                                                            >
                                                                <i className={`fa-solid fa-chevron-${isExpanded ? 'down' : 'right'}`}></i>
                                                            </button>
                                                            <div style={{ position: 'relative', flex: 1 }}>
                                                                <input
                                                                    type="text"
                                                                    name={`Parts[${i}].Name`}
                                                                    placeholder="Search part..."
                                                                    value={p.name}
                                                                    onChange={e => handlePartSearch(e.target.value, i)}
                                                                    onFocus={() => setActivePartIndex(i)}
                                                                    onBlur={() => {
                                                                        suggestionsRef.current?.selectHighlighted();
                                                                        setTimeout(() => setActivePartIndex(null), 200);
                                                                    }}
                                                                    onKeyDown={(e) => suggestionsRef.current?.handleKeyDown(e)}
                                                                    disabled={job.status === 2}
                                                                    autoComplete="off"
                                                                    style={{ width: '100%' }}
                                                                />
                                                                <Suggestions
                                                                    ref={suggestionsRef}
                                                                    isOpen={activePartIndex === i}
                                                                    suggestions={activePartIndex === i ? suggestions : []}
                                                                    onSelect={addPart}
                                                                    onClose={() => setActivePartIndex(null)}
                                                                    renderItem={(part) => (
                                                                        <div className="part-suggestion">
                                                                            <span className="part-name">{part.name}</span>
                                                                            <span className="part-number">{part.partNumber}</span>
                                                                            <span className="part-price">${part.price.toFixed(2)}</span>
                                                                        </div>
                                                                    )}
                                                                />
                                                            </div>
                                                        </div>
                                                        <div className="mobile-only mobile-price">
                                                            {p.price.toFixed(2)}
                                                        </div>
                                                    </div>
                                                </td>
                                                <td className="mobile-collapsible" data-label="Planned">
                                                    <div style={{ width: '120px' }}>
                                                        <input
                                                            type="number"
                                                            name={`Parts[${i}].PlannedQuantity`}
                                                            className={sentError ? 'input-error' : ''}
                                                            value={p.plannedQuantity}
                                                            min="0"
                                                            onChange={e => updatePartRow(i, 'plannedQuantity', e.target.value)}
                                                            disabled={job.status === 2 || mechanicView || (!hasStockAccess && !isAssignedWorker)}
                                                            title={(job.status === 2) ? "Job is done" : (mechanicView || (!hasStockAccess && !isAssignedWorker)) ? "Only Parts Stock access or assigned worker can edit this" : ""}
                                                            style={{ width: '100%' }}
                                                        />
                                                    </div>
                                                </td>
                                                <td className="mobile-collapsible" data-label="Sent">
                                                    <div style={{ width: '120px' }}>
                                                        <input
                                                            type="number"
                                                            name={`Parts[${i}].SentQuantity`}
                                                            className={usedError ? 'input-error' : ''}
                                                            value={p.sentQuantity}
                                                            min="0"
                                                            onChange={e => updatePartRow(i, 'sentQuantity', e.target.value)}
                                                            disabled={job.status === 2 || mechanicView || !hasStockAccess}
                                                            title={(job.status === 2) ? "Job is done" : (mechanicView || !hasStockAccess) ? "Only Parts Stock access can edit this" : ""}
                                                            style={{ width: '100%' }}
                                                        />
                                                    </div>
                                                </td>
                                                <td className="mobile-collapsible" data-label="Used">
                                                    <div style={{ width: '120px' }}>
                                                        <input
                                                            type="number"
                                                            name={`Parts[${i}].UsedQuantity`}
                                                            value={p.usedQuantity}
                                                            min="0"
                                                            onChange={e => updatePartRow(i, 'usedQuantity', e.target.value)}
                                                            disabled={job.status === 2 || !isAssignedWorker}
                                                            title={(job.status === 2) ? "Job is done" : !isAssignedWorker ? "Only assigned worker can edit this" : ""}
                                                            style={{ width: '100%' }}
                                                        />
                                                    </div>
                                                </td>
                                                <td className="mobile-collapsible" data-label="Requested">
                                                    <div style={{ display: 'flex', alignItems: 'center', gap: '5px', width: '120px' }}>
                                                        <input
                                                            type="number"
                                                            name={`Parts[${i}].RequestedQuantity`}
                                                            value={p.requestedQuantity}
                                                            min="0"
                                                            onChange={e => updatePartRow(i, 'requestedQuantity', e.target.value)}
                                                            disabled={job.status === 2 || !isAssignedWorker}
                                                            title={(job.status === 2) ? "Job is done" : !isAssignedWorker ? "Only assigned worker can edit this" : ""}
                                                            style={{ width: '80px' }}
                                                        />
                                                        <button
                                                            type="button"
                                                            className="btn icon-btn"
                                                            disabled={job.status === 2 || !p.requestedQuantity}
                                                            onClick={() => openTransferPopup(i)}
                                                            title="Transfer to Planned"
                                                        >
                                                            <i className="fa-solid fa-plus"></i>
                                                        </button>
                                                    </div>
                                                </td>
                                                <td className="mobile-collapsible hide-sm" data-label="Unit Price">
                                                    {p.price.toFixed(2)}
                                                </td>
                                                <td className="mobile-collapsible" data-label="Total Projected">
                                                    {((p.plannedQuantity || 0) * (p.price || 0)).toFixed(2)}
                                                </td>
                                                <td className="mobile-collapsible" data-label="Total Spent">
                                                    {((p.usedQuantity || 0) * (p.price || 0)).toFixed(2)}
                                                </td>
                                                <td className="mobile-collapsible" data-label="Actions">
                                                    <button type="button" className="btn icon-btn delete" onClick={() => removePart(i)} disabled={job.status === 2}>
                                                        <i className="fa-solid fa-trash"></i>
                                                    </button>
                                                </td>
                                            </tr>
                                            {partErrors && (
                                                <tr className="error-row no-hover" style={{ borderTop: 'none', height: 'auto' }}>
                                                    <td colSpan="9" style={{ borderTop: 'none', padding: '0 0 10px 10px', overflow: 'visible', whiteSpace: 'normal' }}>
                                                        <p className="field-error" style={{ margin: 0 }}>
                                                            {partErrors}
                                                        </p>
                                                    </td>
                                                </tr>
                                            )}
                                        </React.Fragment>
                                    );
                                })}
                            </tbody>
                        </table>
                    </div>
                    <div className="form-footer">
                        <FieldError name="Parts" errors={errors} />
                        <button type="button" className="btn" onClick={addNewRow} disabled={job.status === 2}>+ Add Part</button>
                    </div>
                </div>


            </div>
        </form>
    );
};

export default JobForm;
