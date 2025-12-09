import { useState, useEffect } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';

// Import your components
import LogInPage from './components/auth/LogIn';
import SignUpPage from './components/auth/SignUp';

import Dashboard from './components/dashboard/Dashboard';
import Workers from './components/workers/Workers';
import Activities from './components/orders/Activities';
import ServiceDetails from './components/ServiceDetails/ServiceDetails';
import ServiceDetailsInitial from './components/ServiceDetails/ServiceDetailsInitial';

import Header from './components/common/Header.jsx';
import Sidebar from './components/common/Sidebar.jsx';

// PrivateRoute component to check authentication
const PrivateRoute = ({ children }) => {
  const token = localStorage.getItem('accessToken');
  if (!token) {
    return <Navigate to="/login" />;
  }
  return children;
};

function App() {
  const [sidebarOpen, setSidebarOpen] = useState(false);

  useEffect(() => {
    const handleResize = () => {
      const isDesktop = window.innerWidth > 1000;
      if (isDesktop) {
        setSidebarOpen(false);
      }
    };
    window.addEventListener('resize', handleResize);
    handleResize();
    return () => {
      window.removeEventListener('resize', handleResize);
    };
  }, []);

  const routes = [
    { path: '/', element: <Dashboard />},
    { path: '/orders', element: <Dashboard /> },
    { path: '/parts', element: <Dashboard />},
    { path: '/workers', element: <Workers /> },
    { path: '/activities', element: <Activities /> },
    { path: '/clients', element: <Dashboard /> },
    { path: '/service-details', element: <ServiceDetails /> },
  ];

  return (
    <>
      <BrowserRouter>
        <Routes>
          {/* Public Routes */}
          <Route path="/login" element={<LogInPage />} />
          <Route path="/signup" element={<SignUpPage />} />

          {/* Private Routes */}
          <Route path="/service-details-initial" element={<PrivateRoute><LayoutWithHeader
            selection={0}
            sidebarOpen={sidebarOpen}
            setSidebarOpen={setSidebarOpen}>
            <Header onToggleSidebar={() => setSidebarOpen(!sidebarOpen)} />
            <ServiceDetailsInitial />
          </LayoutWithHeader></PrivateRoute>} />

          {routes.map((route, i) => (
            <Route
              path={route.path}
              element={
                <PrivateRoute>
                  <LayoutWithHeader
                    selection={i}
                    sidebarOpen={sidebarOpen}
                    setSidebarOpen={setSidebarOpen}
                  >
                    {route.element}
                  </LayoutWithHeader>
                </PrivateRoute>} />
          ))}

        </Routes>
      </BrowserRouter>
    </>
  );
}

export default App;

function LayoutWithHeader({ selection, children, sidebarOpen, setSidebarOpen }) {
  return (
    <>
      <Header onToggleSidebar={() => setSidebarOpen(!sidebarOpen)} />
      <div className="horizontal-layout">
        <Sidebar selection={selection} open={sidebarOpen} onClose={() => setSidebarOpen(false)} />
        {children}
      </div>
    </>
  );
}
