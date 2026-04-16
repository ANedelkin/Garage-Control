import React, { useState, useEffect } from 'react';
import { adminApi } from '../../services/adminApi';
import '../../assets/css/common/tile.css';
import '../../assets/css/dashboard.css';
import usePageTitle from '../../hooks/usePageTitle.js';

const AdminDashboard = () => {
    usePageTitle('Admin Dashboard');
    const [stats, setStats] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const fetchStats = async () => {
            try {
                const data = await adminApi.getStats();
                setStats(data);
            } catch (error) {
                console.error("Failed to fetch admin stats", error);
            } finally {
                setLoading(false);
            }
        };

        fetchStats();
    }, []);

    const formatDate = (dateString) => {
        if (!dateString || dateString === '0001-01-01T00:00:00') return 'Never';
        const date = new Date(dateString);
        return date.toLocaleString([], { 
            month: 'short', 
            day: 'numeric', 
            hour: '2-digit', 
            minute: '2-digit' 
        });
    };

    return (
        <div className="main">
            {loading ? <div className="tile">Loading stats...</div> : !stats ? <div className="tile">Failed to load stats.</div> :
                <>
                    <h1>Admin Dashboard</h1>

                    <div className="grid">
                        <div className="tile count-tile glow">
                            <h3>Total Users</h3>
                            <p className="count">{stats.totalUsers}</p>
                        </div>
                        <div className="tile count-tile glow">
                            <h3>Total Workshops</h3>
                            <p className="count">{stats.totalWorkshops}</p>
                        </div>
                        <div className="tile count-tile glow">
                            <h3>Total Orders</h3>
                            <p className="count">{stats.totalOrders}</p>
                        </div>
                    </div>

                    <div className="tile">
                        <h3>Recent Users</h3>
                        <div className="table">
                            <table>
                                <thead>
                                    <tr>
                                        <th>Username</th>
                                        <th className="hide-sm">Email</th>
                                        <th className="hide-md">Workshop</th>
                                        <th>Last Login</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {stats.recentUsers.map(user => (
                                        <tr key={user.id}>
                                            <td>{user.userName}</td>
                                            <td className="hide-sm">{user.email}</td>
                                            <td className="hide-md">{user.workshopName || '-'}</td>
                                            <td>{formatDate(user.lastLogin)}</td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                    </div>
                </>
            }
        </div>
    );
};

export default AdminDashboard;
