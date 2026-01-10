import React, { useState, useEffect } from 'react';
import { useNavigate } from "react-router-dom";
import { orderApi } from '../../services/orderApi';
import Dropdown from '../common/Dropdown';
import '../../assets/css/orders.css';
import { Link } from 'react-router-dom';

const OrdersPage = () => {
    const navigate = useNavigate();
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

    const formatDate = (input) => {
        if (!input) return '';
        const date = new Date(input);
        const day = date.getDate().toString().padStart(2, '0');
        const month = (date.getMonth() + 1).toString().padStart(2, '0');
        const hours = date.getHours().toString().padStart(2, '0');
        const minutes = date.getMinutes().toString().padStart(2, '0');
        return `${day}/${month} ${hours}:${minutes}`;
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

    const filteredOrders = orders.map(order => {
        const filteredJobs = order.jobs.filter(job =>
            filter === 'all' || job.status === filter
        );

        return { ...order, jobs: filteredJobs };
    }).filter(order => {
        if (filter !== 'all' && order.jobs.length === 0) return false;

        return (order.carName.toLowerCase().includes(search.toLowerCase()) ||
            order.clientName.toLowerCase().includes(search.toLowerCase()) ||
            order.carRegistrationNumber.toLowerCase().includes(search.toLowerCase()));
    });

    return (
        <main className="main">
            <div className="header">
                <input
                    type="text"
                    placeholder="Search orders..."
                    value={search}
                    onChange={e => setSearch(e.target.value)}
                />
                <Dropdown value={filter} onChange={e => setFilter(e.target.value)}>
                    <option value="all">All Statuses</option>
                    <option value="pending">Pending</option>
                    <option value="inprogress">In Progress</option>
                    <option value="finished">Finished</option>
                </Dropdown>
                <Link to="/orders/new" className="btn primary">+ New Order</Link>
            </div>

            {loading ? (
                <p>Loading...</p>
            ) : filteredOrders.length === 0 ? (
                <p className="list-empty">No orders found.</p>
            ) : (
                <>
                    {filteredOrders.map(order => (
                        <div key={order.id} className="tile">
                            <div className="tile-header">
                                <div className="order-car-info">
                                    <h4>{order.clientName}</h4>
                                    <div className="order-car-details">
                                        {order.carName} • {order.carRegistrationNumber}
                                    </div>
                                </div>
                            </div>

                            {/* <div className="divider"/> */}

                            <div className="table">
                                <table>
                                    <thead>
                                        <tr>
                                            <th>Status</th>
                                            <th>Time</th>
                                            <th>Type</th>
                                            <th>Mechanic</th>
                                            <th>Cost</th>
                                            <th></th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {order.jobs.map(job => (
                                            <tr key={job.id} onClick={() => navigate(`/orders/${order.id}`)}>
                                                <td>
                                                    <span className={`job-status ${job.status}`}>
                                                        <i className={`fa-solid ${job.status === 'pending' ? 'fa-hourglass-start' :
                                                            job.status === 'inprogress' ? 'fa-screwdriver-wrench' : 'fa-check'
                                                            } status-icon`}></i>
                                                        {job.status}
                                                    </span>
                                                </td>
                                                <td>{formatDate(job.startTime)}</td>
                                                <td>{job.type}</td>
                                                <td>{job.mechanicName}</td>
                                                <td>&euro; {(parseFloat(job.laborCost || 0)).toFixed(2)}</td>
                                                <td onClick={e => e.stopPropagation()}>
                                                    <button className="btn icon-btn delete" onClick={() => handleDeleteJob(order.id, job.id)}>
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
                </>
            )}
        </main>
    );
};

export default OrdersPage;
