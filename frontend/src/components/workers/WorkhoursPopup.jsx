import React, { useState, useEffect } from "react";
import ScheduleSelector from "./ScheduleSelector";
import LeavePopup from "./LeavePopup";
import { workerApi } from "../../services/workerApi";
import { usePopup } from "../../context/PopupContext";

const WorkhoursPopup = ({ id, onClose, onSave }) => {
    const [worker, setWorker] = useState(null);
    const [loading, setLoading] = useState(true);
    const { addPopup, removeLastPopup } = usePopup();
    const [editingLeaveIndex, setEditingLeaveIndex] = useState(-1);

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
        } catch (error) {
            console.error("Error saving worker schedule", error);
            alert(error.message || "Failed to save schedule");
        }
    };

    const handleAddOrUpdateLeave = (leave) => {
        let updatedLeaves = [...worker.leaves];
        if (editingLeaveIndex >= 0) {
            updatedLeaves[editingLeaveIndex] = leave;
        } else {
            updatedLeaves.push(leave);
        }
        setWorker({ ...worker, leaves: updatedLeaves });
        setEditingLeaveIndex(-1);
    };

    const deleteLeave = (index) => {
        const updated = [...worker.leaves];
        updated.splice(index, 1);
        setWorker({ ...worker, leaves: updated });
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
                onConfirm={handleAddOrUpdateLeave}
                currentLeave={leave || { startDate: new Date(), endDate: new Date() }}
                isEditing={index >= 0}
            />
        );
    };

    const onChangeSchedule = (newSchedules) => {
        setWorker({ ...worker, schedules: newSchedules });
    };

    if (loading) return <div>Loading...</div>;
    if (!worker) return <div>Worker not found</div>;

    return (
        <div className="workhours">
            {/* Schedule Section */}
            <div className="schedule-section">
                <ScheduleSelector
                    schedules={worker.schedules}
                    onChange={onChangeSchedule}
                />
            </div>

            {/* Leaves Section */}
            <div className="leaves-section">
                <div className="section-header">
                    <label>Leaves</label>
                    <button type="button" className="btn" onClick={() => openLeavePopup()}>
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
                                    {new Date(leave.startDate).toLocaleDateString()} -{" "}
                                    {new Date(leave.endDate).toLocaleDateString()}
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

            <div className="form-footer" style={{ marginTop: '20px' }}>
                <button type="button" className="btn" onClick={handleSave}>
                    Save Schedule
                </button>
            </div>
        </div>
    );
};

export default WorkhoursPopup;
