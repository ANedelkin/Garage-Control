const API_BASE_URL = 'https://localhost:5173/api';

export async function request(method, url, body = null) {
    let headers = {
        'Content-Type': 'application/json',
    };

    const token = localStorage.getItem('accessToken');
    if (token) {
        headers['Authorization'] = `Bearer ${token}`;
    }

    let request = {
        method: method,
        headers: headers,
        credentials: 'include',
    }
    if (body) {
        request.body = JSON.stringify(body);
    }
    const response = await fetch(`${API_BASE_URL}/${url}`, request);

    return response;
}