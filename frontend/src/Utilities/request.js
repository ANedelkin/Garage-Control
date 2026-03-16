const API_BASE_URL = 'https://localhost:5173/api';

export async function request(method, url, body = null, options = {}) {
    let headers = method == 'get' ? {} : {
        'Content-Type': 'application/json',
    };

    let request = {
        method: method,
        headers: headers,
        credentials: 'include',
    }
    Object.assign(request, options)
    if (body) {
        request.body = JSON.stringify(body);
    }
    const response = await fetch(`${API_BASE_URL}/${url}`, request);

    let data = null;
    const contentType = response.headers.get('content-type');
    if (contentType && (contentType.includes('application/json') || contentType.includes('application/problem+json'))) {
        try {
            data = await response.json();
        } catch (e) {
            // Not valid JSON or empty body
        }
    }

    if (!response.ok) {
        if (response.status === 404 && method.toLowerCase() === 'get') {
            const resourceParts = url.split('/');
            const resourceBase = resourceParts[0]; 
            const resource = resourceBase.charAt(0).toUpperCase() + resourceBase.slice(1);
            window.dispatchEvent(new CustomEvent('api-404', { detail: { resource } }));
            return new Promise(() => {}); // Never resolve/reject so components don't show alerts during redirect
        }

        const errorMessage = data?.message || data?.error || data?.title || response.statusText || 'Request failed';
        const error = new Error(errorMessage);
        error.status = response.status;
        error.data = data;
        throw error;
    }

    return data || response;
}