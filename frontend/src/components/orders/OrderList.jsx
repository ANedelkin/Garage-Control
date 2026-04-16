import React, { useState, useEffect, useRef } from 'react';
import { useNavigate, useSearchParams, Link, useParams } from 'react-router-dom';
import { orderApi } from '../../services/orderApi';
import { jobApi } from '../../services/jobApi';
import { request } from '../../Utilities/request';
import { usePopup } from '../../context/PopupContext';
import Dropdown from '../common/Dropdown';
import OrderDetailsPopup from './OrderDetailsPopup';
import NewOrderSetup from './NewOrderSetup';
import ConfirmationPopup from '../common/ConfirmationPopup';
import '../../assets/css/orders.css';
import { parseValidationErrors } from '../../Utilities/formErrors.js';

const OrderList = ({ mode = 'active' }) => {
    const navigate = useNavigate();
    const { orderId } = useParams();
    const [searchParams] = useSearchParams();
    const { addPopup, removeLastPopup } = usePopup();
    const [orders, setOrders] = useState([]);
    const [cars, setCars] = useState([]);
    const [filter, setFilter] = useState(searchParams.get('status') || 'all');
    const [search, setSearch] = useState('');
    const [loading, setLoading] = useState(true);
    const [editingOrder, setEditingOrder] = useState(null);
    const [errors, setErrors] = useState({});
    const orderRefs = useRef({});
    const jobRefs = useRef({});
    const highlightJob = searchParams.get('highlightJob');
    const highlight = searchParams.get('highlight') === 'true';

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

            const ordersWithJobs = await Promise.all(data.map(async (order) => {
                const jobs = await jobApi.getJobsByOrderId(order.id);
                return { ...order, jobs };
            }));

            setOrders(ordersWithJobs);
        } catch (error) {
            console.error(error);
        } finally {
            setLoading(false);
        }
    };

    const fetchCars = async () => {
        try {
            const data = await request('GET', 'vehicle/all');
            setCars(data);
        } catch (error) {
            console.error(error);
        }
    };

    const handleSaveOrderDetails = (orderId) => async (details) => {
        try {
            const payload = {
                carId: details.carId,
                kilometers: details.kilometers,
                isDone: details.isDone
            };

            await orderApi.updateOrder(orderId, payload);
            removeLastPopup();
            setErrors({});
            fetchOrders();
        } catch (error) {
            console.error("Failed to update order:", error);
            setErrors(parseValidationErrors(error));
        }
    };

    const handleClosePopup = () => {
        removeLastPopup();
        navigate(mode === 'completed' ? '/done-orders' : '/orders');
    };

    const openOrderDetailsPopup = (order) => {
        setEditingOrder(order);
        addPopup(
            'Order Details',
            <OrderDetailsPopup
                order={order}
                cars={cars}
                onClose={handleClosePopup}
                onSave={handleSaveOrderDetails(order.id)}
                errors={errors}
            />,
            false,
            () => navigate(mode === 'completed' ? '/done-orders' : '/orders')
        );
    };

    useEffect(() => {
        if (orderId && !loading && orders.length > 0) {
            const orderRow = orderRefs.current[orderId];
            if (orderRow) {
                orderRow.scrollIntoView({ behavior: 'smooth', block: 'center' });
            }
            if (!highlight && !highlightJob) {
                const order = orders.find(o => o.id === orderId);
                if (order && (!editingOrder || editingOrder.id !== orderId)) {
                    openOrderDetailsPopup(order);
                }
            }
        }
    }, [orderId, loading, orders, highlight, highlightJob]);

    useEffect(() => {
        if (highlightJob && !loading && orders.length > 0) {
            const jobRow = jobRefs.current[highlightJob];
            if (jobRow) {
                jobRow.scrollIntoView({ behavior: 'smooth', block: 'center' });
            }
        }
    }, [highlightJob, loading, orders]);

    const openNewOrderPopup = () => {
        addPopup(
            'New Order',
            <NewOrderSetup
                onClose={removeLastPopup}
                onSuccess={() => {
                    removeLastPopup();
                    fetchOrders();
                }}
            />
        );
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
        addPopup(
            'Delete Job',
            <ConfirmationPopup 
                message="Are you sure you want to delete this job?"
                confirmText="Delete"
                isDanger={true}
                onConfirm={async () => {
                    try {
                        await jobApi.deleteJob(jobId);
                        removeLastPopup();
                        fetchOrders();
                    } catch (error) {
                        console.error('Failed to delete job:', error);
                    }
                }}
                onClose={removeLastPopup}
            />
        );
    };

    const handleDeleteOrder = async (orderId) => {
        addPopup(
            'Delete Order',
            <ConfirmationPopup 
                message="Are you sure you want to delete this entire order and all its jobs? This cannot be undone."
                confirmText="Delete"
                isDanger={true}
                onConfirm={async () => {
                    try {
                        await orderApi.deleteOrder(orderId);
                        removeLastPopup();
                        fetchOrders();
                    } catch (error) {
                        console.error('Failed to delete order:', error);
                        alert("Error deleting order");
                    }
                }}
                onClose={removeLastPopup}
            />
        );
    };

    const filteredOrders = orders.map(order => {
        const filteredJobs = order.jobs
            .filter(job => filter === 'all' || job.status === filter)
            .sort((a, b) => new Date(a.startTime) - new Date(b.startTime));

        return { ...order, jobs: filteredJobs };
    }).filter(order => {
        if (filter !== 'all' && order.jobs.length === 0) return false;

        return (order.carName.toLowerCase().includes(search.toLowerCase()) ||
            order.clientName.toLowerCase().includes(search.toLowerCase()) ||
            order.carRegistrationNumber.toLowerCase().includes(search.toLowerCase()));
    });

    const handleContainerClick = () => {
        if (orderId || highlightJob) {
            navigate(mode === 'completed' ? '/done-orders' : '/orders', { replace: true });
        }
    };

    return (
        <main className="main orders-page" onClick={handleContainerClick}>
            <div className="header">
                <input
                    type="text"
                    placeholder={`Search ${mode} orders...`}
                    value={search}
                    onChange={e => setSearch(e.target.value)}
                />
                <Dropdown className="dropdown" value={filter} onChange={e => setFilter(e.target.value)}>
                    <option value="all">All Statuses</option>
                    <option value="pending">Pending</option>
                    <option value="inprogress">In Progress</option>
                    <option value="finished">Finished</option>
                </Dropdown>
                {mode === 'active' && <button className="btn primary" onClick={openNewOrderPopup}>+ New Order</button>}
            </div>

            {loading ? (
                <p>Loading...</p>
            ) : filteredOrders.length === 0 ? (
                <p className="list-empty">No {mode} orders found.</p>
            ) : (
                <div className="orders-list">
                    {filteredOrders.map(order => (
                        <div
                            key={order.id}
                            ref={el => orderRefs.current[order.id] = el}
                            className={`tile order-tile ${(orderId === order.id && !highlightJob) ? 'highlight-outline' : ''}`}
                            onClick={(e) => e.stopPropagation()}
                        >
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
                                <div className="order-actions desktop-only">
                                    <button className="icon-btn btn" onClick={() => openOrderDetailsPopup(order)} title="Order Details">
                                        <i className="fa-solid fa-circle-info"></i>
                                    </button>
                                    <button className="icon-btn btn delete" onClick={() => handleDeleteOrder(order.id)} title="Delete Order">
                                        <i className="fa-solid fa-trash"></i>
                                    </button>
                                </div>
                                <div className="order-actions mobile-only">
                                    <button className="btn secondary" onClick={() => openOrderDetailsPopup(order)}>
                                        <i className="fa-solid fa-circle-info"></i>
                                        Info
                                    </button>
                                    <button className="btn delete" onClick={() => handleDeleteOrder(order.id)}>
                                        <i className="fa-solid fa-trash"></i>
                                        Delete
                                    </button>
                                </div>
                            </div>

                            <div className="table">
                                <div className="table">
                                    {order.jobs.length === 0 ? (
                                        <p className="list-empty">
                                            No jobs added to order
                                        </p>
                                    ) : (
                                        <table>
                                            <thead>
                                                <tr>
                                                    <th>Status</th>
                                                    <th className="hide-md">Time</th>
                                                    <th>Type</th>
                                                    <th className="hide-md">Mechanic</th>
                                                    <th className="hide-sm">Cost</th>
                                                    <th></th>
                                                </tr>
                                            </thead>
                                            <tbody>
                                                {order.jobs.map(job => (
                                                    <tr
                                                        key={job.id}
                                                        ref={el => jobRefs.current[job.id] = el}
                                                        onClick={() => navigate(`/jobs/${job.id}`)}
                                                        className={`clickable ${highlightJob === job.id ? 'highlight-outline' : ''}`}
                                                    >
                                                        <td>
                                                            <span className={`job-status ${job.status}`}>
                                                                <i className={`fa-solid ${job.status === 'pending' ? 'fa-hourglass-start' :
                                                                    job.status === 'inprogress' ? 'fa-screwdriver-wrench' : 'fa-check'
                                                                    } job-status-${job.status} status-icon`}></i>
                                                                <span className="hide-sm">
                                                                    {job.status === 'pending' ? 'Pending' :
                                                                        job.status === 'inprogress' ? 'In Progress' : 'Done'
                                                                    }
                                                                </span>
                                                            </span>
                                                        </td>
                                                        <td className="hide-md">{formatDate(job.startTime)}</td>
                                                        <td>{job.type}</td>
                                                        <td className="hide-md">{job.mechanicName}</td>
                                                        <td className="hide-sm">&euro; {(parseFloat(job.laborCost || 0) + parseFloat(job.partsCost || 0)).toFixed(2)}</td>
                                                        <td onClick={e => e.stopPropagation()}>
                                                            <button className="btn icon-btn delete" onClick={() => handleDeleteJob(order.id, job.id)}>
                                                                <i className="fa-solid fa-trash"></i>
                                                            </button>
                                                        </td>
                                                    </tr>
                                                ))}
                                            </tbody>
                                        </table>
                                    )}
                                </div>
                            </div>
                            {mode === 'active' && (
                                <div className="tile-footer" style={{ borderTop: '1px solid var(--border-color)', paddingTop: '10px', marginTop: '10px' }}>
                                    <button className="btn secondary small" onClick={() => navigate(`/jobs/new?orderId=${order.id}`)}>
                                        + Add Job
                                    </button>
                                </div>
                            )}
                        </div>
                    ))}
                </div>
            )}


        </main>
    );
};

export default OrderList;
