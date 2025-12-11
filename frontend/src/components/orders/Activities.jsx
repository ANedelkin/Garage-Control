import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import Dropdown from '../common/Dropdown';  // Assuming you're using a common Dropdown component
import '../../assets/css/common.css';
import '../../assets/css/activities.css';

const Activities = () => {
  const [filter, setFilter] = useState('all');
  const [search, setSearch] = useState('');
  
  // Sample activities data
  const activities = [
    { name: 'Inspection', description: 'Routine checkup of vehicle parts', color: '#ffb74d' },
    { name: 'Repair', description: 'Fixing vehicle components', color: '#81c784' },
    { name: 'Maintenance', description: 'Oil change and other fluid replacements', color: '#64b5f6' },
    { name: 'Detailing', description: 'Cleaning and polishing vehicles', color: '#f06292' },
  ];

  const filteredActivities = activities.filter(activity => 
    (filter === 'all' || activity.name.toLowerCase().includes(filter.toLowerCase())) && 
    (activity.name.toLowerCase().includes(search.toLowerCase()) || 
    activity.description.toLowerCase().includes(search.toLowerCase()))
  );

  return (
    <main className="main">
      {/* Header: Search, Filter */}
      <div className="header">
        <input
          type="text"
          placeholder="Search activities..."
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
        <Link to="/activities/new" className="btn">+ New Activity</Link>
      </div>

      {/* Activities List */}
      <div className="activity-list">
        {filteredActivities.map((activity, index) => (
          <Link 
            to={`/activities/${activity.name.toLowerCase()}`} // Example routing to activity details page
            key={index}
            className="tile horizontal" 
            style={{ borderLeft: `5px solid ${activity.color}` }}
          >
            <div className="activity-content">
              <h3>{activity.name}</h3>
              <p>{activity.description}</p>
            </div>
            <button className="btn delete">
              <i className="fa-solid fa-trash"></i>
            </button>
          </Link>
        ))}
      </div>

      <footer>GarageFlow — Activities Management</footer>
    </main>
  );
};

export default Activities;
