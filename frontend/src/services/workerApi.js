import { request } from "../Utilities/request";

export const workerApi = {
    getWorkers: async () => {
        return await (await request('GET', 'worker/all')).json();
    },
    getWorker: async (id) => {
        return await (await request('GET', `worker/${id}`)).json();
    },
    editWorker: async (workerData) => {
        return await (await request('PUT', 'worker/edit', workerData)).json();
    },
    getRoles: async () => {
        return await (await request('GET', 'worker/roles')).json();
    },
    deleteWorker: async (id) => {
        return await (await request('DELETE', `worker/${id}`)).json();
    }
}
