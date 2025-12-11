import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import Dropdown from '../common/Dropdown';
import '../../assets/css/common.css';
import '../../assets/css/workers.css';

const Workers = () => {
    const [roleFilter, setRoleFilter] = useState('all');
    const [search, setSearch] = useState('');

    // Example data
    const workers = [
        { name: 'Genco Gencin', roles: ['Mechanic', 'Diagnostics'], hiredOn: '2023-04-12' },
        { name: 'Mike Smith', roles: ['Mechanic'], hiredOn: '2024-01-08' },
        { name: 'Anna L.', roles: ['Inspection'], hiredOn: '2022-11-20' },
        { name: 'Toni B.', roles: ['Bodywork', 'Painting'], hiredOn: '2023-09-01' },
    ];

    // Unique roles for filter dropdown
    const allRoles = Array.from(new Set(workers.flatMap(w => w.roles)));

    const filteredWorkers = workers.filter(w =>
        (roleFilter === 'all' || w.roles.includes(roleFilter)) &&
        w.name.toLowerCase().includes(search.toLowerCase())
    );

    return (
        <main className="main">
            {/* Header: search + filter + new worker */}
            <div className="header">
                <input
                    type="text"
                    placeholder="Search workers..."
                    value={search}
                    onChange={e => setSearch(e.target.value)}
                />

                <Dropdown value={roleFilter} onChange={e => setRoleFilter(e.target.value)}>
                    <option value="all">All Roles</option>
                    {allRoles.map((r, i) => (
                        <option value={r} key={i}>{r}</option>
                    ))}
                </Dropdown>

                <Link className="btn" to="/workers/new">+ New Worker</Link>
            </div>

            {/* Workers table */}
            <div className="tile">
                <h3>Workers</h3>
                <div style={{ overflowX: 'auto' }}>
                    <table>
                        <colgroup>
                            <col style={{ width: '200px' }} />
                            <col />                          
                            <col style={{ width: '140px' }} /> 
                            <col style={{ width: '70px' }} />  
                        </colgroup>

                        <thead>
                            <tr>
                                <th>Name</th>
                                <th>Roles</th>
                                <th>Hired On</th>
                                <th></th>
                            </tr>
                        </thead>

                        <tbody>
                            {filteredWorkers.map((w, i) => (
                                <tr key={i}>
                                    <td>{w.name}</td>

                                    <td className="description" title={w.roles.join(', ')}>
                                        {w.roles.join(', ')}
                                    </td>

                                    <td>{w.hiredOn}</td>

                                    <td>
                                        <button className="btn delete">
                                            <i className="fa-solid fa-trash"></i>
                                        </button>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            </div>

            <footer>GarageFlow — Workers Management</footer>
        </main>
    );
};

export default Workers;
