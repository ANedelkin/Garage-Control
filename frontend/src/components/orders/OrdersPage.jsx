import OrderList from './OrderList';
import usePageTitle from '../../hooks/usePageTitle';

const OrdersPage = ({ mode = 'active' }) => {
    usePageTitle(mode === 'active' ? 'Orders' : 'Done Orders');
    return <OrderList mode={mode} />;
};

export default OrdersPage;
