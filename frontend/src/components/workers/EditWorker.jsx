import React, { useState } from "react";
import "../../assets/css/workers.css";
import { DatePicker } from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css"; // Make sure to install react-datepicker

const EditWorker = () => {
  // States for worker data
  const [workerName, setWorkerName] = useState("");
  const [password, setPassword] = useState("");
  const [roles, setRoles] = useState({
    partsStockManager: false,
    mechanic: false,
  });
  const [abilities, setAbilities] = useState({
    tyreChange: false,
    repair: false,
  });
  const [workHours, setWorkHours] = useState({
    monday: { start: "", end: "" },
    tuesday: { start: "", end: "" },
    wednesday: { start: "", end: "" },
    thursday: { start: "", end: "" },
    friday: { start: "", end: "" },
    saturday: { start: "", end: "" },
    sunday: { start: "", end: "" },
  });
  const [leaves, setLeaves] = useState([]);
  const [newLeave, setNewLeave] = useState({ startDate: null, endDate: null, isPaid: false });
  const [showLeavePopup, setShowLeavePopup] = useState(false);

  const handleRoleChange = (e) => {
    setRoles({ ...roles, [e.target.name]: e.target.checked });
  };

  const handleAbilityChange = (e) => {
    setAbilities({ ...abilities, [e.target.name]: e.target.checked });
  };

  const handleWorkHourChange = (e, day) => {
    const { name, value } = e.target;
    setWorkHours({
      ...workHours,
      [day]: { ...workHours[day], [name]: value },
    });
  };

  const handleLeaveChange = (e) => {
    const { name, checked } = e.target;
    setNewLeave({ ...newLeave, [name]: checked });
  };

  const handleAddLeave = () => {
    setLeaves([...leaves, newLeave]);
    setNewLeave({ startDate: null, endDate: null, isPaid: false });
    setShowLeavePopup(false);
  };

  return (
    <div className="edit-worker-page">
      <h2>Edit Worker</h2>

      {/* Worker Info Section */}
      <div className="worker-info">
        <div className="form-group">
          <label>Name</label>
          <input
            type="text"
            value={workerName}
            onChange={(e) => setWorkerName(e.target.value)}
            placeholder="Enter worker's name"
          />
        </div>
        <div className="form-group">
          <label>Password</label>
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder="Enter worker's password"
          />
        </div>
      </div>

      {/* Roles Section */}
      <div className="roles-section">
        <h3>Roles</h3>
        <label>
          <input
            type="checkbox"
            name="partsStockManager"
            checked={roles.partsStockManager}
            onChange={handleRoleChange}
          />
          Parts Stock Manager
        </label>
        <label>
          <input
            type="checkbox"
            name="mechanic"
            checked={roles.mechanic}
            onChange={handleRoleChange}
          />
          Mechanic
        </label>
      </div>

      {/* Abilities Section (only if mechanic role is selected) */}
      {roles.mechanic && (
        <div className="abilities-section">
          <h3>Abilities</h3>
          <label>
            <input
              type="checkbox"
              name="tyreChange"
              checked={abilities.tyreChange}
              onChange={handleAbilityChange}
            />
            Tyre Change
          </label>
          <label>
            <input
              type="checkbox"
              name="repair"
              checked={abilities.repair}
              onChange={handleAbilityChange}
            />
            Repair
          </label>
        </div>
      )}

      {/* Work Hours Section */}
      <div className="work-hours-section">
        <h3>Work Hours</h3>
        <table>
          <thead>
            <tr>
              <th>Day</th>
              <th>Start Time</th>
              <th>End Time</th>
            </tr>
          </thead>
          <tbody>
            {Object.keys(workHours).map((day) => (
              <tr key={day}>
                <td>{day.charAt(0).toUpperCase() + day.slice(1)}</td>
                <td>
                  <input
                    type="time"
                    name="start"
                    value={workHours[day].start}
                    onChange={(e) => handleWorkHourChange(e, day)}
                  />
                </td>
                <td>
                  <input
                    type="time"
                    name="end"
                    value={workHours[day].end}
                    onChange={(e) => handleWorkHourChange(e, day)}
                  />
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Leave Management Section */}
      <div className="leave-management">
        <h3>Leave Management</h3>
        <ul>
          {leaves.map((leave, index) => (
            <li key={index}>
              {`Leave from ${new Date(leave.startDate).toLocaleDateString()} to ${new Date(
                leave.endDate
              ).toLocaleDateString()} (${leave.isPaid ? "Paid" : "Unpaid"})`}
            </li>
          ))}
        </ul>
        <button onClick={() => setShowLeavePopup(true)}>New Leave</button>
      </div>

      {/* Leave Popup */}
      {showLeavePopup && (
        <div className="leave-popup">
          <h4>New Leave</h4>
          <div>
            <label>Start Date</label>
            <DatePicker
              selected={newLeave.startDate}
              onChange={(date) => setNewLeave({ ...newLeave, startDate: date })}
              dateFormat="yyyy/MM/dd"
            />
          </div>
          <div>
            <label>End Date</label>
            <DatePicker
              selected={newLeave.endDate}
              onChange={(date) => setNewLeave({ ...newLeave, endDate: date })}
              dateFormat="yyyy/MM/dd"
            />
          </div>
          <div>
            <label>
              <input
                type="checkbox"
                name="isPaid"
                checked={newLeave.isPaid}
                onChange={handleLeaveChange}
              />
              Paid Leave
            </label>
          </div>
          <button onClick={handleAddLeave}>Add Leave</button>
          <button onClick={() => setShowLeavePopup(false)}>Close</button>
        </div>
      )}
    </div>
  );
};

export default EditWorker;
