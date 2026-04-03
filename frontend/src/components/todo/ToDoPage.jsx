import React, { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
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
import '../../assets/css/todo.css';

const ToDoPage = () => {
    const { workerId } = useParams();
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
        const targetWorkerId = workerId || user?.workerId;
        if (targetWorkerId) {
            fetchWorker(targetWorkerId);
        }
    }, [workerId, user?.workerId]);

    const fetchWorker = async (id) => {
        try {
            const data = await workerApi.getWorker(id);
            setWorker(data);
        } catch (error) {
            console.error("Failed to fetch worker details:", error);
        }
    };

    const fetchJobs = async () => {
        try {
            setLoading(true);
            const data = workerId 
                ? await jobApi.getWorkerJobs(workerId)
                : await jobApi.getMyJobs();
            setJobs(data);
        } catch (error) {
            console.error(error);
        } finally {
            setLoading(false);
        }
    };

    const getPageTitle = () => {
        if (workerId && worker) {
            return `${worker.name}'s Jobs`;
        }
        return "My Jobs";
    };

    return (
        <main className={`main ${viewMode !== 'list' ? 'calendar-layout' : ''}`}>
            <div className="header">
                <h1>{getPageTitle()}</h1>
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
