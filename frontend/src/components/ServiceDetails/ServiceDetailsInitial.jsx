import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';

import ThemeToggle from '../common/ThemeToggle.jsx';
import ServiceDetailsForm from './ServiceDetailsForm.jsx';
import { carServiceApi } from '../../services/carServiceApi.js';

const ServiceDetailsInitial = () => {
  const navigate = useNavigate();
  const handleSubmit = (e, formData) => {
    e.preventDefault();
    carServiceApi.create(formData);
    navigate('/');
  };

  return (
    <div className="horizontal-layout">
      <main className="main container">
        <ThemeToggle className="theme-toggle" />
        <ServiceDetailsForm handleSubmit={handleSubmit} />
      </main>
    </div>
  );
};

export default ServiceDetailsInitial;
