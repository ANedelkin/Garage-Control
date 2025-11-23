import { useState } from 'react'
import { BrowserRouter, Routes, Route } from 'react-router-dom'

import LogInPage from './components/auth/LogIn'
import SignUpPage from './components/auth/SignUp'

import ServiceDetailsInitial from './components/ServiceDetails/ServiceDetailsInitial'

function App() {
  const [count, setCount] = useState(0)

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
        </Routes>
      </BrowserRouter>
    </>
  )
}

export default App
