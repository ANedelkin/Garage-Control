import { useState, useEffect } from 'react';
import { useNavigate, BrowserRouter, Routes, Route, Navigate, useParams } from 'react-router-dom';

// Import your components
import LogInPage from './components/auth/LogIn';
import SignUpPage from './components/auth/SignUp';

import Dashboard from './components/dashboard/Dashboard';
import Workers from './components/workers/Workers';
import EditWorker from './components/workers/EditWorker';
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
import NewOrderSetup from './components/orders/NewOrderSetup.jsx';
import EditJobPage from './components/orders/EditJobPage.jsx';
import ToDoPage from './components/todo/ToDoPage.jsx';
import ActivityLog from './components/activityLog/ActivityLog.jsx';

import AdminDashboard from './components/admin/AdminDashboard';
import AdminUsers from './components/admin/AdminUsers';
import AdminWorkshops from './components/admin/AdminWorkshops';
import AdminMakesModels from './components/admin/AdminMakesModels';

import Header from './components/common/Header.jsx';
import Sidebar from './components/common/Sidebar.jsx';
import AccessDenied from './components/common/AccessDenied.jsx';

import { authApi } from './services/authApi';
import { useAuth } from './context/AuthContext';

const PrivateRoute = ({ children, access }) => {
  const { loggedIn, accesses } = useAuth();

  if (!loggedIn) {
    return <Navigate to="/login" />;
  }

  const hasWorkshop = localStorage.getItem('HasWorkshop');
  if (hasWorkshop === 'false' && window.location.pathname !== '/workshop-details-initial') {
    return <Navigate to="/workshop-details-initial" />;
  }

  if (access) {
    if (!accesses.includes(access)) {
      return <AccessDenied />;
    }
  }

  return children;
};

function App() {
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const { accesses, loading } = useAuth();

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
    { path: '/todo', element: <ToDoPage />, access: 'To Do', children: [] },
    {
      path: '/orders', element: <OrdersPage mode="active" />, access: 'Orders', children: [
        { path: '/new', element: <NewOrderSetup /> },
        { path: '/:orderId/jobs/new', element: <EditJobPage /> },
        { path: '/:orderId/jobs/:jobId', element: <EditJobPage /> }
      ]
    },
    { path: '/done-orders', element: <OrdersPage mode="completed" />, access: 'Orders', children: [] },
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
    { path: '/workshop-details', element: <WorkshopDetails />, children: [], access: 'Workshop Details' },
    { path: '/makes-and-models', element: <MakesAndModels />, children: [], access: 'Makes and Models' },
    { path: '/cars', element: <Cars />, children: [], access: 'Cars' },
    { path: '/activity-log', element: <ActivityLog />, children: [], access: 'Activity Log' },
    { path: '/admin/dashboard', element: <AdminDashboard />, children: [], access: 'Admin Dashboard' },
    { path: '/admin/makes-models', element: <AdminMakesModels />, children: [], access: 'Admin Makes and Models' },
    { path: '/admin/users', element: <AdminUsers />, children: [], access: 'Admin Users' },
    { path: '/admin/workshops', element: <AdminWorkshops />, children: [], access: 'Admin Workshops' },
  ];

  return (
    <>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<LogInPage />} />
          <Route path="/signup" element={<SignUpPage />} />

          <Route path="/access-denied" element={
            <PrivateRoute>
              <LayoutWithHeader
                sidebarOpen={sidebarOpen}
                setSidebarOpen={setSidebarOpen}
                accesses={accesses}
              >
                <AccessDenied />
              </LayoutWithHeader>
            </PrivateRoute>
          } />

          <Route path="/workshop-details-initial" element={<PrivateRoute>
            <Header onToggleSidebar={() => setSidebarOpen(!sidebarOpen)} />
            <WorkshopDetailsInitial />
          </PrivateRoute>} />

          {routes.map((route, i) => (
            <>
              <Route
                key={route.path}
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
            </>

          ))}

        </Routes>
      </BrowserRouter>
    </>
  );
}

export default App;

function LayoutWithHeader({ children, sidebarOpen, setSidebarOpen, accesses }) {
  return (
    <>
      <Header onToggleSidebar={() => setSidebarOpen(!sidebarOpen)} />
      <div className="horizontal work-area">
        <Sidebar open={sidebarOpen} onClose={() => setSidebarOpen(false)} accesses={accesses} />
        {children}
      </div>
    </>
  );
}
