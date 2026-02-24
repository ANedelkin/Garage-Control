import React, { useState, useEffect } from 'react';
import { adminApi } from '../../services/adminApi';
import Dropdown from '../common/Dropdown';
import JustificationPopup from '../common/JustificationPopup';
import '../../assets/css/admin-users.css'; // Reuse scoped styles

const AdminWorkshops = () => {
    const [workshops, setWorkshops] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [search, setSearch] = useState('');
    const [statusFilter, setStatusFilter] = useState('All');
    const [isJustifyOpen, setIsJustifyOpen] = useState(false);
    const [selectedWorkshopId, setSelectedWorkshopId] = useState(null);

    useEffect(() => {
        fetchWorkshops();
    }, []);

    const fetchWorkshops = async () => {
        try {
            setLoading(true);
            const data = await adminApi.getWorkshops();
            setWorkshops(data);
            setError(null);
        } catch (err) {
            console.error('Error fetching workshops:', err);
            setError('Failed to load workshops');
        } finally {
            setLoading(false);
        }
    };

    const handleToggleBlock = async (workshop) => {
        if (!workshop.isBlocked) {
            // Opening block popup
            setSelectedWorkshopId(workshop.id);
            setIsJustifyOpen(true);
        } else {
            // Direct unblock
            await performToggleBlock(workshop.id);
        }
    };

    const performToggleBlock = async (workshopId, reason = null) => {
        try {
            const result = await adminApi.toggleWorkshopBlock(workshopId, reason);
            if (result.success) {
                setWorkshops(workshops.map(w =>
                    w.id === workshopId
                        ? { ...w, isBlocked: !w.isBlocked }
                        : w
                ));
            }
            setIsJustifyOpen(false);
            setSelectedWorkshopId(null);
        } catch (err) {
            console.error('Error toggling workshop block status:', err);
            alert(err.message || 'Failed to update workshop status');
        }
    };

    const statuses = ['All', 'Active', 'Blocked'];

    const filteredWorkshops = workshops.filter(w => {
        const matchesSearch = w.name.toLowerCase().includes(search.toLowerCase()) ||
            w.address.toLowerCase().includes(search.toLowerCase()) ||
            w.bossEmail.toLowerCase().includes(search.toLowerCase());
        const matchesStatus = statusFilter === 'All' ||
            (statusFilter === 'Active' && !w.isBlocked) ||
            (statusFilter === 'Blocked' && w.isBlocked);

        return matchesSearch && matchesStatus;
    });

    if (loading) {
        return (
            <main className="main admin-users-page">
                <div className="loading-container">Loading workshops...</div>
            </main>
        );
    }

    if (error) {
        return (
            <main className="main admin-users-page">
                <div className="error-message">{error}</div>
            </main>
        );
    }

    return (
        <main className="main admin-users-page">
            <div className="header">
                <input
                    type="text"
                    placeholder="Search workshops..."
                    value={search}
                    onChange={e => setSearch(e.target.value)}
                />
                <Dropdown
                    value={statusFilter}
                    onChange={e => setStatusFilter(e.target.value)}
                >
                    {statuses.map(status => (
                        <option key={status} value={status}>{status}</option>
                    ))}
                </Dropdown>
            </div>

            <div className="tile">
                <h3>Workshops</h3>
                <div style={{ overflowX: 'auto' }}>
                    <table>
                        <thead>
                            <tr>
                                <th>Name</th>
                                <th>Boss Email</th>
                                <th>Address</th>
                                <th style={{ textAlign: 'center' }}>Workers</th>
                                <th style={{ width: '120px', textAlign: 'center' }}>Status</th>
                            </tr>
                        </thead>
                        <tbody>
                            {filteredWorkshops.map(w => (
                                <tr key={w.id}>
                                    <td>{w.name}</td>
                                    <td>{w.bossEmail}</td>
                                    <td>{w.address}</td>
                                    <td style={{ textAlign: 'center' }}>{w.workerCount}</td>
                                    <td style={{ textAlign: 'center', height: '61px' }}>
                                        <button
                                            className={`status-btn btn ${w.isBlocked ? 'admin-blocked' : 'admin-active'}`}
                                            onClick={() => handleToggleBlock(w)}
                                        >
                                            {w.isBlocked ? 'Blocked' : 'Active'}
                                        </button>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            </div>

            <footer>GarageFlow — Workshops Management</footer>

            <JustificationPopup
                isOpen={isJustifyOpen}
                onClose={() => setIsJustifyOpen(false)}
                onConfirm={(reason) => performToggleBlock(selectedWorkshopId, reason)}
                title="Block Workshop"
                message="Please provide a reason for blocking this workshop."
            />
        </main>
    );
};

export default AdminWorkshops;

