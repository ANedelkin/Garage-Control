import React, { useState } from 'react';
import '../../assets/css/common.css';

import ThemeToggle from '../common/ThemeToggle.jsx';
import ServiceDetailsForm from './ServiceDetailsForm.jsx';

const ServiceDetailsInitial = () => {
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
    <main className="background">
      <ThemeToggle />
      <div className="container">
        <ServiceDetailsForm />
      </div>
    </main>
  );
};

export default ServiceDetailsInitial;
