import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import '../../assets/css/common.css';
import '../../assets/css/auth.css';
import AuthTemplate from './AuthTemplate.jsx';
import { authApi } from '../../services/authApi.js';

const LogInPage = () => {
    const navigate = useNavigate();
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');

    const handleSubmit = async (e, formData) => {
        e.preventDefault();
        setLoading(true);
        setError('');

        try {
            const response = await authApi.login(formData.email, formData.password);

            if (response.Success) {
                navigate('/service-details-initial');
            } else {
                setError(response.Message || 'Login failed');
            }
        } catch (err) {
            setError(err.message || 'An error occurred during login');
        } finally {
            setLoading(false);
        }
    };

    const handleGoogle = (e) => {
        e.preventDefault();
        // TODO: Add Google sign-in logic
        console.log('Google sign-in not yet implemented');
    };

    const handleMicrosoft = (e) => {
        e.preventDefault();
        // TODO: Add Microsoft sign-in logic
        console.log('Microsoft sign-in not yet implemented');
    };

    return (
        <AuthTemplate
            title="Welcome Back"
            handlers={{ handleSubmit, handleGoogle, handleMicrosoft }}
        >
            {error && <p className="error-message" style={{ color: 'red', marginBottom: '10px' }}>{error}</p>}
            <p className="lnk">No account yet? <a href="/signup">Create one!</a></p>
            <button type="submit" className="btn" disabled={loading}>
                {loading ? 'Logging In...' : 'Log In'}
            </button>
        </AuthTemplate>
    );
};

export default LogInPage;
