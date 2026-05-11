import React, { useState, useEffect } from 'react';
import { partApi } from '../../services/partApi';
import FieldError from '../common/FieldError.jsx';
import { parseValidationErrors } from '../../Utilities/formErrors.js';
import { usePopup } from '../../context/PopupContext';
import ConfirmationPopup from '../common/ConfirmationPopup';
import { useStatus } from '../../context/StatusContext';

const PartDetails = ({ part, onUpdate, onDelete, onBack }) => {
    const { addPopup, removeLastPopup } = usePopup();
    const { showStatus } = useStatus();
    const [formData, setFormData] = useState({
        name: '',
        partNumber: '',
        price: '',
        quantity: '',
        availabilityBalance: '',
        partsToSend: '',
        minimumQuantity: ''
    });
    const [stockAdj, setStockAdj] = useState('');
    const [isDirty, setIsDirty] = useState(false);
    const [errors, setErrors] = useState({});

    useEffect(() => {
        if (part) {
            setFormData({
                name: part.name,
                partNumber: part.partNumber,
                price: part.price,
                quantity: part.quantity,
                availabilityBalance: part.availabilityBalance,
                partsToSend: part.partsToSend || 0, // Renamed from part.partsToBeSent
                minimumQuantity: part.minimumQuantity
            });
            setStockAdj('');
            setIsDirty(false);
        }
    }, [part]);

    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData(prev => ({ ...prev, [name]: value }));
        setIsDirty(true);
    };

    const handleStockAdj = async (amount) => {
        const adj = parseInt(amount);
        if (isNaN(adj)) return;

        const newQuantity = Math.max(0, (parseInt(formData.quantity) || 0) + adj);
        const updatedFormData = { ...formData, quantity: newQuantity };

        setFormData(updatedFormData);
        setIsDirty(true);

        // Auto-save after adjustment
        showStatus('Saving adjustment...', 'loading');

        const parseNum = (val) => (val === '' || val === null || val === undefined) ? null : Number(val);
        try {
            await partApi.updatePart(part.id, {
                ...updatedFormData,
                price: parseNum(updatedFormData.price),
                quantity: parseNum(updatedFormData.quantity),
                minimumQuantity: parseNum(updatedFormData.minimumQuantity)
            });
            onUpdate();
            setIsDirty(false);
            setStockAdj('');
            window.dispatchEvent(new CustomEvent('refresh-notifications'));
            showStatus('Adjustment saved', 'success');
        } catch (error) {
            console.error("Error saving part after adjustment", error);
            setErrors(parseValidationErrors(error));
            showStatus(error.message || 'Failed to save adjustment', 'error');
        }
    };

    const handleSave = async (e) => {
        e.preventDefault();
        showStatus('Saving changes...', 'loading');

        const parseNum = (val) => (val === '' || val === null || val === undefined) ? null : Number(val);
        try {
            await partApi.updatePart(part.id, {
                ...formData,
                price: parseNum(formData.price),
                quantity: parseNum(formData.quantity),
                minimumQuantity: parseNum(formData.minimumQuantity)
            });
            onUpdate();
            setIsDirty(false);
            setStockAdj('');

            // Trigger notification refresh in header
            window.dispatchEvent(new CustomEvent('refresh-notifications'));

            showStatus('Saved successfully', 'success');
        } catch (error) {
            console.error("Error saving part", error);
            setErrors(parseValidationErrors(error));
            showStatus(error.message || 'Failed to save changes', 'error');
        }
    };

    if (!part) {
        return (
            <div className="section-header">
                <h3>Select a part to view details</h3>
            </div>
        );
    }

    return (
        <div className="part-details">
            <div className="section-header">
                <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
                    <button className="icon-btn btn mobile-only" onClick={onBack} title="Back to list">
                        <i className="fa-solid fa-arrow-left"></i>
                    </button>
                    <h3>Part Details</h3>
                </div>
                <div>
                    <button className="btn delete" onClick={() => {
                        addPopup(
                            'Delete Part',
                            <ConfirmationPopup
                                message={`Are you sure you want to delete part "${part.name}"?`}
                                confirmText="Delete"
                                isDanger={true}
                                onConfirm={async () => {
                                    showStatus('Deleting part...', 'loading');
                                    try {
                                        await partApi.deletePart(part.id);
                                        removeLastPopup();
                                        onDelete();
                                        showStatus('Part deleted successfully', 'success');
                                    } catch (error) {
                                        showStatus(error.message || 'Failed to delete part', 'error');
                                    }
                                }}
                                onClose={removeLastPopup}
                            />
                        );
                    }}>
                        <i className="fa-solid fa-trash"></i> Delete
                    </button>
                </div>
            </div>

            <form className="part-details-form" onSubmit={handleSave}>
                <div className="details-grid">
                    {/* Left Column */}
                    <div className="form-column">
                        <div className="form-section">
                            <label>Name</label>
                            <input
                                type="text"
                                name="name"
                                value={formData.name}
                                onChange={handleChange}
                                required
                            />
                            <FieldError name="Name" errors={errors} />
                        </div>
                        <div className="form-section">
                            <label>Part Number</label>
                            <input
                                type="text"
                                name="partNumber"
                                value={formData.partNumber}
                                onChange={handleChange}
                                required
                            />
                            <FieldError name="PartNumber" errors={errors} />
                        </div>
                        <div className="form-section">
                            <label>Price</label>
                            <div className="input-group">
                                <input
                                    type="number"
                                    step="0.01"
                                    name="price"
                                    min="0"
                                    value={formData.price}
                                    onChange={handleChange}
                                    required
                                />
                                <FieldError name="Price" errors={errors} />
                            </div>
                        </div>
                    </div>

                    {/* Right Column */}
                    <div className="form-column">
                        <div className={`form-section`}>
                            <label>Stockpile</label>
                            <input
                                className={formData.quantity < formData.minimumQuantity ? 'low-stock' : ''}
                                type="number"
                                name="quantity"
                                min="0"
                                value={formData.quantity}
                                onChange={handleChange}
                                required
                            />
                            <FieldError name="Quantity" errors={errors} />
                        </div>

                        <div className="form-section-row">
                            <div className={`form-section`}>
                                <label>Availability Balance</label>
                                <input
                                    className={
                                        formData.availabilityBalance < 0
                                            ? 'negative-stock'
                                            : (formData.availabilityBalance < formData.minimumQuantity ? 'low-stock-yellow' : '')
                                    }
                                    type="number"
                                    name="availabilityBalance"
                                    value={formData.availabilityBalance}
                                    disabled
                                />
                                <FieldError name="AvailabilityBalance" errors={errors} />
                            </div>

                            <div className={`form-section`}>
                                <label>Parts to send</label>
                                <input
                                    type="number"
                                    name="partsToSend"
                                    value={formData.partsToSend}
                                    disabled
                                />
                                <FieldError name="PartsToSend" errors={errors} />
                            </div>
                        </div>

                        <div className="form-section">
                            <label>Minimum Quantity</label>
                            <input
                                type="number"
                                name="minimumQuantity"
                                min="0"
                                value={formData.minimumQuantity}
                                onChange={handleChange}
                                required
                            />
                            <FieldError name="MinimumQuantity" errors={errors} />
                        </div>

                        <div className="form-section">
                            <label>Add/Remove Quantity</label>
                            <input
                                type="number"
                                min="0"
                                placeholder="Amount"
                                value={stockAdj}
                                onChange={(e) => setStockAdj(e.target.value)}
                            />
                            <div className="stock-controls">
                                <button
                                    type="button"
                                    className="btn"
                                    onClick={() => handleStockAdj(stockAdj)}
                                    disabled={!stockAdj}
                                >
                                    Add
                                </button>
                                <button
                                    type="button"
                                    className="btn"
                                    onClick={() => handleStockAdj(-stockAdj)}
                                    disabled={!stockAdj}
                                >
                                    Remove
                                </button>
                            </div>
                        </div>
                    </div>
                </div>

                <div className="form-footer">
                    {errors.general && <p className="form-error">{errors.general}</p>}
                    <button type="submit" className="btn" disabled={!isDirty}>Save Changes</button>
                </div>
            </form>
        </div>
    );
};

export default PartDetails;
