import React, { useState, useEffect } from 'react';
import '../../assets/css/common.css';
import '../../assets/css/auth.css';
import AuthTemplate from './AuthTemplate.jsx';

const LogInPage = () => {
    const handleSubmit = (e) => {
        e.preventDefault();
        // TODO: Login logic
    };

    const handleGoogle = () => {
        // TODO: Add Google sign‑in logic
    };

    const handleMicrosoft = () => {
        // TODO: Add Microsoft sign‑in logic
    };

    return (
        <AuthTemplate handlers={{ handleSubmit, handleGoogle, handleMicrosoft }}>
            <p className="lnk">No account yet? <a href="/signup">Create one!</a></p>
            <button type="submit" className="btn">Log In</button>
        </AuthTemplate>
    );
};

export default LogInPage;
