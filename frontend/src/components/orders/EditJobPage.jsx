import React, { useState, useEffect } from 'react';
import { useNavigate, useParams, useSearchParams } from 'react-router-dom';
import { orderApi } from '../../services/orderApi';
import { jobApi } from '../../services/jobApi';
import { partApi } from '../../services/partApi';
import { request } from '../../Utilities/request';
import ServiceForm from './ServiceForm';
import { parseValidationErrors } from '../../Utilities/formErrors.js';
import { usePopup } from '../../context/PopupContext';
import { useAuth } from '../../context/AuthContext';
import ConfirmationPopup from '../common/ConfirmationPopup';
import '../../assets/css/job-time-picker.css';
import '../../assets/css/orders.css';
import usePageTitle from '../../hooks/usePageTitle';
import ExcelExportButton from '../common/ExcelExportButton';
import PdfExportButton from '../common/PdfExportButton';
import { useStatus } from '../../context/StatusContext.jsx';

const EditJobPage = ({ mechanicView = false }) => {

    const { addPopup, removeLastPopup } = usePopup();
    const { showStatus } = useStatus();
    const { orderId: paramOrderId, jobId } = useParams();
    const [searchParams] = useSearchParams();
    const queryOrderId = searchParams.get('orderId');
    const navigate = useNavigate();
    const isEdit = !!jobId;

    usePageTitle(isEdit ? 'Edit Job' : 'Add New Job');

    const [job, setJob] = useState(null);
    const [order, setOrder] = useState(null);
    const [jobTypes, setJobTypes] = useState([]);
    const [workers, setWorkers] = useState([]);
    const [allParts, setAllParts] = useState([]);
    const [loading, setLoading] = useState(true);
    const [errors, setErrors] = useState({});

    // Derived orderId (either from params, query, or fetched job)
    const [fetchedOrderId, setFetchedOrderId] = useState(null);
    const orderId = paramOrderId || queryOrderId || fetchedOrderId;

    useEffect(() => {
        const loadData = async () => {
            try {
                let currentOrderId = orderId;  // Use the derived orderId (path or query)
                let jobData = null;

                if (isEdit) {
                    // If it's editing, fetch the job data
                    jobData = await jobApi.getJob(jobId);

                    // Map parts if available
                    if (jobData.parts) {
                        jobData.parts = jobData.parts.map(p => ({
                            ...p,
                            name: p.partName
                        }));
                    }

                    // Set the fetched order details from jobData
                    setOrder({
                        clientName: jobData.clientName,
                        carName: jobData.carName,
                        carRegistrationNumber: jobData.carRegistrationNumber
                    });

                    // If no orderId is provided via URL params, use jobData's orderId
                    if (!currentOrderId && jobData.orderId) {
                        currentOrderId = jobData.orderId;
                        setFetchedOrderId(currentOrderId);
                    }

                    setJob(jobData);
                } else {
                    if (!currentOrderId) {
                        console.error("No orderId provided for new job");
                    }
                    setJob({
                        id: 'temp-' + Date.now(),
                        jobTypeId: '',
                        workerId: '',
                        laborCost: 0,
                        startTime: null,
                        endTime: null,
                        description: '',
                        parts: [],
                        status: 0
                    });
                }

                // Fetch other necessary data (job types, workers, parts)
                const promises = [
                    request('GET', 'jobtype/all'),
                    request('GET', 'worker/all'),
                    partApi.getAllParts()
                ];

                // If it's a NEW job, fetch the order details separately
                if (!isEdit && currentOrderId) {
                    promises.push(orderApi.getOrder(currentOrderId));
                }

                const [jtData, workerData, partsData, orderData] = await Promise.all(promises);

                // Set the fetched data to the state
                setJobTypes(jtData);
                setWorkers(workerData);
                setAllParts(partsData);

                // If order data was fetched (for new jobs), set it
                if (orderData) setOrder(orderData);

            } catch (error) {
                console.error("Failed to load data", error);
                showStatus(error.message || "Error loading job details", 'error');
            } finally {
                setLoading(false);
            }
        };

        loadData();
    }, [paramOrderId, queryOrderId, jobId, isEdit]);
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

            const parseNum = (val) => (val === '' || val === null || val === undefined) ? null : Number(val);

            const payload = {
                jobTypeId: job.jobTypeId,
                workerId: job.workerId,
                laborCost: parseNum(job.laborCost),
                startTime: job.startTime || null,
                endTime: job.endTime || null,
                description: job.description,
                status: job.status,
                parts: job.parts.map(p => ({
                    partId: p.partId,
                    partName: p.name,
                    plannedQuantity: parseNum(p.plannedQuantity),
                    sentQuantity: parseNum(p.sentQuantity),
                    usedQuantity: parseNum(p.usedQuantity),
                    requestedQuantity: parseNum(p.requestedQuantity),
                    price: p.price
                }))
            };

            showStatus('Saving job...', 'loading');
            if (isEdit) {
                await jobApi.updateJob(jobId, payload);
            } else {
                await jobApi.createJob(orderId, payload);
            }

            // Trigger notification refresh in header
            window.dispatchEvent(new CustomEvent('refresh-notifications'));

            navigate(-1);
            showStatus('Job saved successfully', 'success');
        } catch (error) {
            console.error(error);
            const parsedErrors = parseValidationErrors(error);
            setErrors(parsedErrors);
            showStatus(parsedErrors.general || error.message || 'Failed to save job', 'error');
        }
    };

    const handleDelete = async () => {
        addPopup(
            'Delete Job',
            <ConfirmationPopup
                message="Are you sure you want to delete this job?"
                confirmText="Delete"
                isDanger={true}
                onConfirm={async () => {
                    showStatus('Deleting job...', 'loading');
                    try {
                        await jobApi.deleteJob(jobId);
                        window.dispatchEvent(new CustomEvent('refresh-notifications'));
                        removeLastPopup();
                        navigate(-1);
                        showStatus('Job deleted successfully', 'success');
                    } catch (error) {
                        console.error("Failed to delete job", error);
                        showStatus(error.message || 'Failed to delete job', 'error');
                    }
                }}
                onClose={removeLastPopup}
            />
        );
    };

    if (loading) return <main className="main"><p>Loading...</p></main>;
    if (!job) return <main className="main"><p>Job not found</p></main>;

    return (
        <main className="main edit-order">
            <div className="section-header order-header">
                <div className="order-context-info">
                    <h2>{isEdit ? 'Edit Job' : 'Add New Job'}</h2>
                    <p>{order?.clientName} • {order?.carName} ({order?.carRegistrationNumber})</p>
                </div>
                <div className="order-actions">
                    <button className="btn secondary" onClick={() => navigate(-1)}>
                        Cancel
                    </button>
                    {isEdit && !mechanicView && job?.status !== 2 && (
                        <button className="btn delete" onClick={handleDelete}>
                            Delete
                        </button>
                    )}
                    <button className="btn primary" onClick={handleSave}>
                        Save Job
                    </button>
                    {isEdit && <ExcelExportButton endpoint={`export/job/${jobId}`} />}
                    {isEdit && <PdfExportButton endpoint={`export/job/${jobId}`} />}
                </div>
            </div>

            <ServiceForm
                index={0}
                service={job}
                updateService={updateJob}
                removeService={null}
                jobTypes={jobTypes}
                workers={workers}
                allParts={allParts}
                mechanicView={mechanicView}
                errors={errors}
            />
        </main>
    );
};

export default EditJobPage;
