import { request } from '../Utilities/request.js';

export const authApi = {
    register: async (email, password) => {
        try {
            const response = await request('POST', 'auth/signup', { email, password });
            const data = await response.json();
            if (!response.ok) {
                throw new Error(data.message || 'Registration failed');
            }

            // Store token in localStorage
            if (data.token) {
                localStorage.setItem('accessToken', data.token);
                localStorage.setItem('accesses', JSON.stringify(data.accesses || []));
            }

            return data;
        } catch (error) {
            console.error('Registration error:', error);
            throw error;
        }
    },

    login: async (email, password) => {
        try {
            const response = await request('POST', 'auth/login', { email, password });
            const data = await response.json();
            if (!response.ok) {
                throw new Error(data.message || 'Login failed');
            }

            // Store token in localStorage
            if (data.token) {
                localStorage.setItem('accessToken', data.token);
                localStorage.setItem('accesses', JSON.stringify(data.accesses || []));
            }

            return data;
        } catch (error) {
            console.error('Login error:', error);
            throw error;
        }
    },

    logout: async () => {
        // Just clear the token from localStorage
        // No need to call backend since we're using localStorage-based auth
        localStorage.removeItem('accessToken');
        localStorage.removeItem('accesses');
        return { success: true };
    },

    refreshToken: async () => {
        try {
            const response = await request('POST', 'auth/refresh');
            const data = await response.json();
            if (!response.ok) {
                throw new Error('Token refresh failed');
            }

            const dataJson = JSON.parse(data);
            if (dataJson.accesses) {
                localStorage.setItem('accesses', JSON.stringify(dataJson.accesses));
            }
            return dataJson;
        } catch (error) {
            console.error('Token refresh error:', error);
            throw error;
        }
    },
};
