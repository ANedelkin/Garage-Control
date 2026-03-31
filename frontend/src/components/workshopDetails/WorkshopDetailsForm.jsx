import React, { useState, useEffect } from 'react';
import FieldError from '../common/FieldError.jsx';
import '../../assets/css/workshop-details.css';

const WorkshopDetailsForm = ({ handleSubmit, initialData = null, errors = {} }) => {
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
        <form onSubmit={handleFormSubmit} className="workshop-details-form">
            <div className="form-section">
                <label htmlFor="name">Workshop Name</label>
                <input
                    type="text"
                    id="name"
                    name="Name"
                    placeholder="Enter workshop name"
                    value={workshopData ? workshopData.name : ''}
                    onChange={(e) => setWorkshopData({ ...workshopData, name: e.target.value })}
                    required
                />
                <FieldError name="Name" errors={errors} />
            </div>
            <div className="form-section">
                <label htmlFor="address">Address</label>
                <input
                    type="text"
                    id="address"
                    name="Address"
                    placeholder="Enter address"
                    value={workshopData ? workshopData.address : ''}
                    onChange={(e) => setWorkshopData({ ...workshopData, address: e.target.value })}
                    required
                />
                <FieldError name="Address" errors={errors} />
            </div>
            <div className="form-section">
                <label htmlFor="registrationNumber">Registration Number</label>
                <input
                    type="text"
                    id="registrationNumber"
                    name="RegistrationNumber"
                    placeholder="Enter registration number"
                    value={workshopData ? workshopData.registrationNumber : ''}
                    onChange={(e) => setWorkshopData({ ...workshopData, registrationNumber: e.target.value })}
                    required
                />
                <FieldError name="RegistrationNumber" errors={errors} />
            </div>
            <div className="form-section">
                <label htmlFor="phoneNumber">Phone Number</label>
                <input
                    type="tel"
                    id="phoneNumber"
                    name="PhoneNumber"
                    placeholder="Enter phone number"
                    value={workshopData ? workshopData.phoneNumber : ''}
                    onChange={(e) => setWorkshopData({ ...workshopData, phoneNumber: e.target.value })}
                    required
                />
                <FieldError name="PhoneNumber" errors={errors} />
            </div>
            <div className="form-section">
                <label htmlFor="email">Email</label>
                <input
                    type="email"
                    id="email"
                    name="Email"
                    placeholder="Enter email"
                    value={workshopData ? workshopData.email : ''}
                    onChange={(e) => setWorkshopData({ ...workshopData, email: e.target.value })}
                />
                <FieldError name="Email" errors={errors} />
            </div>
            <div className="form-footer">
                {errors.general && <p className="form-error">{errors.general}</p>}
                <button type="submit" className="btn">Done</button>
            </div>
        </form>
    );
};

export default WorkshopDetailsForm;
