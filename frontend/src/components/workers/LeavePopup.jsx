import React, { useState, useEffect } from 'react';
import DatePicker from 'react-datepicker';

const LeavePopup = ({ onClose, onConfirm, currentLeave, isEditing, existingLeaves, currentIndex }) => {
    const [leave, setLeave] = useState({
        startDate: null,
        endDate: null
    });
    const [error, setError] = useState("");

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

    const handleSave = async () => {
        if (!leave.startDate || !leave.endDate) {
            setError("Both start and end dates are required.");
            return;
        }

        // Overlap check
        const start = new Date(leave.startDate);
        const end = new Date(leave.endDate);
        start.setHours(0, 0, 0, 0);
        end.setHours(23, 59, 59, 999);

        if (start > end) {
            setError("Start date cannot be later than end date.");
            return;
        }

        const isOverlap = existingLeaves.some((existing, idx) => {
            if (idx === currentIndex) return false; // Skip the one we are editing

            const exStart = new Date(existing.startDate);
            const exEnd = new Date(existing.endDate);
            exStart.setHours(0, 0, 0, 0);
            exEnd.setHours(23, 59, 59, 999);

            return (start <= exEnd && end >= exStart);
        });

        if (isOverlap) {
            setError("Leave dates overlap with an existing leave.");
            return;
        }

        try {
            await onConfirm(leave);
        } catch (e) {
            // Error handling expected in onConfirm
        }
    };

    const today = new Date();
    today.setHours(0, 0, 0, 0);

    return (
        <div>
            <div className="form-section">
                <label>Start Date</label>
                <DatePicker
                    selected={leave.startDate}
                    onChange={date => {
                        setLeave({ ...leave, startDate: date });
                        setError("");
                    }}
                    minDate={today}
                    dateFormat="dd.MM.yyyy"
                />
            </div>
            <div className="form-section">
                <label>End Date</label>
                <DatePicker
                    selected={leave.endDate}
                    onChange={date => {
                        setLeave({ ...leave, endDate: date });
                        setError("");
                    }}
                    minDate={leave.startDate || today}
                    dateFormat="dd.MM.yyyy"
                />
            </div>
            {error && <p className="form-error">{error}</p>}
            <div className="form-footer">
                <button type="button" className="btn" onClick={handleSave}>Save</button>
                <button type="button" className="btn" onClick={onClose}>Cancel</button>
            </div>
        </div>
    );
};

export default LeavePopup;
