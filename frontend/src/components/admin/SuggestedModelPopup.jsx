import React, { useState } from 'react';
import FieldError from '../common/FieldError.jsx';

const SuggestedModelPopup = ({ node, onClose, onConfirm, errors = {} }) => {
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
                    name="MakeName"
                    className="form-control"
                    value={makeName}
                    onChange={(e) => setMakeName(e.target.value)}
                    disabled={isMakeExisting}
                />
                <FieldError name="MakeName" errors={errors} />
            </div>
            <div className="form-section">
                <label>Model Name</label>
                <input
                    type="text"
                    name="ModelName"
                    className="form-control"
                    value={modelName}
                    onChange={(e) => setModelName(e.target.value)}
                />
                <FieldError name="ModelName" errors={errors} />
            </div>
            <div className="form-footer">
                {errors.general && <p className="form-error">{errors.general}</p>}
                <button type="button" className="btn" onClick={onClose}>Cancel</button>
                <button type="submit" className="btn">Add</button>
            </div>
        </form>
    );
};

export default SuggestedModelPopup;
