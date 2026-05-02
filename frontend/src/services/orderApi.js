import { request } from "../Utilities/request";

const orderApi = {
    getOrders: async () => {
        return await request('GET', 'order');
    },
    getActiveOrders: async () => {
        return await request('GET', 'order/active');
    },
    getArchivedOrders: async () => {
        return await request('GET', 'order/archived');
    },
    createOrder: async (data) => {
        return await request('POST', 'order/create', data);
    },
    getOrder: async (id) => {
        return await request('GET', `order/${id}`);
    },
    updateOrder: async (id, data) => {
        return await request('PUT', `order/${id}`, data);
    },
    deleteOrder: async (id) => {
        return await request('DELETE', `order/${id}`);
    }
};

export { orderApi };
