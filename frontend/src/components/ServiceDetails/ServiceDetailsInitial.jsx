import React, { useState } from 'react';
import '../../assets/css/common.css';

import ThemeToggle from '../common/ThemeToggle.jsx';
import ServiceDetailsForm from './ServiceDetailsForm.jsx';
import { carServiceApi } from '../../services/carServiceApi.js';

const ServiceDetailsInitial = () => {
  const handleSubmit = (e, formData) => {
    e.preventDefault();
    carServiceApi.create(formData);
  };

  return (
    <main className="background">
      <ThemeToggle className="theme-toggle"/>
      <div className="container">
        <ServiceDetailsForm handleSubmit={handleSubmit}/>
      </div>
    </main>
  );
};

export default ServiceDetailsInitial;
