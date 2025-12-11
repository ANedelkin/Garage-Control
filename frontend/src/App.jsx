import { useState, useEffect } from 'react';
import { BrowserRouter, Routes, Route, Navigate, useParams } from 'react-router-dom';

// Import your components
import LogInPage from './components/auth/LogIn';
import SignUpPage from './components/auth/SignUp';

import Dashboard from './components/dashboard/Dashboard';
import Workers from './components/workers/Workers';
import EditWorker from './components/workers/EditWorker';
import ServiceDetails from './components/ServiceDetails/ServiceDetails';
import ServiceDetailsInitial from './components/ServiceDetails/ServiceDetailsInitial';
import JobTypes from './components/orders/JobTypes';
import EditJobType from './components/orders/EditJobType.jsx';

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

// Wrapper to pass params to EditJobType
const EditJobTypeWrapper = () => {
  const { id } = useParams();
  return <EditJobType id={id} />;
};

function App() {
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const [curSelection, setCurSelection] = useState(0);

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
    { path: '/', element: <Dashboard />, children: [] },
    { path: '/orders', element: <Dashboard />, children: [] },
    { path: '/parts', element: <Dashboard />, children: [] },
    {
      path: '/workers', element: <Workers />, children: [
        { path: '/new', element: <EditWorker /> },
        { path: '/:id', element: <EditWorker /> }
      ]
    },
    {
      path: '/job-types', element: <JobTypes />, children: [
        { path: '/new', element: <EditJobType id="" /> },
        { path: '/:id', element: <EditJobTypeWrapper /> }
      ]
    },
    { path: '/clients', element: <Dashboard />, children: [] },
    { path: '/service-details', element: <ServiceDetails />, children: [] },
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
            sidebarOpen={sidebarOpen}
            setSidebarOpen={setSidebarOpen}>
            <Header onToggleSidebar={() => setSidebarOpen(!sidebarOpen)} />
            <ServiceDetailsInitial />
          </LayoutWithHeader></PrivateRoute>} />

          {routes.map((route, i) => (
            <>
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
                  </PrivateRoute>
                }
              />
              {route.children.map(childRoute => (
                <Route
                  path={route.path + childRoute.path}
                  element={
                    <PrivateRoute>
                      <LayoutWithHeader
                        selection={i}
                        sidebarOpen={sidebarOpen}
                        setSidebarOpen={setSidebarOpen}
                      >
                        {childRoute.element}
                      </LayoutWithHeader>
                    </PrivateRoute>
                  }
                />
              ))
              }
            </>

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
      <div className="horizontal work-area">
        <Sidebar selection={selection} open={sidebarOpen} onClose={() => setSidebarOpen(false)} />
        {children}
      </div>
    </>
  );
}
