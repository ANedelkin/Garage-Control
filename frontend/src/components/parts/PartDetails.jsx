import React, { useState, useEffect } from 'react';
import { partApi } from '../../services/partApi';

const PartDetails = ({ part, onUpdate, onDelete }) => {
    const [formData, setFormData] = useState({
        name: '',
        partNumber: '',
        price: '',
        quantity: '',
        availabilityBalance: '',
        partsReserved: '',
        minimumQuantity: ''
    });
    const [stockAdj, setStockAdj] = useState('');
    const [isDirty, setIsDirty] = useState(false);

    useEffect(() => {
        if (part) {
            setFormData({
                name: part.name,
                partNumber: part.partNumber,
                price: part.price,
                quantity: part.quantity,
                availabilityBalance: part.availabilityBalance,
                partsReserved: part.partsReserved || 0,
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

    const handleStockAdj = (amount) => {
        const adj = parseInt(amount);
        if (isNaN(adj)) return;

        setFormData(prev => {
            const currentQty = parseInt(prev.quantity) || 0;
            return { ...prev, quantity: Math.max(0, currentQty + adj) };
        });
        setIsDirty(true);
    };

    const handleSave = async (e) => {
        e.preventDefault();
        try {
            await partApi.updatePart({
                id: part.id,
                ...formData,
                price: parseFloat(formData.price),
                quantity: parseInt(formData.quantity),
                minimumQuantity: parseInt(formData.minimumQuantity)
            });
            onUpdate();
            setIsDirty(false);
            setStockAdj('');
            // alert("Saved successfully");
        } catch (error) {
            console.error("Error saving part", error);
            alert("Failed to save part");
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
                <h3>Part Details</h3>
                <div>
                    <button className="btn delete" onClick={() => { if (window.confirm('Delete this part?')) partApi.deletePart(part.id).then(onDelete); }}>
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
                        </div>
                        <div className="form-section">
                            <label>Price</label>
                            <div className="input-group">
                                {/* <span className="input-prefix">$</span> */}
                                <input
                                    type="number"
                                    step="0.01"
                                    name="price"
                                    value={formData.price}
                                    onChange={handleChange}
                                    required
                                />
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
                                value={formData.quantity}
                                onChange={handleChange}
                                required
                            />
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
                            </div>

                            <div className={`form-section`}>
                                <label>Parts Requested</label>
                                <input
                                    type="number"
                                    name="partsReserved"
                                    value={formData.partsReserved}
                                    disabled
                                />
                            </div>
                        </div>

                        <div className="form-section">
                            <label>Minimum Quantity</label>
                            <input
                                type="number"
                                name="minimumQuantity"
                                value={formData.minimumQuantity}
                                onChange={handleChange}
                                required
                            />
                        </div>

                        <div className="form-section">
                            <label>Add/Remove Quantity</label>
                            <input
                                type="number"
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
                    <button type="submit" className="btn" disabled={!isDirty}>Save Changes</button>
                </div>
            </form>
        </div>
    );
};

export default PartDetails;
