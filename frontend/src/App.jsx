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
import MakesAndModels from './components/cars/MakesAndModels.jsx';
import Cars from './components/cars/Cars.jsx';
import Clients from './components/clients/Clients.jsx';
import EditClient from './components/clients/EditClient.jsx';

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
  const [curSelection, setCurSelection] = useState(0);
  const [hydrated, setHydrated] = useState(false);
  const [accesses, setAccesses] = useState([]);

  useEffect(() => {
    // Check for tokens in the URL (Google OAuth redirect)
    const urlParams = new URLSearchParams(window.location.search);
    const accessToken = urlParams.get('access_token');
    const refreshToken = urlParams.get('refresh_token');
    const accessesParam = urlParams.get('accesses');

    if (accessToken) {
      localStorage.setItem('accessToken', accessToken);
      if (refreshToken) {
        localStorage.setItem('refreshToken', refreshToken);
      }
      if (accessesParam) {
        try {
          const decodedAccesses = decodeURIComponent(accessesParam);
          localStorage.setItem('accesses', decodedAccesses);
          setAccesses(JSON.parse(decodedAccesses));
        } catch (e) {
          console.error("Failed to decode accesses", e);
        }
      }
      // Clean up the URL
      window.history.replaceState({}, document.title, window.location.pathname);
    } else {
      // Load initial accesses if not coming from a redirect
      const storedAccesses = localStorage.getItem('accesses');
      if (storedAccesses) {
        try {
          setAccesses(JSON.parse(storedAccesses));
        } catch (e) {
          console.error("Failed to parse stored accesses", e);
        }
      }
    }

    setHydrated(true);

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

  if (!hydrated) {
    return null; // Or a loading spinner
  }

  const routes = [
    { path: '/', element: <Dashboard />, children: [] },
    { path: '/orders', element: <Dashboard />, children: [], access: 'Orders' },
    { path: '/parts', element: <Dashboard />, children: [], access: 'Parts Stock' },
    {
      path: '/workers', element: <Workers />, access: 'Workers', children: [
        { path: '/new', element: <EditWorker /> },
        { path: '/:id', element: <EditWorker /> }
      ]
    },
    {
      path: '/job-types', element: <JobTypes />, access: 'Job Types', children: [
        { path: '/new', element: <EditJobType /> },
        { path: '/:id', element: <EditJobType /> }
      ]
    },
    {
      path: '/clients', element: <Clients />, access: 'Clients', children: [
        { path: '/new', element: <EditClient /> },
        { path: '/:id', element: <EditClient /> }
      ]
    },
    { path: '/service-details', element: <ServiceDetails />, children: [], access: 'Service Details' },
    { path: '/makes-and-models', element: <MakesAndModels />, children: [], access: 'Makes and Models' },
    { path: '/cars', element: <Cars />, children: [], access: 'Cars' },
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

          {routes.filter(r => !r.access || accesses.includes(r.access)).map((route, i) => (
            <>
              <Route
                key={route.path}
                path={route.path}
                element={
                  <PrivateRoute>
                    <LayoutWithHeader
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
                  key={route.path + childRoute.path}
                  path={route.path + childRoute.path}
                  element={
                    <PrivateRoute>
                      <LayoutWithHeader
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

function LayoutWithHeader({ children, sidebarOpen, setSidebarOpen }) {
  return (
    <>
      <Header onToggleSidebar={() => setSidebarOpen(!sidebarOpen)} />
      <div className="horizontal work-area">
        <Sidebar open={sidebarOpen} onClose={() => setSidebarOpen(false)} />
        {children}
      </div>
    </>
  );
}
