import React, { useState, useEffect } from 'react';

import '../../assets/css/common.css';
import '../../assets/css/auth.css';
import ThemeToggle from '../common/ThemeToggle.jsx';
import EmailFormContent from './EmailFormContent.jsx';

const SignUpPage = () => {
    const handleSubmit = (e) => {
        e.preventDefault();
        //TODO: SignUp logic
    };

    return (
        <div className="background">
            <div className="container">
                <div className="tile">
                    <h3 className="tile-header">Create a profile</h3>
                    <ThemeToggle />
                    <form onSubmit={handleSubmit}>
                        <EmailFormContent/>
                        <div className="form-footer">
                            <p className="lnk">Already have an account? <a href="/">Log in!</a></p>
                            <button type="submit" className="btn">Sign Up</button>
                        </div>
                    </form>
                </div>
            </div>
        </div>
    );
};

export default SignUpPage;
