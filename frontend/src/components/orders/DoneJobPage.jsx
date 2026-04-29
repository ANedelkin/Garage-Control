import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { jobApi } from '../../services/jobApi';
import usePageTitle from '../../hooks/usePageTitle';
import '../../assets/css/orders.css';
import ExcelExportButton from '../common/ExcelExportButton';
import PdfExportButton from '../common/PdfExportButton';

const DoneJobPage = () => {
    const { jobId } = useParams();
    const navigate = useNavigate();
    usePageTitle('View Done Job');

    const [job, setJob] = useState(null);
    const [loading, setLoading] = useState(true);
    const [expandedParts, setExpandedParts] = useState({});

    useEffect(() => {
        const loadData = async () => {
            try {
                const jobData = await jobApi.getCompletedJob(jobId);
                setJob(jobData);
            } catch (e) {
                console.error("Failed to load data", e);
                alert("Error loading job details");
            } finally {
                setLoading(false);
            }
        };

        loadData();
    }, [jobId]);

    const formatDate = (input) => {
        if (!input) return '';
        const date = new Date(input);
        const day = date.getDate().toString().padStart(2, '0');
        const month = (date.getMonth() + 1).toString().padStart(2, '0');
        const hours = date.getHours().toString().padStart(2, '0');
        const minutes = date.getMinutes().toString().padStart(2, '0');
        return `${day}/${month} ${hours}:${minutes}`;
    };

    const togglePartExpand = (index) => {
        setExpandedParts(prev => ({
            ...prev,
            [index]: !prev[index]
        }));
    };

    if (loading) return <main className="main"><p>Loading...</p></main>;
    if (!job) return <main className="main"><p>Job not found</p></main>;



    return (
        <main className="main edit-order">
            <div className="section-header order-header">
                <div className="order-context-info">
                    <h2>View Done Job</h2>
                    <p>{job.clientName} • {job.carName} ({job.carRegistrationNumber})</p>
                </div>
                <div className="order-actions">
                    <button className="btn secondary" onClick={() => navigate(-1)}>
                        Back
                    </button>
                    <ExcelExportButton endpoint={`export/job/${jobId}`} />
                    <PdfExportButton endpoint={`export/job/${jobId}`} />
                </div>
            </div>

            <div className="tile">
                <div className="service-form">
                    <div className="form-row-4">
                        <div className="form-section">
                            <label>Job Type</label>
                            <input type="text" value={job.jobTypeName || ''} readOnly disabled />
                        </div>
                        <div className="form-section">
                            <label>Mechanic</label>
                            <input type="text" value={job.mechanicName || ''} readOnly disabled />
                        </div>
                        <div className="form-section">
                            <label>Labor Cost</label>
                            <input type="text" value={`€ ${parseFloat(job.laborCost || 0).toFixed(2)}`} readOnly disabled />
                        </div>
                        <div className="form-section">
                            <label>Time</label>
                            <input type="text" value={`${formatDate(job.startTime)} - ${formatDate(job.endTime)}`} readOnly disabled />
                        </div>
                    </div>

                    <div className="form-section">
                        <label>Description</label>
                        <textarea
                            className="description"
                            value={job.description || ''}
                            readOnly
                            disabled
                        />
                    </div>
                </div>

                <div className="parts-table-wrapper parts-done-table">
                    <label>Parts Used</label>
                    <table className="table">
                        <thead>
                            <tr>
                                <th>Part Name</th>
                                <th style={{ width: '120px' }}>Used Qty</th>
                                <th style={{ width: '120px' }}>Unit Price</th>
                                <th style={{ width: '120px' }}>Total</th>
                            </tr>
                        </thead>
                        <tbody>
                            {job.parts.map((p, i) => {
                                const isExpanded = !!expandedParts[i];
                                return (
                                    <tr key={i} className={isExpanded ? 'expanded-row' : ''}>
                                        <td>
                                            <div className="mobile-part-header">
                                                <button
                                                    type="button"
                                                    className="btn icon-btn done-expand-btn expand-btn"
                                                    onClick={() => togglePartExpand(i)}
                                                >
                                                    <i className={`fa-solid ${isExpanded ? 'fa-chevron-up' : 'fa-chevron-down'}`}></i>
                                                </button>
                                                <div style={{ flex: 1 }}>{p.partName}</div>
                                            </div>
                                        </td>
                                        <td className="mobile-collapsible" data-label="Used Qty">{p.usedQuantity}</td>
                                        <td className="mobile-collapsible" data-label="Unit Price">€ {parseFloat(p.price || 0).toFixed(2)}</td>
                                        <td className="mobile-collapsible" data-label="Total">€ {(parseFloat(p.usedQuantity || 0) * parseFloat(p.price || 0)).toFixed(2)}</td>
                                    </tr>
                                );
                            })}
                        </tbody>
                    </table>
                </div>

                <div className="form-footer" style={{ justifyContent: 'flex-end', padding: '15px 0' }}>
                    <div style={{ textAlign: 'right', fontWeight: 'bold', fontSize: '1.2rem' }}>
                        Total Job Cost: € {(parseFloat(job.laborCost || 0) + job.parts.reduce((sum, p) => sum + (p.usedQuantity * p.price), 0)).toFixed(2)}
                    </div>
                </div>
            </div>
        </main>
    );
};

export default DoneJobPage;
