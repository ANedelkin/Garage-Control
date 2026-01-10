import React, { useState } from 'react';
import '../../assets/css/common/dropdown.css';

const Dropdown = ({ children, value, onChange, title, className = "" }) => {
    return (
        <div className={`select-wrapper ${className}`}>
            <select value={value} onChange={onChange} title={title}>
                {children}
            </select>
            <i className="fa-solid fa-chevron-down" />
        </div>
    );
};

export default Dropdown;
