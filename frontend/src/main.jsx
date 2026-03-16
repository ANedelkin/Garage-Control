import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import App from './App.jsx'
import { AuthProvider } from './context/AuthContext.jsx'
import { PopupProvider } from './context/PopupContext.jsx'

import './assets/css/common/colors.css';
import './assets/css/common/base.css';
import './assets/css/common/layout.css';
import './assets/css/common/controls.css';
import './assets/css/common/tile.css';

const applyTheme = () => {
  const theme = localStorage.getItem('theme') || 'light';
  document.body.classList.remove('light', 'dark');
  document.body.classList.add(theme);
};

// Apply theme on initial load
applyTheme();

// Re-apply theme when restoring from Back/Forward Cache (BFCache)
window.addEventListener('pageshow', (event) => {
  if (event.persisted) {
    applyTheme();
  }
});

createRoot(document.getElementById('root')).render(
  <StrictMode>
    <AuthProvider>
      <PopupProvider>
        <App />
      </PopupProvider>
    </AuthProvider>
  </StrictMode>,
)
