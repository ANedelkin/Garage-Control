import { request } from "../Utilities/request";

const dashboardApi = {
    getDashboardData: async () => {
        return (await request('GET', 'dashboard')).json();
    }
};

export { dashboardApi };
