import React, { useState, useEffect } from 'react';
import { partApi } from '../../services/partApi';

const PartDetails = ({ part, onUpdate, onDelete }) => {
    const [formData, setFormData] = useState({
        name: '',
        partNumber: '',
        price: '',
        quantity: ''
    });
    const [isDirty, setIsDirty] = useState(false);

    useEffect(() => {
        if (part) {
            setFormData({
                name: part.name,
                partNumber: part.partNumber,
                price: part.price,
                quantity: part.quantity
            });
            setIsDirty(false);
        }
    }, [part]);

    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData(prev => ({ ...prev, [name]: value }));
        setIsDirty(true);
    };

    const handleSave = async (e) => {
        e.preventDefault();
        try {
            await partApi.updatePart({
                id: part.id,
                ...formData,
                price: parseFloat(formData.price),
                quantity: parseInt(formData.quantity)
            });
            onUpdate();
            setIsDirty(false);
            alert("Saved successfully");
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
                <h3>{formData.name || 'Part Details'}</h3>
                <div>
                    <button className="btn delete" onClick={() => { if (window.confirm('Delete this part?')) partApi.deletePart(part.id).then(onDelete); }}>
                        <i className="fa-solid fa-trash"></i> Delete
                    </button>
                </div>
            </div>

            <form className="part-details-form" onSubmit={handleSave}>
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
                <div className="form-group-row">
                    <div className="form-section grow">
                        <label>Price</label>
                        <input
                            type="number"
                            step="0.01"
                            name="price"
                            value={formData.price}
                            onChange={handleChange}
                            required
                        />
                    </div>
                    <div className="form-section grow">
                        <label>Quantity</label>
                        <input
                            type="number"
                            name="quantity"
                            value={formData.quantity}
                            onChange={handleChange}
                            required
                        />
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
