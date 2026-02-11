import { request } from "../Utilities/request";

export const workshopApi = {
    hasWorkshop: async () => {
        return (await request('GET', 'workshop/has-workshop')).hasWorkshop;
    },
    getDetails: async () => {
        return await request('GET', 'workshop/details');
    },
    create: async (workshopData) => {
        return await request('POST', 'workshop/create', workshopData);
    },
    edit: async (workshopData) => {
        return await request('PUT', 'workshop/edit', workshopData);
    }
}
