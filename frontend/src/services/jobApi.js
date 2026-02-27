import { request } from "../Utilities/request";

const jobApi = {
    getMyJobs: async () => {
        return await request('GET', 'job/my-jobs');
    },
    getJob: async (jobId) => {
        return await request('GET', `job/${jobId}`);
    },
    getJobsByOrderId: async (orderId) => {
        return await request('GET', `job/order/${orderId}`);
    },
    createJob: async (orderId, data) => {
        return await request('POST', `job/order/${orderId}`, data);
    },
    updateJob: async (jobId, data) => {
        return await request('PUT', `job/${jobId}`, data);
    },
    deleteJob: async (jobId) => {
        return await request('DELETE', `job/${jobId}`);
    }
};

export { jobApi };
