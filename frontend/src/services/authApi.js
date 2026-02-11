import { request } from '../Utilities/request.js';

export const authApi = {
    register: async (email, password) => {
        try {
            const data = await request('POST', 'auth/signup', { email, password });

            if (data.success) {
                localStorage.setItem('LoggedIn', 'true');
                localStorage.setItem('accesses', JSON.stringify(data.accesses || []));
                localStorage.setItem('HasWorkshop', data.hasWorkshop);
            }

            return data;
        } catch (error) {
            console.error('Registration error:', error);
            throw error;
        }
    },

    login: async (email, password) => {
        try {
            const data = await request('POST', 'auth/login', { email, password });

            if (data.success) {
                localStorage.setItem('LoggedIn', 'true');
                localStorage.setItem('accesses', JSON.stringify(data.accesses || []));
                localStorage.setItem('HasWorkshop', data.hasWorkshop);
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
            localStorage.removeItem('HasWorkshop');
        }
        return { success: true };
    },

    refreshToken: async () => {
        try {
            const data = await request('POST', 'auth/refresh');

            if (!data.success) {
                localStorage.removeItem('LoggedIn');
                localStorage.removeItem('accesses');
                localStorage.removeItem('HasWorkshop');
                throw new Error('Token refresh failed');
            }
            localStorage.setItem('LoggedIn', 'true');
            if (data.accesses) {
                localStorage.setItem('accesses', JSON.stringify(data.accesses));
            }
            localStorage.setItem('HasWorkshop', data.hasWorkshop);
            return data;
        } catch (error) {
            console.error('Token refresh error:', error);
            throw error;
        }
    },
};
