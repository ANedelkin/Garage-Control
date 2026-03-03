import React from "react";
import ScheduleSelector from "./ScheduleSelector";
import LeavePopup from "./LeavePopup";

const WorkhoursPopup = ({
    worker,
    onChangeSchedule,
    openLeavePopup,
    handleAddOrUpdateLeave,
    deleteLeave
}) => {
    return (
        <div className="workhours">
            {/* Schedule Section */}
            <div className="schedule-section">
                <ScheduleSelector
                    schedules={worker.schedules} // latest schedules
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
        </div>
    );
};

export default WorkhoursPopup;
