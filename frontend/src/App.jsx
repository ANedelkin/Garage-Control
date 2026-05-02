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
import PartsStock from './components/parts/PartsStock.jsx';
import ActiveOrdersPage from './components/orders/ActiveOrdersPage.jsx';
import ArchivedOrdersPage from './components/orders/ArchivedOrdersPage.jsx';
import EditJobPage from './components/orders/EditJobPage.jsx';
import DoneJobPage from './components/orders/DoneJobPage.jsx';
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

const routes = [
  { path: '/', element: Dashboard, label: 'Home', icon: 'fa-house', accesses: ['Dashboard'] },
  {
    path: '/todo',
    element: ToDoPage,
    label: 'To Do',
    icon: 'fa-clipboard-list',
    accesses: ['To Do'],
    children: [
      { path: '/:jobId', element: EditJobPage, props: { mechanicView: true } }
    ]
  },
  {
    path: '/orders',
    element: ActiveOrdersPage,
    label: 'Orders',
    icon: 'fa-screwdriver-wrench',
    accesses: ['Orders'],
    children: [
      { path: '/:orderId', element: ActiveOrdersPage }
    ]
  },
  {
    path: '/jobs',
    accesses: ['Orders', 'To Do'],
    children: [
      { path: '/new', element: EditJobPage },
      { path: '/:jobId', element: EditJobPage }
    ]
  },
  {
    path: '/done-jobs',
    accesses: ['Orders'],
    children: [
      { path: '/:jobId', element: DoneJobPage }
    ]
  },
  { path: '/parts', element: PartsStock, label: 'Parts Stock', icon: 'fa-boxes-stacked', accesses: ['Parts Stock'] },
  {
    path: '/workers',
    element: Workers,
    label: 'Workers',
    icon: 'fa-users-gear',
    accesses: ['Workers'],
    children: [
      { path: '/new', element: Workers },
      { path: '/:workerId', element: Workers },
      {
        path: '/:workerId/todo',
        element: ToDoPage,
        children: [
          { path: '/:jobId', element: EditJobPage, props: { mechanicView: true } }
        ]
      }
    ]
  },
  {
    path: '/clients',
    element: Clients,
    label: 'Clients',
    icon: 'fa-user',
    accesses: ['Clients'],
    children: [
      { path: '/new', element: Clients },
      { path: '/:clientId', element: Clients }
    ]
  },
  { path: '/cars', element: Cars, label: 'Cars', icon: 'fa-car', accesses: ['Cars'], children: [
    { path: '/new', element: Cars },
    { path: '/:carId', element: Cars }
  ]},
  { path: '/activity-log', element: ActivityLog, label: 'Activity Log', icon: 'fa-clock-rotate-left', accesses: ['Activity Log'] },
  { divider: true, accesses: ['Archived Orders', 'Job Types', 'Makes and Models', 'Workshop Details'] },
  { path: '/archived-orders', element: ArchivedOrdersPage, label: 'Archived Orders', icon: 'fa-clipboard-check', accesses: ['Orders'], children: [
    { path: '/:orderId', element: ArchivedOrdersPage }
  ]},
  { path: '/job-types', element: JobTypes, label: 'Job Types', icon: 'fa-gear', accesses: ['Job Types'], children: [
    { path: '/new', element: EditJobType },
    { path: '/:id', element: EditJobType }
  ]},
  { path: '/makes-and-models', element: MakesAndModels, label: 'Makes & models', icon: 'fa-industry', accesses: ['Makes and Models'], children: [
    { path: '/:makeId', element: MakesAndModels },
    { path: '/:makeId/model/:modelId', element: MakesAndModels }
  ]},
  {
    label: 'Workshop Details',
    icon: 'fa-circle-info',
    accesses: ['Workshop Details'],
    popup: true,
    popupComponent: WorkshopDetails
  },
  { path: '/admin/dashboard', element: AdminDashboard, label: 'Dashboard', icon: 'fa-gauge', accesses: ['Admin'] },
  { path: '/admin/makes-models', element: AdminMakesModels, label: 'Makes & Models', icon: 'fa-industry', accesses: ['Admin'] },
  { path: '/admin/users', element: AdminUsers, label: 'Users', icon: 'fa-users', accesses: ['Admin'] },
  { path: '/admin/workshops', element: AdminWorkshops, label: 'Workshops', icon: 'fa-shop', accesses: ['Admin'] },
];

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

  const renderRoutes = (routeList, parentPath = '') => {
    return routeList.map((route, i) => {
      if (!route.path) return null;
      const fullPath = (parentPath + route.path).replace('//', '/');
      const Element = route.element;
      const routeProps = route.props || {};

      return (
        <Fragment key={fullPath}>
          {Element && (
            <Route
              path={fullPath}
              element={
                <PrivateRoute access={route.access}>
                  <LayoutWithHeader
                    sidebarOpen={sidebarOpen}
                    setSidebarOpen={setSidebarOpen}
                    accesses={accesses}
                    routes={routes}
                  >
                    <Element {...routeProps} />
                  </LayoutWithHeader>
                </PrivateRoute>
              }
            />
          )}
          {route.children && renderRoutes(route.children, fullPath)}
        </Fragment>
      );
    });
  };

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
                    routes={routes}
                  >
                    <Dashboard />
                  </LayoutWithHeader>
                </PrivateRoute>
              } />

              {renderRoutes(routes)}

              <Route path="*" element={<ErrorPage type="404" />} />
            </Routes>
          </GlobalErrorWatcher>
        </ErrorBoundary>
      </BrowserRouter>
    </div>
  );
}

export default App;

function LayoutWithHeader({ children, sidebarOpen, setSidebarOpen, accesses, routes }) {
  return (
    <>
      <Header onToggleSidebar={() => setSidebarOpen(!sidebarOpen)} />
      <div className="horizontal work-area">
        <Sidebar open={sidebarOpen} onClose={() => setSidebarOpen(false)} accesses={accesses} routes={routes} />
        {children}
      </div>
    </>
  );
}

