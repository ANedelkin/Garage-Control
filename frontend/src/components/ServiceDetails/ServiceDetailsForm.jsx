import React, { useState } from 'react';
import '../../assets/css/common.css';

const ServiceDetailsForm = () => {
    const [serviceData, setServiceData] = useState({
        name: '',
        address: '',
        registrationNumber: '',
        phoneNumber: ''
    });

    const handleSubmit = (e) => {
        e.preventDefault();
        // TODO: Implement service data submission logic
        // console.log(serviceData);
    };

    return (
        <div className="tile">
            <h3 className="tile-header">Service Information</h3>
            <form onSubmit={handleSubmit}>
                <div className="form-section">
                    <label htmlFor="name">Service Name</label>
                    <input
                        type="text"
                        id="name"
                        placeholder="Enter service name"
                        value={serviceData.name}
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
                        value={serviceData.address}
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
                        value={serviceData.registrationNumber}
                        onChange={(e) => setServiceData({ ...serviceData, registrationNumber: e.target.value })}
                        required
                    />
                </div>
                <div className="form-section">
                    <label htmlFor="phoneNumber">Phone Number</label>
                    <input
                        type="tel"
                        id="phoneNumber"
                        placeholder="Enter phone number"
                        value={serviceData.phoneNumber}
                        onChange={(e) => setServiceData({ ...serviceData, phoneNumber: e.target.value })}
                        required
                    />
                </div>
                <div className="form-footer">
                    <button type="submit" className="btn">Submit</button>
                </div>
            </form>
        </div>
    );
};

export default ServiceDetailsForm;
