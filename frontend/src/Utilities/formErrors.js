/**
 * Parses ASP.NET validation error responses (ProblemDetails) into a flat object scope-matched to field names.
 * @param {Error} error The error object thrown by the request utility.
 * @returns {Object} An object with keys normalized to lowercase for easy matching.
 */
export function parseValidationErrors(error) {
    const errors = {};
    const data = error.data;

    const validationErrors = data?.errors || data?.Errors;
    if (validationErrors && typeof validationErrors === 'object') {
        // Map ASP.NET ModelState errors
        Object.keys(validationErrors).forEach(key => {
            const fieldErrors = validationErrors[key];
            // Normalize key to lowercase for matching input names
            errors[key.toLowerCase()] = fieldErrors;
        });
    } else if (data?.Message || data?.message || data?.title) {
        // Fallback for general or business logic errors
        errors.general = data.Message || data.message || data.title;
    } else {
        errors.general = error.message || 'An unexpected error occurred';
    }

    return errors;
}
