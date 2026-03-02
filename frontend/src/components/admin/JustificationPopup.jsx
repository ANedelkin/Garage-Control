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
        <div className="form-section">
            <p style={{ marginBottom: '10px', color: 'var(--text-secondary)' }}>
                {message || 'Please provide a reason for blocking this entity. This message will be shown to them.'}
            </p>
            <textarea
                className="form-control"
                rows="4"
                value={justification}
                onChange={(e) => setJustification(e.target.value)}
                placeholder="Enter justification here..."
                style={{ width: '100%', padding: '10px', borderRadius: '4px', background: 'var(--bg-secondary)', color: 'var(--text-primary)', border: '1px solid var(--border-color)', resize: 'vertical' }}
            />
            <div className="form-footer" style={{ marginTop: '20px', display: 'flex', gap: '10px' }}>
                <button className="btn" onClick={handleConfirm} style={{ flex: 1 }}>Block</button>
                <button className="btn" onClick={onClose} style={{ flex: 1 }}>Cancel</button>
            </div>
        </div>
    );
};

export default JustificationPopup;
