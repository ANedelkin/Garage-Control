import { useState } from 'react'
import { BrowserRouter, Routes, Route } from 'react-router-dom'

import LogInPage from './components/auth/LogIn'
import SignUpPage from './components/auth/SignUp'

import ServiceDetailsInitial from './components/ServiceDetails/ServiceDetailsInitial'
import ServiceDetails from './components/ServiceDetails/ServiceDetails'

import Header from './components/common/Header.jsx'
import Sidebar from './components/common/Sidebar.jsx'

function App() {
  const [sidebarOpen, setSidebarOpen] = useState(false);

  return (
    <>
      <BrowserRouter>
        <Routes>
          <Route
            path="/"
            element={
              <LogInPage />
            }
          />
          <Route
            path="/signup"
            element={
              <SignUpPage/>
            }
          />
          <Route
            path="/service-details-initial"
            element={
              <ServiceDetailsInitial/>
            }
          />
          <Route
            path="/service-details"
            element={<LayoutWithHeader
                        selection={5}
                        sidebarOpen={sidebarOpen}
                        onToggleSidebar={() => setSidebarOpen(!sidebarOpen)}
                        onCloseSidebar={() => setSidebarOpen(false)}
                      >
                        <ServiceDetails/>
                      </LayoutWithHeader>}>
          </Route>
        </Routes>
      </BrowserRouter>
    </>
  )
}

export default App

function LayoutWithHeader({ selection, children, sidebarOpen, onToggleSidebar, onCloseSidebar }) {
  return (
    <>
      <Header onToggleSidebar={onToggleSidebar} />
      <div className="horizontal-layout">
        <Sidebar selection={selection} open={sidebarOpen} onClose={onCloseSidebar} />
        {children}
      </div>
    </>
  );
}