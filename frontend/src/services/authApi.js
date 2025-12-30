import { request } from '../Utilities/request.js';

export const authApi = {
    register: async (email, password) => {
        try {
            const response = await request('POST', 'auth/signup', { email, password });
            const data = await response.json();
            if (!response.ok) {
                throw new Error(data.message || 'Registration failed');
            }

            if (data.success) {
                localStorage.setItem('LoggedIn', 'true');
                localStorage.setItem('accesses', JSON.stringify(data.accesses || []));
                localStorage.setItem('HasService', data.hasService);
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

            if (data.success) {
                localStorage.setItem('LoggedIn', 'true');
                localStorage.setItem('accesses', JSON.stringify(data.accesses || []));
                localStorage.setItem('HasService', data.hasService);
            }

            return data;
        } catch (error) {
            console.error('Login error:', error);
            throw error;
        }
    },

    logout: async () => {
        try {
            await request('POST', 'auth/logout');
        } catch (error) {
            console.error('Logout error:', error);
        } finally {
            // Always clear localStorage
            localStorage.removeItem('LoggedIn');
            localStorage.removeItem('accesses');
            localStorage.removeItem('HasService');
        }
        return { success: true };
    },

    refreshToken: async () => {
        try {
            const response = await request('POST', 'auth/refresh');
            const data = await response.json();

            if (!response.ok || !data.success) {
                localStorage.removeItem('LoggedIn');
                localStorage.removeItem('accesses');
                localStorage.removeItem('HasService');
                throw new Error('Token refresh failed');
            }
            localStorage.setItem('LoggedIn', 'true');
            if (data.accesses) {
                localStorage.setItem('accesses', JSON.stringify(data.accesses));
            }
            localStorage.setItem('HasService', data.hasService);
            return data;
        } catch (error) {
            console.error('Token refresh error:', error);
            throw error;
        }
    },
};
