import React from 'react';

const AdminUserPopup = ({ user, onClose }) => {
    if (!user) return null;

    return (
        <div className="admin-popup">
            <div className="popup-info-grid">
                <div className="popup-info-row">
                    <label className="popup-label">Username</label>
                    <div className="popup-val">{user.userName || 'N/A'}</div>
                </div>

                <div className="popup-info-row">
                    <label className="popup-label">Email</label>
                    <div className="popup-val">{user.email || 'N/A'}</div>
                </div>

                <div className="popup-info-row">
                    <label className="popup-label">Last Login</label>
                    <div className="popup-val">
                        {user.lastLogin === '0001-01-01T00:00:00' || !user.lastLogin 
                            ? 'N/A' 
                            : new Date(user.lastLogin).toLocaleString([], { month: 'short', day: 'numeric', year: 'numeric', hour: '2-digit', minute: '2-digit' })}
                    </div>
                </div>

                <div className="popup-info-row">
                    <label className="popup-label">Workshop</label>
                    <div className="popup-val">{user.workshopName || 'N/A'}</div>
                </div>

                <div className="popup-info-row">
                    <label className="popup-label">Status</label>
                    <div className={`popup-val bold ${user.isBlocked ? 'status-blocked' : 'status-active'}`}>
                        {user.isBlocked ? 'Blocked' : 'Active'}
                    </div>
                </div>
            </div>

            <div className="form-footer">
                <button className="btn" onClick={onClose}>Close</button>
            </div>
        </div>
    );
};

export default AdminUserPopup;
