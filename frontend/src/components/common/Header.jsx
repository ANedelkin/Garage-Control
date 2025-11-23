// src/components/common/Header.jsx
import React, { useState } from 'react';

import '../../assets/css/common.css';
import '../../assets/css/header.css';

const Header = ({ onToggleSidebar }) => {
  const [services] = useState(["Main Street Garage", "Downtown Service", "AutoPro - East"]);
  const [selectedService, setSelectedService] = useState(services[0]);

  return (
    <header className="header">
      <div className="brand">
        <button className="hamburger" onClick={onToggleSidebar}>
          <i className="fa-solid fa-bars"></i>
        </button>
        <h1>GarageFlow</h1>
      </div>
      <div className="profile">
        <div className="profile-name">Genco Gencin</div>
        <a className="fa-solid fa-right-from-bracket logout-icon" title="Log out" href="/"></a>
      </div>
    </header>
  );
};

export default Header;
