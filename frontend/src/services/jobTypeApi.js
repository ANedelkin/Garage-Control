import { request } from "../Utilities/request";

export const jobTypeApi = {
    getJobTypes: async () => {
        return await request('GET', 'jobtype/all');
    },
    getJobType: async (id) => {
        return await request('GET', `jobtype/${id}`);
    },
    addJobType: async (jobTypeData) => {
        await request('POST', 'jobtype/create', jobTypeData);
    },
    editJobType: async (id, jobTypeData) => {
        await request('PUT', `jobtype/edit/${id}`, jobTypeData);
    }
}
