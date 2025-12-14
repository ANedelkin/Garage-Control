import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import Dropdown from '../common/Dropdown';
import '../../assets/css/common.css';
import '../../assets/css/workers.css';
import { workerApi } from '../../services/workerApi';

const Workers = () => {
    const [roleFilter, setRoleFilter] = useState('all');
    const [search, setSearch] = useState('');

    const [loading, setLoading] = useState(true);
    const [workers, setWorkers] = useState([]);

    useEffect(() => {
        const fetchWorkers = async () => {
            try {
                const data = await workerApi.getWorkers();
                setWorkers(data);
            } catch (error) {
                console.error("Failed to fetch workers", error);
            } finally {
                setLoading(false);
            }
        };
        fetchWorkers();
    }, []);

    // Unique roles for filter dropdown (assuming backend sends roles as objects with Name)
    // const allRoles = Array.from(new Set(workers.flatMap(w => w.roles.map(r => r.name))));

    const filteredWorkers = workers.filter(w =>
        (roleFilter === 'all' || w.roles.some(r => r.name === roleFilter)) &&
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

                {/* <Dropdown value={roleFilter} onChange={e => setRoleFilter(e.target.value)}>
                    <option value="all">All Roles</option>
                    {allRoles.map((r, i) => (
                        <option value={r} key={i}>{r}</option>
                    ))}
                </Dropdown> */}

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
                            {loading ? <tr><td colSpan="4">Loading...</td></tr> : filteredWorkers.map((w, i) => (
                                <tr key={w.id} onClick={() => window.location.href = `/workers/${w.id}`} style={{ cursor: 'pointer' }}>
                                    <td>{w.name}</td>

                                    {/* <td className="description" title={w.roles.map(r => r.name).join(', ')}>
                                        {w.roles.map(r => r.name).join(', ')}
                                    </td> */}

                                    <td>{new Date(w.hiredOn).toLocaleDateString()}</td>

                                    <td onClick={e => e.stopPropagation()}>
                                        <button className="btn delete" onClick={async (e) => {
                                            if (window.confirm('Delete worker?')) {
                                                await workerApi.deleteWorker(w.id);
                                                setWorkers(workers.filter(worker => worker.id !== w.id));
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

            <footer>GarageFlow — Workers Management</footer>
        </main>
    );
};

export default Workers;
