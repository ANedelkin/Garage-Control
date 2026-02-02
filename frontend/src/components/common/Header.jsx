import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { notificationApi } from '../../services/notificationApi';
import NotificationPopup from './NotificationPopup';

import '../../assets/css/common/controls.css';
import '../../assets/css/header.css';

const Header = ({ onToggleSidebar }) => {
  const [services] = useState(["Main Street Garage", "Downtown Service", "AutoPro - East"]);
  const [selectedService, setSelectedService] = useState(services[0]);
  const [showNotifications, setShowNotifications] = useState(false);
  const [notifications, setNotifications] = useState([]);
  const navigate = useNavigate();
  const { logout } = useAuth();

  const fetchNotifications = async () => {
    try {
      const data = await notificationApi.getNotifications();
      setNotifications(data);
    } catch (error) {
      console.error('Failed to fetch notifications:', error);
    }
  };

  useEffect(() => {
    fetchNotifications();
    // Poll for new notifications every 30 seconds
    const interval = setInterval(fetchNotifications, 30000);
    return () => clearInterval(interval);
  }, []);

  const handleLogout = async () => {
    console.log('Logout clicked');
    await logout();
    navigate('/login');
  };

  const unreadCount = notifications.filter(n => !n.isRead).length;

  return (
    <header>
      <div className="brand">
        <button className="hamburger" onClick={onToggleSidebar}>
          <i className="fa-solid fa-bars"></i>
        </button>
        <h1>GarageControl</h1>
      </div>
      <div className="profile">
        <div className="profile-name">Genco Gencin</div>
        <button
          className="icon-btn btn notification-btn"
          title="Notifications"
          onClick={() => setShowNotifications(!showNotifications)}
        >
          <i className="fa-solid fa-bell"></i>
          {unreadCount > 0 && <span className="notification-badge">{unreadCount}</span>}
        </button>
        <a className="fa-solid fa-right-from-bracket icon-btn btn" title="Log out" onClick={handleLogout}></a>
      </div>
      {showNotifications && (
        <NotificationPopup
          notifications={notifications}
          onClose={() => setShowNotifications(false)}
          onRefresh={fetchNotifications}
        />
      )}
    </header>
  );
};

export default Header;
