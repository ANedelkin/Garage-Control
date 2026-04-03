import React from 'react';
import '../../assets/css/common/popup.css';

const AdminWorkshopPopup = ({ workshop, onClose }) => {
    if (!workshop) return null;

    return (
        <div className="popup-overlay top" onClick={(e) => { if (e.target === e.currentTarget) onClose(); }}>
            <div className="tile popup" style={{ width: '400px', maxWidth: '95vw' }}>
                <div className="popup-header">
                    <h3 style={{ margin: 0 }}>Workshop Details</h3>
                    <button className="btn icon-btn" onClick={onClose} title="Close">
                        <i className="fa-solid fa-xmark"></i>
                    </button>
                </div>
                
                <div style={{ display: 'flex', flexDirection: 'column', gap: '15px' }}>
                    <div>
                        <label style={{ fontSize: '12px', color: 'var(--text-clr2)' }}>Name</label>
                        <div style={{ fontSize: '15px', fontWeight: '500' }}>{workshop.name}</div>
                    </div>
                    
                    <div>
                        <label style={{ fontSize: '12px', color: 'var(--text-clr2)' }}>Contact Email</label>
                        <div style={{ fontSize: '15px' }}>{workshop.contactEmail}</div>
                    </div>

                    <div>
                        <label style={{ fontSize: '12px', color: 'var(--text-clr2)' }}>Address</label>
                        <div style={{ fontSize: '15px' }}>{workshop.address}</div>
                    </div>

                    <div>
                        <label style={{ fontSize: '12px', color: 'var(--text-clr2)' }}>Total Workers</label>
                        <div style={{ fontSize: '15px' }}>{workshop.workersCount}</div>
                    </div>

                    <div>
                        <label style={{ fontSize: '12px', color: 'var(--text-clr2)' }}>Status</label>
                        <div style={{ 
                            fontSize: '14px', 
                            fontWeight: '600', 
                            color: workshop.isBlocked ? 'var(--urgent-text, #dc3545)' : 'var(--safe-text, #28a745)' 
                        }}>
                            {workshop.isBlocked ? 'Blocked' : 'Active'}
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

export default AdminWorkshopPopup;
