import React, { useState, useEffect, useRef } from 'react';
import '../../assets/css/orders.css'; // Ensure we can style it

const TimeSlotPicker = ({ worker, onTimeSelect, initialStart, initialEnd }) => {
    const [isOpen, setIsOpen] = useState(false);

    // Initialize viewDate to Today regardless of selection, or start of selection's week? 
    // Let's default to Today for "future" view
    const [viewDate, setViewDate] = useState(new Date());
    const [selectionStart, setSelectionStart] = useState(initialStart ? new Date(initialStart) : null);
    const [selectionEnd, setSelectionEnd] = useState(initialEnd ? new Date(initialEnd) : null);

    const [isSelectingRange, setIsSelectingRange] = useState(false);

    // Sync state with props when they change (crucial for async data loading in Edit mode)
    useEffect(() => {
        if (initialStart) setSelectionStart(new Date(initialStart));
        if (initialEnd) setSelectionEnd(new Date(initialEnd));
    }, [initialStart, initialEnd]);

    // Columns configuration
    const DAYS_TO_SHOW = 5;

    // Debug: Log worker data to see what we're receiving
    useEffect(() => {
        if (worker) {
            console.log('TimeSlotPicker - Worker data:', worker);
        }
    }, [worker]);

    // Helper to get day of week 0 (Mon) - 6 (Sun)
    const getDayOfWeek = (date) => {
        const day = date.getDay(); // 0 is Sun
        return day === 0 ? 6 : day - 1;
    };

    const isWorkerWorking = (date) => {
        if (!worker || !worker.schedules || worker.schedules.length === 0) {
            return false; // No worker or no schedule = not available
        }

        // 1. Check Schedule
        const dow = getDayOfWeek(date);
        const schedule = worker.schedules.find(s => s.dayOfWeek === dow);
        if (!schedule) return false; // Not working this day

        // 2. Check Leaves
        if (worker.leaves && worker.leaves.length > 0) {
            const hasLeave = worker.leaves.some(l => {
                const start = new Date(l.startDate);
                const end = new Date(l.endDate);
                const d = new Date(date);
                d.setHours(0, 0, 0, 0);
                start.setHours(0, 0, 0, 0);
                end.setHours(0, 0, 0, 0);
                return d >= start && d <= end;
            });

            if (hasLeave) return false;
        }

        return true;
    };

    const isHourAvailable = (date, hour) => {
        if (!worker || !worker.schedules || worker.schedules.length === 0) return false;
        if (!isWorkerWorking(date)) return false;

        const dow = getDayOfWeek(date);
        const schedule = worker.schedules.find(s => s.dayOfWeek === dow);
        if (!schedule) return false;

        const [startH] = schedule.startTime.split(':').map(Number);
        const [endH] = schedule.endTime.split(':').map(Number);

        return !(hour < startH || hour >= endH);
    };

    const handleDayChange = (offset) => {
        const newDate = new Date(viewDate);
        newDate.setDate(newDate.getDate() + offset);
        setViewDate(newDate);
    };

    const handleHourClick = (date, hour) => {
        const clickDate = new Date(date);
        clickDate.setHours(hour, 0, 0, 0);

        if (!isHourAvailable(clickDate, hour)) return;

        if (!isSelectingRange || !selectionStart) {
            setSelectionStart(clickDate);
            setSelectionEnd(null);
            setIsSelectingRange(true);
        } else {
            // Use the current selectionStart state carefully
            if (clickDate <= selectionStart) {
                setSelectionStart(clickDate);
                setSelectionEnd(null);
            } else {
                if (clickDate.getDate() !== selectionStart.getDate() ||
                    clickDate.getMonth() !== selectionStart.getMonth()) {
                    setSelectionStart(clickDate);
                    setSelectionEnd(null);
                    return;
                }

                let valid = true;
                const startH = selectionStart.getHours();
                for (let h = startH; h <= hour; h++) {
                    const checkDate = new Date(selectionStart);
                    checkDate.setHours(h, 0, 0, 0);
                    if (!isHourAvailable(checkDate, h)) {
                        valid = false;
                        break;
                    }
                }

                if (valid) {
                    const finalStart = new Date(selectionStart);
                    const finalEnd = new Date(clickDate);
                    finalEnd.setHours(hour + 1, 0, 0, 0);

                    setSelectionStart(finalStart);
                    setSelectionEnd(finalEnd);
                    setIsSelectingRange(false);
                    setIsOpen(false);

                    if (onTimeSelect) {
                        // Create a local ISO-like string to avoid UTC conversion issues
                        // Format: YYYY-MM-DDTHH:mm:ss (no Z)
                        const formatDate = (d) => {
                            const pad = (n) => n.toString().padStart(2, '0');
                            return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:00:00`;
                        };
                        onTimeSelect(formatDate(finalStart), formatDate(finalEnd));
                    }
                } else {
                    setSelectionStart(clickDate);
                    setSelectionEnd(null);
                }
            }
        }
    };

    const hours = Array.from({ length: 24 }, (_, i) => i);

    // Generate Day Columns
    const dayColumns = [];
    for (let i = 0; i < DAYS_TO_SHOW; i++) {
        const d = new Date(viewDate);
        d.setDate(d.getDate() + i);
        dayColumns.push(d);
    }

    const formatTimeRange = () => {
        if (selectionStart && selectionEnd) {
            return `${selectionStart.toLocaleDateString()} ${selectionStart.getHours()}:00 - ${selectionEnd.getHours()}:00`;
        }
        return "Select Time";
    };

    const getSlotClass = (date, hour) => {
        const currentSlotDate = new Date(date);
        currentSlotDate.setHours(hour, 0, 0, 0);

        // Past check styling
        if (currentSlotDate < new Date()) return "time-slot past";

        const available = isHourAvailable(date, hour);
        if (!available) return "time-slot unavailable";

        if (selectionStart && !selectionEnd) {
            if (currentSlotDate.getTime() === selectionStart.getTime()) return "time-slot selected-start";
        }

        if (selectionStart && selectionEnd) {
            const startTime = selectionStart.getTime();
            const endTime = selectionEnd.getTime();
            const slotTime = currentSlotDate.getTime();
            if (slotTime >= startTime && slotTime < endTime) return "time-slot selected-range";
        }

        return "time-slot available";
    };

    return (
        <div className="time-picker-container">
            <button type="button" className="btn secondary time-display-btn" onClick={() => setIsOpen(true)}>
                <i className="fa-regular fa-clock"></i> {formatTimeRange()}
            </button>

            {isOpen && (
                <>
                    <div className="time-picker-overlay" onClick={() => setIsOpen(false)}></div>
                    <div className="time-picker-modal">
                        <div className="tp-header">
                            <button type="button" className="icon-btn" onClick={() => handleDayChange(-1)}>
                                <i className="fa-solid fa-chevron-left"></i>
                            </button>
                            <span>
                                {viewDate.toLocaleDateString()} - {dayColumns[dayColumns.length - 1].toLocaleDateString()}
                            </span>
                            <button type="button" className="icon-btn" onClick={() => handleDayChange(1)}>
                                <i className="fa-solid fa-chevron-right"></i>
                            </button>
                        </div>
                        <div className="tp-body multi-day">
                            {dayColumns.map((day, dayIndex) => (
                                <div key={dayIndex} className="day-column">
                                    <div className="day-header">
                                        <div className="day-weekday">{day.toLocaleDateString(undefined, { weekday: 'short' })}</div>
                                        <div className="day-date">{day.getDate()}</div>
                                    </div>
                                    <div className="time-list">
                                        {hours.map(h => (
                                            <div
                                                key={h}
                                                className={getSlotClass(day, h)}
                                                onClick={() => handleHourClick(day, h)}
                                                title={`${h}:00`}
                                            >
                                                {h.toString().padStart(2, '0')}
                                            </div>
                                        ))}
                                    </div>
                                </div>
                            ))}
                        </div>
                    </div>
                </>
            )}
        </div>
    );
};

export default TimeSlotPicker;
