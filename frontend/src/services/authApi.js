import { request } from '../Utilities/request.js';

export const authApi = {
    register: async (email, password) => {
        try {
            const data = await request('POST', 'auth/signup', { email, password });

            // Only update localStorage if the request succeeded (2xx)
            localStorage.setItem('LoggedIn', 'true');
            localStorage.setItem('accesses', JSON.stringify(data.accesses || []));
            localStorage.setItem('HasWorkshop', data.hasWorkshop);

            return data;
        } catch (error) {
            // For 4xx/5xx, request() throws
            localStorage.removeItem('LoggedIn');
            localStorage.removeItem('accesses');
            localStorage.removeItem('HasWorkshop');
            console.error('Registration error:', error);
            throw error;
        }
    },

    login: async (email, password) => {
        try {
            const data = await request('POST', 'auth/login', { email, password });

            localStorage.setItem('LoggedIn', 'true');
            localStorage.setItem('accesses', JSON.stringify(data.accesses || []));
            localStorage.setItem('HasWorkshop', data.hasWorkshop);

            return data;
        } catch (error) {
            localStorage.removeItem('LoggedIn');
            localStorage.removeItem('accesses');
            localStorage.removeItem('HasWorkshop');
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
            localStorage.removeItem('LoggedIn');
            localStorage.removeItem('accesses');
            localStorage.removeItem('HasWorkshop');
        }
        return {};
    },

    refreshToken: async () => {
        try {
            const data = await request('POST', 'auth/refresh');

            // Only update localStorage if request succeeded (2xx)
            localStorage.setItem('LoggedIn', 'true');
            localStorage.setItem('accesses', JSON.stringify(data.accesses || []));
            localStorage.setItem('HasWorkshop', data.hasWorkshop);

            return data;
        } catch (error) {
            localStorage.removeItem('LoggedIn');
            localStorage.removeItem('accesses');
            localStorage.removeItem('HasWorkshop');
            console.error('Token refresh error:', error);
            throw error;
        }
    },
};
