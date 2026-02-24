import React, { useState, useEffect } from 'react';

const PartTransferPopup = ({ isOpen, onClose, onConfirm, maxQuantity, partName }) => {
    const [quantity, setQuantity] = useState(maxQuantity);

    useEffect(() => {
        if (isOpen) {
            setQuantity(maxQuantity);
        }
    }, [isOpen, maxQuantity]);

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

    if (!isOpen) return null;

    return (
        <div className="popup-overlay" onClick={onClose}>
            <div className="popup tile" onClick={e => e.stopPropagation()} style={{ width: '400px' }}>
                <div className="section-header">
                    <h3>Transfer to Planned</h3>
                </div>
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
            </div>
        </div>
    );
};

export default PartTransferPopup;
