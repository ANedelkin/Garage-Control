import React, { useState, useEffect } from 'react';
import DatePicker from 'react-datepicker';

const LeavePopup = ({ onClose, onConfirm, currentLeave, isEditing }) => {
    const [leave, setLeave] = useState({
        startDate: null,
        endDate: null
    });

    useEffect(() => {
        if (currentLeave) {
            setLeave(currentLeave);
        } else {
            setLeave({
                startDate: null,
                endDate: null
            });
        }
    }, [currentLeave]);

    const handleSave = () => {
        onConfirm(leave);
        onClose();
    };

    return (
        <>
            <div className="form-section">
                <label>Start Date</label>
                <DatePicker
                    selected={leave.startDate}
                    onChange={date => setLeave({ ...leave, startDate: date })}
                />
            </div>
            <div className="form-section">
                <label>End Date</label>
                <DatePicker
                    selected={leave.endDate}
                    onChange={date => setLeave({ ...leave, endDate: date })}
                />
            </div>
            <div className="form-footer">
                <button type="button" className="btn" onClick={handleSave}>Save</button>
                <button type="button" className="btn" onClick={onClose}>Cancel</button>
            </div>
        </>
    );
};

export default LeavePopup;
