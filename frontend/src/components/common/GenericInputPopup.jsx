import React, { useState } from 'react';
import FieldError from './FieldError.jsx';

const GenericInputPopup = ({ 
    label, 
    initialValue = '', 
    confirmText = 'Save', 
    onConfirm, 
    onClose, 
    errors = {}, 
    width = '100%' 
}) => {
    const [value, setValue] = useState(initialValue);
    const [loading, setLoading] = useState(false);

    const handleSubmit = async (e) => {
        e.preventDefault();
        setLoading(true);
        try {
            await onConfirm(value);
            if (onClose) onClose();
        } catch (error) {
            console.error("Submission error:", error);
            // Error handling is expected to be managed via the 'errors' prop passed from parent
        } finally {
            setLoading(false);
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
                    disabled={loading}
                />
                <FieldError name="Name" errors={errors} />
                <FieldError name="name" errors={errors} />
                <FieldError name="Value" errors={errors} />
                <FieldError name="value" errors={errors} />
            </div>

            <div className="form-footer">
                {errors.general && <p className="form-error">{errors.general}</p>}
                <button type="button" className="btn" onClick={onClose} disabled={loading}>Cancel</button>
                <button type="submit" className="btn" disabled={loading}>
                    {loading ? 'Processing...' : confirmText}
                </button>
            </div>
        </form>
    );
};

export default GenericInputPopup;
