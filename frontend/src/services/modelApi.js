import { request } from "../Utilities/request";

export const modelApi = {
    getAll: async (makeId) => {
        return (await request('GET', `model/all/${makeId}`)).json();
    },
    createModel: async (modelData) => {
        return (await request('POST', 'model/create', modelData)).json();
    },
    editModel: async (modelData) => {
        return (await request('PUT', 'model/edit', modelData)).json();
    },
    deleteModel: async (id) => {
        return (await request('DELETE', `model/${id}`)).json();
    }
}