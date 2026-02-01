import React from 'react';
import { Link } from 'react-router-dom';

const AccessDenied = () => {
    return (
        <div className="main">
            <div className="tile" style={{ textAlign: 'center', marginTop: '50px' }}>
                <h1>Access Denied</h1>
                <p>You do not have permission to view this page.</p>
                <Link to="/" className="btn">Go to Dashboard</Link>
            </div>
        </div>
    );
};

export default AccessDenied;
