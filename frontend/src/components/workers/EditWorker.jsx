import React, { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import "../../assets/css/workers.css";
import DatePicker from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css";
import { workerApi } from "../../services/workerApi";
import { jobTypeApi } from "../../services/jobTypeApi";

const EditWorker = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const isNew = !id || id === 'new';

  const [worker, setWorker] = useState({
    firstName: "",
    lastName: "",
    email: "",
    password: "",
    hiredOn: new Date(),
    roles: [],
    jobTypeIds: [],
    schedules: [],
    leaves: []
  });

  const [allRoles, setAllRoles] = useState([]);
  const [allJobTypes, setAllJobTypes] = useState([]);
  const [loading, setLoading] = useState(true);

  // Leave Popup State
  const [showLeavePopup, setShowLeavePopup] = useState(false);
  const [currentLeave, setCurrentLeave] = useState({ startDate: new Date(), endDate: new Date() });
  const [editingLeaveIndex, setEditingLeaveIndex] = useState(-1);

  // Calendar View State
  const [calendarMonth, setCalendarMonth] = useState(new Date());

  useEffect(() => {
    const fetchData = async () => {
      try {
        const [rolesRes, jobTypesRes] = await Promise.all([
          workerApi.getRoles(),
          jobTypeApi.getJobTypes()
        ]);
        setAllRoles(rolesRes);
        setAllJobTypes(jobTypesRes);

        if (!isNew) {
          const workerRes = await workerApi.getWorker(id);
          // Parse dates
          workerRes.hiredOn = new Date(workerRes.hiredOn);
          workerRes.leaves = workerRes.leaves.map(l => ({
            ...l,
            startDate: new Date(l.startDate),
            endDate: new Date(l.endDate)
          }));
          setWorker(workerRes);
        } else {
          // Initialize empty state with all roles unselected
          setWorker(prev => ({
            ...prev,
            roles: rolesRes.map(r => ({ ...r, isSelected: false }))
          }));
        }
      } catch (error) {
        console.error("Error loading data", error);
      } finally {
        setLoading(false);
      }
    };
    fetchData();
  }, [id, isNew]);

  const handleSave = async (e) => {
    e.preventDefault();
    try {
      console.log(worker);
      await workerApi.editWorker(worker);
      navigate('/workers');
    } catch (error) {
      console.error("Error saving worker", error);
      alert("Failed to save worker");
    }
  };

  // --- Schedule Logic ---
  const hours = Array.from({ length: 24 }, (_, i) => i);
  const days = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];

  const isWorkingHour = (dayIndex, hour) => {
    return worker.schedules.some(s =>
      s.dayOfWeek === dayIndex &&
      parseInt(s.startTime.split(':')[0]) <= hour &&
      parseInt(s.endTime.split(':')[0]) > hour
    );
  };

  const toggleWorkHour = (dayIndex, hour) => {
    const startTime = `${hour.toString().padStart(2, '0')}:00`;
    const endTime = `${(hour + 1).toString().padStart(2, '0')}:00`;

    const existingIndex = worker.schedules.findIndex(s =>
      s.dayOfWeek === dayIndex && s.startTime === startTime
    );

    let newSchedules = [...worker.schedules];
    if (existingIndex >= 0) {
      newSchedules.splice(existingIndex, 1);
    } else {
      newSchedules.push({ dayOfWeek: dayIndex, startTime, endTime });
    }
    setWorker({ ...worker, schedules: newSchedules });
  };

  // --- Leave Logic ---
  const handleAddOrUpdateLeave = () => {
    let updatedLeaves = [...worker.leaves];
    if (editingLeaveIndex >= 0) {
      updatedLeaves[editingLeaveIndex] = currentLeave;
    } else {
      updatedLeaves.push(currentLeave);
    }
    setWorker({ ...worker, leaves: updatedLeaves });
    setShowLeavePopup(false);
    setEditingLeaveIndex(-1);
  };

  const deleteLeave = (index) => {
    const updated = [...worker.leaves];
    updated.splice(index, 1);
    setWorker({ ...worker, leaves: updated });
  };

  const openLeavePopup = (leave = null, index = -1) => {
    if (leave) {
      setCurrentLeave(leave);
      setEditingLeaveIndex(index);
    } else {
      setCurrentLeave({ startDate: new Date(), endDate: new Date() });
      setEditingLeaveIndex(-1);
    }
    setShowLeavePopup(true);
  };

  // Calendar Generation
  const getCalendarDays = () => {
    const year = calendarMonth.getFullYear();
    const month = calendarMonth.getMonth();
    const firstDay = new Date(year, month, 1);
    const lastDay = new Date(year, month + 1, 0);

    const daysInMonth = [];
    // Fill padding for start
    for (let i = 0; i < firstDay.getDay(); i++) daysInMonth.push(null);
    // Fill days
    for (let i = 1; i <= lastDay.getDate(); i++) daysInMonth.push(new Date(year, month, i));

    return daysInMonth;
  };

  const isLeaveDay = (date) => {
    if (!date) return false;
    return worker.leaves.some(l =>
      date >= l.startDate && date <= l.endDate
    );
  };

  if (loading) return <div>Loading...</div>;

  return (
    <main className="main container edit-worker">
      <div className="tile">
        <h3 className="tile-header">{isNew ? "New Worker" : "Edit Worker"}</h3>
        <form onSubmit={handleSave} className="worker-form">

          {/* Upper Part: 3x1 Grid (3 Columns for Basic Info, Roles, Job Types) */}
          <div className="form-upper">
            {/* Basic Info */}
            <div className="form-column">
              <div className="form-section">
                <label>First Name</label>
                <input type="text" value={worker.firstName} onChange={e => setWorker({ ...worker, firstName: e.target.value })} required />
              </div>
              <div className="form-section">
                <label>Last Name</label>
                <input type="text" value={worker.lastName} onChange={e => setWorker({ ...worker, lastName: e.target.value })} required />
              </div>
              <div className="form-section">
                <label>Email</label>
                <input type="email" value={worker.email} onChange={e => setWorker({ ...worker, email: e.target.value })} required />
              </div>
              <div className="form-section">
                <label>Password</label>
                <input type="password" value={worker.password} onChange={e => setWorker({ ...worker, password: e.target.value })} required={isNew} />
              </div>
            </div>

            {/* Roles */}
            <div className="form-column">
              <h4>Roles</h4>
              <div className="checkbox-list">
                {worker.roles.map((role, idx) => (
                  <label key={role.id} className="checkbox-item">
                    <input
                      type="checkbox"
                      checked={role.isSelected}
                      onChange={e => {
                        const updatedRoles = [...worker.roles];
                        updatedRoles[idx].isSelected = e.target.checked;
                        setWorker({ ...worker, roles: updatedRoles });
                      }}
                    />
                    {role.name}
                  </label>
                ))}
              </div>
            </div>

            {/* Job Types */}
            <div className="form-column">
              <h4>Job Types</h4>
              <div className="checkbox-list">
                {allJobTypes.map(jt => (
                  <label key={jt.id} className="checkbox-item">
                    <input
                      type="checkbox"
                      checked={worker.jobTypeIds.includes(jt.id)}
                      onChange={e => {
                        let updated = [...worker.jobTypeIds];
                        if (e.target.checked) updated.push(jt.id);
                        else updated = updated.filter(id => id !== jt.id);
                        setWorker({ ...worker, jobTypeIds: updated });
                      }}
                    />
                    {jt.name}
                  </label>
                ))}
              </div>
            </div>
          </div>

          {/* Lower Part: 2x1 Grid (Schedule + Leaves) */}
          <div className="form-lower">
            {/* Schedule */}
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
                        className={`schedule-cell ${working ? 'working' : ''}`}
                        onClick={() => toggleWorkHour(dayIndex, hour)}
                        onMouseEnter={(e) => { if (e.buttons === 1) toggleWorkHour(dayIndex, hour); }}
                      />
                    );
                  })}
                </React.Fragment>
              ))}
            </div>

            {/* Leaves */}
            <div className="leaves-list">
              <h4>Leaves</h4>
              <button type="button" className="btn" onClick={() => openLeavePopup()}>+ Add Leave</button>
              {worker.leaves.map((leave, i) => (
                <div key={i} className="leave-item" onClick={() => openLeavePopup(leave, i)} style={{ cursor: 'pointer' }}>
                  <span>{new Date(leave.startDate).toLocaleDateString()} - {new Date(leave.endDate).toLocaleDateString()}</span>
                  <button type="button" className="icon-btn delete" onClick={(e) => { e.stopPropagation(); deleteLeave(i); }}>
                    <i className="fa-solid fa-trash"></i>
                  </button>
                </div>
              ))}
            </div>
          </div>

          <div className="form-footer">
            <button type="submit" className="btn">Save Worker</button>
          </div>
        </form>
      </div>
    </main>

  );
};

export default EditWorker;
