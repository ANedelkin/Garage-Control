const API_BASE_URL = 'https://localhost:5173/api';

export async function request(method, url, body = null, options = {}) {
    let headers = method=='get'?{}:{
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

    return response;
}