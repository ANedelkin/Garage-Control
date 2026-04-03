import React, { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import activityLogApi from '../../services/activityLogApi';
import '../../assets/css/common/tile.css';
import '../../assets/css/common/table.css';
import '../../assets/css/activity-log.css';
import usePageTitle from '../../hooks/usePageTitle.js';

const ActivityLog = () => {
    usePageTitle('Activity Log');
    const [logs, setLogs] = useState([]);
    const [loading, setLoading] = useState(true);
    const [search, setSearch] = useState('');
    const [dateFrom, setDateFrom] = useState('');
    const [dateTo, setDateTo] = useState('');
    const [selectedLog, setSelectedLog] = useState(null);
    const navigate = useNavigate();

    useEffect(() => {
        const fetchLogs = async () => {
            try {
                const data = await activityLogApi.getLogs(500);
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

    const formatTimestampShort = (timestamp) => {
        const date = new Date(timestamp);
        return date.toLocaleString(undefined, {
            month: 'short',
            day: 'numeric',
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit',
        });
    };

    const stripHtml = (html) => {
        const div = document.createElement('div');
        div.innerHTML = html;
        return div.textContent || div.innerText || '';
    };

    const filteredLogs = logs.filter(log => {
        const plainText = stripHtml(log.messageHtml || '').toLowerCase();
        const searchLower = search.toLowerCase();
        const matchesSearch = !search || plainText.includes(searchLower);

        const logDate = new Date(log.timestamp);
        const matchesFrom = !dateFrom || logDate >= new Date(dateFrom);
        const matchesTo = !dateTo || logDate <= new Date(dateTo + 'T23:59:59');

        return matchesSearch && matchesFrom && matchesTo;
    });

    const openLogPopup = useCallback((log, e) => {
        const link = e.target.closest('a');
        if (link && link.getAttribute('href')) {
            const href = link.getAttribute('href');
            if (href.startsWith('/')) {
                e.preventDefault();
                navigate(href);
                return;
            }
        }
        setSelectedLog(log);
    }, [navigate]);

    const closePopup = useCallback((e) => {
        if (e.target === e.currentTarget) {
            setSelectedLog(null);
        }
    }, []);

    const formatLogLines = (messageHtml) => {
        if (!messageHtml) return [];
        const plain = stripHtml(messageHtml);
        return plain.split(',').map(s => s.trim()).filter(Boolean);
    };

    const clearFilters = () => {
        setSearch('');
        setDateFrom('');
        setDateTo('');
    };

    const hasFilters = search || dateFrom || dateTo;

    if (loading) {
        return (
            <div className="main activity-log-page">
                <div className="header activity-log-header">
                    <div className="activity-search-wrapper">
                        <div className="activity-search-input disabled" style={{ height: '40px', background: 'var(--solid2)', borderRadius: '8px' }}></div>
                    </div>
                </div>
                <div className="tile">
                    <div className="activity-log-loading">
                        <i className="fa-solid fa-circle-notch fa-spin"></i>
                        <span>Loading activity log...</span>
                    </div>
                </div>
            </div>
        );
    }

    return (
        <div className="main activity-log-page">
            {/* Header / Filters */}
            <div className="header activity-log-header">
                <div className="activity-search-wrapper">
                    <i className="fa-solid fa-magnifying-glass activity-search-icon"></i>
                    <input
                        type="text"
                        className="activity-search-input"
                        placeholder="Search activity..."
                        value={search}
                        onChange={e => setSearch(e.target.value)}
                    />
                </div>
                <div className="activity-date-filters">
                    <div className="activity-date-group">
                        <label className="activity-date-label">From</label>
                        <input
                            type="date"
                            className="activity-date-input"
                            value={dateFrom}
                            onChange={e => setDateFrom(e.target.value)}
                        />
                    </div>
                    <div className="activity-date-group">
                        <label className="activity-date-label">To</label>
                        <input
                            type="date"
                            className="activity-date-input"
                            value={dateTo}
                            onChange={e => setDateTo(e.target.value)}
                        />
                    </div>
                    {hasFilters && (
                        <button className="btn activity-clear-btn" onClick={clearFilters} title="Clear filters">
                            <i className="fa-solid fa-xmark"></i>
                            <span className="activity-clear-label">Clear</span>
                        </button>
                    )}
                </div>
            </div>

            {/* Table Tile */}
            <div className="tile">
                <div className="tile-header">
                    <h3>Activity Log</h3>
                    <span className="activity-count-badge">
                        {filteredLogs.length} {filteredLogs.length === 1 ? 'entry' : 'entries'}
                    </span>
                </div>
                <div className="activity-table-wrapper">
                    <table>
                        <thead>
                            <tr>
                                <th className="col-time">Time</th>
                                <th className="col-action">Action</th>
                            </tr>
                        </thead>
                        <tbody>
                            {filteredLogs.length === 0 ? (
                                <tr>
                                    <td colSpan="2" className="activity-empty">
                                        <i className="fa-solid fa-inbox"></i>
                                        <span>No activity logs found.</span>
                                    </td>
                                </tr>
                            ) : (
                                filteredLogs.map(log => (
                                    <tr
                                        key={log.id}
                                        className="activity-row"
                                        onClick={(e) => openLogPopup(log, e)}
                                    >
                                        <td className="col-time activity-time-cell">
                                            {formatTimestampShort(log.timestamp)}
                                        </td>
                                        <td className="col-action">
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

            {/* Log Detail Popup */}
            {selectedLog && (
                <div className="popup-overlay top activity-log-overlay" onClick={closePopup}>
                    <div className="activity-detail-popup tile popup">
                        <div className="activity-popup-header">
                            <div>
                                <h3 className="activity-popup-title">Activity Detail</h3>
                                <span className="activity-popup-time">
                                    <i className="fa-regular fa-clock"></i>
                                    {formatTimestamp(selectedLog.timestamp)}
                                </span>
                            </div>
                            <button
                                className="btn icon-btn activity-popup-close"
                                onClick={() => setSelectedLog(null)}
                                title="Close"
                            >
                                <i className="fa-solid fa-xmark"></i>
                            </button>
                        </div>
                        <div
                            className="activity-popup-summary"
                            dangerouslySetInnerHTML={{ __html: selectedLog.messageHtml }}
                            onClick={(e) => {
                                const link = e.target.closest('a');
                                if (link?.getAttribute('href')?.startsWith('/')) {
                                    e.preventDefault();
                                    setSelectedLog(null);
                                    navigate(link.getAttribute('href'));
                                }
                            }}
                        />
                        <div className="activity-popup-changes">
                            <div className="activity-popup-changes-label">
                                <i className="fa-solid fa-list-check"></i>
                                Changes
                            </div>
                            <div className="activity-popup-changes-scroll">
                                {formatLogLines(selectedLog.messageHtml).map((line, i) => (
                                    <div key={i} className="activity-change-line">
                                        <span className="activity-change-bullet">
                                            <i className="fa-solid fa-circle-dot"></i>
                                        </span>
                                        <span>{line}</span>
                                    </div>
                                ))}
                            </div>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};

export default ActivityLog;
