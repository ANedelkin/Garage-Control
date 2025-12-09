import React, { useState, useEffect, useRef } from 'react';
import '../../assets/css/activities.css';
import { activityApi } from '../../services/activityApi.js';

const EditActivity = ({ id = undefined }) => {
  const [activityData, setActivityData] = useState(null);
  const [isNew, setIsNew] = useState(id === '');
  const [newMechanic, setNewMechanic] = useState('');
  const colorInputRef = useRef(null);

  useEffect(() => {
    if (!isNew) {
      activityApi
        .getActivity(id)
        .then((res) => {
          res.id = id;
          setActivityData(res);
        })
        .catch((err) => {
          console.error('Error fetching activity details:', err);
        });
    } else {
      setActivityData({
        name: '',
        description: '',
        color: '#000000',
        mechanics: [],
      });
    }
  }, []);

  const handleFormSubmit = (e) => {
    e.preventDefault();
    activityApi.editActivity(activityData);
  };

  const openColorPicker = () => {
    if (colorInputRef.current) colorInputRef.current.click();
  };

  const handleAddMechanic = () => {
    if (newMechanic.trim() === '') return;
    setActivityData({
      ...activityData,
      mechanics: [...activityData.mechanics, newMechanic.trim()],
    });
    setNewMechanic('');
  };

  const handleDeleteMechanic = (index) => {
    const updated = [...activityData.mechanics];
    updated.splice(index, 1);
    setActivityData({ ...activityData, mechanics: updated });
  };

  if (!activityData) return <div>Loading...</div>;

  return (
    <main className="main container">
      <div className="tile">
        <h3 className="tile-header">Activity Information</h3>
        <form onSubmit={handleFormSubmit}>
          {/* Left + Right halves */}
          <div className="activity-edit-tile">

            {/* Left side */}
            <div className="form-left">
              {/* Example: Name + Color + Description */}
              <div className="form-row">
                <div className="form-section ratioed">
                  <label htmlFor="name">Activity Name</label>
                  <input
                    type="text"
                    id="name"
                    placeholder="Enter activity name"
                    value={activityData.name}
                    onChange={(e) =>
                      setActivityData({ ...activityData, name: e.target.value })
                    }
                  />
                </div>

                <div className="form-section ratioed">
                  <label>Color</label>
                  <input
                    ref={colorInputRef}
                    type="color"
                    value={activityData.color}
                    onChange={(e) =>
                      setActivityData({ ...activityData, color: e.target.value })
                    }
                    style={{ display: 'none' }}
                  />
                  <button
                    type="button"
                    className="color-button"
                    onClick={() => colorInputRef.current.click()}
                    style={{
                      backgroundColor: activityData.color,
                      color: '#fff',
                      border: '1px solid var(--border2)',
                      textTransform: 'uppercase',
                    }}
                  >
                    {activityData.color}
                  </button>
                </div>
              </div>

              <div className="form-section">
                <label htmlFor="description">Description</label>
                <textarea
                  id="description"
                  className="description-large"
                  placeholder="Enter activity description"
                  value={activityData.description}
                  onChange={(e) =>
                    setActivityData({ ...activityData, description: e.target.value })
                  }
                />
              </div>
            </div>

            {/* Right side */}
            <div className="form-right">
              <label>Mechanics</label>

              {/* Input + Add button */}
              <div className="mechanics-input-row">
                <input
                  type="text"
                  placeholder="Enter mechanic name"
                  value={newMechanic}
                  onChange={(e) => setNewMechanic(e.target.value)}
                />
                <button
                  type="button"
                  onClick={() => {
                    if (!newMechanic.trim()) return;
                    setActivityData({
                      ...activityData,
                      mechanics: [...activityData.mechanics, newMechanic.trim()],
                    });
                    setNewMechanic('');
                  }}
                >
                  Add
                </button>
              </div>

              {/* Mechanics list */}
              <div className="mechanics-list-container">
                {activityData.mechanics.length === 0 ? (
                  <div className="mechanics-empty">
                    no mechanics assigned to this activity
                  </div>
                ) : (
                  <table className="mechanics-table">
                    <tbody>
                      {activityData.mechanics.map((m, i) => (
                        <tr key={i}>
                          <td>{m}</td>
                          <td>
                            <button
                              type="button"
                              className="btn-delete"
                              onClick={() => {
                                const updated = activityData.mechanics.filter(
                                  (_, idx) => idx !== i
                                );
                                setActivityData({ ...activityData, mechanics: updated });
                              }}
                            >
                              <i className="fa-solid fa-trash"></i>
                            </button>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                )}
              </div>
            </div>
          </div>


          {/* Submit */}
          <div className="form-footer" style={{ marginTop: '16px' }}>
            <button type="submit" className="btn">
              Done
            </button>
          </div>
        </form>
      </div>
    </main>
  );
};

export default EditActivity;
