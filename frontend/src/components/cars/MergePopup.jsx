import React, { useRef, useEffect } from 'react';
import '../../assets/css/common/popup.css';

const MergePopup = ({ isOpen, onClose, mergeType, customItem, globalItem, onMerge }) => {
    const popupRef = useRef(null);

    useEffect(() => {
        const handleClickOutside = (event) => {
            if (popupRef.current && !popupRef.current.contains(event.target)) {
                onClose();
            }
        };

        if (isOpen) {
            document.addEventListener('mousedown', handleClickOutside);
        }

        return () => {
            document.removeEventListener('mousedown', handleClickOutside);
        };
    }, [isOpen, onClose]);

    if (!isOpen || !customItem || !globalItem) return null;

    return (
        <div className="popup-overlay">
            <div className="popup tile" ref={popupRef}>
                <h3>Merge {mergeType === 'make' ? 'Make' : 'Model'}</h3>
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
                </div>

                <div className="form-footer">
                    <button className="btn" onClick={onMerge}>
                        <i className="fa-solid fa-code-merge"></i> Merge
                    </button>
                    <button className="btn" onClick={onClose}>Cancel</button>
                </div>
            </div>
        </div>
    );
};

export default MergePopup;
