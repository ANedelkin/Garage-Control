import { request } from "../Utilities/request";

const orderApi = {
    getOrders: async () => {
        return (await request('GET', 'order')).json();
    },
    createOrder: async (data) => {
        return (await request('POST', 'order/create', data)).json();
    }
};

export { orderApi };
