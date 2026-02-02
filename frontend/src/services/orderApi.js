import { request } from "../Utilities/request";

const orderApi = {
    getOrders: async () => {
        return (await request('GET', 'order')).json();
    },
    createOrder: async (data) => {
        return (await request('POST', 'order/create', data)).json();
    },
    getOrder: async (id) => {
        return (await request('GET', `order/${id}`)).json();
    },
    updateOrder: async (id, data) => {
        return (await request('PUT', `order/${id}`, data)).json();
    },
    getMyJobs: async () => {
        return (await request('GET', 'order/my-jobs')).json();
    },
    getJob: async (jobId) => {
        return (await request('GET', `order/job/${jobId}`)).json();
    },
    createJob: async (orderId, data) => {
        return (await request('POST', `order/${orderId}/job`, data)).json();
    },
    updateJob: async (jobId, data) => {
        return (await request('PUT', `order/job/${jobId}`, data)).json();
    }
};

export { orderApi };
