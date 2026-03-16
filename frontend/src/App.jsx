import { useState, useEffect, Fragment } from 'react';
import { useNavigate, BrowserRouter, Routes, Route, Navigate, useParams } from 'react-router-dom';

// Import your components
import LogInPage from './components/auth/LogIn';
import SignUpPage from './components/auth/SignUp';

import Dashboard from './components/dashboard/Dashboard';
import Workers from './components/workers/Workers';
import WorkshopDetails from './components/workshopDetails/WorkshopDetails';
import WorkshopDetailsInitial from './components/workshopDetails/WorkshopDetailsInitial';
import JobTypes from './components/orders/JobTypes';
import EditJobType from './components/orders/EditJobType.jsx';
import MakesAndModels from './components/cars/MakesAndModels.jsx';
import Cars from './components/cars/Cars.jsx';
import Clients from './components/clients/Clients.jsx';
import EditClient from './components/clients/EditClient.jsx';
import PartsStock from './components/parts/PartsStock.jsx';
import OrdersPage from './components/orders/OrdersPage.jsx';
import EditJobPage from './components/orders/EditJobPage.jsx';
import ToDoPage from './components/todo/ToDoPage.jsx';
import ActivityLog from './components/activityLog/ActivityLog.jsx';

import AdminDashboard from './components/admin/AdminDashboard';
import AdminUsers from './components/admin/AdminUsers';
import AdminWorkshops from './components/admin/AdminWorkshops';
import AdminMakesModels from './components/admin/AdminMakesModels';

import Header from './components/common/Header.jsx';
import Sidebar from './components/common/Sidebar.jsx';
import PopupPortal from './components/common/PopupPortal.jsx'; 
import ErrorPage from './components/common/ErrorPage.jsx';
import ErrorBoundary from './components/common/ErrorBoundary.jsx';
import GlobalErrorWatcher from './components/common/GlobalErrorWatcher.jsx';

import { authApi } from './services/authApi';
import { useAuth } from './context/AuthContext';

const PrivateRoute = ({ children, access }) => {
  const { accesses } = useAuth();

  if (!localStorage.getItem('LoggedIn')) {
    return <Navigate to="/login" />;
  }

  const hasWorkshop = localStorage.getItem('HasWorkshop');
  if (hasWorkshop === 'false' && window.location.pathname !== '/workshop-details-initial') {
    return <Navigate to="/workshop-details-initial" />;
  }

  if (access) {
    if (!accesses.includes(access)) {
      return <ErrorPage title="Access Denied" message="You do not have permission to view this page." />;
    }
  }

  return children;
};

