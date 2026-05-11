import { request } from "../Utilities/request";

const activityLogApi = {
    getLogs: async (skip = 0, take = 10, startDate = null, endDate = null, search = null) => {
        let url = `activitylog?skip=${skip}&take=${take}`;
        if (startDate) url += `&startDate=${startDate}`;
        if (endDate) url += `&endDate=${endDate}`;
        if (search) url += `&search=${encodeURIComponent(search)}`;
        return await request('GET', url);
    }
};

export default activityLogApi;
