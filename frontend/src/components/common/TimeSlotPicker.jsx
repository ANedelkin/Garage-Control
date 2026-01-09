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

    // Columns configuration
    const DAYS_TO_SHOW = 5;

    // Debug: Log worker data to see what we're receiving
    useEffect(() => {
        if (worker) {
            console.log('TimeSlotPicker - Worker data:', worker);
            console.log('TimeSlotPicker - Has schedules?', worker.schedules);
            console.log('TimeSlotPicker - Has leaves?', worker.leaves);
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
        if (!worker || !worker.schedules || worker.schedules.length === 0) {
            return false; // No worker selected or no schedule
        }

        if (!isWorkerWorking(date)) return false;

        const dow = getDayOfWeek(date);
        const schedule = worker.schedules.find(s => s.dayOfWeek === dow);

        if (!schedule) {
            console.log(`No schedule found for day ${dow} (${date.toDateString()})`);
            return false;
        }

        const [startH, startM] = schedule.startTime.split(':').map(Number);
        const [endH, endM] = schedule.endTime.split(':').map(Number);

        const isAvailable = !(hour < startH || hour >= endH);

        if (hour >= 8 && hour <= 18) { // Only log working hours to reduce noise
            console.log(`Hour ${hour} on ${date.toDateString()} (dow:${dow}): schedule ${startH}-${endH}, available: ${isAvailable}`);
        }

        if (hour < startH || hour >= endH) return false;

        return true;
    };

    const handleDayChange = (offset) => {
        const newDate = new Date(viewDate);
        newDate.setDate(newDate.getDate() + offset);

        // Prevent going to past (before Today)
        const today = new Date();
        today.setHours(0, 0, 0, 0);
        if (newDate < today) return;

        setViewDate(newDate);
    };

    const handleHourClick = (date, hour) => {
        const clickDate = new Date(date);
        clickDate.setHours(hour, 0, 0, 0);

        // Validation for past hours if Today
        if (clickDate < new Date()) return; // Can't select past time

        if (!isHourAvailable(clickDate, hour)) return;

        if (!isSelectingRange || !selectionStart) {
            setSelectionStart(clickDate);
            setSelectionEnd(null);
            setIsSelectingRange(true);
        } else {
            if (clickDate <= selectionStart) {
                // Restart logic if clicked before or same
                setSelectionStart(clickDate);
                setSelectionEnd(null);
            } else {
                // Must be same day for this simplified logic
                if (clickDate.getDate() !== selectionStart.getDate() ||
                    clickDate.getMonth() !== selectionStart.getMonth()) {
                    setSelectionStart(clickDate);
                    setSelectionEnd(null);
                    return;
                }

                let valid = true;
                const startH = selectionStart.getHours();
                const endH = hour;

                for (let h = startH; h <= endH; h++) {
                    const checkDate = new Date(selectionStart);
                    checkDate.setHours(h, 0, 0, 0);
                    if (!isHourAvailable(checkDate, h)) {
                        valid = false;
                        break;
                    }
                }

                if (valid) {
                    const finalEnd = new Date(clickDate);
                    finalEnd.setHours(hour + 1, 0, 0, 0);

                    setSelectionEnd(finalEnd);
                    setIsSelectingRange(false);
                    setIsOpen(false);

                    if (onTimeSelect) {
                        onTimeSelect(selectionStart.toISOString(), finalEnd.toISOString());
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
