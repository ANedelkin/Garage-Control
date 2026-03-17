import React, { useEffect, useRef } from 'react';
import '../../assets/css/common/popup.css';

const UserPopup = ({ user, onLogout, onClose }) => {
    const popupRef = useRef(null);

    useEffect(() => {
        const handleClickOutside = (event) => {
            if (popupRef.current && !popupRef.current.contains(event.target)) {
                onClose();
            }
        };
        document.addEventListener('mousedown', handleClickOutside);
        return () => document.removeEventListener('mousedown', handleClickOutside);
    }, [onClose]);

    return (
        <div className="user-popup tile" ref={popupRef}>
            <div className="section-header">
                <h3>{user?.userName || 'User'}</h3>
                <button className="icon-btn btn" onClick={onClose}>
                    <i className="fa-solid fa-xmark"></i>
                </button>
            </div>
            <div className="divider"></div>
            <div className="popup-actions">
                <button className="btn grow" onClick={onLogout}>
                    <i className="fa-solid fa-right-from-bracket"></i>
                    Log out
                </button>
            </div>
        </div>
    );
};

export default UserPopup;
