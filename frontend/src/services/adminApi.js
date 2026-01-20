import { request } from '../Utilities/request';

export const adminApi = {
    getUsers: async () => {
        const response = await request('GET', 'admin/users');
        return response.json();
    },

    toggleUserBlock: async (userId) => {
        const response = await request('POST', `admin/users/${userId}/toggle-block`);
        return response.json();
    }
};

