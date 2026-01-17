import React, { useState, useEffect } from 'react';
import '../../assets/css/service-details.css';
import { workshopApi } from '../../services/workshopApi.js';

const WorkshopDetailsForm = ({ handleSubmit, initialData = null }) => {
    const [workshopData, setWorkshopData] = useState(initialData);

    // Update state when initialData changes (after API call completes)
    useEffect(() => {
        console.log('initialData changed:', initialData);
        if (initialData) {
            setWorkshopData(initialData);
        }
    }, [initialData]);

    const handleFormSubmit = (e) => {
        handleSubmit(e, workshopData);
    }

    return (
        <div className="tile">
            <h3 className="tile-header">Workshop Information</h3>
            <form onSubmit={handleFormSubmit}>
                <div className="form-section">
                    <label htmlFor="name">Workshop Name</label>
                    <input
                        type="text"
                        id="name"
                        placeholder="Enter workshop name"
                        value={workshopData ? workshopData.name : ''}
                        onChange={(e) => setWorkshopData({ ...workshopData, name: e.target.value })}
                        required
                    />
                </div>
                <div className="form-section">
                    <label htmlFor="address">Address</label>
                    <input
                        type="text"
                        id="address"
                        placeholder="Enter address"
                        value={workshopData ? workshopData.address : ''}
                        onChange={(e) => setWorkshopData({ ...workshopData, address: e.target.value })}
                        required
                    />
                </div>
                <div className="form-section">
                    <label htmlFor="registrationNumber">Registration Number</label>
                    <input
                        type="text"
                        id="registrationNumber"
                        placeholder="Enter registration number"
                        value={workshopData ? workshopData.registrationNumber : ''}
                        onChange={(e) => setWorkshopData({ ...workshopData, registrationNumber: e.target.value })}
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

export default WorkshopDetailsForm;
