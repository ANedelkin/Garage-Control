import React, { useState, useEffect, useRef } from 'react';
import '../../assets/css/job-types.css';
import { jobTypeApi } from '../../services/jobTypeApi.js';

const EditJobType = ({ id = undefined }) => {
  const [jobTypeData, setJobTypeData] = useState(null);
  const [isNew, setIsNew] = useState(id === '');
  const [newMechanic, setNewMechanic] = useState('');
  const colorInputRef = useRef(null);

  useEffect(() => {
    if (!isNew) {
      jobTypeApi
        .getJobType(id)
        .then((res) => {
          res.id = id;
          setJobTypeData(res);
        })
        .catch((err) => {
          console.error('Error fetching job type details:', err);
        });
    } else {
      setJobTypeData({
        name: '',
        description: '',
        color: '#000000',
        mechanics: [],
      });
    }
  }, []);

  const handleFormSubmit = (e) => {
    e.preventDefault();
    jobTypeApi.editJobType(jobTypeData);
  };

  const openColorPicker = () => {
    if (colorInputRef.current) colorInputRef.current.click();
  };

  const handleAddMechanic = () => {
    if (newMechanic.trim() === '') return;
    setJobTypeData({
      ...jobTypeData,
      mechanics: [...jobTypeData.mechanics, newMechanic.trim()],
    });
    setNewMechanic('');
  };

  const handleDeleteMechanic = (index) => {
    const updated = [...jobTypeData.mechanics];
    updated.splice(index, 1);
    setJobTypeData({ ...jobTypeData, mechanics: updated });
  };

  if (!jobTypeData) return <div>Loading...</div>;

  return (
    <main className="main container job-type-edit">
      <div className="tile">
        <h3 className="tile-header">Job Type Information</h3>
        <form onSubmit={handleFormSubmit} className="job-type-form">
          <div className="form-row form-main">

            <div className="form-left">
              <div className="form-row">
                <div className="form-section form-section-name">
                  <label htmlFor="name">Job Type Name</label>
                  <input
                    type="text"
                    id="name"
                    placeholder="Enter job type name"
                    value={jobTypeData.name}
                    onChange={(e) =>
                      setJobTypeData({ ...jobTypeData, name: e.target.value })
                    }
                  />
                </div>

                <div className="form-section">
                  <label>Color</label>
                  <input
                    ref={colorInputRef}
                    type="color"
                    value={jobTypeData.color}
                    onChange={(e) =>
                      setJobTypeData({ ...jobTypeData, color: e.target.value })
                    }
                    style={{ display: 'none' }}
                  />
                  <button
                    type="button"
                    className="color-button"
                    onClick={() => colorInputRef.current.click()}
                    style={{
                      backgroundColor: jobTypeData.color,
                      color: '#fff',
                      border: '1px solid var(--border2)',
                      textTransform: 'uppercase',
                    }}
                  >
                    {jobTypeData.color}
                  </button>
                </div>
              </div>

              <div className="form-section max-height">
                <label htmlFor="description">Description</label>
                <textarea
                  id="description"
                  className="description"
                  placeholder="Enter job type description"
                  value={jobTypeData.description}
                  onChange={(e) =>
                    setJobTypeData({ ...jobTypeData, description: e.target.value })
                  }
                />
              </div>
            </div>

            <div className="form-right">
              <div className="form-section max-height max-width">
                <label>Mechanics</label>

                <div className="mechanics-input-row">
                  <input
                    type="text"
                    placeholder="Enter mechanic name"
                    value={newMechanic}
                    onChange={(e) => setNewMechanic(e.target.value)}
                  />
                  <button
                    type="button"
                    className="btn"
                    onClick={() => {
                      if (!newMechanic.trim()) return;
                      setJobTypeData({
                        ...jobTypeData,
                        mechanics: [...jobTypeData.mechanics, newMechanic.trim()],
                      });
                      setNewMechanic('');
                    }}
                  >
                    Add
                  </button>
                </div>

                <div className="mechanics-list-container max-height">
                  {jobTypeData.mechanics.length === 0 ? (
                    <div className="mechanics-empty">
                      no mechanics assigned
                    </div>
                  ) : (
                    <table className="mechanics-table">
                      <tbody>
                        {jobTypeData.mechanics.map((m, i) => (
                          <tr key={i}>
                            <td>{m}</td>
                            <td>
                              <button
                                type="button"
                                className="btn delete"
                                onClick={() => {
                                  const updated = jobTypeData.mechanics.filter(
                                    (_, idx) => idx !== i
                                  );
                                  setJobTypeData({ ...jobTypeData, mechanics: updated });
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

export default EditJobType;
