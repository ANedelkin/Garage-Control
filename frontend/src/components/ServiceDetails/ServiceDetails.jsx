import React, { useState } from 'react';
import '../../assets/css/common.css';

import ThemeToggle from '../common/ThemeToggle.jsx';
import ServiceDetailsForm from './ServiceDetailsForm.jsx';
import { carServiceApi } from '../../services/carServiceApi.js';

const ServiceDetails = () => {

  const handleSubmit = (e, formData) => {
    e.preventDefault();
    carServiceApi.edit(formData);
  };

  return (
    <main className="main">
        <ServiceDetailsForm handleSubmit={handleSubmit}/>
    </main>
  );
};

export default ServiceDetails;
