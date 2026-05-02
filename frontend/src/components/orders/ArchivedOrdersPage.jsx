import React, { useState, useEffect, useRef } from 'react';
import { useNavigate, useSearchParams, useParams } from 'react-router-dom';
import { orderApi } from '../../services/orderApi';
import { jobApi } from '../../services/jobApi';
import { request } from '../../Utilities/request';
import { usePopup } from '../../context/PopupContext';
import OrderDetailsPopup from './OrderDetailsPopup';
import '../../assets/css/orders.css';
import usePageTitle from '../../hooks/usePageTitle';
import ExcelExportButton from '../common/ExcelExportButton';
import PdfExportButton from '../common/PdfExportButton';

const ArchivedOrdersPage = () => {
    usePageTitle('Archived Orders');
    const navigate = useNavigate();
    const { orderId } = useParams();
    const [searchParams] = useSearchParams();
    const { addPopup, removeLastPopup } = usePopup();
    const [orders, setOrders] = useState([]);
    const [cars, setCars] = useState([]);
    const [search, setSearch] = useState('');
    const [loading, setLoading] = useState(true);
    const orderRefs = useRef({});
    const jobRefs = useRef({});
    const highlightJob = searchParams.get('highlightJob');
    const highlight = searchParams.get('highlight') === 'true';

    useEffect(() => {
        fetchOrders();
        fetchCars();
    }, []);

    const fetchOrders = async () => {
        setLoading(true);
        try {
            const data = await orderApi.getArchivedOrders();
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

    const handleClosePopup = () => {
        removeLastPopup();
        navigate('/archived-orders');
    };

    const openOrderDetailsPopup = (order) => {
        addPopup(
            'Order Details',
            <OrderDetailsPopup
                order={order}
                cars={cars}
                onClose={handleClosePopup}
                onSave={() => { }}
                errors={{}}
            />,
            false,
            () => navigate('/archived-orders')
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
                if (order) {
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

    const formatDate = (input) => {
        if (!input) return '';
        const date = new Date(input);
        const day = date.getDate().toString().padStart(2, '0');
        const month = (date.getMonth() + 1).toString().padStart(2, '0');
        const hours = date.getHours().toString().padStart(2, '0');
        const minutes = date.getMinutes().toString().padStart(2, '0');
        return `${day}/${month} ${hours}:${minutes}`;
    };

    const filteredOrders = orders.map(order => {
        const filteredJobs = order.jobs
            .sort((a, b) => new Date(a.startTime) - new Date(b.startTime));
        return { ...order, jobs: filteredJobs };
    }).filter(order => {
        return (order.carName.toLowerCase().includes(search.toLowerCase()) ||
            order.clientName.toLowerCase().includes(search.toLowerCase()) ||
            order.carRegistrationNumber.toLowerCase().includes(search.toLowerCase()));
    });

    const handleContainerClick = () => {
        if (orderId || highlightJob) {
            navigate('/archived-orders', { replace: true });
        }
    };



    return (
        <main className="main orders-page" onClick={handleContainerClick}>
            <div className="header">
                <input
                    type="text"
                    placeholder="Search archived orders..."
                    value={search}
                    onChange={e => setSearch(e.target.value)}
                />
                <div style={{ display: 'flex', gap: '5px' }}>
                    <ExcelExportButton endpoint="export/orders?isArchived=true" />
                    <PdfExportButton endpoint="export/orders?isArchived=true" />
                </div>
            </div>

            {loading ? (
                <p>Loading...</p>
            ) : filteredOrders.length === 0 ? (
                <p className="list-empty">No archived orders found.</p>
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
                                    </div>
                                    <div className="order-car-details">
                                        {order.carName} • {order.carRegistrationNumber} • {order.kilometers} km
                                    </div>
                                </div>
                                <div className="order-actions desktop-only">
                                    <button className="icon-btn btn" onClick={() => openOrderDetailsPopup(order)} title="Order Details">
                                        <i className="fa-solid fa-circle-info"></i>
                                    </button>
                                </div>
                                <div className="order-actions mobile-only">
                                    <button className="btn secondary" onClick={() => openOrderDetailsPopup(order)}>
                                        <i className="fa-solid fa-circle-info"></i>
                                        Info
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
                                                    <th className="hide-md">Time</th>
                                                    <th>Type</th>
                                                    <th className="hide-md">Mechanic</th>
                                                    <th className="hide-sm">Cost</th>
                                                </tr>
                                            </thead>
                                            <tbody>
                                                {order.jobs.map(job => (
                                                    <tr
                                                        key={job.id}
                                                        ref={el => jobRefs.current[job.id] = el}
                                                        onClick={() => navigate(`/archived-jobs/${job.id}`)}
                                                        className={`clickable ${highlightJob === job.id ? 'highlight-outline' : ''}`}
                                                    >
                                                        <td className="hide-md">{formatDate(job.startTime)}</td>
                                                        <td>{job.type}</td>
                                                        <td className="hide-md">{job.mechanicName}</td>
                                                        <td className="hide-sm">&euro; {(parseFloat(job.laborCost || 0) + parseFloat(job.partsCost || 0)).toFixed(2)}</td>
                                                    </tr>
                                                ))}
                                            </tbody>
                                        </table>
                                    )}
                                </div>
                            </div>
                        </div>
                    ))}
                </div>
            )}
        </main>
    );
};

export default ArchivedOrdersPage;
