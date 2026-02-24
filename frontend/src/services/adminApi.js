import { request } from '../Utilities/request';

export const adminApi = {
    getUsers: async () => {
        return await request('GET', 'admin/users');
    },

    toggleUserBlock: async (userId, reason) => {
        const url = reason ? `admin/users/${userId}/toggle-block?reason=${encodeURIComponent(reason)}` : `admin/users/${userId}/toggle-block`;
        return await request('POST', url);
    },

    getWorkshops: async () => {
        return await request('GET', 'admin/workshops');
    },

    toggleWorkshopBlock: async (workshopId, reason) => {
        const url = reason ? `admin/workshops/${workshopId}/toggle-block?reason=${encodeURIComponent(reason)}` : `admin/workshops/${workshopId}/toggle-block`;
        return await request('POST', url);
    },

    getStats: async () => {
        return await request('GET', 'admin/stats');
    }
};


