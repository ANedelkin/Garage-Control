import React, { useState, useEffect } from 'react';
import '../../assets/css/common.css';
import '../../assets/css/service-details.css';
import { carServiceApi } from '../../services/carServiceApi.js';

const ServiceDetailsForm = ({ handleSubmit, initialData = null }) => {
    const [serviceData, setServiceData] = useState(initialData);

    // Update state when initialData changes (after API call completes)
    useEffect(() => {
        console.log('initialData changed:', initialData);
        if (initialData) {
            setServiceData(initialData);
        }
    }, [initialData]);

    const handleFormSubmit = (e) => {
        handleSubmit(e, serviceData);
    }

    return (
        <div className="tile service-details">
            <h3 className="tile-header">Service Information</h3>
            <form onSubmit={handleFormSubmit}>
                <div className="form-section">
                    <label htmlFor="name">Service Name</label>
                    <input
                        type="text"
                        id="name"
                        placeholder="Enter service name"
                        value={serviceData ? serviceData.name : ''}
                        onChange={(e) => setServiceData({ ...serviceData, name: e.target.value })}
                        required
                    />
                </div>
                <div className="form-section">
                    <label htmlFor="address">Address</label>
                    <input
                        type="text"
                        id="address"
                        placeholder="Enter address"
                        value={serviceData ? serviceData.address : ''}
                        onChange={(e) => setServiceData({ ...serviceData, address: e.target.value })}
                        required
                    />
                </div>
                <div className="form-section">
                    <label htmlFor="registrationNumber">Registration Number</label>
                    <input
                        type="text"
                        id="registrationNumber"
                        placeholder="Enter registration number"
                        value={serviceData ? serviceData.registrationNumber : ''}
                        onChange={(e) => setServiceData({ ...serviceData, registrationNumber: e.target.value })}
                        required
                    />
                </div>
                <div className="form-footer">
                    <button type="submit" className="btn">Done</button>
                </div>
            </form>
        </div>
    );
};

export default ServiceDetailsForm;
