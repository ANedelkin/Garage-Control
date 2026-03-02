import React, { useState } from 'react';

const SuggestedModelPopup = ({ node, onClose, onConfirm }) => {
    const [makeName, setMakeName] = useState(node.makeName || '');
    const [modelName, setModelName] = useState(node.name || '');
    const isMakeExisting = node.isMakeExisting || false;

    const handleSubmit = (e) => {
        e.preventDefault();
        onConfirm(makeName, modelName);
    };

    return (
        <form onSubmit={handleSubmit}>
            <div className="form-section">
                <label>Make Name</label>
                <input
                    type="text"
                    className="form-control"
                    value={makeName}
                    onChange={(e) => setMakeName(e.target.value)}
                    disabled={isMakeExisting}
                />
            </div>
            <div className="form-section">
                <label>Model Name</label>
                <input
                    type="text"
                    className="form-control"
                    value={modelName}
                    onChange={(e) => setModelName(e.target.value)}
                />
            </div>
            <div className="form-footer">
                <button type="button" className="btn" onClick={onClose}>Cancel</button>
                <button type="submit" className="btn">Add</button>
            </div>
        </form>
    );
};

export default SuggestedModelPopup;
