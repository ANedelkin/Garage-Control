import { useState, useEffect } from 'react';
import { useNavigate, BrowserRouter, Routes, Route, Navigate, useParams } from 'react-router-dom';

// Import your components
import LogInPage from './components/auth/LogIn';
import SignUpPage from './components/auth/SignUp';

import Dashboard from './components/dashboard/Dashboard';
import Workers from './components/workers/Workers';
import EditWorker from './components/workers/EditWorker';
import ServiceDetails from './components/serviceDetails/ServiceDetails';
import ServiceDetailsInitial from './components/serviceDetails/ServiceDetailsInitial';
import JobTypes from './components/orders/JobTypes';
import EditJobType from './components/orders/EditJobType.jsx';
import MakesAndModels from './components/cars/MakesAndModels.jsx';
import Cars from './components/cars/Cars.jsx';
import Clients from './components/clients/Clients.jsx';
import EditClient from './components/clients/EditClient.jsx';
import PartsStock from './components/parts/PartsStock.jsx';
import OrdersPage from './components/orders/OrdersPage.jsx';
import NewOrderPage from './components/orders/NewOrderPage.jsx';

import Header from './components/common/Header.jsx';
import Sidebar from './components/common/Sidebar.jsx';

import { authApi } from './services/authApi';

const PrivateRoute = ({ children }) => {
  const loggedIn = localStorage.getItem('LoggedIn');
  if (!loggedIn) {
    return <Navigate to="/login" />;
  }
  const hasService = localStorage.getItem('HasService');
  if (hasService === 'false' && window.location.pathname !== '/service-details-initial') {
    return <Navigate to="/service-details-initial" />;
  }
  return children;
};

function App() {
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const [hydrated, setHydrated] = useState(false);
  const [accesses, setAccesses] = useState([]);

  useEffect(() => {
    const initAuth = async () => {
      const loggedIn = localStorage.getItem('LoggedIn');
      const storedAccesses = localStorage.getItem('accesses');

      if (loggedIn) {
        if (storedAccesses) {
          try {
            setAccesses(JSON.parse(storedAccesses));
          } catch (e) {
            console.error("Failed to parse stored accesses", e);
          }
        }

        // if (hasService === 'false' && window.location.pathname !== '/service-details-initial') {
        //   navigate('/service-details-initial');
        // }

        setHydrated(true);
      } else {
        try {
          const data = await authApi.refreshToken();
          console.log(data);
          if (data && data.success) {
            localStorage.setItem('LoggedIn', 'true');
            if (data.accesses) {
              setAccesses(data.accesses);
            }
            // if (data.hasService === false && window.location.pathname !== '/service-details-initial') {
            //   window.location.href = '/service-details-initial';
            //   return;
            // }
          }
        } catch (e) {
          console.log(e);
        } finally {
          setHydrated(true);
        }
      }
    };

    initAuth();

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
    return null;
  }

  const routes = [
    { path: '/', element: <Dashboard />, children: [] },
    {
      path: '/orders', element: <OrdersPage />, access: 'Orders', children: [
        { path: '/new', element: <NewOrderPage /> },
        { path: '/:id', element: <NewOrderPage /> }
      ]
    },
    { path: '/parts', element: <PartsStock />, children: [], access: 'Parts Stock' },
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
          <Route path="/login" element={<LogInPage />} />
          <Route path="/signup" element={<SignUpPage />} />

          <Route path="/service-details-initial" element={<PrivateRoute>
            <Header onToggleSidebar={() => setSidebarOpen(!sidebarOpen)} />
            <ServiceDetailsInitial />
          </PrivateRoute>} />

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
