import React, { useState, useEffect } from 'react';
import { jobApi } from '../../services/jobApi';
import { workerApi } from '../../services/workerApi';
import { useAuth } from '../../context/AuthContext';
import Dropdown from '../common/Dropdown';
import ToDoList from './ToDoList';
import ToDoMonthView from './ToDoMonthView';
import ToDoWeekView from './ToDoWeekView';
import '../../assets/css/common/status.css';
import '../../assets/css/common/tile.css';
import '../../assets/css/orders.css';
import './ToDoPage.css';

const ToDoPage = () => {
    const [jobs, setJobs] = useState([]);
    const [viewMode, setViewMode] = useState(localStorage.getItem('myJobsViewMode') || 'list'); // 'list', 'calendar', or 'week'
    const [loading, setLoading] = useState(true);
    const { user } = useAuth();
    const [worker, setWorker] = useState(null);

    // Common navigation state for calendar/week
    const today = new Date();
    const [currentMonth, setCurrentMonth] = useState(today.getMonth());
    const [currentYear, setCurrentYear] = useState(today.getFullYear());
    const [viewDate, setViewDate] = useState(new Date());

    useEffect(() => {
        fetchJobs();
        if (user?.workerId) {
            fetchWorker();
        }
    }, [user?.workerId]);

    const fetchWorker = async () => {
        try {
            const data = await workerApi.getWorker(user.workerId);
            setWorker(data);
        } catch (error) {
            console.error("Failed to fetch worker details:", error);
        }
    };

    const fetchJobs = async () => {
        try {
            const data = await jobApi.getMyJobs();
            setJobs(data);
        } catch (error) {
            console.error(error);
        } finally {
            setLoading(false);
        }
    };

    return (
        <main className={`main ${viewMode !== 'list' ? 'calendar-layout' : ''}`}>
            <div className="header">
                <h1>My Jobs</h1>
                <Dropdown
                    value={viewMode}
                    onChange={e => {
                        const newMode = e.target.value;
                        setViewMode(newMode);
                        localStorage.setItem('myJobsViewMode', newMode);
                    }}
                >
                    <option value="list">List View</option>
                    <option value="week">Week View</option>
                    <option value="calendar">Month View</option>
                </Dropdown>
            </div>

            {loading ? <p>Loading...</p> : (
                jobs.length === 0 ? <p className="list-empty">No jobs assigned.</p> : (
                    viewMode === 'list' ? <ToDoList jobs={jobs} /> :
                        viewMode === 'calendar' ? (
                            <ToDoMonthView
                                jobs={jobs}
                                currentMonth={currentMonth}
                                setCurrentMonth={setCurrentMonth}
                                currentYear={currentYear}
                                setCurrentYear={setCurrentYear}
                            />
                        ) : (
                            <ToDoWeekView
                                jobs={jobs}
                                worker={worker}
                                viewDate={viewDate}
                                setViewDate={setViewDate}
                            />
                        )
                )
            )}
        </main>
    );
};

export default ToDoPage;