function App() {
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const { accesses, loading } = useAuth();
  const [hasWorkshop, setHasWorkshop] = useState(localStorage.getItem('HasWorkshop') !== 'false');

  useEffect(() => {
    setHasWorkshop(localStorage.getItem('HasWorkshop') !== 'false');
  }, [loading]);

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

  if (loading) {
    return null;
  }

  const routes = [
    { path: '/', element: <Dashboard />, children: [] },
    {
      path: '/todo', element: <ToDoPage />, access: 'To Do', children: [
        { path: '/:jobId', element: <EditJobPage mechanicView={true} /> }
      ]
    },
    {
      path: '/orders', element: <OrdersPage mode="active" />, access: 'Orders', children: [
        { path: '/:orderId/jobs/new', element: <EditJobPage /> },
        { path: '/:orderId/jobs/:jobId', element: <EditJobPage /> },
        { path: '/:orderId', element: <OrdersPage mode="active" /> } // Route for opening order details popup
      ]
    },
    { path: '/done-orders', element: <OrdersPage mode="completed" />, access: 'Orders', children: [
        { path: '/:orderId', element: <OrdersPage mode="completed" /> } // Route for opening done order details popup
    ] },
    { path: '/parts', element: <PartsStock />, children: [], access: 'Parts Stock' },
    { path: '/workers', element: <Workers />, children: [
        { path: '/new', element: <Workers /> },
        { path: '/:workerId', element: <Workers /> } // Edit Worker popup -> Outline Target
    ], access: 'Workers' },
    {
      path: '/job-types', element: <JobTypes />, access: 'Job Types', children: [
        { path: '/new', element: <EditJobType /> },
        { path: '/:id', element: <EditJobType /> }
      ]
    },
    {
      path: '/clients', element: <Clients />, access: 'Clients', children: [
        { path: '/new', element: <Clients /> },
        { path: '/:clientId', element: <Clients /> } // Edit Client popup
      ]
    },
    { path: '/makes-and-models', element: <MakesAndModels />, children: [
        { path: '/:makeId', element: <MakesAndModels /> },
        { path: '/:makeId/model/:modelId', element: <MakesAndModels /> }
    ], access: 'Makes and Models' },
    { path: '/cars', element: <Cars />, children: [
        { path: '/new', element: <Cars /> },
        { path: '/:carId', element: <Cars /> } // Edit Car popup
    ], access: 'Cars' },
    { path: '/activity-log', element: <ActivityLog />, children: [], access: 'Activity Log' },
    { path: '/admin/dashboard', element: <AdminDashboard />, children: [], access: 'Admin' },
    { path: '/admin/makes-models', element: <AdminMakesModels />, children: [], access: 'Admin' },
    { path: '/admin/users', element: <AdminUsers />, children: [], access: 'Admin' },
    { path: '/admin/workshops', element: <AdminWorkshops />, children: [], access: 'Admin' },
  ];

  return (
    <div className="app-container">
      <BrowserRouter>
        <ErrorBoundary>
          <PopupPortal />
          <GlobalErrorWatcher>
            <Routes>
              <Route path="/login" element={<LogInPage />} />
            <Route path="/signup" element={<SignUpPage />} />

            <Route path="/access-denied" element={
              <PrivateRoute>
                 <ErrorPage title="Access Denied" message="You do not have permission to view this page." />
              </PrivateRoute>
            } />

            <Route path="/workshop-details-initial" element={
              <PrivateRoute>
                <WorkshopDetailsInitial />
              </PrivateRoute>
            } />
            <Route path="/workshop-details" element={
              <PrivateRoute>
                <LayoutWithHeader
                  sidebarOpen={sidebarOpen}
                  setSidebarOpen={setSidebarOpen}
                  accesses={accesses}
                >
                  <Dashboard /> {/* Render dashboard behind the workshop details popup */}
                </LayoutWithHeader>
              </PrivateRoute>
            } />


            {routes.map((route, i) => (
              <Fragment key={route.path}>
                <Route
                  path={route.path}
                  element={
                    <PrivateRoute access={route.access}>
                      <LayoutWithHeader
                        sidebarOpen={sidebarOpen}
                        setSidebarOpen={setSidebarOpen}
                        accesses={accesses}
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
                      <PrivateRoute access={route.access}>
                        <LayoutWithHeader
                          sidebarOpen={sidebarOpen}
                          setSidebarOpen={setSidebarOpen}
                          accesses={accesses}
                        >
                          {childRoute.element}
                        </LayoutWithHeader>
                      </PrivateRoute>
                    }
                  />
                ))
                }
              </Fragment>

            ))}

            {/* Catch-all route for 404 */}
            <Route path="*" element={<ErrorPage type="404" />} />

            </Routes>
          </GlobalErrorWatcher>
        </ErrorBoundary>
      </BrowserRouter>
    </div>
  );
}

export default App;

function LayoutWithHeader({ children, sidebarOpen, setSidebarOpen, accesses }) {
  return (
    <>
      {/* <PopupPortal /> */}
      <Header onToggleSidebar={() => setSidebarOpen(!sidebarOpen)} />
      <div className="horizontal work-area">
        <Sidebar open={sidebarOpen} onClose={() => setSidebarOpen(false)} accesses={accesses} />
        {children}
      </div>
    </>
  );
}
