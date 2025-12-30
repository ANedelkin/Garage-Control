import React, { useState, useEffect } from 'react';
import '../../assets/css/common/base.css';
import '../../assets/css/common/layout.css';
import '../../assets/css/common/controls.css';
import '../../assets/css/common/form.css';
import '../../assets/css/common/tile.css';
import '../../assets/css/auth.css';
import ThemeToggle from '../common/ThemeToggle.jsx';
import GoogleIcon from '../../assets/icons/google.svg';
import MicrosoftIcon from '../../assets/icons/microsoft.svg';

const AuthTemplate = ({ title = "Welcome Back", handlers, children }) => {
    const [formData, setFormData] = useState({ email: '', password: '' });

    const handleFormSubmit = (e) => {
        e.preventDefault();
        if (handlers?.handleSubmit) {
            handlers.handleSubmit(e, formData);
        }
    };
    const handleGoogle = async (e) => {
        e.preventDefault();
        window.location.href = 'https://localhost:5173/api/auth/google';
    };

    return (
        <main className="background">
            <ThemeToggle className="theme-toggle" />
            <div className="container auth">
                <div className="tile">
                    <h3 className="tile-header">{title}</h3>
                    <form onSubmit={handleFormSubmit}>
                        <div className="form-section">
                            <label>Email</label>
                            <input
                                type="email"
                                placeholder="Enter email"
                                value={formData.email}
                                onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                                required
                            />
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
                        </div>
                        <div className="form-footer">
                            {children}
                        </div>

                        <div className="divider">
                            <span>OR</span>
                        </div>

                        <div className="auth-buttons">
                            <button type="button" className="btn" onClick={handleGoogle}>
                                <img src={GoogleIcon} alt="Google" />
                                Continue with Google
                            </button>
                            <button type="button" className="btn" onClick={handlers?.handleMicrosoft}>
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

export default AuthTemplate;
