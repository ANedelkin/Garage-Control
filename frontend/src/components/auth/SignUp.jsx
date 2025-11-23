import React, { useState, useEffect } from 'react';

import '../../assets/css/common.css';
import '../../assets/css/auth.css';
import AuthTemplate from './AuthTemplate.jsx';

const SignUpPage = () => {
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
            <p className="lnk">Already have an account? <a href="/">Log In!</a></p>
            <button type="submit" className="btn">Sign Up</button>
        </AuthTemplate>
    );
};

export default SignUpPage;
