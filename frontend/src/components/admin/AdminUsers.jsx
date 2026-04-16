import React, { useState, useEffect } from 'react';
import { adminApi } from '../../services/adminApi';
import Dropdown from '../common/Dropdown';
import { usePopup } from '../../context/PopupContext';
import JustificationPopup from './JustificationPopup';
import AdminUserPopup from './AdminUserPopup';
import '../../assets/css/admin-users.css';
import usePageTitle from '../../hooks/usePageTitle.js';

const AdminUsers = () => {
    usePageTitle('Admin Users');
    const [users, setUsers] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [search, setSearch] = useState('');
    const [statusFilter, setStatusFilter] = useState('All');
    const { addPopup, removeLastPopup } = usePopup();

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
            addPopup(
                'Block User',
                <JustificationPopup
                    onClose={removeLastPopup}
                    onConfirm={(reason) => performToggleBlock(user.id, reason)}
                    title="Block User"
                    message="Please provide a reason for blocking this user."
                />
            );
        } else {
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
            removeLastPopup();
        } catch (err) {
            console.error('Error toggling block status:', err);
            alert(err.message || 'Failed to update user status');
        }
    };

    const roles = ['All', 'Admin', 'Owner', 'Worker'];
    const statuses = ['All', 'Active', 'Blocked'];

    const filteredUsers = users.filter(user => {
        const matchesSearch = user.email.toLowerCase().includes(search.toLowerCase()) ||
            user.userName.toLowerCase().includes(search.toLowerCase()) ||
            (user.workshopName && user.workshopName.toLowerCase().includes(search.toLowerCase()));
        const matchesStatus = statusFilter === 'All' ||
            (statusFilter === 'Active' && !user.isBlocked) ||
            (statusFilter === 'Blocked' && user.isBlocked);

        return matchesSearch && matchesStatus;
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
                                <th>Username</th>
                                <th className="hide-sm">Email</th>
                                <th className="hide-md">Workshop</th>
                                <th>Last Login</th>
                                <th style={{ width: '120px', textAlign: 'center' }}>Status</th>
                            </tr>
                        </thead>
                        <tbody>
                            {filteredUsers.map(user => (
                                <tr key={user.id} className="clickable-row" onClick={() => {
                                    addPopup('User Details', <AdminUserPopup user={user} onClose={removeLastPopup} />);
                                }}>
                                    <td>{user.userName}</td>
                                    <td className="hide-sm">{user.email}</td>
                                    <td className="hide-md">{user.workshopName || '-'}</td>
                                    <td>{new Date(user.lastLogin).toLocaleString([], { month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' }) === 'Invalid Date' || user.lastLogin === '0001-01-01T00:00:00' ? 'Never' : new Date(user.lastLogin).toLocaleString([], { month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' })}</td>
                                    <td style={{ textAlign: 'center', height: '61px' }} onClick={e => e.stopPropagation()}>
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


        </main>
    );
};



export default AdminUsers;

