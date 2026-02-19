import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { orderApi } from '../../services/orderApi';
import { partApi } from '../../services/partApi';
import { request } from '../../Utilities/request';
import ServiceForm from './ServiceForm';
import '../../assets/css/job-time-picker.css';
import '../../assets/css/orders.css';

const EditJobPage = ({ mechanicView = false }) => {
    const { orderId: paramOrderId, jobId } = useParams();
    const navigate = useNavigate();
    const isEdit = !!jobId;

    const [job, setJob] = useState(null);
    const [order, setOrder] = useState(null);
    const [jobTypes, setJobTypes] = useState([]);
    const [workers, setWorkers] = useState([]);
    const [allParts, setAllParts] = useState([]);
    const [loading, setLoading] = useState(true);

    // Derived orderId (either from params or fetched job)
    const [fetchedOrderId, setFetchedOrderId] = useState(null);
    const orderId = paramOrderId || fetchedOrderId;

    useEffect(() => {
        const loadData = async () => {
            try {
                // If we don't have orderId in params, we must be in mechanic view /todo/:jobId
                // We need to fetch the job first to get the orderId
                let currentOrderId = paramOrderId;
                let jobData = null;

                if (isEdit) {
                    jobData = await orderApi.getJob(jobId);
                    if (jobData.parts) {
                        jobData.parts = jobData.parts.map(p => ({
                            ...p,
                            name: p.partName
                        }));
                    }
                    if (!currentOrderId) {
                        currentOrderId = jobData.orderId;
                        setFetchedOrderId(currentOrderId);
                    }
                    setJob(jobData);
                } else {
                    // logic for new job, which won't happen in mechanic view usually, but good to keep robust
                    if (!currentOrderId) {
                        // This shouldn't happen based on routes, but if it does, we can't load order
                        console.error("No orderId provided for new job");
                    }
                    setJob({
                        id: 'temp-' + Date.now(),
                        jobTypeId: '',
                        workerId: '',
                        laborCost: 0,
                        startTime: '',
                        endTime: '',
                        description: '',
                        parts: [],
                        status: 0
                    });
                }

                const promises = [
                    request('GET', 'jobtype/all'),
                    request('GET', 'worker/all'),
                    partApi.getAllParts()
                ];

                if (currentOrderId) {
                    promises.push(orderApi.getOrder(currentOrderId));
                }

                const [jtData, workerData, partsData, orderData] = await Promise.all(promises);

                setJobTypes(jtData);
                setWorkers(workerData);
                setAllParts(partsData);
                if (orderData) setOrder(orderData);

            } catch (e) {
                console.error("Failed to load data", e);
                alert("Error loading job details");
            } finally {
                setLoading(false);
            }
        };
        loadData();
    }, [paramOrderId, jobId, isEdit]);

    const updateJob = (sid, field, value) => {
        setJob(prev => ({ ...prev, [field]: value }));
    };

    const handleSave = async () => {
        try {
            const getLocalISO = (d) => {
                const pad = (n) => n.toString().padStart(2, '0');
                if (!d) d = new Date();
                return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:00:00`;
            };

            const payload = {
                jobTypeId: job.jobTypeId,
                workerId: job.workerId,
                laborCost: job.laborCost,
                startTime: job.startTime || getLocalISO(new Date()),
                endTime: job.endTime || getLocalISO(new Date()),
                description: job.description,
                status: job.status,
                parts: job.parts.map(p => ({
                    partId: p.partId,
                    plannedQuantity: p.plannedQuantity,
                    sentQuantity: p.sentQuantity,
                    usedQuantity: p.usedQuantity,
                    requestedQuantity: p.requestedQuantity,
                    price: p.price
                }))
            };

            if (isEdit) {
                await orderApi.updateJob(jobId, payload);
            } else {
                await orderApi.createJob(orderId, payload);
            }
            if (mechanicView) {
                navigate('/todo');
            } else {
                navigate('/orders');
            }
        } catch (e) {
            console.error(e);
            alert("Failed to save job");
        }
    };

    if (loading) return <main className="main"><p>Loading...</p></main>;
    if (!job) return <main className="main"><p>Job not found</p></main>;

    return (
        <main className="main edit-order">
            <div className="header">
                <div className="order-context-info">
                    <h2>{isEdit ? 'Edit Job' : 'Add New Job'}</h2>
                    <p>{order?.clientName} • {order?.carName} ({order?.carRegistrationNumber})</p>
                </div>
                <div style={{ display: 'flex', gap: '10px' }}>
                    <button className="btn secondary" onClick={() => navigate(mechanicView ? '/todo' : '/orders')}>
                        Cancel
                    </button>
                    <button className="btn primary" onClick={handleSave}>
                        Save Job
                    </button>
                </div>
            </div>

            <ServiceForm
                index={0}
                service={job}
                updateService={updateJob}
                removeService={null} // Don't allow removal from here, use orders page
                jobTypes={jobTypes}
                workers={workers}
                allParts={allParts}
                mechanicView={mechanicView}
            />
        </main>
    );
};

export default EditJobPage;
