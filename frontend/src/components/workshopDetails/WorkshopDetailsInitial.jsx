import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';

import ThemeToggle from '../common/ThemeToggle.jsx';
import WorkshopDetailsForm from './WorkshopDetailsForm.jsx';
import { workshopApi } from '../../services/workshopApi.js';

const WorkshopDetailsInitial = () => {
    const navigate = useNavigate();
    const handleSubmit = (e, formData) => {
        e.preventDefault();
        workshopApi.create(formData);
        navigate('/');
    };

    return (
        <div className="work-area">
            <main className="main workshop-details">
                <ThemeToggle className="theme-toggle" />
                <WorkshopDetailsForm handleSubmit={handleSubmit} />
            </main>
        </div>
    );
};

export default WorkshopDetailsInitial;
