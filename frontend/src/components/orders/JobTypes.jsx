import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import Dropdown from '../common/Dropdown';
import '../../assets/css/job-types.css';
import { jobTypeApi } from '../../services/jobTypeApi.js';

const JobTypes = () => {
  const [filter, setFilter] = useState('all');
  const [search, setSearch] = useState('');
  const [jobTypes, setJobTypes] = useState([]);

  useEffect(() => {
    jobTypeApi.getJobTypes().then(res => {
      setJobTypes(res);
    }).catch(err => {
      console.error("Failed to fetch job types", err);
    });
  }, []);

  const filteredJobTypes = jobTypes.filter(jobType =>
    (filter === 'all' || jobType.name.toLowerCase().includes(filter.toLowerCase())) &&
    (jobType.name.toLowerCase().includes(search.toLowerCase()) ||
      jobType.description && jobType.description.toLowerCase().includes(search.toLowerCase()))
  );

  return (
    <main className="main">
      {/* Header: Search, Filter */}
      <div className="header">
        <input
          type="text"
          placeholder="Search job types..."
          value={search}
          onChange={e => setSearch(e.target.value)}
        />
        <Link to="/job-types/new" className="btn">+ New Job Type</Link>
      </div>

      {/* Job Types List */}
      <div className="job-type-list">
        {filteredJobTypes.map((jobType, index) => (
          <Link
            to={`/job-types/${jobType.id}`}
            key={jobType.id || index}
            className="tile horizontal"
          >
            <div className="job-type-content">
              <h3>{jobType.name}</h3>
              <p>{jobType.description}</p>
            </div>
            <button className="icon-btn delete btn">
              <i className="fa-solid fa-trash"></i>
            </button>
          </Link>
        ))}
      </div>

      <footer>GarageFlow — Job Types Management</footer>
    </main>
  );
};

export default JobTypes;
