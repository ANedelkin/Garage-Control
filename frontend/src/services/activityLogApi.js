import { request } from "../Utilities/request";

const activityLogApi = {
    getLogs: async (page = 0, startDate = null, endDate = null, search = null) => {
        let url = `activitylog?page=${page}`;
        if (startDate) url += `&startDate=${startDate}`;
        if (endDate) url += `&endDate=${endDate}`;
        if (search) url += `&search=${encodeURIComponent(search)}`;
        return await request('GET', url);
    }
};

export default activityLogApi;
