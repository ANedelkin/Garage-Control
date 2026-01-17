import { request } from "../Utilities/request";

export const workshopApi = {
    hasWorkshop: async () => {
        return (await (await request('GET', 'workshop/has-workshop')).json()).hasWorkshop;
    },
    getDetails: async () => {
        return await (await request('GET', 'workshop/details')).json();
    },
    create: async (workshopData) => {
        return await (await request('POST', 'workshop/create', workshopData)).json();
    },
    edit: async (workshopData) => {
        return await (await request('PUT', 'workshop/edit', workshopData)).json();
    }
}
