import React, { useState, useEffect } from 'react';
import { adminApi } from '../../services/adminApi';
import '../../assets/css/common/tile.css';

const AdminDashboard = () => {
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

    if (loading) {
        return <div>Loading stats...</div>;
    }

    if (!stats) {
        return <div>Failed to load stats.</div>;
    }

    return (
        <div className="main">
            <h1>Admin Dashboard</h1>

            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '20px', marginBottom: '30px' }}>
                <div className="tile no-hover">
                    <h3>Total Users</h3>
                    <p style={{ fontSize: '2em', fontWeight: 'bold' }}>{stats.totalUsers}</p>
                </div>
                <div className="tile no-hover">
                    <h3>Total Workshops</h3>
                    <p style={{ fontSize: '2em', fontWeight: 'bold' }}>{stats.totalWorkshops}</p>
                </div>
                <div className="tile no-hover">
                    <h3>Total Orders</h3>
                    <p style={{ fontSize: '2em', fontWeight: 'bold' }}>{stats.totalOrders}</p>
                </div>
            </div>

            <div className="tile">
                <h3>Recent Users</h3>
                <div className="table">
                    <table>
                        <thead>
                            <tr>
                                <th>Email</th>
                                <th>Role</th>
                                <th>Workshop</th>
                            </tr>
                        </thead>
                        <tbody>
                            {stats.recentUsers.map(user => (
                                <tr key={user.id}>
                                    <td>{user.email}</td>
                                    <td>{user.role}</td>
                                    <td>{user.workshopName || '-'}</td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    );
};

export default AdminDashboard;
