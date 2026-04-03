import React, { useState, useEffect } from 'react';
import { jobApi } from '../../services/jobApi';
import { usePopup } from '../../context/PopupContext';
import '../../assets/css/orders.css'; // Ensure we can style it
import '../../assets/css/job-time-picker.css';

const TimeSlotPickerContent = ({
    worker,
    initialStart,
    initialEnd,
    excludeId,
    onClose,
    onTimeSelect
}) => {
    const [viewDate, setViewDate] = useState(() => {
        const today = new Date();
        today.setHours(0, 0, 0, 0);
        if (initialStart) {
            const start = new Date(initialStart);
            start.setHours(0, 0, 0, 0);
            return start < today ? today : start;
        }
        return today;
    });
    const [busySlots, setBusySlots] = useState([]);
    const [selectionStart, setSelectionStart] = useState(initialStart ? new Date(initialStart) : null);
    const [selectionEnd, setSelectionEnd] = useState(initialEnd ? new Date(initialEnd) : null);
    const [isSelectingRange, setIsSelectingRange] = useState(false);

    useEffect(() => {
        if (worker && worker.id) {
            const fetchBusySlots = async () => {
                try {
                    const formatDate = (d) => {
                        const pad = (n) => n.toString().padStart(2, '0');
                        return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;
                    };
                    const start = formatDate(viewDate);
                    const nextDate = new Date(viewDate);
                    nextDate.setDate(nextDate.getDate() + 8); // Fetch 8 days to safely cover the 7-day view window locally
                    const end = formatDate(nextDate);

                    const response = await jobApi.getBusySlots(worker.id, start, end, excludeId);
                    setBusySlots(response.map(s => ({
                        start: new Date(s.start),
                        end: new Date(s.end)
                    })));
                } catch (error) {
                    console.error("Failed to fetch busy slots:", error);
                }
            };
            fetchBusySlots();
        }
    }, [worker, viewDate, excludeId]);

    // Helper to get day of week 0 (Mon) - 6 (Sun)
    const getDayOfWeek = (date) => {
        const day = date.getDay(); // 0 is Sun
        return day === 0 ? 6 : day - 1;
    };

    const isWorkerWorking = (date) => {
        if (!worker || !worker.schedules || worker.schedules.length === 0) return false;
        const dow = getDayOfWeek(date);
        const schedule = worker.schedules.find(s => s.dayOfWeek === dow);
        if (!schedule) return false;

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

    const getSlotStatus = (date, hour) => {
        if (!worker || !worker.schedules || worker.schedules.length === 0) return 'unavailable';
        if (!isWorkerWorking(date)) return 'unavailable';

        const dow = getDayOfWeek(date);
        const schedule = worker.schedules.find(s => s.dayOfWeek === dow);
        if (!schedule) return 'unavailable';

        const [startH] = schedule.startTime.split(':').map(Number);
        const [endH] = schedule.endTime.split(':').map(Number);

        if (hour < startH || hour >= endH) return 'unavailable';

        const currentSlotTime = new Date(date);
        currentSlotTime.setHours(hour, 0, 0, 0);
        const slotStart = currentSlotTime.getTime();
        const slotEnd = slotStart + 3600000;

        const isBusy = busySlots.some(s => {
            const bStart = s.start.getTime();
            const bEnd = s.end.getTime();
            if (bStart === bEnd) {
                return bStart >= slotStart && bStart < slotEnd;
            }
            return slotStart < bEnd && slotEnd > bStart;
        });

        return isBusy ? 'taken' : 'available';
    };

    const handleDayChange = (offset) => {
        const newDate = new Date(viewDate);
        newDate.setDate(newDate.getDate() + offset);

        const today = new Date();
        today.setHours(0, 0, 0, 0);
        if (newDate < today) return;

        setViewDate(newDate);
    };

    const handleHourClick = (date, hour) => {
        const clickDate = new Date(date);
        clickDate.setHours(hour, 0, 0, 0);

        if (clickDate < new Date()) return;

        const status = getSlotStatus(clickDate, hour);
        if (status !== 'available') return;

        if (!isSelectingRange || !selectionStart) {
            setSelectionStart(clickDate);
            setSelectionEnd(null);
            setIsSelectingRange(true);
        } else {
            if (clickDate < selectionStart) {
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
                    if (getSlotStatus(checkDate, h) !== 'available') {
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

                    if (onTimeSelect) {
                        const formatDate = (d) => {
                            const pad = (n) => n.toString().padStart(2, '0');
                            return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:00:00`;
                        };
                        onTimeSelect(formatDate(finalStart), formatDate(finalEnd));
                    }
                    onClose();
                } else {
                    setSelectionStart(clickDate);
                    setSelectionEnd(null);
                }
            }
        }
    };

    const getSlotClass = (date, hour) => {
        const currentSlotDate = new Date(date);
        currentSlotDate.setHours(hour, 0, 0, 0);

        if (currentSlotDate < new Date()) return "time-slot past";

        const status = getSlotStatus(date, hour);
        if (status === 'unavailable') return "time-slot unavailable";
        if (status === 'taken') return "time-slot taken";

        if (selectionStart && !selectionEnd) {
            if (currentSlotDate.getTime() === selectionStart.getTime()) return "time-slot selected-start";
        }

        if (selectionStart && selectionEnd) {
            const startTime = selectionStart.getTime();
            const endTime = selectionEnd.getTime();
            const slotTime = currentSlotDate.getTime();
            if (startTime === endTime) {
                if (slotTime === startTime) return "time-slot selected-range";
            } else if (slotTime >= startTime && slotTime < endTime) {
                return "time-slot selected-range";
            }
        }
        return "time-slot available";
    };

    const DAYS_TO_SHOW = 7;
    const dayColumns = [];
    for (let i = 0; i < DAYS_TO_SHOW; i++) {
        const d = new Date(viewDate);
        d.setDate(d.getDate() + i);
        dayColumns.push(d);
    }
    
    // Filter hours: only show row if any displayed day has 'available' or 'taken' at that hour
    const visibleHours = Array.from({ length: 24 }, (_, i) => i).filter(h => {
        return dayColumns.some(day => {
            // Check if worker is working this day at this hour
            if (!worker || !worker.schedules) return false;
            const dow = getDayOfWeek(day);
            const schedule = worker.schedules.find(s => s.dayOfWeek === dow);
            if (!schedule) return false;
            
            const [startH] = schedule.startTime.split(':').map(Number);
            const [endH] = schedule.endTime.split(':').map(Number);
            const isWorkingHour = h >= startH && h < endH;
            
            if (!isWorkingHour) return false;

            // Check leaves
            if (worker.leaves && worker.leaves.length > 0) {
                const hasLeave = worker.leaves.some(l => {
                    const start = new Date(l.startDate);
                    const end = new Date(l.endDate);
                    const d = new Date(day);
                    d.setHours(0, 0, 0, 0);
                    start.setHours(0, 0, 0, 0);
                    end.setHours(0, 0, 0, 0);
                    return d >= start && d <= end;
                });
                if (hasLeave) return false;
            }
            return true;
        });
    });

    const isFirstDayToday = () => {
        const today = new Date();
        today.setHours(0, 0, 0, 0);
        return viewDate.getTime() <= today.getTime();
    };

    return (
        <div className="time-picker-content tile">
            <div className="tp-header">
                <button 
                    type="button" 
                    className="btn secondary icon-btn" 
                    onClick={() => handleDayChange(-1)}
                    disabled={isFirstDayToday()}
                >
                    <i className="fa-solid fa-chevron-left"></i>
                </button>
                <span style={{ fontWeight: 600 }}>
                    {viewDate.toLocaleDateString(undefined, { month: 'short', day: 'numeric' })} - {dayColumns[dayColumns.length - 1].toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' })}
                </span>
                <button type="button" className="btn secondary icon-btn" onClick={() => handleDayChange(1)}>
                    <i className="fa-solid fa-chevron-right"></i>
                </button>
            </div>
            <div className="tp-table-container">
                <table className="tp-table">
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
                                {dayColumns.map((day, dayIdx) => (
                                    <td
                                        key={dayIdx}
                                        className={getSlotClass(day, h)}
                                        onClick={() => handleHourClick(day, h)}
                                        title={`${day.toLocaleDateString()} ${h}:00`}
                                    >
                                    </td>
                                ))}
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
        </div>
    );
};

const TimeSlotPicker = ({ worker, onTimeSelect, initialStart, initialEnd, readonly = false, excludeId = null }) => {
    const { addPopup, removeLastPopup } = usePopup();

    const openPicker = () => {
        addPopup(
            'Select Time Slot',
            <TimeSlotPickerContent
                worker={worker}
                initialStart={initialStart}
                initialEnd={initialEnd}
                excludeId={excludeId}
                onClose={() => {
                    removeLastPopup();
                }}
                onTimeSelect={onTimeSelect}
            />,
            true // isRaw
        );
    };

    const formatTimeRange = () => {
        if (initialStart && initialEnd) {
            const start = new Date(initialStart);
            const end = new Date(initialEnd);
            return `${start.toLocaleDateString()} ${start.getHours()}:00 - ${end.getHours()}:00`;
        }
        return "Select Time";
    };

    return (
        <div className="time-picker-container">
            <button type="button" className="btn secondary time-display-btn" onClick={openPicker} disabled={readonly}>
                <i className="fa-regular fa-clock"></i> {formatTimeRange()}
            </button>
        </div>
    );
};

export default TimeSlotPicker;
