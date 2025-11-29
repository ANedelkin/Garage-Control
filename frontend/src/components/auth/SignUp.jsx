import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import '../../assets/css/common.css';
import '../../assets/css/auth.css';
import AuthTemplate from './AuthTemplate.jsx';
import { authApi } from '../../services/authApi.js';

const SignUpPage = () => {
    const navigate = useNavigate();
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');

    const handleSubmit = async (e, formData) => {
        e.preventDefault();
        setLoading(true);
        setError('');

        try {
            await authApi.register(formData.email, formData.password);
            navigate('/');
        } catch (err) {
            setError(err.message);
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
            title="Create Account"
            handlers={{ handleSubmit, handleGoogle, handleMicrosoft }}
        >
            {error && <p className="error-message" style={{ color: 'red', marginBottom: '10px' }}>{error}</p>}
            <p className="lnk">Already have an account? <a href="/">Log In!</a></p>
            <button type="submit" className="btn" disabled={loading}>
                {loading ? 'Signing Up...' : 'Sign Up'}
            </button>
        </AuthTemplate>
    );
};

export default SignUpPage;
