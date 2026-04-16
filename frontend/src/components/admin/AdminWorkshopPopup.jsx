import React from 'react';
import '../../assets/css/common/popup.css';

const AdminWorkshopPopup = ({ workshop, onClose }) => {
    if (!workshop) return null;

    return (
        <div className="popup-sm">
            <div className="form-section">
                <div>
                    <label className="popup-label">Name</label>
                    <div className="popup-val bold">{workshop.name}</div>
                </div>
                
                <div className="mt-15">
                    <label className="popup-label">Contact Email</label>
                    <div className="popup-val">{workshop.contactEmail}</div>
                </div>

                <div className="mt-15">
                    <label className="popup-label">Address</label>
                    <div className="popup-val">{workshop.address}</div>
                </div>

                <div className="mt-15">
                    <label className="popup-label">Total Workers</label>
                    <div className="popup-val">{workshop.workersCount}</div>
                </div>

                <div className="mt-15">
                    <label className="popup-label">Status</label>
                    <div className={`popup-val status ${workshop.isBlocked ? 'status-blocked' : 'status-active'}`}>
                        {workshop.isBlocked ? 'Blocked' : 'Active'}
                    </div>
                </div>
            </div>

            <div className="form-footer">
                <button className="btn" onClick={onClose}>Close</button>
            </div>
        </div>
    );
};

export default AdminWorkshopPopup;
