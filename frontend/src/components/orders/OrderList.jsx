import React, { useState, useEffect } from 'react';
import { useNavigate, useSearchParams, Link } from 'react-router-dom';
import { orderApi } from '../../services/orderApi';
import { request } from '../../Utilities/request';
import Dropdown from '../common/Dropdown';
import OrderDetailsPopup from './OrderDetailsPopup';
import '../../assets/css/orders.css';

const OrderList = ({ mode = 'active' }) => {
    const navigate = useNavigate();
    const [searchParams] = useSearchParams();
    const [orders, setOrders] = useState([]);
    const [cars, setCars] = useState([]);
    const [filter, setFilter] = useState(searchParams.get('status') || 'all');
    const [search, setSearch] = useState('');
    const [loading, setLoading] = useState(true);
    const [editingOrder, setEditingOrder] = useState(null);

    useEffect(() => {
        fetchOrders();
        fetchCars();
    }, [mode]);

    const fetchOrders = async () => {
        setLoading(true);
        try {
            const data = mode === 'completed'
                ? await orderApi.getCompletedOrders()
                : await orderApi.getActiveOrders();
            setOrders(data);
        } catch (error) {
            console.error(error);
        } finally {
            setLoading(false);
        }
    };

    const fetchCars = async () => {
        try {
            const data = await (await request('GET', 'vehicle/all')).json();
            setCars(data);
        } catch (error) {
            console.error(error);
        }
    };

    const handleSaveOrderDetails = async (details) => {
        try {
            const fullOrder = await orderApi.getOrder(editingOrder.id);
            const payload = {
                carId: details.carId,
                kilometers: details.kilometers,
                isDone: details.isDone,
                jobs: fullOrder.jobs.map(j => ({
                    id: j.id,
                    jobTypeId: j.jobTypeId,
                    workerId: j.workerId,
                    description: j.description,
                    status: j.status,
                    laborCost: j.laborCost,
                    startTime: j.startTime,
                    endTime: j.endTime,
                    parts: j.parts.map(p => ({
                        partId: p.partId,
                        quantity: p.quantity
                    }))
                }))
            };

            await orderApi.updateOrder(editingOrder.id, payload);
            setEditingOrder(null);
            fetchOrders();
        } catch (error) {
            console.error("Failed to update order:", error);
            alert("Error updating order details");
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
            const fullOrder = await orderApi.getOrder(orderId);
            const payload = {
                carId: fullOrder.carId,
                kilometers: fullOrder.kilometers,
                isDone: fullOrder.isDone,
                jobs: fullOrder.jobs.filter(j => j.id !== jobId).map(j => ({
                    id: j.id,
                    jobTypeId: j.jobTypeId,
                    workerId: j.workerId,
                    description: j.description,
                    status: j.status,
                    laborCost: j.laborCost,
                    startTime: j.startTime,
                    endTime: j.endTime,
                    parts: j.parts.map(p => ({
                        partId: p.partId,
                        quantity: p.quantity
                    }))
                }))
            };
            await orderApi.updateOrder(orderId, payload);
            fetchOrders();
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
                    placeholder={`Search ${mode} orders...`}
                    value={search}
                    onChange={e => setSearch(e.target.value)}
                />
                <Dropdown value={filter} onChange={e => setFilter(e.target.value)}>
                    <option value="all">All Statuses</option>
                    <option value="pending">Pending</option>
                    <option value="inprogress">In Progress</option>
                    <option value="finished">Finished</option>
                </Dropdown>
                {mode === 'active' && <Link to="/orders/new" className="btn primary">+ New Order</Link>}
            </div>

            {loading ? (
                <p>Loading...</p>
            ) : filteredOrders.length === 0 ? (
                <p className="list-empty">No {mode} orders found.</p>
            ) : (
                <div className="orders-list">
                    {filteredOrders.map(order => (
                        <div key={order.id} className="tile order-tile">
                            <div className="tile-header">
                                <div className="order-car-info">
                                    <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
                                        <h4>{order.clientName}</h4>
                                        {order.isDone && <span className="badge success">Done</span>}
                                    </div>
                                    <div className="order-car-details">
                                        {order.carName} • {order.carRegistrationNumber} • {order.kilometers} km
                                    </div>
                                </div>
                                <button className="btn secondary icon-btn" onClick={() => setEditingOrder(order)} title="Order Details">
                                    <i className="fa-solid fa-circle-info"></i>
                                </button>
                            </div>

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
                                            <tr key={job.id} onClick={() => navigate(`/orders/${order.id}/jobs/${job.id}`)} className="clickable">
                                                <td>
                                                    <span className={`job-status ${job.status}`}>
                                                        <i className={`fa-solid ${job.status === 'awaitingparts' ? 'fa-clock' :
                                                            job.status === 'pending' ? 'fa-hourglass-start' :
                                                            job.status === 'inprogress' ? 'fa-screwdriver-wrench' : 'fa-check'
                                                            } job-status-${job.status} status-icon`}></i>
                                                        {job.status === 'awaitingparts' ? 'Awaiting Parts' :
                                                            job.status === 'pending' ? 'Pending' :
                                                            job.status === 'inprogress' ? 'In Progress' : 'Done'
                                                        }
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
                            {mode === 'active' && (
                                <div className="tile-footer" style={{ borderTop: '1px solid var(--border-color)', paddingTop: '10px', marginTop: '10px' }}>
                                    <button className="btn secondary small" onClick={() => navigate(`/orders/${order.id}/jobs/new`)}>
                                        + Add Job
                                    </button>
                                </div>
                            )}
                        </div>
                    ))}
                </div>
            )}

            {editingOrder && (
                <OrderDetailsPopup
                    order={editingOrder}
                    cars={cars}
                    onClose={() => setEditingOrder(null)}
                    onSave={handleSaveOrderDetails}
                />
            )}
        </main>
    );
};

export default OrderList;
