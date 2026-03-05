import { request } from "../Utilities/request";

export const workerApi = {
    getWorkers: async () => {
        return await request('GET', 'worker/all');
    },
    getWorker: async (id) => {
        return await request('GET', `worker/${id}`);
    },
    create: async (workerData) => {
        await request('POST', 'worker/create', workerData);
    },
    edit: async (id, workerData) => {
        await request('PUT', `worker/edit/${id}`, workerData);
    },
    getAccesses: async () => {
        return await request('GET', 'worker/accesses');
    },
    deleteWorker: async (id) => {
        await request('DELETE', `worker/${id}`);
    }
}
