import { request } from '../Utilities/request';

export const adminApi = {
    getUsers: async () => {
        const response = await request('GET', 'admin/users');
        return response.json();
    },

    toggleUserBlock: async (userId) => {
        const response = await request('POST', `admin/users/${userId}/toggle-block`);
        return response.json();
    },

    getWorkshops: async () => {
        const response = await request('GET', 'admin/workshops');
        return response.json();
    },

    toggleWorkshopBlock: async (workshopId) => {
        const response = await request('POST', `admin/workshops/${workshopId}/toggle-block`);
        return response.json();
    }
};


