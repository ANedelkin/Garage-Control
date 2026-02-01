import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import AuthTemplate from './AuthTemplate.jsx';
import { useAuth } from '../../context/AuthContext';
import { authApi } from '../../services/authApi.js';

const LogInPage = () => {
    const navigate = useNavigate();
    const { login } = useAuth();
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');

    const handleSubmit = async (e, formData) => {
        e.preventDefault();
        setLoading(true);
        setError('');

        try {
            const data = await authApi.login(formData.email, formData.password);

            // Update AuthContext
            login(data);

            if (data.accesses && data.accesses.includes('Admin Dashboard')) {
                navigate('/admin/dashboard');
            } else {
                navigate('/');
            }
        } catch (err) {
            setError(err.message || 'An error occurred during login');
        } finally {
            setLoading(false);
        }
    };

    const handleMicrosoft = (e) => {
        e.preventDefault();
        // TODO: Add Microsoft sign-in logic
        console.log('Microsoft sign-in not yet implemented');
    };

    return (
        <AuthTemplate
            title="Welcome Back"
            handlers={{ handleSubmit, handleMicrosoft }}
        >
            {error && <p className="error-message" style={{ color: 'red', marginBottom: '10px' }}>{error}</p>}
            <p className="lnk">No account yet? <Link to="/signup">Create one!</Link></p>
            <button type="submit" className="btn" disabled={loading}>
                {loading ? 'Logging In...' : 'Log In'}
            </button>
        </AuthTemplate>
    );
};

export default LogInPage;
