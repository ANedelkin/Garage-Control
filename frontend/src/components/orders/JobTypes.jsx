import React, { useState, useEffect, useRef } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import Dropdown from '../common/Dropdown';
import '../../assets/css/common/table.css';
import '../../assets/css/job-types.css';
import { jobTypeApi } from '../../services/jobTypeApi.js';

const JobTypes = () => {
  const navigate = useNavigate();
  const [filter, setFilter] = useState('all');
  const [search, setSearch] = useState('');
  const [jobTypes, setJobTypes] = useState([]);
  const [searchParams] = useSearchParams();
  const highlightId = searchParams.get('highlightId');
  const rowRefs = useRef({});

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

  useEffect(() => {
    if (highlightId) {
      const row = rowRefs.current[highlightId];
      if (row) {
        row.scrollIntoView({ behavior: 'smooth', block: 'center' });
      }
    }
  }, [highlightId, jobTypes]);

  const handleContainerClick = () => {
    if (highlightId) {
      navigate('/job-types', { replace: true });
    }
  };

  return (
    <main className="main" onClick={handleContainerClick}>
      {/* Header: search + new job type */}
      <div className="header">
        <input
          type="text"
          placeholder="Search job types..."
          value={search}
          onChange={e => setSearch(e.target.value)}
        />
        <Link className="btn" to="/job-types/new">+ New Job Type</Link>
      </div>

      {/* Job Types table */}
      <div className="tile">
        <h3>Job Types</h3>
        <div style={{ overflowX: 'auto' }}>
          <table>
            <colgroup>
              <col style={{ width: '250px' }} />
              <col />
              <col style={{ width: '70px' }} />
            </colgroup>

            <thead>
              <tr>
                <th>Name</th>
                <th>Description</th>
                <th></th>
              </tr>
            </thead>

            <tbody>
              {filteredJobTypes.map((jobType, index) => (
                <tr 
                  key={jobType.id || index} 
                  ref={el => rowRefs.current[jobType.id] = el}
                  onClick={(e) => { e.stopPropagation(); navigate(`/job-types/${jobType.id}`); }}
                  className={highlightId === jobType.id ? 'highlight-outline' : ''}
                >
                  <td>{jobType.name}</td>
                  <td className="description" title={jobType.description}>
                    {jobType.description}
                  </td>
                  <td onClick={e => e.stopPropagation()}>
                    <button className="btn delete icon-btn" onClick={async (e) => {
                      if (window.confirm('Delete job type?')) {
                        await jobTypeApi.deleteJobType(jobType.id);
                        setJobTypes(jobTypes.filter(jt => jt.id !== jobType.id));
                      }
                    }}>
                      <i className="fa-solid fa-trash"></i>
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      <footer>GarageFlow — Job Types Management</footer>
    </main>
  );
};

export default JobTypes;
