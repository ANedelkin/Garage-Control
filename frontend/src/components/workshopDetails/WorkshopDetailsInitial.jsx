import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';

import ThemeToggle from '../common/ThemeToggle.jsx';
import WorkshopDetailsForm from './WorkshopDetailsForm.jsx';
import { workshopApi } from '../../services/workshopApi.js';

const WorkshopDetailsInitial = ({ onClose }) => {
    const navigate = useNavigate();
    const handleSubmit = async (e, formData) => {
        e.preventDefault();
        await workshopApi.create(formData);
        localStorage.setItem('HasWorkshop', 'true');
        if (onClose) onClose();
        navigate('/');
    };

    return (
        <WorkshopDetailsForm handleSubmit={handleSubmit} />
    );
};

export default WorkshopDetailsInitial;
