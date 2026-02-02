import React from 'react';
import OrderList from './OrderList';

const OrdersPage = ({ mode = 'active' }) => {
    return <OrderList mode={mode} />;
};

export default OrdersPage;
