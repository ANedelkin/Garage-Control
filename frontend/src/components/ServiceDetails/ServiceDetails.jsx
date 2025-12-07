import React, { useState, useEffect } from 'react';
import '../../assets/css/common.css';

import ServiceDetailsForm from './ServiceDetailsForm.jsx';
import { carServiceApi } from '../../services/carServiceApi.js';

const ServiceDetails = () => {
  const [serviceDetails, setServiceDetails] = useState(null);

  const handleSubmit = (e, formData) => {
    e.preventDefault();
    carServiceApi.edit(formData);
  };

  useEffect(() => {
    carServiceApi.getDetails().then((res) => {
      console.log('Fetched service details:', res);
      setServiceDetails(res);
    }).catch((err) => {
      console.error('Error fetching service details:', err);
    });
  }, []);

  return (
    <main className="main container">
      <ServiceDetailsForm handleSubmit={handleSubmit} initialData={serviceDetails} />
    </main>
  );
};

export default ServiceDetails;
