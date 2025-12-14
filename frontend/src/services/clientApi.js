import { request } from "../Utilities/request";

export const clientApi = {
    getAll: async () => {
        return await (await request('GET', 'client/all')).json();
    },
    getDetails: async (id) => {
        return await (await request('GET', `client/${id}`)).json();
    },
    create: async (clientData) => {
        return await (await request('POST', 'client/create', clientData)).json();
    },
    edit: async (clientData) => {
        return await (await request('PUT', 'client/edit', clientData)).json();
    },
    delete: async (id) => {
        return await (await request('DELETE', `client/${id}`)).json();
    }
};
