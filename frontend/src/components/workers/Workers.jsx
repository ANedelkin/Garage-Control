import React, { useState, useEffect, useRef } from 'react';
import { useNavigate, useParams, useLocation, useSearchParams } from 'react-router-dom';
import { usePopup } from '../../context/PopupContext';
import Dropdown from '../common/Dropdown';
import '../../assets/css/common/table.css';
import '../../assets/css/workers.css';
import { workerApi } from '../../services/workerApi';
import EditWorker from './EditWorker';
import WorkhoursPopup from './WorkhoursPopup';

const Workers = () => {
    const { addPopup, removeLastPopup } = usePopup();
    const navigate = useNavigate();
    const { workerId } = useParams();
    const location = useLocation();
    const [searchParams] = useSearchParams();
    const highlight = searchParams.get('highlight') === 'true';

    const [roleFilter, setRoleFilter] = useState('all');
    const [search, setSearch] = useState('');

    const [loading, setLoading] = useState(true);
    const [workers, setWorkers] = useState([]);
    const rowRefs = useRef({});

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
            <EditWorker id={id} onClose={() => { removeLastPopup(); navigate('/workers'); }} onSave={fetchWorkers} />,
            false,
            () => navigate('/workers')
        );
    };

    const openWorkhours = (id) => {
        addPopup(
            "Edit Schedule",
            <WorkhoursPopup id={id} onClose={() => { removeLastPopup(); navigate('/workers'); }} onSave={fetchWorkers} />,
            false,
            () => navigate('/workers')
        );
    };

    // Highlighting and scrolling logic
    useEffect(() => {
        if (!loading && workerId && workerId !== 'new') {
            const row = rowRefs.current[workerId];
            if (row) {
                row.scrollIntoView({ behavior: 'smooth', block: 'center' });
            }
            if (!highlight) {
                openEditWorker(workerId);
            }
        }
    }, [loading, workerId, workers, highlight]);

    useEffect(() => {
        if (workerId === 'new') {
            openEditWorker('new');
        }
    }, [workerId]);

    // Unique roles for filter dropdown (assuming backend sends roles as objects with Name)
    // const allRoles = Array.from(new Set(workers.flatMap(w => w.roles.map(r => r.name))));

    const filteredWorkers = workers.filter(w =>
        (roleFilter === 'all' || w.roles.some(r => r.name === roleFilter)) &&
        w.name.toLowerCase().includes(search.toLowerCase())
    );

    const handleContainerClick = () => {
        if (workerId) {
            navigate('/workers', { replace: true });
        }
    };

    return (
        <main className="main" onClick={handleContainerClick}>
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

                <button className="btn" onClick={(e) => { e.stopPropagation(); navigate("/workers/new"); }}>+ New Worker</button>
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
                                <tr 
                                    key={w.id} 
                                    ref={el => rowRefs.current[w.id] = el}
                                    onClick={(e) => { e.stopPropagation(); openEditWorker(w.id); }}
                                    className={workerId === w.id ? 'highlight-outline' : ''}
                                >
                                    <td>{w.name}</td>

                                    <td className="description" title={w.accesses.filter(r => r.isSelected).map(r => r.name).join(', ')}>
                                        {(() => {
                                            const selected = w.accesses.filter(r => r.isSelected);
                                            if (selected.length === 0) return "-";
                                            if (selected.length === w.accesses.length) return "Full";
                                            return selected.map(r => r.name).join(', ');
                                        })()}
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
