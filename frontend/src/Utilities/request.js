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
    if (contentType && contentType.includes('application/json')) {
        try {
            data = await response.json();
        } catch (e) {
            // Not valid JSON or empty body
        }
    }

    if (!response.ok) {
        const errorMessage = data?.message || data?.error || response.statusText || 'Request failed';
        const error = new Error(errorMessage);
        error.status = response.status;
        error.data = data;
        throw error;
    }

    return data || response;
}