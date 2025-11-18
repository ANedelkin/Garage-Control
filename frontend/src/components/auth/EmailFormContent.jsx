import React, { useState, useEffect } from 'react';

import '../../assets/css/common.css';
import '../../assets/css/auth.css';

const EmailFormContent = () => {
    const [formData, setFormData] = useState({ email: '', password: '' });

    return (
        <>
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
        </>
    )
}

export default EmailFormContent;