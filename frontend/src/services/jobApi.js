import { request } from "../Utilities/request";

const jobApi = {
    getMyJobs: async () => {
        return await request('GET', 'job/my-jobs');
    },
    getWorkerJobs: async (workerId) => {
        return await request('GET', `job/worker/${workerId}`);
    },
    getJob: async (jobId) => {
        return await request('GET', `job/${jobId}`);
    },
    getCompletedJob: async (jobId) => {
        return await request('GET', `job/completed/${jobId}`);
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
    },
    getBusySlots: async (workerId, start, end, excludeId) => {
        let url = `job/busy-slots?workerId=${workerId}&start=${start}&end=${end}`;
        if (excludeId) url += `&excludeId=${excludeId}`;
        return await request('GET', url);
    }
};

export { jobApi };
