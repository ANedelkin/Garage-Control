import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';

const ToDoWeekView = ({ jobs, worker, viewDate, setViewDate }) => {
    const navigate = useNavigate();
    const { accesses } = useAuth();
    const hasOrdersAccess = accesses.includes('Orders');

    if (!worker) return <p>Loading worker schedule...</p>;

    const handleEventClick = (job) => {
        navigate(`/jobs/${job.id}`);
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
        <div className="calendar-workspace week-view tile">
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

            <div className="calendar-table-wrapper">
                <table className="calendar-table">
                    <thead>
                        <tr>
                            <th className="hour-col"></th>
                            {dayColumns.map((day, idx) => (
                                <th key={idx} className="calendar-th">
                                    <div className="day-weekday">{day.toLocaleDateString(undefined, { weekday: 'short' })}</div>
                                    <div className="day-date">{day.getDate()}</div>
                                </th>
                            ))}
                        </tr>
                    </thead>
                    <tbody>
                        {visibleHours.map(h => (
                            <tr key={h}>
                                <td className="hour-label">{h.toString().padStart(2, '0')}<span className="hour-minutes">:00</span></td>
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
                                            className={`calendar-td ${isActuallyWorking ? 'working' : 'transparent'}`}
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
                                                        className={`week-job tile glow job-status-${j.status}`}
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
        </div>
    );
};

export default ToDoWeekView;
