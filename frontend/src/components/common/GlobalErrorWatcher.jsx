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
        const handleNetworkError = () => setApiError({
            title: 'Connection Error',
            message: 'Unable to connect to the server. Please check your internet connection and try again.'
        });
        const handleServerError = () => setApiError({
            title: 'Server Error',
            message: 'The server encountered an unexpected condition. Please try again later.'
        });
        const handle403 = () => setApiError({
            title: 'Access Denied',
            message: 'You do not have permission to perform this action or view this resource.'
        });

        window.addEventListener('api-404', handle404);
        window.addEventListener('api-network-error', handleNetworkError);
        window.addEventListener('api-server-error', handleServerError);
        window.addEventListener('api-403', handle403);

        return () => {
            window.removeEventListener('api-404', handle404);
            window.removeEventListener('api-network-error', handleNetworkError);
            window.removeEventListener('api-server-error', handleServerError);
            window.removeEventListener('api-403', handle403);
        };
    }, []);

    if (apiError) {
        return <ErrorPage title={apiError.title} message={apiError.message} />;
    }

    return <>{children}</>;
};

export default GlobalErrorWatcher;
