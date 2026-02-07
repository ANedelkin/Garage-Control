import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { orderApi } from '../../services/orderApi';
import Dropdown from '../common/Dropdown';
import '../../assets/css/common/status.css';
import '../../assets/css/common/tile.css';
import '../../assets/css/orders.css';

const ToDoPage = () => {
    const navigate = useNavigate();
    const [jobs, setJobs] = useState([]);
    const [viewMode, setViewMode] = useState('list'); // 'list' or 'calendar'
    const [loading, setLoading] = useState(true);

    // Calendar state
    const today = new Date();
    const [currentMonth, setCurrentMonth] = useState(today.getMonth());
    const [currentYear, setCurrentYear] = useState(today.getFullYear());

    useEffect(() => {
        fetchJobs();
    }, []);

    const fetchJobs = async () => {
        try {
            const data = await orderApi.getMyJobs();
            setJobs(data);
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

    const renderList = () => (
        <div className="tile" style={{ marginTop: '20px' }}>
            <div className="table">
                <table>
                    <thead>
                        <tr>
                            <th>Status</th>
                            <th>Type</th>
                            <th>Car</th>
                            <th>Description</th>
                            <th>Start Time</th>
                            <th></th>
                        </tr>
                    </thead>
                    <tbody>
                        {jobs.map(job => (
                            <tr key={job.id} onClick={() => navigate(`/orders/${job.orderId}`)}>
                                <td>
                                    <span className={`job-status ${job.status}`}>
                                        <i className={`fa-solid ${job.status === 0 ? 'fa-clock' :
                                            job.status === 1 ? 'fa-hourglass-start' :
                                            job.status === 2 ? 'fa-screwdriver-wrench' : 'fa-check'
                                            } job-status-${job.status} status-icon`}></i>
                                        {job.status === 0 ? 'Awaiting Parts' : job.status === 1 ? 'Pending' : job.status === 2 ? 'In Progress' : 'Done'}
                                    </span>
                                </td>
                                <td>{job.typeName}</td>
                                <td>
                                    <div>{job.carName}</div>
                                    <div style={{ fontSize: '0.8em', color: 'var(--text-secondary)' }}>{job.carRegistrationNumber}</div>
                                </td>
                                <td>{job.description}</td>
                                <td>{formatDate(job.startTime)}</td>
                                <td><i className="fa-solid fa-chevron-right"></i></td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
        </div>
    );

    const renderCalendar = () => {
        const daysInMonth = new Date(currentYear, currentMonth + 1, 0).getDate();
        const firstDayOfMonth = new Date(currentYear, currentMonth, 1).getDay(); // 0 is Sunday

        const monthNames = ["January", "February", "March", "April", "May", "June",
            "July", "August", "September", "October", "November", "December"
        ];

        const days = [];
        // Empty slots for previous month days
        for (let i = 0; i < firstDayOfMonth; i++) {
            days.push(<div key={`empty-${i}`} className="calendar-day empty"></div>);
        }

        for (let day = 1; day <= daysInMonth; day++) {
            const dayJobs = jobs.filter(j => {
                const jDate = new Date(j.startTime);
                return jDate.getDate() === day && jDate.getMonth() === currentMonth && jDate.getFullYear() === currentYear;
            });

            days.push(
                <div key={day} className="calendar-day">
                    <div className="day-number">{day}</div>
                    <div className="day-events">
                        {dayJobs.map(j => (
                            <div key={j.id} className={`event-dot status-${j.status}`} title={`${j.typeName}`} onClick={() => navigate(`/orders/${j.orderId}`)}>
                                {j.typeName}
                            </div>
                        ))}
                    </div>
                </div>
            );
        }

        return (
            <div className="calendar-container tile">
                <div className="calendar-header">
                    <button className="btn icon-btn" onClick={() => {
                        if (currentMonth === 0) { setCurrentMonth(11); setCurrentYear(currentYear - 1); }
                        else { setCurrentMonth(currentMonth - 1); }
                    }}>{"<"}</button>
                    <h3 style={{ margin: 0 }}>{monthNames[currentMonth]} {currentYear}</h3>
                    <button className="btn icon-btn" onClick={() => {
                        if (currentMonth === 11) { setCurrentMonth(0); setCurrentYear(currentYear + 1); }
                        else { setCurrentMonth(currentMonth + 1); }
                    }}>{">"}</button>
                </div>
                <div className="calendar-grid">
                    {['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'].map(d => <div key={d} className="calendar-head">{d}</div>)}
                    {days}
                </div>
                <style>{`
                   .calendar-container {
                       margin-top: 20px;
                   }
                   .calendar-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
                   .calendar-grid { display: grid; grid-template-columns: repeat(7, 1fr); gap: 10px; }
                   .calendar-head { font-weight: bold; text-align: center; margin-bottom: 10px; color: var(--text-secondary); }
                   .calendar-day {
                       border: 1px solid var(--border2);
                       min-height: 100px;
                       padding: 8px;
                       border-radius: 8px;
                       background: var(--bg-secondary);
                       display: flex;
                       flex-direction: column;
                   }
                   .calendar-day.empty { background: transparent; border: none; }
                   .day-number { font-weight: bold; margin-bottom: 5px; align-self: flex-end; color: var(--text-secondary); }
                   .day-events { display: flex; flex-direction: column; gap: 4px; flex: 1; }
                   .event-dot {
                       font-size: 0.75em;
                       padding: 4px 6px;
                       border-radius: 4px;
                       background: var(--primary);
                       color: white;
                       cursor: pointer;
                       white-space: nowrap;
                       overflow: hidden;
                       text-overflow: ellipsis;
                   }
                   .event-dot.status-0 { background: #f0ad4e; color: black; }
                   .event-dot.status-1 { background: #337ab7; }
                   .event-dot.status-2 { background: #5cb85c; }
               `}</style>
            </div>
        );
    };

    return (
        <main className="main">
            <div className="header">
                <h1>My Jobs</h1>
                <Dropdown value={viewMode} onChange={e => setViewMode(e.target.value)}>
                    <option value="list">List View</option>
                    <option value="calendar">Calendar View</option>
                </Dropdown>
            </div>

            {loading ? <p>Loading...</p> : (
                jobs.length === 0 ? <p className="list-empty">No jobs assigned.</p> : (
                    viewMode === 'list' ? renderList() : renderCalendar()
                )
            )}
        </main>
    );
};

export default ToDoPage;
