import React from 'react';

/**
 * Renders a validation error message for a specific field if it exists.
 * @param {string} name - The name of the field (should match ViewModel property).
 * @param {Object} errors - The errors object from state.
 */
const FieldError = ({ name, errors }) => {
    if (!name || !errors) return null;

    // Normalize to lowercase to handle case-mismatches between VM and HTML
    const error = errors[name.toLowerCase()];

    return error ? <p className="field-error">{error}</p> : null;
};

export default FieldError;
