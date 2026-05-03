import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { authApi } from '../../services/authApi.js';
import ThemeToggle from '../common/ThemeToggle.jsx';
import FieldError from '../common/FieldError.jsx';
import { parseValidationErrors } from '../../Utilities/formErrors.js';
import GoogleIcon from '../../assets/icons/google.svg';
import MicrosoftIcon from '../../assets/icons/microsoft.svg';
import '../../assets/css/auth.css';
import usePageTitle from '../../hooks/usePageTitle';

const SignUpPage = () => {
    usePageTitle('Sign Up');
    const navigate = useNavigate();
    const { login } = useAuth();
    const [loading, setLoading] = useState(false);
    const [errors, setErrors] = useState({});
    const [formData, setFormData] = useState({ username: '', password: '' });

    const handleSubmit = async (e, formData) => {
        e.preventDefault();
        setLoading(true);
        setErrors({});

        try {
            const response = await authApi.register(formData.username, formData.password);
            if (response) {
                login(response);
            }
            navigate('/');
        } catch (err) {
            setErrors(parseValidationErrors(err));
        } finally {
            setLoading(false);
        }
    };


    const handleGoogle = async (e) => {
        e.preventDefault();
        window.location.href = '/api/auth/google';
    };

    const handleMicrosoft = async (e) => {
        e.preventDefault();
        window.location.href = '/api/auth/microsoft';
    };

    return (
        <main className="background">
            <ThemeToggle className="theme-toggle" />
            <div className="container auth">
                <div className="tile">
                    <h3 className="tile-header">Create Account</h3>
                    <form onSubmit={(e) => handleSubmit(e, formData)}>
                        <div className="form-section">
                            <label>Username</label>
                            <input
                                type="text"
                                name="Username"
                                placeholder="Enter username"
                                value={formData.username}
                                onChange={(e) => setFormData({ ...formData, username: e.target.value })}
                                required
                            />
                            <FieldError name="Username" errors={errors} />
                        </div>
                        <div className="form-section">
                            <label>Password</label>
                            <input
                                type="password"
                                name="Password"
                                placeholder="Enter password"
                                value={formData.password}
                                onChange={(e) => setFormData({ ...formData, password: e.target.value })}
                                required
                            />
                            <FieldError name="Password" errors={errors} />
                        </div>
                        {errors.general && <p className="form-error">{errors.general}</p>}

                        <div className="form-footer">
                            <p className="lnk">Already have an account? <Link to="/">Log In!</Link></p>
                            <button type="submit" className="btn" disabled={loading}>
                                {loading ? 'Signing Up...' : 'Sign Up'}
                            </button>
                        </div>

                        <div className="divider">
                            <span>OR</span>
                        </div>

                        <div className="auth-buttons">
                            <button type="button" className="btn" onClick={handleGoogle}>
                                <img src={GoogleIcon} alt="Google" />
                                Continue with Google
                            </button>
                            <button type="button" className="btn" onClick={handleMicrosoft}>
                                <img src={MicrosoftIcon} alt="Microsoft" />
                                Continue with Microsoft
                            </button>
                        </div>
                    </form>
                </div>
            </div>
        </main>
    );
};

export default SignUpPage;
