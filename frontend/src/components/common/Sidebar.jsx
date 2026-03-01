import { Link, useLocation } from 'react-router-dom';
import { useState, useEffect } from 'react';
import '../../assets/css/sidebar.css';

import ThemeToggle from './ThemeToggle';
import Popup from './Popup';
import WorkshopDetails from '../workshopDetails/WorkshopDetails';

const Sidebar = ({ open, onClose, accesses = [] }) => {
  const [theme, setTheme] = useState(localStorage.getItem('theme') || 'light');
  const location = useLocation();

  useEffect(() => {
    document.body.classList.remove('light', 'dark');
    document.body.classList.add(theme);
    localStorage.setItem('theme', theme);
  }, [theme]);

  const navItems = [
    { path: '/', icon: 'fa-house', label: 'Home', accesses: ['Dashboard'] },
    { path: '/todo', icon: 'fa-clipboard-list', label: 'To Do', accesses: ['To Do'] },
    { path: '/orders', icon: 'fa-screwdriver-wrench', label: 'Orders', accesses: ['Orders'] },
    { path: '/parts', icon: 'fa-boxes-stacked', label: 'Parts Stock', accesses: ['Parts Stock'] },
    { path: '/workers', icon: 'fa-users-gear', label: 'Workers', accesses: ['Workers'] },
    { path: '/clients', icon: 'fa-user', label: 'Clients', accesses: ['Clients'] },
    { path: '/cars', icon: 'fa-car', label: 'Cars', accesses: ['Cars'] },
    { path: '/activity-log', icon: 'fa-clock-rotate-left', label: 'Activity Log', accesses: ['Activity Log'] },
    { divider: true, accesses: ['Done Orders', 'Job Types', 'Makes and Models', 'Workshop Details'] },
    { path: '/done-orders', icon: 'fa-clipboard-check', label: 'Done Orders', accesses: ['Orders'] },
    { path: '/job-types', icon: 'fa-gear', label: 'Job Types', accesses: ['Job Types'] },
    { path: '/makes-and-models', icon: 'fa-industry', label: 'Makes & models', accesses: ['Makes and Models'] },
    { icon: 'fa-circle-info', label: 'Workshop Details', accesses: ['Workshop Details'], popup: true, popupComponent: WorkshopDetails },
    { path: '/admin/dashboard', icon: 'fa-gauge', label: 'Dashboard', accesses: ['Admin'] },
    { path: '/admin/makes-models', icon: 'fa-industry', label: 'Makes & Models', accesses: ['Admin'] },
    { path: '/admin/users', icon: 'fa-users', label: 'Users', accesses: ['Admin'] },
    { path: '/admin/workshops', icon: 'fa-shop', label: 'Workshops', accesses: ['Admin'] },
  ];

  const isPathActive = (itemPath) => {
    // Exact match for root path
    if (itemPath === '/') {
      return location.pathname === '/';
    }
    // For other paths, check if current path starts with item path
    // and is followed by either nothing or a slash
    return location.pathname === itemPath ||
      location.pathname.startsWith(itemPath + '/');
  };

  console.log(accesses);
  const filteredNavItems = navItems.filter(item => {
    if (!item.accesses) return true;
    return accesses.some(access => item.accesses.includes(access));
  });

  const [ActivePopup, setActivePopup] = useState(null);

  return (
    <>
      <div className={`sidebar-overlay ${open ? 'show' : ''}`} onClick={onClose}></div>
      <aside className={`sidebar ${open ? 'open' : ''}`}>
        <nav>
          {filteredNavItems.map((item, index) => (
            item.divider ? //Divider
              <div key={index} className="divider" style={{ margin: '6px 0' }}></div> :
              item.popup ? ( //Popup
                <div
                  key={index}
                  className="nav-item list-item"
                  onClick={() => {
                    setActivePopup(() => item.popupComponent);
                    onClose();
                  }}
                  style={{ cursor: 'pointer' }}
                >
                  <div className="horizontal">
                    <i className={`fa-solid ${item.icon}`}></i>
                    <span>{item.label}</span>
                  </div>
                </div>
              ) : ( //Link
                <Link
                  key={index}
                  to={item.path}
                  className={`nav-item list-item ${isPathActive(item.path) ? 'active' : ''}`}
                  onClick={onClose}
                >
                  <div className="horizontal">
                    <i className={`fa-solid ${item.icon}`}></i>
                    <span>{item.label}</span>
                  </div>
                </Link>
              )
          ))}
        </nav>
        <ThemeToggle />
      </aside>

      <Popup
        isOpen={!!ActivePopup}
        onClose={() => setActivePopup(null)}
        title={filteredNavItems.find(i => i.popupComponent === ActivePopup)?.label}
      >
        {ActivePopup && <ActivePopup onClose={() => setActivePopup(null)} />}
      </Popup>
    </>
  );
};

export default Sidebar;
