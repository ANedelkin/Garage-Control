import React, { useState, useEffect } from 'react';

const AddEditItemModal = ({ itemType, currentName, onClose, onConfirm }) => {
    const [name, setName] = useState('');

    useEffect(() => {
        if (currentName) {
            setName(currentName);
        } else {
            setName('');
        }
    }, [currentName]);

    const handleSubmit = (e) => {
        e.preventDefault();
        onConfirm(name);
        onClose();
    };

    return (
        <>
            <form onSubmit={handleSubmit}>
                <div className="form-section">
                    <label>Name</label>
                    <input
                        type="text"
                        value={name}
                        onChange={e => setName(e.target.value)}
                        required
                        autoFocus
                    />
                </div>
                <div className="form-footer">
                    <button type="submit" className="btn">Save</button>
                    <button type="button" className="btn" onClick={onClose}>Cancel</button>
                </div>
            </form>
        </>
    );
};

export default AddEditItemModal;
