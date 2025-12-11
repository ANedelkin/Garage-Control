import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import '../../assets/css/common.css';
import '../../assets/css/sidebar.css';

import ThemeToggle from './ThemeToggle';

const Sidebar = ({ selection, open, onClose }) => {
  const [theme, setTheme] = useState(localStorage.getItem('theme') || 'light');

  useEffect(() => {
    document.body.classList.remove('light', 'dark');
    document.body.classList.add(theme);
    localStorage.setItem('theme', theme);
  }, [theme]);

  const navItems = [
    { path: '/', icon: 'fa-house', label: 'Home' },
    { path: '/orders', icon: 'fa-list-check', label: 'Orders' },
    { path: '/parts', icon: 'fa-boxes-stacked', label: 'Parts Stock' },
    { path: '/workers', icon: 'fa-users-gear', label: 'Workers' },
    { path: '/job-types', icon: 'fa-gear', label: 'Job Types' },
    { path: '/clients', icon: 'fa-user', label: 'Clients' },
    { path: '/service-details', icon: 'fa-gear', label: 'Service Details' },
  ];

  return (
    <>
      <div className={`sidebar-overlay ${open ? 'show' : ''}`} onClick={onClose}></div>
      <aside className={`sidebar ${open ? 'open' : ''}`}>
        <nav>
          {navItems.map((item, index) => (
            <Link
              key={index}
              to={item.path}
              className={`nav-item ${index === selection ? 'active' : ''}`}
              onClick={onClose}
            >
              <i className={`fa-solid ${item.icon}`}></i>
              <span>{item.label}</span>
            </Link>
          ))}
        </nav>
        <ThemeToggle />
      </aside>
    </>
  );
};

export default Sidebar;
