import React from 'react';
import '../../assets/css/common/dropdown.css';

const DropDown = ({ children, value, onChange, title, className = "", disabled = false }) => {
    return (
        <div className={`select-wrapper ${className}`}>
            <select value={value} onChange={onChange} title={title} disabled={disabled}>
                {children}
            </select>
            <i className="fa-solid fa-chevron-down" />
        </div>
    );
};

export default DropDown;
