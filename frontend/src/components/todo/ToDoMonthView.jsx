import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';

const ToDoMonthView = ({ jobs, currentMonth, setCurrentMonth, currentYear, setCurrentYear }) => {
    const navigate = useNavigate();
    const { accesses } = useAuth();
    const hasOrdersAccess = accesses.includes('Orders');

    const daysInMonth = new Date(currentYear, currentMonth + 1, 0).getDate();
    const firstDayOfMonth = new Date(currentYear, currentMonth, 1).getDay(); // 0 is Sunday

    const monthNames = ["January", "February", "March", "April", "May", "June",
        "July", "August", "September", "October", "November", "December"
    ];

    const handleEventClick = (job) => {
        navigate(`/jobs/${job.id}`);
    };

    const rows = [];
    let cells = [];

    // Add empty cells for days from the previous month
    for (let i = 0; i < firstDayOfMonth; i++) {
        cells.push(
            <td key={`empty-${i}`} className="calendar-td">
                <div className="month-cell-wrapper">
                    <div className="month-cell-content"></div>
                </div>
            </td>
        );
    }

    // Add cells for the current month
    for (let day = 1; day <= daysInMonth; day++) {
        const dayJobs = jobs.filter(j => {
            const jDate = new Date(j.startTime);
            return jDate.getDate() === day && jDate.getMonth() === currentMonth && jDate.getFullYear() === currentYear;
        });

        cells.push(
            <td key={day} className="calendar-td">
                <div className="month-cell-wrapper">
                    <div className="day-header">
                        <span className="day-number">{day}</span>
                        {dayJobs.length > 0 && <span className="day-count">{dayJobs.length} tasks</span>}
                    </div>
                    <div className="month-cell-content">
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
                    </div>
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
            cells.push(
                <td key={`empty-end-${cells.length}`} className="calendar-td">
                    <div className="month-cell-wrapper">
                        <div className="month-cell-content"></div>
                    </div>
                </td>
            );
        }
        rows.push(<tr key={`row-${rows.length}`}>{cells}</tr>);
    }

    return (
        <div className="calendar-workspace month-view tile">
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

            <div className="calendar-table-wrapper">
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
            </div>
        </div>
    );
};

export default ToDoMonthView;
