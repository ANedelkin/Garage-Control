import { useState } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';

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

  return (
    <>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<LogInPage />} />
          <Route path="/signup" element={<SignUpPage />} />

          <Route path="/service-details-initial" element={<PrivateRoute>
            <Header />
            <ServiceDetailsInitial />
          </PrivateRoute>} />

          <Route path="/" element={<PrivateRoute> <LayoutWithHeader selection={0}>
            <Dashboard />
          </LayoutWithHeader></PrivateRoute>} />

          <Route path="/service-details" element={<PrivateRoute> <LayoutWithHeader selection={5}>
            <ServiceDetails />
          </LayoutWithHeader></PrivateRoute>} />

        </Routes>
      </BrowserRouter>
    </>
  );

  function LayoutWithHeader({ selection, children }) {
    return (
      <>
        <Header onToggleSidebar={setSidebarOpen(!sidebarOpen)} />
        <div className="horizontal-layout">
          <Sidebar selection={selection} open={sidebarOpen} onClose={setSidebarOpen(false)} />
          {children}
        </div>
      </>
    );
  }
}

export default App;
