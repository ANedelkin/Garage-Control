import React, { useState, useEffect, useRef } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import Dropdown from '../common/Dropdown';
import '../../assets/css/common/table.css';
import '../../assets/css/job-types.css';
import { jobTypeApi } from '../../services/jobTypeApi.js';
import usePageTitle from '../../hooks/usePageTitle.js';
import { usePopup } from '../../context/PopupContext';
import ConfirmationPopup from '../common/ConfirmationPopup';
import ExcelExportButton from '../common/ExcelExportButton';
import PdfExportButton from '../common/PdfExportButton';

const JobTypes = () => {
    const { addPopup, removeLastPopup } = usePopup();
    usePageTitle('Job Types');
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
        <div style={{ display: 'flex', gap: '5px' }}>
          <ExcelExportButton endpoint="export/job-types" />
          <PdfExportButton endpoint="export/job-types" />
        </div>
      </div>

      {/* Job Types table */}
      <div className="tile">
        <h3>Job Types</h3>
        <div style={{ overflowX: 'auto' }}>
          <table>
            <colgroup>
              <col style={{ width: '250px' }} />
              <col className="hide-md" />
              <col style={{ width: '70px' }} />
            </colgroup>

            <thead>
              <tr>
                <th>Name</th>
                <th className="hide-sm">Description</th>
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
                  <td className="description hide-sm" title={jobType.description}>
                    {jobType.description}
                  </td>
                  <td onClick={e => e.stopPropagation()}>
                    <button className="btn delete icon-btn" onClick={(e) => {
                      addPopup(
                        'Delete Job Type',
                        <ConfirmationPopup 
                          message={`Are you sure you want to delete job type "${jobType.name}"?`}
                          confirmText="Delete"
                          isDanger={true}
                          onConfirm={async () => {
                            await jobTypeApi.deleteJobType(jobType.id);
                            setJobTypes(jobTypes.filter(jt => jt.id !== jobType.id));
                            removeLastPopup();
                          }}
                          onClose={removeLastPopup}
                        />
                      );
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
