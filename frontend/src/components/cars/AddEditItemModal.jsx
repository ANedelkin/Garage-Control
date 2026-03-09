import React, { useState, useEffect } from 'react';
import FieldError from '../common/FieldError.jsx';

const AddEditItemModal = ({ itemType, currentName, onClose, onConfirm, errors = {} }) => {
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
                        name="Name"
                        value={name}
                        onChange={e => setName(e.target.value)}
                        required
                        autoFocus
                    />
                    <FieldError name="Name" errors={errors} />
                </div>
                <div className="form-footer">
                    {errors.general && <p className="form-error">{errors.general}</p>}
                    <button type="submit" className="btn">Save</button>
                    <button type="button" className="btn" onClick={onClose}>Cancel</button>
                </div>
            </form>
        </>
    );
};

export default AddEditItemModal;
