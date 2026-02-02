import React, { useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { notificationApi } from '../../services/notificationApi';
import '../../assets/css/common/popup.css';
// import '../../assets/css/common/lists.css';

const NotificationPopup = ({ notifications, onClose, onRefresh }) => {
    const popupRef = useRef(null);
    const navigate = useNavigate();

    useEffect(() => {
        const handleClickOutside = (event) => {
            if (popupRef.current && !popupRef.current.contains(event.target)) {
                onClose();
            }
        };
        document.addEventListener('mousedown', handleClickOutside);
        return () => document.removeEventListener('mousedown', handleClickOutside);
    }, [onClose]);

    const handleNotificationClick = async (notification) => {
        if (!notification.isRead) {
            await notificationApi.markAsRead(notification.id);
            onRefresh();
        }
        if (notification.link) {
            navigate(notification.link);
            onClose();
        }
    };

    const formatDate = (dateString) => {
        const date = new Date(dateString);
        const now = new Date();
        const diffMs = now - date;
        const diffMins = Math.floor(diffMs / 60000);
        const diffHours = Math.floor(diffMs / 3600000);
        const diffDays = Math.floor(diffMs / 86400000);

        if (diffMins < 1) return 'Just now';
        if (diffMins < 60) return `${diffMins}m ago`;
        if (diffHours < 24) return `${diffHours}h ago`;
        if (diffDays < 7) return `${diffDays}d ago`;
        return date.toLocaleDateString();
    };

    return (
        <div className="popup-overlay">
            <div className="popup notification-popup" ref={popupRef}>
                <div className="popup-header">
                    <h2>Notifications</h2>
                    <button className="icon-btn btn" onClick={onClose}>
                        <i className="fa-solid fa-xmark"></i>
                    </button>
                </div>
                <div className="popup-content">
                    <div className="list-container">
                        {notifications.length === 0 ? (
                            <div className="empty-state">
                                <i className="fa-solid fa-bell-slash"></i>
                                <p>No notifications</p>
                            </div>
                        ) : (
                            notifications.map((notification) => (
                                <div
                                    key={notification.id}
                                    className={`list-item ${notification.isRead ? 'read' : 'unread'}`}
                                    onClick={() => handleNotificationClick(notification)}
                                    style={{ cursor: notification.link ? 'pointer' : 'default' }}
                                >
                                    <div className="notification-content">
                                        <div className="notification-message">
                                            {!notification.isRead && <span className="unread-dot"></span>}
                                            {notification.message}
                                        </div>
                                        <div className="notification-time">
                                            {formatDate(notification.createdAt)}
                                        </div>
                                    </div>
                                    {notification.link && (
                                        <i className="fa-solid fa-chevron-right"></i>
                                    )}
                                </div>
                            ))
                        )}
                    </div>
                </div>
            </div>
        </div>
    );
};

export default NotificationPopup;
