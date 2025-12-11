import { request } from "../Utilities/request";

export const jobTypeApi = {
    getJobTypes: async () => {
        return await (await request('GET', 'jobtype/all')).json();
    },
    getJobType: async (id) => {
        return await (await request('GET', `jobtype/${id}`)).json();
    },
    editJobType: async (jobTypeData) => {
        return await (await request('PUT', 'jobtype/edit', jobTypeData)).json();
    }
}