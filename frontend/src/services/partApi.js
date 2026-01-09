import { request } from "../Utilities/request";

const partApi = {
    getFolderContent: async (folderId) => {
        return (await request('GET', `part/folder-content?folderId=${folderId || ''}`)).json();
    },
    getAllParts: async () => {
        return (await request('GET', 'part/all')).json();
    },
    createPart: async (data) => {
        return (await request('POST', 'part/create', data)).json();
    },
    updatePart: async (data) => {
        await request('PUT', 'part/update', data);
    },
    deletePart: async (id) => {
        await request('DELETE', `part/delete/${id}`);
    },
    createFolder: async (data) => {
        return (await request('POST', 'part/folder/create', data)).json();
    },
    renameFolder: async (id, newName) => {
        await request('PUT', `part/folder/rename/${id}`, newName);
    },
    deleteFolder: async (id) => {
        await request('DELETE', `part/folder/delete/${id}`);
    }
};

export { partApi };
