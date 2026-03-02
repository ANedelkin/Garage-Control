import React, { useState, useEffect } from 'react';

const PartTransferPopup = ({ onClose, onConfirm, maxQuantity, partName }) => {
    const [quantity, setQuantity] = useState(maxQuantity);

    useEffect(() => {
        setQuantity(maxQuantity);
    }, [maxQuantity]);

    const handleConfirm = () => {
        const val = parseFloat(quantity);
        if (isNaN(val) || val <= 0) {
            alert('Please enter a valid quantity.');
            return;
        }
        if (val > maxQuantity) {
            alert('Cannot transfer more than requested.');
            return;
        }
        onConfirm(val);
        onClose();
    };

    return (
        <>
            <div className="form-section">
                <label>Quantity</label>
                <input
                    type="number"
                    value={quantity}
                    onChange={(e) => setQuantity(e.target.value)}
                    max={maxQuantity}
                    min={1}
                    step="any"
                    autoFocus
                />
            </div>
            <div className="form-footer" style={{ marginTop: '20px', display: 'flex', gap: '10px' }}>
                <button className="btn" onClick={handleConfirm}>Transfer</button>
                <button className="btn" onClick={onClose}>Cancel</button>
            </div>
        </>
    );
};

export default PartTransferPopup;
