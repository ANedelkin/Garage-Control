import React, { useState, useEffect } from 'react';
import { useStatus } from '../../context/StatusContext';

const PartTransferPopup = ({ onClose, onConfirm, maxQuantity, partName }) => {
    const [quantity, setQuantity] = useState(maxQuantity);
    const { showStatus } = useStatus();

    useEffect(() => {
        setQuantity(maxQuantity);
    }, [maxQuantity]);

    const handleConfirm = async () => {
        const val = parseFloat(quantity);
        if (isNaN(val) || val <= 0) {
            showStatus('Please enter a valid quantity.', 'error');
            return;
        }
        if (val > maxQuantity) {
            showStatus('Cannot transfer more than requested.', 'error');
            return;
        }
        try {
            await onConfirm(val);
        } catch (e) {
            // Error handling expected in onConfirm
        }
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
