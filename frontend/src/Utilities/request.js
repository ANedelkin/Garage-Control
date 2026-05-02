const API_BASE_URL = 'https://localhost:5173/api';
let refreshPromise = null;

export async function request(method, url, body = null, options = {}) {
    const isRefreshCall = url.includes('auth/refresh');

    // If there's an ongoing refresh call, and this IS a refresh call, wait for it.
    if (isRefreshCall && refreshPromise) {
        return await refreshPromise;
    }

    let headers = method.toLowerCase() === 'get' ? {} : {
        'Content-Type': 'application/json',
    };

    let requestOptions = {
        method: method,
        headers: headers,
        credentials: 'include',
    };
    Object.assign(requestOptions, options);
    if (body) {
        requestOptions.body = JSON.stringify(body);
    }

    const performRequest = async () => {
        const response = await fetch(`${API_BASE_URL}/${url}`, requestOptions);

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
            // Handle 401 and attempt refresh if it's not a refresh call itself
            if (response.status === 401 && !isRefreshCall) {
                try {
                    if (!refreshPromise) {
                        refreshPromise = request('POST', 'auth/refresh');
                    }
                    
                    await refreshPromise;
                    refreshPromise = null;
                    
                    // Retry original request
                    return await request(method, url, body, options);
                } catch (refreshError) {
                    refreshPromise = null;
                    const isAuthPage = window.location.pathname.includes('/login') || window.location.pathname.includes('/signup');
                    if (!isAuthPage) {
                        window.dispatchEvent(new CustomEvent('api-401'));
                    }
                    throw refreshError;
                }
            }

            // Handle failed refresh call
            if (response.status === 401 && isRefreshCall) {
                const isAuthPage = window.location.pathname.includes('/login') || window.location.pathname.includes('/signup');
                if (!isAuthPage) {
                    window.dispatchEvent(new CustomEvent('api-401'));
                }
            }

            if (response.status === 404 && method.toLowerCase() === 'get') {
                const resourceParts = url.split('/');
                const resourceBase = resourceParts[0];
                const resource = resourceBase.charAt(0).toUpperCase() + resourceBase.slice(1);
                window.dispatchEvent(new CustomEvent('api-404', { detail: { resource } }));
                return new Promise(() => { }); 
            }

            if (response.status === 403) {
                window.dispatchEvent(new CustomEvent('api-403'));
            }

            const errorMessage = data?.message || data?.error || data?.title || response.statusText || 'Request failed';
            const error = new Error(errorMessage);
            error.status = response.status;
            error.data = data;
            throw error;
        }

        return data || response;
    };

    // If this is a refresh call, ensure we lock it
    if (isRefreshCall) {
        if (!refreshPromise) {
            refreshPromise = performRequest().finally(() => {
                refreshPromise = null;
            });
        }
        return await refreshPromise;
    }

    return await performRequest();
}