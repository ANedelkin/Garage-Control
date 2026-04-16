import React, { useState } from 'react';

const SimpleInputPopup = ({ title, label, initialValue = '', width = '300px', onClose, onConfirm }) => {
    const [value, setValue] = useState(initialValue);

    const handleSubmit = (e) => {
        e.preventDefault();
        if (value.trim()) {
            onConfirm(value.trim());
        }
    };

    return (
        <form onSubmit={handleSubmit} style={{ width: width }}>
            <div className="form-section">
                <label>{label}</label>
                <input
                    type="text"
                    value={value}
                    onChange={(e) => setValue(e.target.value)}
                    autoFocus
                    required
                />
            </div>

            <div className="form-footer">
                <button type="button" className="btn" onClick={onClose}>Cancel</button>
                <button type="submit" className="btn">Add</button>
            </div>
        </form>
    );
};

export default SimpleInputPopup;
