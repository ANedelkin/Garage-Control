import React, { useState, useEffect } from 'react';
import '../../assets/css/popup.css'; // Assuming styles will be added here or reuse existing

const SuggestedModelPopup = ({ node, onClose, onConfirm }) => {
    // node: { id, name, count, isExisting (false for model?), parentName (Make Name) }
    // Actually node here is the MODEL suggestion. Its parent is the Make.
    // The make might be existing or not.
    // We need to know the Make name. It's not directly in node unless passed.
    // Let's assume onConfirm takes (makeName, modelName).

    // We need parent make name. 
    // In AdminMakesModels, fetchSuggestedModels knows the make name.
    // We should pass makeName as prop.

    const [makeName, setMakeName] = useState(node.makeName || '');
    const [modelName, setModelName] = useState(node.name || '');
    const [isMakeExisting, setIsMakeExisting] = useState(node.isMakeExisting || false);

    const handleSubmit = (e) => {
        e.preventDefault();
        onConfirm(makeName, modelName);
    };

    return (
        <div className="popup-overlay">
            <div className="popup-content">
                <div className="popup-header">
                    <h3>Add Suggested Model</h3>
                    <button className="btn icon-btn" onClick={onClose}>
                        <i className="fa-solid fa-times"></i>
                    </button>
                </div>
                <form onSubmit={handleSubmit}>
                    <div className="form-group">
                        <label>Make Name</label>
                        <input
                            type="text"
                            className="form-control"
                            value={makeName}
                            onChange={(e) => setMakeName(e.target.value)}
                            disabled={isMakeExisting} // If make exists, probably shouldn't change it here? Or allow changing to move it?
                        // User request: "inputs for the model's make,'s name if the make isn't an already existing one"
                        />
                    </div>
                    <div className="form-group">
                        <label>Model Name</label>
                        <input
                            type="text"
                            className="form-control"
                            value={modelName}
                            onChange={(e) => setModelName(e.target.value)}
                        />
                    </div>
                    <div className="popup-actions">
                        <button type="button" className="btn btn-secondary" onClick={onClose}>Cancel</button>
                        <button type="submit" className="btn btn-primary">Add</button>
                    </div>
                </form>
            </div>
        </div>
    );
};

export default SuggestedModelPopup;
