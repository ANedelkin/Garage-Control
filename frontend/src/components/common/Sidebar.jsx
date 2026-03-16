import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useState, useEffect, useRef } from 'react';
import '../../assets/css/sidebar.css';

import ThemeToggle from './ThemeToggle';
import { usePopup } from '../../context/PopupContext';
import WorkshopDetails from '../workshopDetails/WorkshopDetails';

const Sidebar = ({ open, onClose, accesses = [] }) => {
  const [theme, setTheme] = useState(localStorage.getItem('theme') || 'light');
  const { addPopup, removeLastPopup } = usePopup();
  const [lastActivePath, setLastActivePath] = useState(localStorage.getItem('lastActiveSidebarPath') || '/');
  const location = useLocation();
  const navigate = useNavigate();
  const workshopPopupOpened = useRef(false);

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

  useEffect(() => {
    // Check if current path matches any nav item (excluding popups)
    const matchingItem = navItems.find(item => item.path && (
      location.pathname === item.path
    ));

    if (matchingItem) {
      setLastActivePath(matchingItem.path);
      localStorage.setItem('lastActiveSidebarPath', matchingItem.path);
    }
  }, [location.pathname]);

  const isPathActive = (itemPath) => {
    // Check if this item is the "best" match for the current URL
    const isDirectMatch = location.pathname === itemPath;

    // If no navigation item matches the current path exactly, 
    // fallback to highlighting the last successful match.
    const anyItemMatches = navItems.some(item => item.path && (
      location.pathname === item.path
    ));

    if (anyItemMatches) {
      return isDirectMatch;
    }

    return itemPath === lastActivePath;
  };

  console.log(accesses);
  const filteredNavItems = navItems.filter(item => {
    if (!item.accesses) return true;
    return accesses.some(access => item.accesses.includes(access));
  });

  const handlePopupOpen = (item) => {
    if (item.label === 'Workshop Details') {
      navigate('/workshop-details');
    } else {
      const PopupComponent = item.popupComponent;
      addPopup(
        item.label,
        <PopupComponent onClose={removeLastPopup} />
      );
    }
    onClose();
  };

  useEffect(() => {
    if (location.pathname === '/workshop-details' && !workshopPopupOpened.current) {
        workshopPopupOpened.current = true;
        addPopup(
            'Workshop Details',
            <WorkshopDetails onClose={() => { removeLastPopup(); navigate('/'); workshopPopupOpened.current = false; }} />,
            false,
            () => { navigate('/'); workshopPopupOpened.current = false; }
        );
    }
  }, [location.pathname]);

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
                  onClick={() => handlePopupOpen(item)}
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
    </>
  );
};

export default Sidebar;
