import React, { useState, useEffect } from 'react';

import WorkshopDetailsForm from './WorkshopDetailsForm.jsx';
import { workshopApi } from '../../services/workshopApi.js';

const WorkshopDetails = ({ onClose }) => {
    const [workshopDetails, setWorkshopDetails] = useState(null);

    const handleSubmit = async (e, formData) => {
        e.preventDefault();
        await workshopApi.edit(formData);
        if (onClose) onClose();
    };

    useEffect(() => {
        workshopApi.getDetails().then((res) => {
            console.log('Fetched workshop details:', res);
            setWorkshopDetails(res);
        }).catch((err) => {
            console.error('Error fetching workshop details:', err);
        });
    }, []);

    return (
        <WorkshopDetailsForm handleSubmit={handleSubmit} initialData={workshopDetails} />
    );
};

export default WorkshopDetails;
