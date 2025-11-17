import { useState } from 'react'
import { BrowserRouter, Routes, Route } from 'react-router-dom'

import LogInPage from './components/auth/LogIn'

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
        </Routes>
      </BrowserRouter>
    </>
  )
}

export default App
