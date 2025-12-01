import {request} from '../Utilities/request.js';

export const authApi = {
    register: async (email, password) => {
        try {      
            const response = await request('POST', 'auth/signup', { email, password });      
            const data = await response.json();
            if (!response.ok) {
                throw new Error(data.message || 'Registration failed');
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
            return data;
        } catch (error) {
            console.error('Login error:', error);
            throw error;
        }
    },

    logout: async () => {
        try {
            const response = await request('POST', 'auth/logout');
            if (!response.ok) {
                throw new Error('Logout failed');
            }

            return await response.json();
        } catch (error) {
            console.error('Logout error:', error);
            throw error;
        }
    },

    refreshToken: async () => {
        try {
            const response = await request('POST', 'auth/refresh');
            const data = await response.json();
            if (!response.ok) {
                throw new Error('Token refresh failed');
            }

            return JSON.parse(data);
        } catch (error) {
            console.error('Token refresh error:', error);
            throw error;
        }
    },
};
