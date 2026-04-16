import React, { useState } from 'react';

const RenamePopup = ({ node, width = '300px', onClose, onConfirm }) => {
    const [newName, setNewName] = useState(node.name || '');

    const handleSubmit = (e) => {
        e.preventDefault();
        if (newName.trim()) {
            onConfirm(newName.trim());
        }
    };

    return (
        <form onSubmit={handleSubmit} style={{ width: width }}>
            <div className="form-section">
                <label className="popup-label">New Name</label>
                <input
                    type="text"
                    value={newName}
                    onChange={(e) => setNewName(e.target.value)}
                    autoFocus
                    required
                />
            </div>

            <div className="form-footer mt-15">
                <button type="button" className="btn" onClick={onClose}>Cancel</button>
                <button type="submit" className="btn">Rename</button>
            </div>
        </form>
    );
};

export default RenamePopup;
