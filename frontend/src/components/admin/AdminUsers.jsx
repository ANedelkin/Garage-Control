import React, { useState, useEffect } from 'react';
import { adminApi } from '../../services/adminApi';
import Dropdown from '../common/Dropdown';
import JustificationPopup from '../common/JustificationPopup';
import '../../assets/css/admin-users.css';

const AdminUsers = () => {
    const [users, setUsers] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [search, setSearch] = useState('');
    const [roleFilter, setRoleFilter] = useState('All');
    const [statusFilter, setStatusFilter] = useState('All');
    const [isJustifyOpen, setIsJustifyOpen] = useState(false);
    const [selectedUserId, setSelectedUserId] = useState(null);

    useEffect(() => {
        fetchUsers();
    }, []);

    const fetchUsers = async () => {
        try {
            setLoading(true);
            const data = await adminApi.getUsers();
            setUsers(data);
            setError(null);
        } catch (err) {
            console.error('Error fetching users:', err);
            setError('Failed to load users');
        } finally {
            setLoading(false);
        }
    };

    const handleToggleBlock = async (user) => {
        if (!user.isBlocked) {
            // Opening block popup
            setSelectedUserId(user.id);
            setIsJustifyOpen(true);
        } else {
            // Direct unblock
            await performToggleBlock(user.id);
        }
    };

    const performToggleBlock = async (userId, reason = null) => {
        try {
            await adminApi.toggleUserBlock(userId, reason);
            setUsers(users.map(u =>
                u.id === userId
                    ? { ...u, isBlocked: !u.isBlocked }
                    : u
            ));
            setIsJustifyOpen(false);
            setSelectedUserId(null);
        } catch (err) {
            console.error('Error toggling block status:', err);
            alert(err.message || 'Failed to update user status');
        }
    };

    const roles = ['All', 'Admin', 'Owner', 'Worker'];
    const statuses = ['All', 'Active', 'Blocked'];

    const filteredUsers = users.filter(user => {
        const matchesSearch = user.email.toLowerCase().includes(search.toLowerCase()) ||
            (user.workshopName && user.workshopName.toLowerCase().includes(search.toLowerCase()));
        const matchesRole = roleFilter === 'All' || user.role === roleFilter;
        const matchesStatus = statusFilter === 'All' ||
            (statusFilter === 'Active' && !user.isBlocked) ||
            (statusFilter === 'Blocked' && user.isBlocked);

        return matchesSearch && matchesRole && matchesStatus;
    });


    if (loading) {
        return (
            <main className="main">
                <div className="loading-container">Loading users...</div>
            </main>
        );
    }

    if (error) {
        return (
            <main className="main">
                <div className="error-message">{error}</div>
            </main>
        );
    }

    return (
        <main className="main admin-users-page">
            <div className="header">
                <input
                    type="text"
                    placeholder="Search users..."
                    value={search}
                    onChange={e => setSearch(e.target.value)}
                />
                <Dropdown
                    value={roleFilter}
                    onChange={e => setRoleFilter(e.target.value)}
                >
                    {roles.map(role => (
                        <option key={role} value={role}>{role}</option>
                    ))}
                </Dropdown>
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
                <h3>Users</h3>
                <div style={{ overflowX: 'auto' }}>
                    <table>
                        <thead>
                            <tr>
                                <th>Email</th>
                                <th>Role</th>
                                <th>Workshop</th>
                                <th style={{ width: '120px', textAlign: 'center' }}>Status</th>
                            </tr>
                        </thead>
                        <tbody>
                            {filteredUsers.map(user => (
                                <tr key={user.id}>
                                    <td>{user.email}</td>
                                    <td>{user.role}</td>
                                    <td>{user.workshopName || '-'}</td>
                                    <td style={{ textAlign: 'center', height: '61px' }}>
                                        {user.role !== 'Admin' && (
                                            <button
                                                className={`status-btn btn ${user.isBlocked ? 'admin-blocked' : 'admin-active'}`}
                                                onClick={() => handleToggleBlock(user)}
                                            >
                                                {user.isBlocked ? 'Blocked' : 'Active'}
                                            </button>
                                        )}
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            </div>

            <footer>GarageFlow — Users Management</footer>

            <JustificationPopup
                isOpen={isJustifyOpen}
                onClose={() => setIsJustifyOpen(false)}
                onConfirm={(reason) => performToggleBlock(selectedUserId, reason)}
                title="Block User"
                message="Please provide a reason for blocking this user."
            />
        </main>
    );
};



export default AdminUsers;

