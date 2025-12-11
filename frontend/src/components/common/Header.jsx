import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { authApi } from '../../services/authApi.js';

import '../../assets/css/common.css';
import '../../assets/css/header.css';

const Header = ({ onToggleSidebar }) => {
  const [services] = useState(["Main Street Garage", "Downtown Service", "AutoPro - East"]);
  const [selectedService, setSelectedService] = useState(services[0]);
  const navigate = useNavigate();

  const handleLogout = async () => {
    console.log('Logout clicked');
    try {
      await authApi.logout();
      navigate('/login');
    } catch (error) {
      console.error('Logout failed:', error);
      navigate('/login');
    }
  };

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
        <a className="fa-solid fa-right-from-bracket icon-btn" title="Log out" onClick={handleLogout}></a>
      </div>
    </header>
  );
};

export default Header;
