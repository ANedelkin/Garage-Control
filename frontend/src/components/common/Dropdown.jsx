import React, { useState } from 'react';
import '../../assets/css/common.css';

const Dropdown = ({ children, value, onChange, title }) => {
    const [selection, setSelection] = useState(children[0]);
    return (
        <div className="select-wrapper">
            <select value={value} onChange={onChange} title={title}>
                {children}
            </select>
            <i className="fa-solid fa-chevron-down" />
        </div>
    );
};

export default Dropdown;
