import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { jobApi } from '../../services/jobApi';
import { workerApi } from '../../services/workerApi';
import { useAuth } from '../../context/AuthContext';
import Dropdown from '../common/Dropdown';
import '../../assets/css/common/status.css';
import '../../assets/css/common/tile.css';
import '../../assets/css/orders.css';

const ToDoPage = () => {
    const navigate = useNavigate();
    const [jobs, setJobs] = useState([]);
    const [viewMode, setViewMode] = useState(localStorage.getItem('myJobsViewMode') || 'list'); // 'list' or 'calendar'
    const [loading, setLoading] = useState(true);
    const { user } = useAuth();
    const [worker, setWorker] = useState(null);

    // Common navigation state for calendar/week
    const today = new Date();
    const [currentMonth, setCurrentMonth] = useState(today.getMonth());
    const [currentYear, setCurrentYear] = useState(today.getFullYear());
    const [viewDate, setViewDate] = useState(new Date());

    useEffect(() => {
        fetchJobs();
        if (user?.workerId) {
            fetchWorker();
        }
    }, [user?.workerId]);

    const fetchWorker = async () => {
        try {
            const data = await workerApi.getWorker(user.workerId);
            setWorker(data);
        } catch (error) {
            console.error("Failed to fetch worker details:", error);
        }
    };

    const fetchJobs = async () => {
        try {
            const data = await jobApi.getMyJobs();
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

    const renderList = () => {
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
            <div className="daily-groups" style={{ display: 'flex', flexDirection: 'column', gap: '30px' }}>
                {Object.entries(groupedJobs).map(([date, dayJobs]) => (
                    <div key={date} className="day-group">
                        <div className="tile">
                            <div className="tile-header">
                                <h3 style={{ margin: 0 }}>{date}</h3>
                                <span style={{ color: 'var(--text-secondary)', fontSize: '0.9rem' }}>{dayJobs.length} {dayJobs.length === 1 ? 'task' : 'tasks'}</span>
                            </div>
                            <div className="table">
                                <table>
                                    <thead>
                                        <tr>
                                            <th>Status</th>
                                            <th>Type</th>
                                            <th>Car</th>
                                            <th>Description</th>
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
                                                        {job.status === 'pending' ? 'Pending' : job.status === 'inprogress' ? 'In Progress' : 'Done'}
                                                    </span>
                                                </td>
                                                <td>{job.typeName}</td>
                                                <td>
                                                    <div>{job.carName}</div>
                                                    <div style={{ fontSize: '0.8em', color: 'var(--text-secondary)' }}>{job.carRegistrationNumber}</div>
                                                </td>
                                                <td>{job.description}</td>
                                                <td style={{ fontWeight: 600 }}>{new Date(job.startTime).toLocaleTimeString(undefined, { hour: '2-digit', minute: '2-digit' })}</td>
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

    const renderCalendar = () => {
        const { accesses } = useAuth();
        const hasOrdersAccess = accesses.includes('Orders');

        const daysInMonth = new Date(currentYear, currentMonth + 1, 0).getDate();
        const firstDayOfMonth = new Date(currentYear, currentMonth, 1).getDay(); // 0 is Sunday

        const monthNames = ["January", "February", "March", "April", "May", "June",
            "July", "August", "September", "October", "November", "December"
        ];

        const handleEventClick = (job) => {
            if (hasOrdersAccess) {
                navigate(`/orders/${job.orderId}/jobs/${job.id}`);
            } else {
                navigate(`/todo/${job.id}`);
            }
        };

        const rows = [];
        let cells = [];

        // Add empty cells for days from the previous month
        for (let i = 0; i < firstDayOfMonth; i++) {
            cells.push(<td key={`empty-${i}`} className="calendar-cell empty"></td>);
        }

        // Add cells for the current month
        for (let day = 1; day <= daysInMonth; day++) {
            const dayJobs = jobs.filter(j => {
                const jDate = new Date(j.startTime);
                return jDate.getDate() === day && jDate.getMonth() === currentMonth && jDate.getFullYear() === currentYear;
            });

            cells.push(
                <td key={day} className="calendar-cell">
                    <div className="day-header">
                        <span className="day-number">{day}</span>
                        {dayJobs.length > 0 && <span className="day-count">{dayJobs.length} tasks</span>}
                    </div>
                    <div className="day-events">
                        {dayJobs.map(j => (
                            <div
                                key={j.id}
                                className={`calendar-event status-${j.status}`}
                                title={`${j.typeName}`}
                                onClick={() => handleEventClick(j)}
                            >
                                <span className="event-time">{new Date(j.startTime).getHours().toString().padStart(2, '0')}:00</span>
                                <span className="event-name">{j.typeName}</span>
                            </div>
                        ))}
                    </div>
                </td>
            );

            if (cells.length === 7) {
                rows.push(<tr key={`row-${rows.length}`}>{cells}</tr>);
                cells = [];
            }
        }

        // Fill remaining cells in the last row
        if (cells.length > 0) {
            while (cells.length < 7) {
                cells.push(<td key={`empty-end-${cells.length}`} className="calendar-cell empty"></td>);
            }
            rows.push(<tr key={`row-${rows.length}`}>{cells}</tr>);
        }

        return (
            <div className="calendar-workspace tile">
                <div className="calendar-controls">
                    <button className="btn icon-btn" onClick={() => {
                        if (currentMonth === 0) { setCurrentMonth(11); setCurrentYear(currentYear - 1); }
                        else { setCurrentMonth(currentMonth - 1); }
                    }}><i className="fa-solid fa-chevron-left"></i></button>
                    <h3 className="calendar-title">{monthNames[currentMonth]} {currentYear}</h3>
                    <button className="btn icon-btn" onClick={() => {
                        if (currentMonth === 11) { setCurrentMonth(0); setCurrentYear(currentYear + 1); }
                        else { setCurrentMonth(currentMonth + 1); }
                    }}><i className="fa-solid fa-chevron-right"></i></button>
                </div>

                <table className="calendar-table">
                    <thead>
                        <tr>
                            {['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'].map(d => (
                                <th key={d} className="calendar-th">{d}</th>
                            ))}
                        </tr>
                    </thead>
                    <tbody>
                        {rows}
                    </tbody>
                </table>

                <style>{`
                    .calendar-workspace {
                        margin-top: 20px;
                        flex: 1;
                        display: flex;
                        flex-direction: column;
                        padding: 24px !important;
                    }
                    .calendar-controls { 
                        display: flex; 
                        justify-content: center; 
                        align-items: center; 
                        gap: 20px;
                    }
                    .calendar-title {
                        font-size: 1.5rem;
                        font-weight: 600;
                        min-width: 200px;
                        text-align: center;
                        margin: 0;
                    }
                    .calendar-table {
                        width: 100%;
                        border-collapse: collapse;
                        table-layout: fixed;
                        flex: 1;
                    }
                    .calendar-table tbody {
                        background: var(--solid3);
                    }
                    .calendar-th {
                        padding: 12px;
                        text-align: center;
                        font-weight: 600;
                        color: var(--text-clr2);
                        border-bottom: 1px solid var(--border2);
                    }
                    .calendar-cell {
                        border: 1px solid var(--border2);
                        vertical-align: top;
                        height: 120px;
                        padding: 10px;
                        transition: background 0.2s;
                    }
                    .calendar-cell:hover {
                        background: var(--solid4) !important;
                    }
                    .calendar-table tr:hover {
                        background: transparent !important;
                    }
                    .calendar-cell.empty {
                        background: rgba(255, 255, 255, 0.02);
                    }
                    .day-header {
                        display: flex;
                        justify-content: space-between;
                        align-items: center;
                        margin-bottom: 8px;
                    }
                    .day-number {
                        font-weight: 600;
                        font-size: 0.6rem;
                        color: var(--text-clr2);
                    }
                    .day-count {
                        font-size: 0.5rem;
                        color: var(--text-secondary);
                        background: var(--solid3);
                        padding: 2px 6px;
                        border-radius: 10px;
                        opacity: 0.8;
                    }
                    .day-events {
                        display: flex;
                        flex-direction: column;
                        gap: 4px;
                        max-height: 80px;
                        overflow-y: auto;
                        padding-right: 2px;
                    }
                    .day-events::-webkit-scrollbar {
                        width: 3px;
                    }
                    .day-events::-webkit-scrollbar-thumb {
                        background: var(--border2);
                        border-radius: 3px;
                    }
                    .calendar-event {
                        font-size: 0.75rem;
                        padding: 4px 8px;
                        border-radius: 6px;
                        cursor: pointer;
                        display: flex;
                        gap: 6px;
                        align-items: center;
                        transition: transform 0.1s;
                        border: 1px solid transparent;
                    }
                    .calendar-event:hover {
                        transform: translateY(-1px);
                        filter: brightness(1.1);
                        border-color: rgba(255, 255, 255, 0.2);
                    }
                    .event-time {
                        opacity: 0.8;
                        font-weight: 600;
                        font-family: monospace;
                    }
                    .event-name {
                        overflow: hidden;
                        text-overflow: ellipsis;
                        white-space: nowrap;
                    }
                    .calendar-event.status-pending { background: rgba(240, 173, 78, 0.2); border-left: 3px solid #f0ad4e; color: #f0ad4e; }
                    .calendar-event.status-inprogress { background: rgba(51, 122, 183, 0.2); border-left: 3px solid #337ab7; color: #337ab7; }
                    .calendar-event.status-done { background: rgba(92, 184, 92, 0.2); border-left: 3px solid #5cb85c; color: #5cb85c; }
                    
                    @media (max-width: 1000px) {
                        .calendar-cell { height: 80px; padding: 5px; }
                        .event-time { display: none; }
                    }
                `}</style>
                <style>{`
                    .main.calendar-layout {
                        padding-left: 2%;
                        padding-right: 2%;
                        max-width: none;
                    }
                    .main.calendar-layout .header h1 {
                        margin: 0;
                    }
                `}</style>
            </div>
        );
    };

    const renderWeekView = () => {
        if (!worker) return <p>Loading worker schedule...</p>;

        const { accesses } = useAuth();
        const hasOrdersAccess = accesses.includes('Orders');

        const handleEventClick = (job) => {
            if (hasOrdersAccess) {
                navigate(`/orders/${job.orderId}/jobs/${job.id}`);
            } else {
                navigate(`/todo/${job.id}`);
            }
        };

        const getDayOfWeek = (date) => {
            const day = date.getDay(); // 0 is Sun
            return day === 0 ? 6 : day - 1;
        };

        const startOfWeek = new Date(viewDate);
        startOfWeek.setHours(0, 0, 0, 0);

        const dayColumns = [];
        for (let i = 0; i < 7; i++) {
            const d = new Date(startOfWeek);
            d.setDate(d.getDate() + i);
            dayColumns.push(d);
        }

        const isOnLeave = (date) => {
            if (!worker.leaves) return false;
            return worker.leaves.some(l => {
                const start = new Date(l.startDate);
                const end = new Date(l.endDate);
                const d = new Date(date);
                d.setHours(0, 0, 0, 0);
                start.setHours(0, 0, 0, 0);
                end.setHours(0, 0, 0, 0);
                return d >= start && d <= end;
            });
        };

        const isWorking = (dayIndex, hour) => {
            if (!worker.schedules) return false;
            return worker.schedules.some(s => {
                if (s.dayOfWeek !== dayIndex) return false;
                const [startH] = s.startTime.split(':').map(Number);
                const [endH] = s.endTime.split(':').map(Number);
                return hour >= startH && hour < endH;
            });
        };

        // Determine visible hours (union of all working hours in the schedule)
        const visibleHours = Array.from({ length: 24 }, (_, i) => i).filter(h => {
            return worker.schedules?.some(s => {
                const [startH] = s.startTime.split(':').map(Number);
                const [endH] = s.endTime.split(':').map(Number);
                return h >= startH && h < endH;
            });
        });

        const handleWeekChange = (offset) => {
            const newDate = new Date(viewDate);
            newDate.setDate(newDate.getDate() + offset * 7);
            setViewDate(newDate);
        };

        return (
            <div className="week-workspace tile">
                <div className="calendar-controls">
                    <button className="btn icon-btn" onClick={() => handleWeekChange(-1)}>
                        <i className="fa-solid fa-chevron-left"></i>
                    </button>
                    <h3 className="calendar-title">
                        {dayColumns[0].toLocaleDateString(undefined, { month: 'short', day: 'numeric' })} - {dayColumns[6].toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' })}
                    </h3>
                    <button className="btn icon-btn" onClick={() => handleWeekChange(1)}>
                        <i className="fa-solid fa-chevron-right"></i>
                    </button>
                </div>

                <div className="week-table-wrapper">
                    <table className="week-table">
                        <thead>
                            <tr>
                                <th className="hour-col"></th>
                                {dayColumns.map((day, idx) => (
                                    <th key={idx}>
                                        <div className="day-weekday">{day.toLocaleDateString(undefined, { weekday: 'short' })}</div>
                                        <div className="day-date">{day.getDate()}</div>
                                    </th>
                                ))}
                            </tr>
                        </thead>
                        <tbody>
                            {visibleHours.map(h => (
                                <tr key={h}>
                                    <td className="hour-label">{h.toString().padStart(2, '0')}:00</td>
                                    {dayColumns.map((day, dayIdx) => {
                                        const dow = getDayOfWeek(day);
                                        const working = isWorking(dow, h);
                                        const leave = isOnLeave(day);
                                        const isActuallyWorking = working && !leave;

                                        // Find jobs starting at this hour
                                        const hourJobs = jobs.filter(j => {
                                            const jStart = new Date(j.startTime);
                                            return jStart.getDate() === day.getDate() &&
                                                jStart.getMonth() === day.getMonth() &&
                                                jStart.getFullYear() === day.getFullYear() &&
                                                jStart.getHours() === h;
                                        });

                                        return (
                                            <td
                                                key={dayIdx}
                                                className={`week-cell ${isActuallyWorking ? 'working' : 'transparent'}`}
                                            >
                                                {hourJobs.map(j => {
                                                    const jStart = new Date(j.startTime);

                                                    // Ensure jEnd is correctly parsed or defaulted
                                                    let jEnd;
                                                    if (j.endTime) {
                                                        jEnd = new Date(j.endTime);
                                                    } else {
                                                        jEnd = new Date(jStart);
                                                        jEnd.setHours(jEnd.getHours() + 1);
                                                    }

                                                    const durationHours = Math.max(1, Math.ceil((jEnd - jStart) / 3600000));

                                                    return (
                                                        <div
                                                            key={j.id}
                                                            className={`week-job status-${j.status}`}
                                                            style={{
                                                                height: `calc(${durationHours * 100}% - 4px)`,
                                                                zIndex: 5
                                                            }}
                                                            onClick={(e) => {
                                                                e.stopPropagation();
                                                                handleEventClick(j);
                                                            }}
                                                            title={`${j.typeName}: ${j.description}`}
                                                        >
                                                            <div className="job-inner">
                                                                <span className="job-time">
                                                                    {jStart.getHours().toString().padStart(2, '0')}:00 - {jEnd.getHours().toString().padStart(2, '0')}:00
                                                                </span>
                                                                <span className="job-name">{j.typeName}</span>
                                                            </div>
                                                        </div>
                                                    );
                                                })}
                                            </td>
                                        );
                                    })}
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>

                <style>{`
                    .week-workspace {
                        margin-top: 20px;
                        padding: 24px !important;
                        display: flex;
                        flex-direction: column;
                        min-height: 600px;
                    }
                    .calendar-controls {
                        display: flex;
                        justify-content: center;
                        align-items: center;
                        gap: 20px;
                        flex-direction: row;
                    }
                    .week-table-wrapper {
                        margin-top: 20px;
                        overflow-x: auto;
                        flex: 1;
                    }
                    .week-table {
                        width: 100%;
                        border-collapse: collapse;
                        table-layout: fixed;
                    }
                    .week-table th, .week-table td {
                        border: 1px solid var(--border2);
                        padding: 0;
                        position: relative;
                        height: 50px;
                        background: var(--solid3);
                        overflow: visible !important; /* Ensure multi-hour jobs aren't clipped */
                    }
                    .hour-col { width: 80px; }
                    .hour-label {
                        text-align: center;
                        font-size: 0.8rem;
                        color: var(--text-clr2);
                        background: transparent !important;
                        font-family: monospace;
                        padding: 0 10px;
                    }
                    .day-weekday { font-size: 0.8rem; font-weight: 600; color: var(--text-clr2); }
                    .day-date { font-size: 1.1rem; font-weight: 700; margin-top: 2px; }
                    .week-table thead th {
                        padding: 10px 0;
                        background: transparent;
                    }
                    .week-table tr:hover .week-cell.working {
                        background: rgba(255, 255, 255, 0.02) !important;
                    }
                    .week-table tr:hover .hour-label {
                        background: rgba(255, 255, 255, 0.05) !important;
                    }
                    .week-cell.working {
                        background: var(--solid3);
                    }
                    .week-cell.transparent {
                        background: transparent;
                    }
                    .week-job {
                        position: absolute;
                        top: 1px;
                        left: 2px;
                        right: 2px;
                        background: var(--accent);
                        border-radius: 4px;
                        padding: 4px;
                        cursor: pointer;
                        overflow: hidden;
                        transition: filter 0.2s, transform 0.1s;
                        border: 1px solid rgba(255,255,255, 0.1);
                        box-shadow: 0 2px 4px rgba(0,0,0,0.2);
                    }
                    .week-job:hover {
                        filter: brightness(1.2);
                        z-index: 20 !important; /* Ensure hovered job is on top of everything */
                    }
                    .job-inner {
                        display: flex;
                        flex-direction: column;
                        height: 100%;
                        font-size: 0.75rem;
                        color: white;
                    }
                    .job-time { font-size: 0.65rem; opacity: 0.9; font-weight: 600; }
                    .job-name { font-weight: 700; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
                    
                    .week-job.status-pending { background: #f0ad4e33; border-left: 3px solid #f0ad4e; color: #f0ad4e; }
                    .week-job.status-inprogress { background: #337ab733; border-left: 3px solid #337ab7; color: #337ab7; }
                    .week-job.status-done { background: #5cb85c33; border-left: 3px solid #5cb85c; color: #5cb85c; }
                    .week-job.status-pending .job-inner,
                    .week-job.status-inprogress .job-inner,
                    .week-job.status-done .job-inner { color: inherit; }
                `}</style>
            </div>
        );
    };

    return (
        <main className={`main ${viewMode !== 'list' ? 'calendar-layout' : ''}`}>
            <div className="header">
                <h1>My Jobs</h1>
                <Dropdown
                    value={viewMode}
                    onChange={e => {
                        const newMode = e.target.value;
                        setViewMode(newMode);
                        localStorage.setItem('myJobsViewMode', newMode);
                    }}
                >
                    <option value="list">List View</option>
                    <option value="calendar">Calendar View</option>
                    <option value="week">Week View</option>
                </Dropdown>
            </div>

            {loading ? <p>Loading...</p> : (
                jobs.length === 0 ? <p className="list-empty">No jobs assigned.</p> : (
                    viewMode === 'list' ? renderList() :
                        viewMode === 'calendar' ? renderCalendar() : renderWeekView()
                )
            )}
        </main>
    );
};

export default ToDoPage;
