import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import '../../assets/css/common/error.css';
import usePageTitle from '../../hooks/usePageTitle.js';

const ErrorPage = ({ title, message, type = 'error', details }) => {
    const is404 = type === '404';
    usePageTitle(title || (is404 ? "Page Not Found" : "Error"));
    const navigate = useNavigate();

    useEffect(() => {
        const theme = localStorage.getItem('theme') || 'light';
        document.body.classList.remove('light', 'dark');
        document.body.classList.add(theme);
    }, []);

    return (
        <main className="background centered" style={{ height: '100vh', width: '100vw' }}>
            <div className="error-card tile">
                <div className="error-icon">
                    <i className={is404 ? "fa-solid fa-map-marked-alt" : "fa-solid fa-circle-exclamation"}></i>
                </div>
                <h1>{title || (is404 ? "Page Not Found" : "Something Went Wrong")}</h1>
                <p>
                    {message || (is404
                        ? "Oops! The page you're looking for doesn't exist or has been moved."
                        : "We've encountered an unexpected error. Don't worry, our team has been notified.")}
                </p>

                {details && (
                    <div className="error-details">
                        {details}
                    </div>
                )}

                <div className="error-actions">
                    <button
                        className="btn secondary"
                        onClick={() => navigate(-1)}
                    >
                        <i className="fa-solid fa-arrow-left"></i> Go Back
                    </button>
                    <button
                        className="btn secondary"
                        onClick={() => window.location.reload()}
                    >
                        <i className="fa-solid fa-rotate"></i> Refresh
                    </button>
                </div>
            </div>
        </main>
    );
};

export default ErrorPage;
