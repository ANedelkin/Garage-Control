import React, { useState } from 'react';
import '../../assets/css/common.css';

import ThemeToggle from '../common/ThemeToggle.jsx';
import ServiceDetailsForm from './ServiceDetailsForm.jsx';

const ServiceDetails = () => {
  const [serviceData, setServiceData] = useState({
    name: '',
    address: '',
    registrationNumber: '',
    phoneNumber: ''
  });

  const handleSubmit = (e) => {
    e.preventDefault();
    // TODO: Implement service data submission logic
    console.log('Service Info:', serviceData);
  };

  return (
    <main className="main">
        <ServiceDetailsForm />
    </main>
  );
};

export default ServiceDetails;
