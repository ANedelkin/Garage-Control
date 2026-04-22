import React, { useState, useEffect } from "react";
import ScheduleSelector from "./ScheduleSelector";
import LeavePopup from "./LeavePopup";
import { workerApi } from "../../services/workerApi";
import { usePopup } from "../../context/PopupContext";
import { parseValidationErrors } from "../../Utilities/formErrors.js";

const WorkhoursPopup = ({ id, onClose, onSave }) => {
    const [worker, setWorker] = useState(null);
    const [loading, setLoading] = useState(true);
    const { addPopup, removeLastPopup } = usePopup();
    const [editingLeaveIndex, setEditingLeaveIndex] = useState(-1);
    const [errors, setErrors] = useState({});

    useEffect(() => {
        const fetchWorker = async () => {
            try {
                const workerRes = await workerApi.getWorker(id);
                // Parse dates
                workerRes.hiredOn = new Date(workerRes.hiredOn);
                workerRes.leaves = workerRes.leaves.map((l) => ({
                    ...l,
                    startDate: new Date(l.startDate),
                    endDate: new Date(l.endDate)
                }));
                setWorker(workerRes);
            } catch (error) {
                console.error("Error loading worker", error);
            } finally {
                setLoading(false);
            }
        };
        fetchWorker();
    }, [id]);

    const handleSave = async () => {
        try {
            await workerApi.edit(id, worker);
            if (onSave) onSave();
            if (onClose) onClose();
            setErrors({});
        } catch (error) {
            console.error("Error saving worker schedule", error);
            setErrors(parseValidationErrors(error));
        }
    };

    const handleAddOrUpdateLeave = async (leave, index) => {
        let updatedLeaves = [...worker.leaves];
        if (index >= 0) {
            updatedLeaves[index] = leave;
        } else {
            updatedLeaves.push(leave);
        }
        
        const updatedWorker = { ...worker, leaves: updatedLeaves };
        setWorker(updatedWorker);
        setEditingLeaveIndex(-1);

        // Immediate persistence
        try {
            await workerApi.edit(id, updatedWorker);
            setErrors({});
        } catch (error) {
            console.error("Error persisting leave", error);
            setErrors(parseValidationErrors(error));
        }
    };

    const deleteLeave = async (index) => {
        const updatedLeaves = [...worker.leaves];
        updatedLeaves.splice(index, 1);
        
        const updatedWorker = { ...worker, leaves: updatedLeaves };
        setWorker(updatedWorker);

        // Immediate persistence
        try {
            await workerApi.edit(id, updatedWorker);
            setErrors({});
        } catch (error) {
            console.error("Error deleting leave", error);
            setErrors(parseValidationErrors(error));
        }
    };

    const openLeavePopup = (leave = null, index = -1) => {
        if (leave) {
            setEditingLeaveIndex(index);
        } else {
            setEditingLeaveIndex(-1);
        }
        addPopup(
            index >= 0 ? "Edit Leave" : "Add Leave",
            <LeavePopup
                onClose={removeLastPopup}
                onConfirm={(updatedLeave) => handleAddOrUpdateLeave(updatedLeave, index)}
                currentLeave={leave || { startDate: new Date(), endDate: new Date() }}
                isEditing={index >= 0}
                existingLeaves={worker.leaves}
                currentIndex={index}
            />
        );
    };

    const onChangeSchedule = (newSchedules) => {
        setWorker({ ...worker, schedules: newSchedules });
    };

    const [activeTab, setActiveTab] = useState("schedule");
    const [isMobile, setIsMobile] = useState(window.innerWidth < 800);

    useEffect(() => {
        const handleResize = () => setIsMobile(window.innerWidth < 800);
        window.addEventListener("resize", handleResize);
        return () => window.removeEventListener("resize", handleResize);
    }, []);

    const formatDate = (date) => {
        if (!date) return "";
        const d = new Date(date);
        const day = d.getDate().toString().padStart(2, '0');
        const month = (d.getMonth() + 1).toString().padStart(2, '0');
        const year = d.getFullYear();
        return `${day}.${month}.${year}`;
    };

    if (loading) return <div>Loading...</div>;
    if (!worker) return <div>Worker not found</div>;

    return (
        <div className={`workhours ${isMobile ? "mobile-view" : "desktop-view"}`}>
            {/* Tabs Header - Mobile only */}
            {isMobile && (
                <div className="popup-tabs">
                    <button
                        type="button"
                        className={`tab-btn ${activeTab === "schedule" ? "active" : ""}`}
                        onClick={() => setActiveTab("schedule")}
                    >
                        <i className="fa-solid fa-calendar-days"></i> Schedule
                    </button>
                    <button
                        type="button"
                        className={`tab-btn ${activeTab === "leaves" ? "active" : ""}`}
                        onClick={() => setActiveTab("leaves")}
                    >
                        <i className="fa-solid fa-umbrella-beach"></i> Leaves
                    </button>
                </div>
            )}

            <div className="tab-content">
                {/* Schedule Section - Show if desktop OR activeTab is schedule */}
                {(!isMobile || activeTab === "schedule") && (
                    <div className="schedule-section form-section">
                        {!isMobile && <label className="desktop-section-label">Schedule</label>}
                        <ScheduleSelector
                            schedules={worker.schedules}
                            onChange={onChangeSchedule}
                        />
                    </div>
                )}

                {/* Leaves Section - Show if desktop OR activeTab is leaves */}
                {(!isMobile || activeTab === "leaves") && (
                    <div className="leaves-section form-section">
                        <div className="section-header">
                            <label>Leaves</label>
                            <button type="button" className="btn" onClick={(e) => {
                                e.currentTarget.blur();
                                openLeavePopup();
                            }}>
                                + Add Leave
                            </button>
                        </div>
                        <div className="list-container max-height">
                            {worker.leaves.length ? (
                                worker.leaves.map((leave, i) => (
                                    <div
                                        key={i}
                                        className="list-item"
                                        onClick={() => openLeavePopup(leave, i)}
                                        style={{ cursor: "pointer" }}
                                    >
                                        <span className="item-label">
                                            {formatDate(leave.startDate)} -{" "}
                                            {formatDate(leave.endDate)}
                                        </span>
                                        <button
                                            type="button"
                                            className="icon-btn delete btn"
                                            onClick={(e) => {
                                                e.stopPropagation();
                                                deleteLeave(i);
                                            }}
                                        >
                                            <i className="fa-solid fa-trash"></i>
                                        </button>
                                    </div>
                                ))
                            ) : (
                                <div className="list-empty">No leaves added</div>
                            )}
                        </div>
                    </div>
                )}
            </div>

            <div className="form-footer">
                {errors.general && <p className="form-error">{errors.general}</p>}
                <button type="button" className="btn" onClick={handleSave}>
                    Save Changes
                </button>
            </div>
        </div>
    );
};

export default WorkhoursPopup;
