import React, { useState, useEffect } from "react";
import { useAuth } from "../../context/AuthContext";
import { usePopup } from "../../context/PopupContext";
import "../../assets/css/common/popup.css";
import "../../assets/css/common/layout.css";
import "../../assets/css/common/colors.css";
import "../../assets/css/workers.css";
import DatePicker from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css";
import { workerApi } from "../../services/workerApi";
import { jobTypeApi } from "../../services/jobTypeApi";
import LeavePopup from "./LeavePopup";
import WorkhoursPopup from "./WorkhoursPopup";

const EditWorker = ({ id, onClose, onSave }) => {
  const { user, refreshAuth } = useAuth();
  const isNew = !id || id === "new";

  const [worker, setWorker] = useState({
    name: "",
    username: "",
    email: "",
    password: "",
    hiredOn: new Date(),
    accesses: [],
    jobTypeIds: [],
    schedules: [],
    leaves: []
  });

  const [allJobTypes, setAllJobTypes] = useState([]);
  const [loading, setLoading] = useState(true);

  const { addPopup, removeLastPopup } = usePopup();
  const [editingLeaveIndex, setEditingLeaveIndex] = useState(-1);

  useEffect(() => {
    const fetchData = async () => {
      try {
        const [accessesRes, jobTypesRes] = await Promise.all([
          workerApi.getAccesses(),
          jobTypeApi.getJobTypes()
        ]);
        setAllJobTypes(jobTypesRes);

        if (!isNew) {
          const workerRes = await workerApi.getWorker(id);
          // Parse dates
          workerRes.hiredOn = new Date(workerRes.hiredOn);
          workerRes.leaves = workerRes.leaves.map((l) => ({
            ...l,
            startDate: new Date(l.startDate),
            endDate: new Date(l.endDate)
          }));
          setWorker(workerRes);
        } else {
          setWorker((prev) => ({
            ...prev,
            accesses: accessesRes.map((r) => ({ ...r, isSelected: false }))
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
      if (isNew) {
        await workerApi.create(worker);
      } else {
        await workerApi.edit(id, worker);
      }

      if (user && user.workerId === id) {
        await refreshAuth();
      }

      onSave();
      onClose();
    } catch (error) {
      console.error("Error saving worker", error);
      alert(error.message || "Failed to save worker");
    } finally { }
  };

  if (loading) return <div>Loading...</div>;

  return (
    <div className="edit-worker">
      <form onSubmit={handleSave} className="worker-form">
        <div className="form-upper">
          <div className="form-column">
            <div className="form-section">
              <label>Name</label>
              <input
                type="text"
                value={worker.name}
                onChange={(e) => setWorker({ ...worker, name: e.target.value })}
                required
              />
            </div>
            <div className="form-section">
              <label>Username</label>
              <input
                type="text"
                value={worker.username || ""}
                onChange={(e) => setWorker({ ...worker, username: e.target.value })}
                required
              />
            </div>
            <div className="form-section">
              <label>Password</label>
              <input
                type="password"
                value={worker.password}
                onChange={(e) => setWorker({ ...worker, password: e.target.value })}
                required={isNew}
              />
            </div>
            <div className="form-section">
              <label>Email (Optional)</label>
              <input
                type="email"
                value={worker.email || ""}
                onChange={(e) => setWorker({ ...worker, email: e.target.value })}
              />
            </div>
            <div className="form-section">
              <label>Hired On</label>
              <DatePicker
                selected={worker.hiredOn}
                onChange={(date) => setWorker({ ...worker, hiredOn: date })}
              />
            </div>
          </div>

          <div className="form-section">
            <label>Access</label>
            <div className="list-container grow">
              {worker.accesses.map((access, idx) => (
                <div className="list-item" key={access.id}>
                  <label className="checkbox-item">
                    <input
                      type="checkbox"
                      checked={access.isSelected}
                      onChange={(e) => {
                        const updatedAccesses = [...worker.accesses];
                        updatedAccesses[idx].isSelected = e.target.checked;
                        setWorker({ ...worker, accesses: updatedAccesses });
                      }}
                    />
                    {access.name}
                  </label>
                </div>
              ))}
            </div>
          </div>

          <div className="form-section">
            <label>Job Types</label>
            <div className="list-container grow">
              {allJobTypes.map((jt) => (
                <div className="list-item" key={jt.id}>
                  <label className="checkbox-item">
                    <input
                      type="checkbox"
                      checked={worker.jobTypeIds.includes(jt.id)}
                      onChange={(e) => {
                        let updated = [...worker.jobTypeIds];
                        if (e.target.checked) updated.push(jt.id);
                        else updated = updated.filter((id) => id !== jt.id);
                        setWorker({ ...worker, jobTypeIds: updated });
                      }}
                    />
                    {jt.name}
                  </label>
                </div>
              ))}
            </div>
          </div>
        </div>
        {/* 
          <div className="form-lower">
            
          </div> */}

        <div className="form-footer">
          <button type="submit" className="btn">
            Save Worker
          </button>
        </div>
      </form >
    </div >
  );
};

export default EditWorker;
