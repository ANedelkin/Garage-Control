import React, { useState, useEffect } from "react";

const ScheduleSelector = ({ schedules = [], onChange }) => {
    const hours = Array.from({ length: 24 }, (_, i) => i);
    const days = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];

    const [localSchedules, setLocalSchedules] = useState([]);
    const [selectionStart, setSelectionStart] = useState(null);
    const [selectedCell, setSelectedCell] = useState(null);  // Track the clicked cell

    // Sync from parent safely
    useEffect(() => {
        if (Array.isArray(schedules)) {
            setLocalSchedules(schedules);
        } else {
            setLocalSchedules([]);
        }
    }, [schedules]);

    // -------------------------
    // SAFE TIME HELPERS
    // -------------------------

    const extractTime = (s, type) => {
        if (!s) return null;

        const value =
            type === "start"
                ? s.startTime ?? s.StartTime ?? s.start
                : s.endTime ?? s.EndTime ?? s.end;

        if (!value || typeof value !== "string") return null;

        return value;
    };

    const getStartHour = (s) => {
        const time = extractTime(s, "start");
        if (!time) return 0;
        return parseInt(time.split(":")[0], 10);
    };

    const getEndHour = (s) => {
        const time = extractTime(s, "end");
        if (!time) return 0;
        const [h, m] = time.split(":").map(Number);
        if (h === 23 && m === 59) return 24;
        return h;
    };

    // -------------------------
    // WORKING CHECK
    // -------------------------

    const isWorkingHour = (dayIndex, hour) => {
        return localSchedules.some((s) => {
            if (s.dayOfWeek !== dayIndex) return false;

            const start = getStartHour(s);
            const end = getEndHour(s);

            return start <= hour && end > hour;
        });
    };

    // -------------------------
    // MERGE LOGIC (IMMUTABLE)
    // -------------------------

    const mergeSchedules = (input) => {
        const byDay = {};

        input.forEach((s) => {
            if (!byDay[s.dayOfWeek]) byDay[s.dayOfWeek] = [];
            byDay[s.dayOfWeek].push({ ...s });
        });

        const merged = [];

        Object.keys(byDay).forEach((day) => {
            const daySchedules = byDay[day].sort(
                (a, b) => getStartHour(a) - getStartHour(b)
            );

            if (!daySchedules.length) return;

            let current = daySchedules[0];

            for (let i = 1; i < daySchedules.length; i++) {
                const next = daySchedules[i];

                if (getStartHour(next) <= getEndHour(current)) {
                    current = {
                        ...current,
                        endTime:
                            getEndHour(next) > getEndHour(current)
                                ? extractTime(next, "end")
                                : extractTime(current, "end")
                    };
                } else {
                    merged.push(current);
                    current = next;
                }
            }

            merged.push(current);
        });

        return merged;
    };

    // -------------------------
    // CLICK HANDLING
    // -------------------------

    const handleCellClick = (dayIndex, hour) => {
        if (!selectionStart) {
            setSelectionStart({ dayIndex, hour });
            setSelectedCell({ dayIndex, hour });  // Set clicked cell
            return;
        }

        if (selectionStart.dayIndex !== dayIndex) {
            setSelectionStart({ dayIndex, hour });
            setSelectedCell({ dayIndex, hour });  // Update clicked cell
            return;
        }

        const startHour = Math.min(selectionStart.hour, hour);
        const endHour = Math.max(selectionStart.hour, hour);

        const endHourVal = endHour + 1;
        const newEntry = {
            dayOfWeek: dayIndex,
            startTime: `${startHour.toString().padStart(2, "0")}:00`,
            endTime: endHourVal === 24 ? "23:59" : `${endHourVal.toString().padStart(2, "0")}:00`
        };

        const updated = mergeSchedules([...localSchedules, newEntry]);

        setLocalSchedules(updated);
        onChange?.(updated);

        setSelectionStart(null);
        setSelectedCell(null);  // Reset clicked cell after selection
    };

    const handleRightClick = (e, dayIndex, hour) => {
        e.preventDefault();

        const remaining = localSchedules.filter((s) => {
            if (s.dayOfWeek !== dayIndex) return true;

            const start = getStartHour(s);
            const end = getEndHour(s);

            return !(hour >= start && hour < end);
        });

        setLocalSchedules(remaining);
        onChange?.(remaining);
    };

    // -------------------------
    // RENDER
    // -------------------------

    return (
        <div className="schedule-grid">
            <div className="schedule-header-cell">Time</div>

            {days.map((d) => (
                <div key={d} className="schedule-header-cell">
                    {d}
                </div>
            ))}

            {hours.map((hour) => (
                <React.Fragment key={hour}>
                    <div className="schedule-time-cell">{hour}:00</div>

                    {days.map((_, dayIndex) => {
                        const working = isWorkingHour(dayIndex, hour);
                        const isSelected =
                            selectedCell?.dayIndex === dayIndex && selectedCell?.hour === hour; // Check if the cell is selected

                        return (
                            <div
                                key={`${dayIndex}-${hour}`}
                                className={`schedule-cell ${working ? "working" : ""} ${isSelected ? "selecting" : ""}`}  // Add selected class if it's clicked
                                onClick={() => handleCellClick(dayIndex, hour)}
                                onContextMenu={(e) =>
                                    handleRightClick(e, dayIndex, hour)
                                }
                            />
                        );
                    })}
                </React.Fragment>
            ))}
        </div>
    );
};

export default ScheduleSelector;
