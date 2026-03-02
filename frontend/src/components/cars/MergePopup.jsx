import React from 'react';

const MergePopup = ({ mergeType, customItem, globalItem, onMerge, onClose }) => {
    if (!customItem || !globalItem) return null;

    return (
        <div className="form-section">
            <p>A global version of this {mergeType} has been created. Would you like to replace your custom version with the global one?</p>

            <div className="merge-comparison">
                <div className="merge-item">
                    <label>Your Custom {mergeType === 'make' ? 'Make' : 'Model'}:</label>
                    <div className="merge-name">{customItem.name}</div>
                </div>

                <div className="merge-arrow">
                    <i className="fa-solid fa-arrow-right"></i>
                </div>

                <div className="merge-item">
                    <label>Global {mergeType === 'make' ? 'Make' : 'Model'}:</label>
                    <div className="merge-name">{globalItem.name}</div>
                </div>
            </div>

            <div className="merge-warning">
                <i className="fa-solid fa-triangle-exclamation"></i>
                <span>This will update all your vehicles to use the global version and delete your custom {mergeType}.</span>
            </div>

            <div className="form-footer">
                <button className="btn" onClick={onMerge}>
                    <i className="fa-solid fa-code-merge"></i> Merge
                </button>
                <button className="btn" onClick={onClose}>Cancel</button>
            </div>
        </div>
    );
};

export default MergePopup;
