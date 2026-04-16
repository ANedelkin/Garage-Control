import React from 'react';

const ConfirmationPopup = ({ 
    title = 'Confirm Action', 
    message = 'Are you sure you want to proceed?', 
    confirmText = 'Confirm', 
    cancelText = 'Cancel', 
    onConfirm, 
    onClose,
    isDanger = false 
}) => {
    const handleConfirm = () => {
        onConfirm();
    };

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
                >
                    {confirmText}
                </button>
                <button 
                    type="button" 
                    className="btn" 
                    onClick={onClose}
                >
                    {cancelText}
                </button>
            </div>
        </div>
    );
};

export default ConfirmationPopup;
