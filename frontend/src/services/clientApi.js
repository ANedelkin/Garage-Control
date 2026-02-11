import { request } from "../Utilities/request";

export const clientApi = {
    getAll: async () => {
        return await request('GET', 'client/all');
    },
    getDetails: async (id) => {
        return await request('GET', `client/${id}`);
    },
    create: async (clientData) => {
        return await request('POST', 'client/create', clientData);
    },
    edit: async (id, clientData) => {
        await request('PUT', `client/edit/${id}`, clientData);
    },
    delete: async (id) => {
        await request('DELETE', `client/${id}`);
    }
};
