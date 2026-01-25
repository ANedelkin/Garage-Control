import { request } from "../Utilities/request";

export const makeApi = {
    getAll: async () => {
        return await (await request('GET', 'make/all')).json();
    },
    createMake: async (makeData) => {
        return await (await request('POST', 'make/create', makeData)).json();
    },
    editMake: async (makeData) => {
        return await (await request('PUT', 'make/edit', makeData)).json();
    },
    deleteMake: async (id) => {
        return await (await request('DELETE', `make/${id}`)).json();
    },
    getSuggestions: async () => {
        return await (await request('GET', 'make/suggestions')).json();
    },
    promote: async (data) => {
        return await (await request('POST', 'make/promote', data)).json();
    },
    getSuggestedModels: async (makeName) => {
        return await (await request('GET', `make/suggestions/${makeName}/models`)).json();
    }
}