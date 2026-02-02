import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import activityLogApi from '../../services/activityLogApi';
import '../../assets/css/common/tile.css';
import '../../assets/css/common/table.css';

const ActivityLog = () => {
    const [logs, setLogs] = useState([]);
    const [loading, setLoading] = useState(true);

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

    const renderActivityContent = (log) => {
        const actorLink = log.actorTargetId ? (
            <Link to={`/workers/${log.actorTargetId}`} className="log-link actor-link">{log.actorName}</Link>
        ) : (
            <span className="actor-name">{log.actorName}</span>
        );

        let targetElement = null;
        if (log.targetId && log.targetName) {
            let path = '';
            switch (log.targetType) {
                case 'Order':
                    path = '/orders';
                    break;
                case 'Client':
                    path = `/clients/${log.targetId}`;
                    break;
                case 'Part':
                    path = `/parts?partId=${log.targetId}`;
                    break;
                case 'group of parts':
                    path = '/parts';
                    break;
                case 'Worker':
                    path = `/workers/${log.targetId}`;
                    break;
                default:
                    path = null;
            }

            targetElement = (
                <>
                    {log.targetType && <span className="target-type">{log.targetType.toLowerCase()} </span>}
                    {path ? (
                        <Link to={path} className="log-link target-link">{log.targetName}</Link>
                    ) : (
                        <span className="target-name">{log.targetName}</span>
                    )}
                </>
            );
        }

        return (
            <div className="activity-sentence">
                {actorLink} <span className="action-text">{log.action}</span> {targetElement}
            </div>
        );
    };

    if (loading) {
        return <div className="main"><h1>Activity Log</h1><p>Loading logs...</p></div>;
    }

    return (
        <div className="main">
            <div className="horizontal between center" style={{ marginBottom: '20px' }}>
                <h1>Activity Log</h1>
            </div>

            <div className="tile no-hover" style={{ padding: '0' }}>
                <div className="table">
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
                                        <td style={{ whiteSpace: 'nowrap', verticalAlign: 'top' }}>{formatTimestamp(log.timestamp)}</td>
                                        <td>
                                            {renderActivityContent(log)}
                                        </td>
                                    </tr>
                                ))
                            )}
                        </tbody>
                    </table>
                </div>
            </div>

            <style>{`
                .activity-sentence {
                    line-height: 1.5;
                    font-size: 1.1em;
                }
                .log-link {
                    font-weight: 600;
                    color: var(--primary-color);
                    text-decoration: none;
                }
                .log-link:hover {
                    text-decoration: underline;
                }
                .actor-link {
                    color: #2c3e50;
                }
                [data-theme='dark'] .actor-link {
                    color: #ecf0f1;
                }
                .target-link {
                    color: var(--primary-color);
                }
                .target-type {
                    color: #888;
                    font-style: italic;
                    font-size: 0.9em;
                    margin-right: 4px;
                }
                .action-text {
                    color: #555;
                }
                [data-theme='dark'] .action-text {
                    color: #bbb;
                }
                .actor-name {
                    font-weight: 600;
                }
            `}</style>
        </div>
    );
};

export default ActivityLog;
