import { Link, useLocation, useNavigate } from 'react-router-dom';
import React, { useState, useEffect, useRef } from 'react';
import '../../assets/css/sidebar.css';

import ThemeToggle from './ThemeToggle';
import { usePopup } from '../../context/PopupContext';
import WorkshopDetails from '../workshopDetails/WorkshopDetails';

const Sidebar = ({ open, onClose, accesses = [], routes = [] }) => {
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

  // Helper to recursively check if a path or any of its children match the current pathname
  const isRouteMatching = (route, currentPath, parentPath = '') => {
    if (!route.path) return false;
    const fullPath = (parentPath + route.path).replace('//', '/');
    
    // Check exact match or if current path starts with this path (and it's not root)
    if (currentPath === fullPath) return true;
    if (fullPath !== '/' && currentPath.startsWith(fullPath)) return true;

    if (route.children) {
      return route.children.some(child => isRouteMatching(child, currentPath, fullPath));
    }
    return false;
  };

  const isPathActive = (navItem) => {
    if (navItem.path) {
      if (isRouteMatching(navItem, location.pathname)) return true;

      // Fallback: If no sidebar item matches the current URL (e.g. /jobs/123),
      // we check if this item matches the lastActivePath that was set.
      const anyMatch = routes.some(r => r.path && r.label && isRouteMatching(r, location.pathname));
      if (!anyMatch && navItem.path === lastActivePath) {
        return true;
      }
    }
    return false;
  };

  useEffect(() => {
    if (!routes || routes.length === 0) return;
    const matchingItem = routes.find(item => item.path && item.label && isRouteMatching(item, location.pathname));
    if (matchingItem && matchingItem.path) {
      setLastActivePath(matchingItem.path);
      localStorage.setItem('lastActiveSidebarPath', matchingItem.path);
    }
  }, [location.pathname, routes]);

  const filteredNavItems = routes.filter(item => {
    if (!item.divider && !item.label) return false;
    if (!item.accesses) return true;
    return accesses.some(access => item.accesses.includes(access));
  }).reduce((acc, current, idx, arr) => {
    // Post-filter pass to remove redundant dividers
    if (current.divider) {
      // 1. Skip if it's the first item
      if (acc.length === 0) return acc;
      
      // 2. Skip if it's the last item (will handle below)
      if (idx === arr.length - 1) return acc;

      // 3. Skip if there's already a divider at the end of acc (consecutive)
      if (acc[acc.length - 1].divider) return acc;
    }
    
    acc.push(current);
    return acc;
  }, []);

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
        <div className="sidebar-nav">
          <nav>
            {filteredNavItems.map((item, index) => {
              if (item.divider) {
                return <div key={index} className="divider" style={{ margin: '10px 0', borderTop: '1px solid var(--border2)', opacity: 0.5 }}></div>;
              }

              if (item.popup) {
                const PopupComponent = item.popupComponent;
                return (
                  <div
                    key={index}
                    className="nav-item list-item"
                    onClick={() => {
                      onClose();
                      addPopup(
                        item.label,
                        <PopupComponent onClose={removeLastPopup} />
                      );
                    }}
                  >
                    <div className="horizontal">
                      <i className={`fa-solid ${item.icon}`}></i>
                      <span>{item.label}</span>
                    </div>
                  </div>
                );
              }

              return (
                <Link
                  key={item.path}
                  to={item.path}
                  className={`nav-item list-item ${isPathActive(item) ? 'active' : ''}`}
                  onClick={onClose}
                >
                  <div className="horizontal">
                    <i className={`fa-solid ${item.icon}`}></i>
                    <span>{item.label}</span>
                  </div>
                </Link>
              );
            })}
          </nav>
        </div>

        <div className="sidebar-footer">
          <ThemeToggle theme={theme} setTheme={setTheme} />
        </div>
      </aside>
    </>
  );
};

export default Sidebar;
