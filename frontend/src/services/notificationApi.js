import { request } from "../Utilities/request";

const notificationApi = {
    getNotifications: async () => {
        return await request('GET', 'notification');
    },
    markAsRead: async (id) => {
        await request('PUT', `notification/${id}/read`);
    }
};

export { notificationApi };
