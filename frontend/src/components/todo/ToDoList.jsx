import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';

const ToDoList = ({ jobs }) => {
    const navigate = useNavigate();
    const { accesses } = useAuth();
    const hasOrdersAccess = accesses.includes('Orders');

    const handleRowClick = (job) => {
        if (hasOrdersAccess) {
            navigate(`/orders/${job.orderId}/jobs/${job.id}`);
        } else {
            navigate(`/todo/${job.id}`);
        }
    };

    // Group jobs by date (ignoring time)
    const groupedJobs = jobs.reduce((acc, job) => {
        const date = new Date(job.startTime).toLocaleDateString(undefined, {
            weekday: 'long',
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        });
        if (!acc[date]) acc[date] = [];
        acc[date].push(job);
        return acc;
    }, {});

    return (
        <div className="daily-groups">
            {Object.entries(groupedJobs).map(([date, dayJobs]) => (
                <div key={date} className="day-group">
                    <div className="tile">
                        <div className="tile-header">
                            <h3>{date}</h3>
                            <span>{dayJobs.length} {dayJobs.length === 1 ? 'task' : 'tasks'}</span>
                        </div>
                        <div className="table" style={{ overflowX: 'auto' }}>
                            <table>
                                <thead>
                                    <tr>
                                        <th>Status</th>
                                        <th>Type</th>
                                        <th className="hide-sm">Car</th>
                                        <th className="hide-md">Description</th>
                                        <th>Time</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {dayJobs.map(job => (
                                        <tr key={job.id} onClick={() => handleRowClick(job)}>
                                            <td>
                                                <span className={`job-status ${job.status}`}>
                                                    <i className={`fa-solid ${job.status === 'pending' ? 'fa-hourglass-start' :
                                                        job.status === 'inprogress' ? 'fa-screwdriver-wrench' : 'fa-check'
                                                        } job-status-${job.status} status-icon`}></i>
                                                    <span className="hide-sm">
                                                        {job.status === 'pending' ? ' Pending' : job.status === 'inprogress' ? ' In Progress' : ' Done'}
                                                    </span>
                                                </span>
                                            </td>
                                            <td>{job.typeName}</td>
                                            <td className="hide-sm">
                                                <div>{job.carName}</div>
                                                <div className="car-reg">{job.carRegistrationNumber}</div>
                                            </td>
                                            <td className="hide-md">{job.description}</td>
                                            <td className="job-time-meta">{new Date(job.startTime).toLocaleTimeString(undefined, { hour: '2-digit', minute: '2-digit' })}</td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                    </div>
                </div>
            ))}
        </div>
    );
};

export default ToDoList;
