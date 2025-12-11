import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import Dropdown from '../common/Dropdown';
import '../../assets/css/common.css';
import '../../assets/css/job-types.css';

const JobTypes = () => {
  const [filter, setFilter] = useState('all');
  const [search, setSearch] = useState('');

  // Sample job types data
  const jobTypes = [
    { name: 'Inspection', description: 'Routine checkup of vehicle parts', color: '#ffb74d' },
    { name: 'Repair', description: 'Fixing vehicle components', color: '#81c784' },
    { name: 'Maintenance', description: 'Oil change and other fluid replacements', color: '#64b5f6' },
    { name: 'Detailing', description: 'Cleaning and polishing vehicles', color: '#f06292' },
  ];

  const filteredJobTypes = jobTypes.filter(jobType =>
    (filter === 'all' || jobType.name.toLowerCase().includes(filter.toLowerCase())) &&
    (jobType.name.toLowerCase().includes(search.toLowerCase()) ||
      jobType.description.toLowerCase().includes(search.toLowerCase()))
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
        <Dropdown value={filter} onChange={e => setFilter(e.target.value)}>
          <option value="all">All</option>
          <option value="inspection">Inspection</option>
          <option value="repair">Repair</option>
          <option value="maintenance">Maintenance</option>
          <option value="detailing">Detailing</option>
        </Dropdown>
        <Link to="/job-types/new" className="btn">+ New Job Type</Link>
      </div>

      {/* Job Types List */}
      <div className="job-type-list">
        {filteredJobTypes.map((jobType, index) => (
          <Link
            to={`/job-types/${jobType.name.toLowerCase()}`}
            key={index}
            className="tile horizontal"
            style={{ borderLeft: `5px solid ${jobType.color}` }}
          >
            <div className="job-type-content">
              <h3>{jobType.name}</h3>
              <p>{jobType.description}</p>
            </div>
            <button className="icon-btn delete">
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
