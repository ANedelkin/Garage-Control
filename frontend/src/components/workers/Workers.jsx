import React, { useState, useEffect } from 'react';
import { usePopup } from '../../context/PopupContext';
import Dropdown from '../common/Dropdown';
import '../../assets/css/common/table.css';
import '../../assets/css/workers.css';
import { workerApi } from '../../services/workerApi';
import EditWorker from './EditWorker';
import WorkhoursPopup from './WorkhoursPopup';

const Workers = () => {
    const { addPopup, removeLastPopup } = usePopup();
    const [roleFilter, setRoleFilter] = useState('all');
    const [search, setSearch] = useState('');

    const [loading, setLoading] = useState(true);
    const [workers, setWorkers] = useState([]);

    const fetchWorkers = async () => {
        setLoading(true);
        try {
            const data = await workerApi.getWorkers();
            setWorkers(data);
        } catch (error) {
            console.error("Failed to fetch workers", error);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchWorkers();
    }, []);

    const openEditWorker = (id) => {
        addPopup(
            id === "new" ? "New Worker" : "Edit Worker",
            <EditWorker id={id} onClose={removeLastPopup} onSave={fetchWorkers} />
        );
    };

    const openWorkhours = (id) => {
        addPopup(
            "Edit Schedule",
            <WorkhoursPopup id={id} onClose={removeLastPopup} onSave={fetchWorkers} />
        );
    };

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

                <button className="btn" onClick={() => openEditWorker("new")}>+ New Worker</button>
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
                            <col style={{ width: '150px' }} />
                        </colgroup>

                        <thead>
                            <tr>
                                <th>Name</th>
                                <th>Access</th>
                                <th>Hired On</th>
                                <th></th>
                            </tr>
                        </thead>

                        <tbody>
                            {loading ? <tr><td colSpan="4">Loading...</td></tr> : filteredWorkers.map((w, i) => (
                                <tr key={w.id} onClick={() => openEditWorker(w.id)}>
                                    <td>{w.name}</td>

                                    <td className="description" title={w.accesses.map(r => r.name).join(', ')}>
                                        {w.accesses.length ? w.accesses.map(r => r.name).join(', ') : "-"}
                                    </td>

                                    <td>{new Date(w.hiredOn).toLocaleDateString()}</td>

                                    <td onClick={e => e.stopPropagation()}>
                                        <div style={{ display: 'flex', gap: '5px', justifyContent: 'flex-end' }}>
                                            <button className="btn edit icon-btn" onClick={() => openEditWorker(w.id)} title="Edit Worker">
                                                <i className="fa-solid fa-pen"></i>
                                            </button>
                                            <button className="btn schedule icon-btn" onClick={() => openWorkhours(w.id)} title="Edit Schedule">
                                                <i className="fa-solid fa-calendar"></i>
                                            </button>
                                            <button className="btn delete icon-btn" onClick={async () => {
                                                if (window.confirm('Delete worker?')) {
                                                    await workerApi.deleteWorker(w.id);
                                                    setWorkers(workers.filter(worker => worker.id !== w.id));
                                                }
                                            }} title="Delete Worker">
                                                <i className="fa-solid fa-trash"></i>
                                            </button>
                                        </div>
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
