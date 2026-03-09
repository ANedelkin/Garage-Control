import React, { useState, useEffect } from 'react';

import WorkshopDetailsForm from './WorkshopDetailsForm.jsx';
import { workshopApi } from '../../services/workshopApi.js';
import { parseValidationErrors } from '../../Utilities/formErrors.js';

const WorkshopDetails = ({ onClose }) => {
    const [workshopDetails, setWorkshopDetails] = useState(null);
    const [errors, setErrors] = useState({});

    const handleSubmit = async (e, formData) => {
        e.preventDefault();
        try {
            await workshopApi.edit(formData);
            if (onClose) onClose();
        } catch (error) {
            console.error('Error editing workshop:', error);
            setErrors(parseValidationErrors(error));
        }
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
        <WorkshopDetailsForm handleSubmit={handleSubmit} initialData={workshopDetails} errors={errors} />
    );
};

export default WorkshopDetails;
