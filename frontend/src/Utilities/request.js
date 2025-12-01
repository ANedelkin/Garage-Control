const API_BASE_URL = 'https://localhost:5173/api';

export async function request(method, url, body = {}) {
    const response = await fetch(`${API_BASE_URL}/${url}`, {
        method: method,
        headers: {
            'Content-Type': 'application/json',
        },
        credentials: 'include',
        body: JSON.stringify(body)
    });

    return response;
}