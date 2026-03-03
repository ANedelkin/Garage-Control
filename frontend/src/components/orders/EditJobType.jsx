import React, { useState, useEffect, useRef } from 'react';
import { useParams, useNavigate } from "react-router-dom";
import Suggestions from '../common/Suggestions';
import '../../assets/css/job-types.css';
import { jobTypeApi } from '../../services/jobTypeApi.js';
import { workerApi } from '../../services/workerApi.js';

const EditJobType = () => {
  const { id } = useParams();
  const isNew = !id || id === 'new';
  const navigate = useNavigate();
  const [jobTypeData, setJobTypeData] = useState(null);
  const [newMechanic, setNewMechanic] = useState('');
  const [workers, setWorkers] = useState([]);
  const [showSuggestions, setShowSuggestions] = useState(false);
  const [mechanicSuggestions, setMechanicSuggestions] = useState([]);
  const suggestionsRef = useRef(null);

  useEffect(() => {
    workerApi.getWorkers()
      .then(res => setWorkers(res))
      .catch(err => console.error("Error fetching workers:", err));
  }, []);

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
  }, [id, isNew]);

  const handleFormSubmit = (e) => {
    e.preventDefault();
    const action = isNew ? jobTypeApi.addJobType(jobTypeData) : jobTypeApi.editJobType(id, jobTypeData);

    action
      .then(() => navigate('/job-types'))
      .catch(error => {
        console.error("Error saving job type:", error);
        alert(error.message || "Failed to save job type");
      });
  };

  const handleAddMechanic = (mechanicName) => {
    const name = (mechanicName && typeof mechanicName === 'object' ? mechanicName.name : mechanicName) || newMechanic;
    if (!name.trim()) return;

    // Prevent duplicates
    if (jobTypeData.mechanics.includes(name.trim())) {
      setNewMechanic('');
      setShowSuggestions(false);
      return;
    }

    setJobTypeData({
      ...jobTypeData,
      mechanics: [...jobTypeData.mechanics, name.trim()],
    });
    setNewMechanic('');
    setShowSuggestions(false);
    setMechanicSuggestions([]);
  };

  const handleMechanicSearch = (val) => {
    setNewMechanic(val);

    if (!val.trim()) {
      setMechanicSuggestions([]);
      setShowSuggestions(false);
      return;
    }

    const filtered = workers.filter(w =>
      w.name.toLowerCase().includes(val.toLowerCase()) &&
      !jobTypeData.mechanics.includes(w.name)
    );

    setMechanicSuggestions(filtered);
    setShowSuggestions(true);
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
              <div className="form-section">
                <label>Mechanics</label>

                <div className="header suggestion-wrapper">
                  <input
                    type="text"
                    placeholder="Enter mechanic name"
                    value={newMechanic}
                    onChange={(e) => handleMechanicSearch(e.target.value)}
                    onFocus={() => newMechanic && setShowSuggestions(true)}
                    onBlur={() => setTimeout(() => setShowSuggestions(false), 200)}
                    onKeyDown={(e) => {
                      if (e.key === 'Enter') {
                        e.preventDefault();
                        if (showSuggestions && mechanicSuggestions.length > 0) {
                          suggestionsRef.current?.handleKeyDown(e);
                        } else {
                          handleAddMechanic();
                        }
                      } else {
                        suggestionsRef.current?.handleKeyDown(e);
                      }
                    }}
                  />
                  <Suggestions
                    ref={suggestionsRef}
                    suggestions={showSuggestions ? mechanicSuggestions : []}
                    isOpen={showSuggestions && mechanicSuggestions.length > 0}
                    onSelect={handleAddMechanic}
                    onClose={() => setShowSuggestions(false)}
                    renderItem={(worker) => worker.name}
                    maxHeight="200px"
                    style={{ width: '100%' }}
                  />
                  <button
                    type="button"
                    className="btn"
                    onClick={() => handleAddMechanic()}
                  >
                    Add
                  </button>
                </div>

              </div>
              <div className="list-container max-height form-section">
                {jobTypeData.mechanics.length === 0 ? (
                  <div className="list-empty">
                    No mechanics assigned
                  </div>
                ) : (
                  jobTypeData.mechanics.map((m, i) => {
                    return (
                      <div key={i} className="list-item">
                        <span className="item-label">{m}</span>
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
