import { request } from "../Utilities/request";

const activityLogApi = {
    getLogs: async (count = 100) => {
        return await request('GET', `activitylog?count=${count}`);
    }
};

export default activityLogApi;
