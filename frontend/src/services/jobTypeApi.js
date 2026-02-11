import { request } from "../Utilities/request";

export const jobTypeApi = {
    getJobTypes: async () => {
        return await request('GET', 'jobtype/all');
    },
    getJobType: async (id) => {
        return await request('GET', `jobtype/${id}`);
    },
    editJobType: async (id, jobTypeData) => {
        await request('PUT', `jobtype/edit/${id}`, jobTypeData);
    }
}