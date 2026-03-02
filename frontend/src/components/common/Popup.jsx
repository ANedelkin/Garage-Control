import React from 'react';
import '../../assets/css/popup.css';

const Popup = ({ children, title }) => {
    return (
        <div
            className="popup tile"
            style={{ width: 'fit-content' }}
            onClick={e => e.stopPropagation()}
        >
            {title && (
                <div className="section-header">
                    <h3>{title}</h3>
                </div>
            )}
            {children}
        </div>
    );
};

export default Popup;
