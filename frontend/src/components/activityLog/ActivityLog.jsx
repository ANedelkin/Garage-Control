import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import activityLogApi from '../../services/activityLogApi';
import '../../assets/css/common/tile.css';
import '../../assets/css/common/table.css';
import '../../assets/css/activity-log.css';

const ActivityLog = () => {
    const [logs, setLogs] = useState([]);
    const [loading, setLoading] = useState(true);
    const navigate = useNavigate();

    useEffect(() => {
        const fetchLogs = async () => {
            try {
                const data = await activityLogApi.getLogs(100);
                setLogs(data);
            } catch (error) {
                console.error("Failed to fetch activity logs", error);
            } finally {
                setLoading(false);
            }
        };

        fetchLogs();
    }, []);

    const formatTimestamp = (timestamp) => {
        const date = new Date(timestamp);
        return date.toLocaleString();
    };

    const handleLogClick = (e) => {
        const link = e.target.closest('a');
        if (link && link.getAttribute('href')) {
            const href = link.getAttribute('href');
            if (href.startsWith('/')) {
                e.preventDefault();
                navigate(href);
            }
        }
    };

    if (loading) {
        return <div className="main"><h1>Activity Log</h1><p>Loading logs...</p></div>;
    }

    return (
        <div className="main">

            <div className="tile">
                <h3>Activity Log</h3>
                <div style={{ overflowX: 'auto' }} onClick={handleLogClick}>
                    <table>
                        <thead>
                            <tr>
                                <th style={{ width: '200px' }}>Time</th>
                                <th>Action</th>
                            </tr>
                        </thead>
                        <tbody>
                            {logs.length === 0 ? (
                                <tr>
                                    <td colSpan="2" style={{ textAlign: 'center', padding: '20px' }}>No activity logs found.</td>
                                </tr>
                            ) : (
                                logs.map(log => (
                                    <tr key={log.id}>
                                        <td>{formatTimestamp(log.timestamp)}</td>
                                        <td>
                                            <div
                                                className="activity-sentence"
                                                dangerouslySetInnerHTML={{ __html: log.messageHtml }}
                                            />
                                        </td>
                                    </tr>
                                ))
                            )}
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    );
};

export default ActivityLog;
