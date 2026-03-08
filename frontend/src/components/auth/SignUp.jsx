import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { authApi } from '../../services/authApi.js';
import ThemeToggle from '../common/ThemeToggle.jsx';
import GoogleIcon from '../../assets/icons/google.svg';
import MicrosoftIcon from '../../assets/icons/microsoft.svg';
import '../../assets/css/auth.css';

const SignUpPage = () => {
    const navigate = useNavigate();
    const [loading, setLoading] = useState(false);
    const [errors, setErrors] = useState({});
    const [formData, setFormData] = useState({ username: '', password: '' });

    const handleSubmit = async (e, formData) => {
        e.preventDefault();
        setLoading(true);
        setErrors({});

        try {
            await authApi.register(formData.username, formData.password);
            localStorage.setItem('HasWorkshop', 'false'); // Ensure popup shows up
            navigate('/');
        } catch (err) {
            console.error('Registration error detail:', err, err.data);
            const newErrors = {};
            const data = err.data;

            if (data?.errors && typeof data.errors === 'object') {
                // Map ASP.NET ModelState errors
                Object.keys(data.errors).forEach(key => {
                    const fieldErrors = data.errors[key];
                    const message = Array.isArray(fieldErrors) ? fieldErrors[0] : fieldErrors;
                    newErrors[key.toLowerCase()] = message;
                });
            } else if (data?.Message || data?.message) {
                // Map specific business logic errors
                const errorMessage = data.Message || data.message;
                const msg = errorMessage.toLowerCase();
                if (msg.includes('user already exists') || msg.includes('username')) {
                    newErrors.username = errorMessage;
                } else if (msg.includes('password')) {
                    newErrors.password = errorMessage;
                } else {
                    newErrors.general = errorMessage;
                }
            } else {
                newErrors.general = err.message || 'An error occurred during registration';
            }
            setErrors(newErrors);
        } finally {
            setLoading(false);
        }
    };


    const handleGoogle = async (e) => {
        e.preventDefault();
        window.location.href = 'https://localhost:5173/api/auth/google';
    };

    const handleMicrosoft = async (e) => {
        e.preventDefault();
        window.location.href = 'https://localhost:5173/api/auth/microsoft';
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
                                placeholder="Enter username"
                                value={formData.username}
                                onChange={(e) => setFormData({ ...formData, username: e.target.value })}
                                required
                            />
                            {errors.username && <p className="field-error">{errors.username}</p>}
                        </div>
                        <div className="form-section">
                            <label>Password</label>
                            <input
                                type="password"
                                placeholder="Enter password"
                                value={formData.password}
                                onChange={(e) => setFormData({ ...formData, password: e.target.value })}
                                required
                            />
                            {errors.password && <p className="field-error">{errors.password}</p>}
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
