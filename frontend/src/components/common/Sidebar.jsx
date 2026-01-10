import { Link, useLocation } from 'react-router-dom';
import { useState, useEffect } from 'react';
import '../../assets/css/sidebar.css';

import ThemeToggle from './ThemeToggle';

const Sidebar = ({ open, onClose }) => {
  const [theme, setTheme] = useState(localStorage.getItem('theme') || 'light');
  const location = useLocation();

  useEffect(() => {
    document.body.classList.remove('light', 'dark');
    document.body.classList.add(theme);
    localStorage.setItem('theme', theme);
  }, [theme]);

  const navItems = [
    { path: '/', icon: 'fa-house', label: 'Home', access: null },
    { path: '/orders', icon: 'fa-screwdriver-wrench', label: 'Orders', access: 'Orders' },
    { path: '/parts', icon: 'fa-boxes-stacked', label: 'Parts Stock', access: 'Parts Stock' },
    { path: '/workers', icon: 'fa-users-gear', label: 'Workers', access: 'Workers' },
    { path: '/job-types', icon: 'fa-gear', label: 'Job Types', access: 'Job Types' },
    { path: '/clients', icon: 'fa-user', label: 'Clients', access: 'Clients' },
    { path: '/service-details', icon: 'fa-circle-info', label: 'Service Details', access: 'Service Details' },
    { path: '/makes-and-models', icon: 'fa-industry', label: 'Makes & models', access: 'Makes and Models' },
    { path: '/cars', icon: 'fa-car', label: 'Cars', access: 'Cars' },
  ];

  const accesses = JSON.parse(localStorage.getItem('accesses') || '[]');
  const filteredNavItems = navItems.filter(item => {
    if (!item.access) return true;
    return accesses.includes(item.access);
  });

  return (
    <>
      <div className={`sidebar-overlay ${open ? 'show' : ''}`} onClick={onClose}></div>
      <aside className={`sidebar ${open ? 'open' : ''}`}>
        <nav>
          {filteredNavItems.map((item, index) => (
            <Link
              key={index}
              to={item.path}
              className={`nav-item list-item ${location.pathname === item.path ? 'active' : ''}`}
              onClick={onClose}
            >
              <div className="horizontal">
                <i className={`fa-solid ${item.icon}`}></i>
                <span>{item.label}</span>
              </div>
            </Link>
          ))}
        </nav>
        <ThemeToggle />
      </aside>
    </>
  );
};

export default Sidebar;
