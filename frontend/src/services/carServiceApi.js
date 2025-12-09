import { request } from "../Utilities/request";

export const carServiceApi = {
    hasService: async () => {
        return (await (await request('GET', 'service/hasService')).json()).hasService;
    },
    getDetails: async () => {
        return await (await request('GET', 'service/details')).json();
    },
    create: async (serviceData) => {
        return await (await request('POST', 'service/create', serviceData)).json();
    },
    edit: async (serviceData) => {
        return await (await request('PUT', 'service/edit', serviceData)).json();
    }
}