import React, { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import DatePicker from 'react-datepicker';
import 'react-datepicker/dist/react-datepicker.css';
import activityLogApi from '../../services/activityLogApi';
import '../../assets/css/common/tile.css';
import '../../assets/css/common/table.css';
import '../../assets/css/activity-log.css';
import usePageTitle from '../../hooks/usePageTitle.js';
import { parseMarkup, stripMarkup, renderAst } from '../../Utilities/markupHelper';

const ActivityLog = () => {
    usePageTitle('Activity Log');
    const [logs, setLogs] = useState([]);
    const [totalCount, setTotalCount] = useState(0);
    const [loading, setLoading] = useState(true);
    const [search, setSearch] = useState('');
    const [page, setPage] = useState(1);
    const take = 100;
    const [dateFrom, setDateFrom] = useState(null);
    const [dateTo, setDateTo] = useState(null);
    const [selectedLog, setSelectedLog] = useState(null);
    const navigate = useNavigate();

    useEffect(() => {
        const fetchLogs = async () => {
            setLoading(true);
            try {
                const formatDate = (date) => {
                    if (!date) return null;
                    const y = date.getFullYear();
                    const m = String(date.getMonth() + 1).padStart(2, '0');
                    const d = String(date.getDate()).padStart(2, '0');
                    return `${y}-${m}-${d}`;
                };
                const fromStr = formatDate(dateFrom);
                const toStr = formatDate(dateTo);
                const skip = (page - 1) * take;
                const data = await activityLogApi.getLogs(skip, take, fromStr, toStr, search);
                setLogs(data.logs || []);
                setTotalCount(data.totalCount || 0);
            } catch (error) {
                console.error("Failed to fetch activity logs", error);
            } finally {
                setLoading(false);
            }
        };

        const timeoutId = setTimeout(() => {
            fetchLogs();
        }, 500);

        return () => clearTimeout(timeoutId);
    }, [dateFrom, dateTo, search, page]);

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



    const filteredLogs = logs;
    const totalPages = Math.ceil(totalCount / take) || 1;

    const Pagination = () => {
        return (
            <div className={`activity-pagination form-footer`}>
                <button
                    type="button"
                    className="btn secondary"
                    disabled={page === 1}
                    onClick={() => setPage(p => p - 1)}
                >
                    <i className="fa-solid fa-chevron-left"></i> Previous
                </button>
                <span>Page {page} of {totalPages}</span>
                <button
                    type="button"
                    className="btn secondary"
                    disabled={page === totalPages}
                    onClick={() => setPage(p => p + 1)}
                >
                    Next <i className="fa-solid fa-chevron-right"></i>
                </button>
            </div>
        );
    };

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

    const formatLogLines = (log) => {
        return log.details || [];
    };

    const clearFilters = () => {
        setSearch('');
        setDateFrom(null);
        setDateTo(null);
        setPage(1);
    };

    const handleSearchChange = (e) => {
        setSearch(e.target.value);
        setPage(1);
    };

    const hasFilters = search || dateFrom || dateTo;



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
                        onChange={handleSearchChange}
                    />
                </div>
                <div className="activity-date-filters">
                    <div className="activity-date-group">
                        <label className="activity-date-label">From</label>
                        <DatePicker
                            selected={dateFrom}
                            onChange={date => { setDateFrom(date); setPage(1); }}
                            maxDate={dateTo}
                            dateFormat="dd.MM.yy"
                            className="activity-date-input"
                            placeholderText="Start Date"
                        />
                    </div>
                    <div className="activity-date-group">
                        <label className="activity-date-label">To</label>
                        <DatePicker
                            selected={dateTo}
                            onChange={date => { setDateTo(date); setPage(1); }}
                            minDate={dateFrom}
                            dateFormat="dd.MM.yy"
                            className="activity-date-input"
                            placeholderText="End Date"
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
                    <div className="horizontal">
                        <span className="activity-count-badge">
                            {totalCount} {totalCount === 1 ? 'entry' : 'entries'}
                        </span>
                        <Pagination />
                    </div>
                </div>
                <div className="activity-table-wrapper form-section">
                    {filteredLogs.length === 0 ? (
                        <div className="list-empty">No activity logs found.</div>
                    ) : (
                        <table>
                            <thead>
                                <tr>
                                    <th className="col-time">Time</th>
                                    <th className="col-action">Action</th>
                                </tr>
                            </thead>
                            <tbody>
                                {filteredLogs.map(log => (
                                    <tr
                                        key={log.id}
                                        className="activity-row"
                                        onClick={(e) => openLogPopup(log, e)}
                                    >
                                        <td className="col-time activity-time-cell">
                                            {formatTimestampShort(log.timestamp)}
                                        </td>
                                        <td className="col-action">
                                            <div className="activity-sentence activity-sentence-truncate">
                                                {renderAst(parseMarkup(log.message), `msg-${log.id}`)}
                                            </div>
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    )}
                </div>
                <Pagination />
            </div>

            {/* Log Detail Popup */}
            {selectedLog && (
                <div className="popup-overlay top activity-log-overlay" onClick={closePopup}>
                    <div className="activity-detail-popup tile popup" onClick={(e) => {
                        const link = e.target.closest('a');
                        if (link?.getAttribute('href')?.startsWith('/')) {
                            e.preventDefault();
                            setSelectedLog(null);
                            navigate(link.getAttribute('href'));
                        }
                    }}>
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
                        <div className="activity-popup-summary">
                            {renderAst(parseMarkup(selectedLog.message), 'popup-msg')}
                        </div>
                        <div className="activity-popup-changes">
                            <div className="activity-popup-changes-label">
                                <i className="fa-solid fa-list-check"></i>
                                Changes
                            </div>
                            <div className="activity-popup-changes-scroll">
                                {formatLogLines(selectedLog).map((line, i) => (
                                    <div key={i} className="activity-change-line">
                                        <span className="activity-change-bullet">
                                            <i className="fa-solid fa-circle-dot"></i>
                                        </span>
                                        <span>{renderAst(parseMarkup(line), `change-${i}`)}</span>
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
