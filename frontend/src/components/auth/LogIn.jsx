import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { authApi } from '../../services/authApi.js';
import GoogleIcon from '../../assets/icons/google.svg';
import MicrosoftIcon from '../../assets/icons/microsoft.svg';
import ThemeToggle from '../common/ThemeToggle.jsx';
import '../../assets/css/common/base.css';
import '../../assets/css/common/layout.css';
import '../../assets/css/common/controls.css';
import '../../assets/css/common/form.css';
import '../../assets/css/common/tile.css';
import '../../assets/css/auth.css';

const LogInPage = () => {
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
            const data = await authApi.login(formData.username, formData.password);

            login(data);

            if (data.accesses && data.accesses.includes('To Do')) {
                navigate('/todo');
            } else if (data.accesses && data.accesses.includes('Admin')) {
                navigate('/admin/dashboard');
            } else {
                navigate('/');
            }
        } catch (err) {
            console.error('Login error detail:', err, err.data);
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
                if (msg.includes('invalid credentials')) {
                    newErrors.general = errorMessage;
                } else if (msg.includes('blocked')) {
                    newErrors.general = errorMessage;
                } else if (msg.includes('username')) {
                    newErrors.username = errorMessage;
                } else if (msg.includes('password')) {
                    newErrors.password = errorMessage;
                } else {
                    newErrors.general = errorMessage;
                }
            } else {
                newErrors.general = err.message || 'An error occurred during login';
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
    }

    return (
        <main className="background">
            <ThemeToggle className="theme-toggle" />
            <div className="container auth">
                <div className="tile">
                    <h3 className="tile-header">Welcome Back</h3>
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
                            <p className="lnk">No account yet? <Link to="/signup">Create one!</Link></p>
                            <button type="submit" className="btn" disabled={loading}>
                                {loading ? 'Logging In...' : 'Log In'}
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
        // <AuthTemplate
        //     title="Welcome Back"
        //     handlers={{ handleSubmit }}
        //     errors={errors}
        // >
        //     <p className="lnk">No account yet? <Link to="/signup">Create one!</Link></p>
        //     <button type="submit" className="btn" disabled={loading}>
        //         {loading ? 'Logging In...' : 'Log In'}
        //     </button>
        // </AuthTemplate>
    );
};

export default LogInPage;
