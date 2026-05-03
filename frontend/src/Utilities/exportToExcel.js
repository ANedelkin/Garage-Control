const API_BASE_URL = '/api';

/**
 * Downloads a file from the backend Export controller.
 * @param {string} endpoint - The API endpoint (e.g., 'export/orders?isArchived=false')
 * @param {string} format - The file format ('excel' or 'pdf')
 */
export async function exportFile(endpoint, format = 'excel') {
    try {
        const separator = endpoint.includes('?') ? '&' : '?';
        const finalEndpoint = `${endpoint}${separator}format=${format}`;

        const response = await fetch(`${API_BASE_URL}/${finalEndpoint}`, {
            method: 'GET',
            headers: {
                // Authentication is handled via cookies (credentials: 'include')
            },
            credentials: 'include'
        });

        if (!response.ok) {
            throw new Error('Export failed');
        }

        const blob = await response.blob();
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        
        // Try to get filename from content-disposition header
        const contentDisposition = response.headers.get('content-disposition');
        let filename = format === 'pdf' ? 'export.pdf' : 'export.xlsx';
        if (contentDisposition && contentDisposition.indexOf('attachment') !== -1) {
            const filenameRegex = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/;
            const matches = filenameRegex.exec(contentDisposition);
            if (matches != null && matches[1]) {
                filename = matches[1].replace(/['"]/g, '');
            }
        }
        
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
        a.remove();
    } catch (error) {
        console.error('Export error:', error);
        alert(`Failed to export ${format} file. Please try again.`);
    }
}

// Keep backward compatibility
export const exportToExcel = (endpoint) => exportFile(endpoint, 'excel');
export const exportToPdf = (endpoint) => exportFile(endpoint, 'pdf');
