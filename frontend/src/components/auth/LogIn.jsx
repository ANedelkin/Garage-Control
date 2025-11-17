import React, { useState, useEffect } from 'react';

import '../../assets/css/common.css';
import '../../assets/css/login.css';

const LogInPage = () => {
    const [theme, setTheme] = useState(localStorage.getItem('theme') || 'light');
    const [formData, setFormData] = useState({ email: '', password: '' });

    useEffect(() => {
        document.body.classList.remove('light', 'dark');
        document.body.classList.add(theme);
        localStorage.setItem('theme', theme);
    }, [theme]);

    const toggleTheme = () => {
        setTheme(theme === 'light' ? 'dark' : 'light');
    };

    const handleSubmit = (e) => {
        e.preventDefault();
        //TODO: Login logic
    };

    return (
        <>
            <div className="btn theme-toggle" onClick={toggleTheme}>
                <i className={`fa-solid ${theme === 'light' ? 'fa-moon' : 'fa-sun'}`}></i>
                <span>{theme === 'light' ? 'Dark mode' : 'Light mode'}</span>
            </div>

            <div className="container">
                <div className="tile">
                    <h3 className="tile-header">Welcome Back</h3>

                    <form onSubmit={handleSubmit}>
                        <div className="form-section">
                            <label>Email</label>
                            <input
                                type="email"
                                placeholder="Enter email"
                                value={formData.email}
                                onChange={(e) =>
                                    setFormData({ ...formData, email: e.target.value })
                                }
                                required
                            />
                        </div>

                        <div className="form-section">
                            <label>Password</label>
                            <input
                                type="password"
                                placeholder="Enter password"
                                value={formData.password}
                                onChange={(e) =>
                                    setFormData({ ...formData, password: e.target.value })
                                }
                                required
                            />
                        </div>
                            <button type="submit" className="btn">
                                Log in
                            </button>
                    </form>
                </div>
            </div>
        </>
    );
};

export default LogInPage;
