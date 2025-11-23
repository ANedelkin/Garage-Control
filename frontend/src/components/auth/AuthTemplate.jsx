import React, { useState, useEffect } from 'react';
import '../../assets/css/auth.css';
import '../../assets/css/common.css';
import ThemeToggle from '../common/ThemeToggle.jsx';
import GoogleIcon from '../../assets/icons/google.svg';
import MicrosoftIcon from '../../assets/icons/microsoft.svg';

const AuthTemplate = ({ handleSubmit, handleGoogle, handleMicrosoft, children }) => {
    const [formData, setFormData] = useState({ email: '', password: '' });

    return (
        <main className="background">
            <ThemeToggle />
            <div className="container">
                <div className="tile">
                    <h3 className="tile-header">Welcome Back</h3>
                    <form onSubmit={handleSubmit}>
                        <div className="form-section">
                            <label>Email</label>
                            <input
                                type="email"
                                placeholder="Enter email"
                                // value={formData.email}
                                // onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                                required
                            />
                        </div>
                        <div className="form-section">
                            <label>Password</label>
                            <input
                                type="password"
                                placeholder="Enter password"
                                // value={formData.password}
                                // onChange={(e) => setFormData({ ...formData, password: e.target.value })}
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
                            <button className="btn icon-btn" onClick={handleGoogle}>
                                <img src={GoogleIcon} alt="Google" />
                                Continue with Google
                            </button>
                            <button className="btn icon-btn" onClick={handleMicrosoft}>
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
