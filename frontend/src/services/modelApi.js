import { request } from "../Utilities/request";

export const modelApi = {
    getAll: async (makeId) => {
        return await request('GET', `model/all/${makeId}`);
    },
    getModel: async (id) => {
        return await request('GET', `model/${id}`);
    },
    createModel: async (modelData) => {
        return await request('POST', 'model/create', modelData);
    },
    editModel: async (id, modelData) => {
        await request('PUT', `model/edit/${id}`, modelData);
    },
    deleteModel: async (id) => {
        await request('DELETE', `model/${id}`);
    },
    mergeModelWithGlobal: async (customModelId, globalModelId) => {
        return await request('POST', 'model/merge-with-global', { customModelId, globalModelId });
    }
}