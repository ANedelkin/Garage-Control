import React, { useState } from 'react';

const PromoteMakePopup = ({ node, onClose, onConfirm }) => {
    const [newName, setNewName] = useState(node.name || '');

    const handleSubmit = (e) => {
        e.preventDefault();
        if (newName.trim()) {
            onConfirm(newName.trim());
        }
    };

    return (
        <form onSubmit={handleSubmit} style={{ width: '300px' }}>
            <div className="form-section">
                <label>Make Name</label>
                <input
                    type="text"
                    value={newName}
                    onChange={(e) => setNewName(e.target.value)}
                    autoFocus
                    required
                />
            </div>
            
            <div className="form-footer">
                <button type="button" className="btn" onClick={onClose}>Cancel</button>
                <button type="submit" className="btn">Promote</button>
            </div>
        </form>
    );
};

export default PromoteMakePopup;
