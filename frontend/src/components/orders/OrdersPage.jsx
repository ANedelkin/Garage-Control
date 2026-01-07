import React, { useState, useEffect } from 'react';
import { orderApi } from '../../services/orderApi';
import '../../assets/css/orders.css';
import { Link } from 'react-router-dom';

const OrdersPage = () => {
    const [orders, setOrders] = useState([]);
    const [filter, setFilter] = useState('all');
    const [search, setSearch] = useState('');
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        fetchOrders();
    }, []);

    const fetchOrders = async () => {
        try {
            const data = await orderApi.getOrders();
            setOrders(data);
        } catch (error) {
            console.error(error);
        } finally {
            setLoading(false);
        }
    };

    const filteredOrders = orders.filter(o =>
        (filter === 'all' || o.status === filter) &&
        (o.carName.toLowerCase().includes(search.toLowerCase()) ||
            o.clientName.toLowerCase().includes(search.toLowerCase()) ||
            o.carRegistrationNumber.toLowerCase().includes(search.toLowerCase()))
    );

    // Group by Date for display? Or just flat list as per prototype grouped by day
    // Prototype: ordersByDay.
    const ordersByDay = filteredOrders.reduce((acc, order) => {
        const dateStr = new Date(order.date).toLocaleDateString();
        if (!acc[dateStr]) acc[dateStr] = [];
        acc[dateStr].push(order);
        return acc;
    }, {});

    return (
        <main className="main">
            <div className="orders-header">
                <div>
                    <h3>Orders</h3>
                </div>
                <div style={{ display: 'flex', gap: '1rem' }}>
                    <input
                        type="text"
                        placeholder="Search orders..."
                        value={search}
                        onChange={e => setSearch(e.target.value)}
                    />
                    <select className="btn" value={filter} onChange={e => setFilter(e.target.value)}>
                        <option value="all">All Statuses</option>
                        <option value="pending">Pending</option>
                        <option value="inProgress">In Progress</option>
                        <option value="finished">Finished</option>
                    </select>
                    <Link to="/orders/new" className="btn primary">
                        <i className="fa-solid fa-plus"></i> New Order
                    </Link>
                </div>
            </div>

            {loading ? <p>Loading...</p> : Object.keys(ordersByDay).length === 0 ? <p className="list-empty">No orders found.</p> : null}

            {Object.keys(ordersByDay).map(date => (
                <div key={date} className="tile" style={{ marginBottom: '1rem' }}>
                    <h3>{date}</h3>
                    <div style={{ overflowX: 'auto' }}>
                        <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                            <colgroup>
                                <col style={{ width: '80px' }} /> {/* Status */}
                                <col style={{ width: '150px' }} />  {/* Car */}
                                <col style={{ width: '150px' }} /> {/* Client */}
                                <col />                             {/* Jobs Description */}
                                <col style={{ width: '100px' }} /> {/* Action */}
                            </colgroup>
                            <thead>
                                <tr style={{ textAlign: 'left', borderBottom: '1px solid var(--border-color)' }}>
                                    <th>Status</th>
                                    <th>Car</th>
                                    <th>Client</th>
                                    <th>Jobs</th>
                                    <th>Action</th>
                                </tr>
                            </thead>
                            <tbody>
                                {ordersByDay[date].map(order => (
                                    <tr key={order.id} style={{ borderBottom: '1px solid var(--border-color)' }}>
                                        <td>
                                            <span className={`status ${order.status}`}>
                                                {order.status}
                                            </span>
                                        </td>
                                        <td>
                                            <div>{order.carName}</div>
                                            <small>{order.carRegistrationNumber}</small>
                                        </td>
                                        <td>{order.clientName}</td>
                                        <td>
                                            {order.jobs.map(j => (
                                                <div key={j.id}>
                                                    <small><b>{j.type}</b> ({j.mechanicName}) - {j.status}</small>
                                                </div>
                                            ))}
                                        </td>
                                        <td>
                                            <button className="btn icon-btn delete">
                                                <i className="fa-solid fa-trash"></i>
                                            </button>
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>
                </div>
            ))}
        </main>
    );
};

export default OrdersPage;
