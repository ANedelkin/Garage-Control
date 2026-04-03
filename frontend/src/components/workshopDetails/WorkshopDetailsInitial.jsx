import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import Header from '../common/Header.jsx';
import WorkshopDetailsForm from './WorkshopDetailsForm.jsx';
import { workshopApi } from '../../services/workshopApi.js';
import { parseValidationErrors } from '../../Utilities/formErrors.js';
import React, { useState } from 'react';
import usePageTitle from '../../hooks/usePageTitle.js';

const WorkshopDetailsInitial = () => {
    usePageTitle('Setup Workshop');
    const navigate = useNavigate();
    const { login } = useAuth();
    const [errors, setErrors] = useState({});

    const handleSubmit = async (e, formData) => {
        e.preventDefault();
        try {
            const response = await workshopApi.create(formData);

            // Update auth context with new accesses and user info from response
            if (response) {
                login(response);
            }

            localStorage.setItem('HasWorkshop', 'true');
            navigate('/');
        } catch (error) {
            console.error('Error creating workshop:', error);
            setErrors(parseValidationErrors(error));
        }
    };

    return (
        <div className="vertical" style={{ height: '100vh' }}>
            <Header />
            <main className="main" style={{ display: 'flex', alignItems: 'center' }}>
                <div className="tile" style={{ width: 'fit-content', marginTop: '75px' }}>
                    <h3 className="tile-header">Workshop Information</h3>
                    <WorkshopDetailsForm handleSubmit={handleSubmit} errors={errors} />
                </div>
            </main>
        </div>
    );
};

export default WorkshopDetailsInitial;
