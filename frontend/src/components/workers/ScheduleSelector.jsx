import React, { useState, useEffect } from 'react';

const ScheduleSelector = ({ schedules, onChange }) => {
    const hours = Array.from({ length: 24 }, (_, i) => i);
    const days = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];

    const [selectionStart, setSelectionStart] = useState(null);

    useEffect(() => {
        const handleGlobalClick = (e) => {
            if (!e.target.classList.contains('schedule-cell')) {
                setSelectionStart(null);
            }
        };
        document.addEventListener('click', handleGlobalClick);
        return () => document.removeEventListener('click', handleGlobalClick);
    }, []);

    const getStartHour = (s) => parseInt((s.startTime || s.StartTime).split(':')[0]);
    const getEndHour = (s) => parseInt((s.endTime || s.EndTime).split(':')[0]);

    const isWorkingHour = (dayIndex, hour) => {
        return schedules.some(s =>
            s.dayOfWeek === dayIndex &&
            getStartHour(s) <= hour &&
            getEndHour(s) > hour
        );
    };

    const mergeSchedules = (currentSchedules) => {
        const byDay = {};
        currentSchedules.forEach(s => {
            if (!byDay[s.dayOfWeek]) byDay[s.dayOfWeek] = [];
            byDay[s.dayOfWeek].push(s);
        });

        let merged = [];

        Object.keys(byDay).forEach(day => {
            const daySchedules = byDay[day];
            daySchedules.sort((a, b) => getStartHour(a) - getStartHour(b));

            if (daySchedules.length === 0) return;

            let current = daySchedules[0];

            for (let i = 1; i < daySchedules.length; i++) {
                const next = daySchedules[i];

                const currentEnd = getEndHour(current);
                const nextStart = getStartHour(next);
                const nextEnd = getEndHour(next);

                if (nextStart <= currentEnd) {
                    if (nextEnd > currentEnd) {
                        current.endTime = next.endTime || next.EndTime; // Keep existing format if possible, or normalize
                        if (current.EndTime) current.EndTime = current.endTime; // Ensure both if mixed
                    }
                } else {
                    merged.push(current);
                    current = next;
                }
            }
            merged.push(current);
        });

        return merged;
    };

    const handleCellClick = (dayIndex, hour) => {
        if (!selectionStart) {
            setSelectionStart({ dayIndex, hour });
        } else {
            if (selectionStart.dayIndex !== dayIndex) {
                setSelectionStart({ dayIndex, hour });
                return;
            }

            const startHour = Math.min(selectionStart.hour, hour);
            const endHour = Math.max(selectionStart.hour, hour);

            const startTime = `${startHour.toString().padStart(2, '0')}:00`;
            const endTime = `${(endHour + 1).toString().padStart(2, '0')}:00`;

            const newEntry = { dayOfWeek: dayIndex, startTime, endTime };
            let updatedSchedules = [...schedules, newEntry];

            updatedSchedules = mergeSchedules(updatedSchedules);

            onChange(updatedSchedules);
            setSelectionStart(null);
        }
    };

    const handleRightClick = (e, dayIndex, hour) => {
        e.preventDefault();
        const remaining = schedules.filter(s => {
            if (s.dayOfWeek !== dayIndex) return true;
            const start = getStartHour(s);
            const end = getEndHour(s);
            return !(hour >= start && hour < end);
        });

        onChange(remaining);
    };

    return (
        <div className="schedule-grid">
            <div className="schedule-header-cell">Time</div>
            {days.map(d => <div key={d} className="schedule-header-cell">{d}</div>)}

            {hours.map(hour => (
                <React.Fragment key={hour}>
                    <div className="schedule-time-cell">{hour}:00</div>
                    {days.map((_, dayIndex) => {
                        const working = isWorkingHour(dayIndex, hour);
                        return (
                            <div
                                key={`${dayIndex}-${hour}`}
                                className={`schedule-cell ${working ? 'working' : ''} ${selectionStart && selectionStart.dayIndex === dayIndex && selectionStart.hour === hour ? 'selecting' : ''}`}
                                onClick={() => handleCellClick(dayIndex, hour)}
                                onContextMenu={(e) => handleRightClick(e, dayIndex, hour)}
                            />
                        );
                    })}
                </React.Fragment>
            ))}
        </div>
    );
};

export default ScheduleSelector;
