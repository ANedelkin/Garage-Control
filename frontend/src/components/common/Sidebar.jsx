import { Link, useLocation } from 'react-router-dom';
import { useState, useEffect } from 'react';
import '../../assets/css/sidebar.css';

import ThemeToggle from './ThemeToggle';

const Sidebar = ({ open, onClose, accesses = [] }) => {
  const [theme, setTheme] = useState(localStorage.getItem('theme') || 'light');
  const location = useLocation();

  useEffect(() => {
    document.body.classList.remove('light', 'dark');
    document.body.classList.add(theme);
    localStorage.setItem('theme', theme);
  }, [theme]);

  const navItems = [
    { path: '/', icon: 'fa-house', label: 'Home', access: 'Dashboard' },
    { path: '/todo', icon: 'fa-clipboard-list', label: 'To Do', access: 'To Do' },
    { path: '/orders', icon: 'fa-screwdriver-wrench', label: 'Orders', access: 'Orders' },
    { path: '/done-orders', icon: 'fa-check-double', label: 'Done Orders', access: 'Orders' },
    { path: '/parts', icon: 'fa-boxes-stacked', label: 'Parts Stock', access: 'Parts Stock' },
    { path: '/workers', icon: 'fa-users-gear', label: 'Workers', access: 'Workers' },
    { path: '/job-types', icon: 'fa-gear', label: 'Job Types', access: 'Job Types' },
    { path: '/clients', icon: 'fa-user', label: 'Clients', access: 'Clients' },
    { path: '/workshop-details', icon: 'fa-circle-info', label: 'Workshop Details', access: 'Workshop Details' },
    { path: '/makes-and-models', icon: 'fa-industry', label: 'Makes & models', access: 'Makes and Models' },
    { path: '/cars', icon: 'fa-car', label: 'Cars', access: 'Cars' },
    { path: '/admin/dashboard', icon: 'fa-gauge', label: 'Dashboard', access: 'Admin Dashboard' },
    { path: '/admin/makes-models', icon: 'fa-industry', label: 'Makes & Models', access: 'Admin Makes and Models' },
    { path: '/admin/users', icon: 'fa-users', label: 'Users', access: 'Admin Users' },
    { path: '/admin/workshops', icon: 'fa-shop', label: 'Workshops', access: 'Admin Workshops' },
  ];

  console.log(accesses);
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
