import { request } from '../Utilities/request';

export const adminApi = {
    getUsers: async () => {
        return await request('GET', 'admin/users');
    },

    toggleUserBlock: async (userId) => {
        return await request('POST', `admin/users/${userId}/toggle-block`);
    },

    getWorkshops: async () => {
        return await request('GET', 'admin/workshops');
    },

    toggleWorkshopBlock: async (workshopId) => {
        return await request('POST', `admin/workshops/${workshopId}/toggle-block`);
    },

    getStats: async () => {
        return await request('GET', 'admin/stats');
    }
};


