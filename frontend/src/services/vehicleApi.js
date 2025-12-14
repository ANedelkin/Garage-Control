import { request } from "../Utilities/request";

export const vehicleApi = {
    getAll: async () => {
        return await (await request('GET', 'vehicle/all')).json();
    },
    getByClient: async (clientId) => {
        return await (await request('GET', `vehicle/by-client/${clientId}`)).json();
    },
    create: async (vehicleData) => {
        return await (await request('POST', 'vehicle/create', vehicleData)).json();
    },
    edit: async (vehicleData) => {
        return await (await request('PUT', 'vehicle/edit', vehicleData)).json();
    },
    delete: async (id) => {
        return await (await request('DELETE', `vehicle/${id}`)).json();
    },
    getDetails: async (id) => {
        return await (await request('GET', `vehicle/${id}`)).json();
    }
};
