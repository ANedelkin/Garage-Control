import React, { useState } from 'react';

const ConfirmationPopup = ({ 
    title = 'Confirm Action', 
    message = 'Are you sure you want to proceed?', 
    confirmText = 'Confirm', 
    cancelText = 'Cancel', 
    onConfirm, 
    onClose,
    isDanger = false 
}) => {
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);

    const handleConfirm = async () => {
        setLoading(true);
        try {
            await onConfirm();
        } catch (err) {
            setError(err?.message || 'An unexpected error occurred.');
        } finally {
            setLoading(false);
        }
    };

    if (error) {
        return (
            <div style={{ width: '300px' }}>
                <div className="form-section">
                    <p style={{ margin: 0, lineHeight: '1.5' }}>{error}</p>
                </div>
                <div className="form-footer">
                    <button type="button" className="btn" onClick={onClose}>
                        Close
                    </button>
                </div>
            </div>
        );
    }

    return (
        <div style={{ width: '300px' }}>
            <div className="form-section">
                <p style={{ margin: 0, lineHeight: '1.5' }}>{message}</p>
            </div>
            
            <div className="form-footer">
                <button 
                    type="button" 
                    className={`btn ${isDanger ? 'delete' : ''}`} 
                    onClick={handleConfirm}
                    disabled={loading}
                >
                    {loading ? 'Processing...' : confirmText}
                </button>
                <button 
                    type="button" 
                    className="btn" 
                    onClick={onClose}
                    disabled={loading}
                >
                    {cancelText}
                </button>
            </div>
        </div>
    );
};

export default ConfirmationPopup;
