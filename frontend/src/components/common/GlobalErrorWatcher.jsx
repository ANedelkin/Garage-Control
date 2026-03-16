import React, { useState, useEffect } from 'react';
import { useLocation } from 'react-router-dom';
import ErrorPage from './ErrorPage';

const GlobalErrorWatcher = ({ children }) => {
    const [apiError, setApiError] = useState(null);
    const location = useLocation();

    // Clear error state on route change
    useEffect(() => {
        setApiError(null);
    }, [location.pathname]);

    useEffect(() => {
        const handle404 = (e) => setApiError({
            title: `${e.detail.resource} Not Found`,
            message: `The requested ${e.detail.resource.toLowerCase()} could not be found or has been deleted.`
        });
        window.addEventListener('api-404', handle404);
        return () => window.removeEventListener('api-404', handle404);
    }, []);

    if (apiError) {
        return <ErrorPage title={apiError.title} message={apiError.message} />;
    }

    return <>{children}</>;
};

export default GlobalErrorWatcher;
