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

    const handleDeleteJob = async (orderId, jobId) => {
        if (!confirm('Are you sure you want to delete this job?')) return;

        try {
            // TODO: Implement delete job API call
            console.log('Delete job', jobId, 'from order', orderId);
            // await orderApi.deleteJob(orderId, jobId);
            // fetchOrders(); // Refresh
        } catch (error) {
            console.error('Failed to delete job:', error);
        }
    };

    const filteredOrders = orders.filter(o =>
        (filter === 'all' || o.status === filter) &&
        (o.carName.toLowerCase().includes(search.toLowerCase()) ||
            o.clientName.toLowerCase().includes(search.toLowerCase()) ||
            o.carRegistrationNumber.toLowerCase().includes(search.toLowerCase()))
    );

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

            {loading ? (
                <p>Loading...</p>
            ) : filteredOrders.length === 0 ? (
                <p className="list-empty">No orders found.</p>
            ) : (
                <div className="orders-grid">
                    {filteredOrders.map(order => (
                        <div key={order.id} className="order-tile">
                            <div className="order-tile-header">
                                <div className="order-car-info">
                                    <h4>{order.clientName}</h4>
                                    <div className="order-car-details">
                                        {order.carName} • {order.carRegistrationNumber}
                                    </div>
                                </div>
                                <span className={`status-badge ${order.status}`}>
                                    {order.status}
                                </span>
                            </div>

                            <div className="order-jobs-table">
                                <table>
                                    <thead>
                                        <tr>
                                            <th>Status</th>
                                            <th>Type</th>
                                            <th>Mechanic</th>
                                            <th>Cost</th>
                                            <th></th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {order.jobs.map(job => (
                                            <tr key={job.id}>
                                                <td>
                                                    <span className={`job-status ${job.status}`}>
                                                        {job.status}
                                                    </span>
                                                </td>
                                                <td>{job.type}</td>
                                                <td>{job.mechanicName}</td>
                                                <td>BGN {(parseFloat(job.laborCost || 0)).toFixed(2)}</td>
                                                <td style={{ display: 'flex', gap: '5px' }}>
                                                    <Link
                                                        className="btn icon-btn"
                                                        to={`/orders/${order.id}`}
                                                        title="Edit order/job"
                                                    >
                                                        <i className="fa-solid fa-pen-to-square"></i>
                                                    </Link>
                                                    <button
                                                        className="btn icon-btn delete"
                                                        onClick={() => handleDeleteJob(order.id, job.id)}
                                                        title="Delete job"
                                                    >
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
                </div>
            )}
        </main>
    );
};

export default OrdersPage;
