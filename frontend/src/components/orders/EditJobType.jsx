import React, { useState, useEffect, useRef } from 'react';
import { useParams } from "react-router-dom";
import '../../assets/css/job-types.css';
import { jobTypeApi } from '../../services/jobTypeApi.js';

const EditJobType = () => {
  const { id } = useParams();
  const [jobTypeData, setJobTypeData] = useState(null);
  const [isNew, setIsNew] = useState(id === '');
  const [newMechanic, setNewMechanic] = useState('');

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
        mechanics: [],
      });
    }
  }, []);

  const handleFormSubmit = (e) => {
    e.preventDefault();
    jobTypeApi.editJobType(jobTypeData);
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
          <div className="horizontal grow">

            <div className="form-left">
              <div className="horizontal">
                <div className="form-section form-section-name grow">
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

                <div className="header">
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

                <div className="list-container max-height">
                  {jobTypeData.mechanics.length === 0 ? (
                    <div className="list-empty">
                      No mechanics assigned
                    </div>
                  ) : (
                    jobTypeData.mechanics.map((m, i) => {
                      return (
                        <div key={i} className="list-item">
                          {m}
                          <button
                            type="button"
                            className="btn delete icon-btn"
                            onClick={() => {
                              const updated = jobTypeData.mechanics.filter(
                                (_, idx) => idx !== i
                              );
                              setJobTypeData({ ...jobTypeData, mechanics: updated });
                            }}
                          >
                            <i className="fa-solid fa-trash"></i>
                          </button>
                        </div>
                      );
                    })
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
