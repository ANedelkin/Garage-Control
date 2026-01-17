import React, { useState, useEffect } from 'react';

import WorkshopDetailsForm from './WorkshopDetailsForm.jsx';
import { workshopApi } from '../../services/workshopApi.js';

const WorkshopDetails = () => {
    const [workshopDetails, setWorkshopDetails] = useState(null);

    const handleSubmit = (e, formData) => {
        e.preventDefault();
        workshopApi.edit(formData);
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
        <main className="main workshop-details">
            <WorkshopDetailsForm handleSubmit={handleSubmit} initialData={workshopDetails} />
        </main>
    );
};

export default WorkshopDetails;
