import React from 'react';
import '../../assets/css/popup.css';

const Popup = ({ isOpen, onClose, children, title, maxWidth }) => {
    if (!isOpen) return null;

    return (
        <div className="popup-overlay" onClick={onClose}>
            <div
                className="popup tile"
                style={{ width: 'fit-content', maxWidth: maxWidth || 'none' }}
                onClick={e => e.stopPropagation()}
            >
                {title && (
                    <div className="section-header">
                        <h3>{title}</h3>
                    </div>
                )}
                {children}
            </div>
        </div>
    );
};

export default Popup;
