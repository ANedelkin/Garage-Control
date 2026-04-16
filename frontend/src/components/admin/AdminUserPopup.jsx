import React from 'react';
import '../../assets/css/common/popup.css';

const AdminUserPopup = ({ user, onClose }) => {
    if (!user) return null;

    return (
        <div className="popup-sm">
            <div className="form-section">
                <div>
                    <label className="popup-label">Username</label>
                    <div className="popup-val bold">{user.userName}</div>
                </div>

                <div className="mt-15">
                    <label className="popup-label">Email</label>
                    <div className="popup-val">{user.email}</div>
                </div>

                <div className="mt-15">
                    <label className="popup-label">Last Login</label>
                    <div className="popup-val">{user.lastLogin === '0001-01-01T00:00:00' || !user.lastLogin ? 'Never' : new Date(user.lastLogin).toLocaleString([], { month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' })}</div>
                </div>

                <div className="mt-15">
                    <label className="popup-label">Workshop</label>
                    <div className="popup-val">{user.workshopName || 'N/A'}</div>
                </div>

                <div className="mt-15">
                    <label className="popup-label">Status</label>
                    <div className={`popup-val status ${user.isBlocked ? 'status-blocked' : 'status-active'}`}>
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
