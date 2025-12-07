import { useState, useEffect } from 'react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';

// Import your components
import LogInPage from './components/auth/LogIn';
import SignUpPage from './components/auth/SignUp';
import Dashboard from './components/dashboard/Dashboard';
import ServiceDetailsInitial from './components/ServiceDetails/ServiceDetailsInitial';
import ServiceDetails from './components/ServiceDetails/ServiceDetails';
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

          <Route path="/" element={<PrivateRoute><LayoutWithHeader
            selection={0}
            sidebarOpen={sidebarOpen}
            setSidebarOpen={setSidebarOpen}>
            <Dashboard />
          </LayoutWithHeader></PrivateRoute>} />

          <Route path="/service-details" element={<PrivateRoute><LayoutWithHeader
            selection={5}
            sidebarOpen={sidebarOpen}
            setSidebarOpen={setSidebarOpen}>
            <ServiceDetails />
          </LayoutWithHeader></PrivateRoute>} />

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
