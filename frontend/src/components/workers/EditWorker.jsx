import React, { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import "../../assets/css/common/popup.css";
import "../../assets/css/common/layout.css";
import "../../assets/css/common/colors.css";
import "../../assets/css/workers.css";
import DatePicker from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css";
import { workerApi } from "../../services/workerApi";
import { jobTypeApi } from "../../services/jobTypeApi";
import ScheduleSelector from "./ScheduleSelector";

const EditWorker = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const isNew = !id || id === 'new';

  const [worker, setWorker] = useState({
    name: "",
    email: "",
    password: "",
    hiredOn: new Date(),
    accesses: [],
    jobTypeIds: [],
    schedules: [],
    leaves: []
  });

  const [allAccesses, setAllAccesses] = useState([]);
  const [allJobTypes, setAllJobTypes] = useState([]);
  const [loading, setLoading] = useState(true);

  const [showLeavePopup, setShowLeavePopup] = useState(false);
  const [currentLeave, setCurrentLeave] = useState({ startDate: new Date(), endDate: new Date() });
  const [editingLeaveIndex, setEditingLeaveIndex] = useState(-1);

  useEffect(() => {
    const fetchData = async () => {
      try {
        const [accessesRes, jobTypesRes] = await Promise.all([
          workerApi.getAccesses(),
          jobTypeApi.getJobTypes()
        ]);
        setAllAccesses(accessesRes);
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
          setWorker(prev => ({
            ...prev,
            accesses: accessesRes.map(r => ({ ...r, isSelected: false }))
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

  const mergeSchedules = (schedules) => {
    const byDay = {};
    schedules.forEach(s => {
      if (!byDay[s.dayOfWeek]) byDay[s.dayOfWeek] = [];
      byDay[s.dayOfWeek].push(s);
    });

    let merged = [];

    Object.keys(byDay).forEach(day => {
      const daySchedules = byDay[day];
      daySchedules.sort((a, b) => parseInt(a.startTime) - parseInt(b.startTime));

      if (daySchedules.length === 0) return;

      let current = daySchedules[0];

      for (let i = 1; i < daySchedules.length; i++) {
        const next = daySchedules[i];

        const currentEnd = parseInt(current.endTime.split(':')[0]);
        const nextStart = parseInt(next.startTime.split(':')[0]);
        const nextEnd = parseInt(next.endTime.split(':')[0]);

        if (nextStart <= currentEnd) {
          if (nextEnd > currentEnd) {
            current.endTime = next.endTime;
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
      let updatedSchedules = [...worker.schedules, newEntry];

      updatedSchedules = mergeSchedules(updatedSchedules);

      setWorker({ ...worker, schedules: updatedSchedules });
      setSelectionStart(null);
    }
  };

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

  if (loading) return <div>Loading...</div>;

  return (
    <main className="main edit-worker">
      <div className="tile">
        <h3 className="tile-header">{isNew ? "New Worker" : "Edit Worker"}</h3>
        <form onSubmit={handleSave} className="worker-form">

          <div className="form-upper">
            <div className="form-column">
              <div className="form-section">
                <label>Name</label>
                <input type="text" value={worker.name} onChange={e => setWorker({ ...worker, name: e.target.value })} required />
              </div>
              <div className="form-section">
                <label>Email</label>
                <input type="email" value={worker.email} onChange={e => setWorker({ ...worker, email: e.target.value })} required />
              </div>
              <div className="form-section">
                <label>Password</label>
                <input type="password" value={worker.password} onChange={e => setWorker({ ...worker, password: e.target.value })} required={isNew} />
              </div>
              <div className="form-section">
                <label>Hired On</label>
                <DatePicker
                  selected={worker.hiredOn}
                  onChange={date => setWorker({ ...worker, hiredOn: date })}
                />
              </div>
            </div>

            <div className="form-section">
              <label>Access</label>
              <div className="list-container grow">
                {worker.accesses.map((access, idx) => (
                  <label key={access.id} className="checkbox-item">
                    <input
                      type="checkbox"
                      checked={access.isSelected}
                      onChange={e => {
                        const updatedAccesses = [...worker.accesses];
                        updatedAccesses[idx].isSelected = e.target.checked;
                        setWorker({ ...worker, accesses: updatedAccesses });
                      }}
                    />
                    {access.name}
                  </label>
                ))}
              </div>
            </div>

            <div className="form-section">
              <label>Job Types</label>
              <div className="list-container grow">
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

          <div className="form-lower">
            <div className="form-section">
              <label>Schedule</label>
              <ScheduleSelector
                schedules={worker.schedules}
                onChange={(newSchedules) => setWorker({ ...worker, schedules: newSchedules })}
              />
            </div>

            <div className="form-section">
              <div className="section-header">
                <label>Leaves</label>
                <button type="button" className="btn" onClick={() => openLeavePopup()}>+ Add Leave</button>
              </div>
              <div className="list-container max-height">
                {worker.leaves.length ? (
                  worker.leaves.map((leave, i) => (
                    <div
                      key={i}
                      className="list-item"
                      onClick={() => openLeavePopup(leave, i)}
                      style={{ cursor: 'pointer' }}
                    >
                      <span>
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

          <div className="form-footer">
            <button type="submit" className="btn">Save Worker</button>
          </div>
        </form>
      </div>
      {showLeavePopup && (
        <div className="popup-overlay" onClick={() => setShowLeavePopup(false)}>
          <div className="tile popup" onClick={e => e.stopPropagation()}>
            <h3>{editingLeaveIndex >= 0 ? "Edit Leave" : "Add Leave"}</h3>
            <div className="form-section">
              <label>Start Date</label>
              <DatePicker
                selected={currentLeave.startDate}
                onChange={date => setCurrentLeave({ ...currentLeave, startDate: date })}
              />
            </div>
            <div className="form-section">
              <label>End Date</label>
              <DatePicker
                selected={currentLeave.endDate}
                onChange={date => setCurrentLeave({ ...currentLeave, endDate: date })}
              />
            </div>
            <div className="form-footer">
              <button type="button" className="btn" onClick={handleAddOrUpdateLeave}>Save</button>
              <button type="button" className="btn" onClick={() => setShowLeavePopup(false)}>Cancel</button>
            </div>
          </div>
        </div>
      )}
    </main>

  );
};

export default EditWorker;
