import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import App from './App.jsx'
import { AuthProvider } from './context/AuthContext.jsx'
import { PopupProvider } from './context/PopupContext.jsx'

createRoot(document.getElementById('root')).render(
  <StrictMode>
    <AuthProvider>
      <PopupProvider>
        <App />
      </PopupProvider>
    </AuthProvider>
  </StrictMode>,
)
