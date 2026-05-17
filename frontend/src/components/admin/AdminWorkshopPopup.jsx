import React from 'react';

const AdminWorkshopPopup = ({ workshop, onClose }) => {
    if (!workshop) return null;

    return (
        <div className="admin-popup">
            <div className="popup-info-grid">
                <div className="popup-info-row">
                    <label className="popup-label">Workshop Name</label>
                    <div className="popup-val">{workshop.name || 'N/A'}</div>
                </div>
                
                <div className="popup-info-row">
                    <label className="popup-label">Contact Email</label>
                    <div className="popup-val">{workshop.contactEmail || 'N/A'}</div>
                </div>

                <div className="popup-info-row">
                    <label className="popup-label">Address</label>
                    <div className="popup-val">{workshop.address || 'N/A'}</div>
                </div>

                <div className="popup-info-row">
                    <label className="popup-label">Team Size</label>
                    <div className="popup-val">{workshop.workersCount || 0} workers</div>
                </div>

                <div className="popup-info-row">
                    <label className="popup-label">Status</label>
                    <div className={`popup-val bold ${workshop.isBlocked ? 'status-blocked' : 'status-active'}`}>
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
