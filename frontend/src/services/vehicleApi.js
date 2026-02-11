import { request } from "../Utilities/request";

export const vehicleApi = {
    getAll: async () => {
        return await request('GET', 'vehicle/all');
    },
    getByClient: async (clientId) => {
        return await request('GET', `vehicle/by-client/${clientId}`);
    },
    create: async (vehicleData) => {
        return await request('POST', 'vehicle/create', vehicleData);
    },
    edit: async (id, vehicleData) => {
        await request('PUT', `vehicle/edit/${id}`, vehicleData);
    },
    delete: async (id) => {
        await request('DELETE', `vehicle/${id}`);
    },
    getDetails: async (id) => {
        return await request('GET', `vehicle/${id}`);
    }
};
