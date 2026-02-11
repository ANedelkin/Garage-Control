import { request } from "../Utilities/request";

export const makeApi = {
    getAll: async () => {
        return await request('GET', 'make/all');
    },
    createMake: async (makeData) => {
        return await request('POST', 'make/create', makeData);
    },
    editMake: async (id, makeData) => {
        await request('PUT', `make/edit/${id}`, makeData);
    },
    deleteMake: async (id) => {
        await request('DELETE', `make/${id}`);
    },
    getSuggestions: async () => {
        return await request('GET', 'make/suggestions');
    },
    promote: async (data) => {
        return await request('POST', 'make/promote', data);
    },
    promoteModel: async (data) => {
        return await request('POST', 'make/promote-model', data);
    },
    getSuggestedModels: async (makeName) => {
        return await request('GET', `make/suggestions/${makeName}/models`);
    },
    mergeMakeWithGlobal: async (customMakeId, globalMakeId) => {
        return await request('POST', 'make/merge-with-global', { customMakeId, globalMakeId });
    }
}