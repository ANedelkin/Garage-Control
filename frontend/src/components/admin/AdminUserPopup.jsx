import React from 'react';
import '../../assets/css/common/popup.css';

const AdminUserPopup = ({ user, onClose }) => {
    if (!user) return null;

    return (
        <div className="popup-overlay top" onClick={(e) => { if (e.target === e.currentTarget) onClose(); }}>
            <div className="tile popup" style={{ width: '400px', maxWidth: '95vw' }}>
                <div className="popup-header">
                    <h3 style={{ margin: 0 }}>User Details</h3>
                    <button className="btn icon-btn" onClick={onClose} title="Close">
                        <i className="fa-solid fa-xmark"></i>
                    </button>
                </div>
                
                <div style={{ display: 'flex', flexDirection: 'column', gap: '15px' }}>
                    <div>
                        <label style={{ fontSize: '12px', color: 'var(--text-clr2)' }}>Username</label>
                        <div style={{ fontSize: '15px', fontWeight: '500' }}>{user.userName}</div>
                    </div>
                    
                    <div>
                        <label style={{ fontSize: '12px', color: 'var(--text-clr2)' }}>Email</label>
                        <div style={{ fontSize: '15px' }}>{user.email}</div>
                    </div>

                    <div>
                        <label style={{ fontSize: '12px', color: 'var(--text-clr2)' }}>Role</label>
                        <div style={{ fontSize: '15px' }}>{user.role}</div>
                    </div>

                    <div>
                        <label style={{ fontSize: '12px', color: 'var(--text-clr2)' }}>Workshop</label>
                        <div style={{ fontSize: '15px' }}>{user.workshopName || 'N/A'}</div>
                    </div>

                    <div>
                        <label style={{ fontSize: '12px', color: 'var(--text-clr2)' }}>Status</label>
                        <div style={{ 
                            fontSize: '14px', 
                            fontWeight: '600', 
                            color: user.isBlocked ? 'var(--urgent-text, #dc3545)' : 'var(--safe-text, #28a745)' 
                        }}>
                            {user.isBlocked ? 'Blocked' : 'Active'}
                        </div>
                    </div>
                </div>

                <div style={{ marginTop: '20px', display: 'flex', justifyContent: 'flex-end' }}>
                    <button className="btn" onClick={onClose}>Close</button>
                </div>
            </div>
        </div>
    );
};

export default AdminUserPopup;
