import React, { useState } from 'react';

const JustificationPopup = ({ onClose, onConfirm, title, message }) => {
    const [justification, setJustification] = useState('');

    const handleConfirm = () => {
        if (!justification.trim()) {
            alert('Please enter a justification.');
            return;
        }
        onConfirm(justification);
        setJustification('');
    };

    return (
        <>
            <div className="form-section">
                <p>
                    {message || 'Please provide a reason for blocking this entity. This message will be shown to them.'}
                </p>
                <textarea
                    className="description"
                    rows="4"
                    value={justification}
                    onChange={(e) => setJustification(e.target.value)}
                    placeholder="Enter justification here..."
                />
            </div>
            <div className="form-footer">
                <button className="btn" onClick={handleConfirm}>Block</button>
                <button className="btn" onClick={onClose}>Cancel</button>
            </div>
        </>
    );
};

export default JustificationPopup;
