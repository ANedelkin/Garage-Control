import { request } from "../Utilities/request";

export const activityApi = {
    getActivities: async () => {
        return await (await request('GET', 'activity/all')).json();
    },
    getActivity: async (id) => {
        return await (await request('GET', `activity/${id}`)).json();
    },
    editActivity: async (activityData) => {
        return await (await request('PUT', 'activity/edit', activityData)).json();
    }
}