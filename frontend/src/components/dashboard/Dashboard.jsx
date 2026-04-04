// src/components/dashboard/Dashboard.jsx
import React, { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import Chart from 'chart.js/auto';

import '../../assets/css/dashboard.css';
import '../../assets/css/common/glow.css';
import '../../assets/css/common/status.css';
import { dashboardApi } from '../../services/dashboardApi';
import usePageTitle from '../../hooks/usePageTitle';

const Dashboard = () => {
    usePageTitle('Dashboard');
    const navigate = useNavigate();
    const jobsChartRef = useRef(null);
    const jobsChartInstanceRef = useRef(null);
    const pieChartRef = useRef(null);
    const pieChartInstanceRef = useRef(null);
    const colors = [
        'rgb(255,122,24)',
        'rgb(54, 162, 235)',
        'rgb(255, 205, 86)',
        'rgb(75, 192, 192)',
        'rgb(153, 102, 255)',
        'rgb(255, 99, 132)'
    ];

    const [dashboardData, setDashboardData] = useState(null);
    const [loading, setLoading] = useState(true);

    const jobTypes = React.useMemo(() => {
        if (!dashboardData) return [];
        const allJobTypes = new Set();
        dashboardData.jobsCompletedByDay.forEach(day => {
            Object.keys(day.jobTypesCounts).forEach(type => allJobTypes.add(type));
        });
        return Array.from(allJobTypes);
    }, [dashboardData]);

    useEffect(() => {
        fetchDashboardData();
    }, []);

    const fetchDashboardData = async () => {
        try {
            const data = await dashboardApi.getDashboardData();
            setDashboardData(data);
            setLoading(false);
        } catch (error) {
            console.error("Error fetching dashboard data", error);
            setLoading(false);
        }
    };

    // Jobs completed chart
    useEffect(() => {
        if (!dashboardData || !jobsChartRef.current) return;

        if (jobsChartInstanceRef.current) jobsChartInstanceRef.current.destroy();

        const datasets = jobTypes.map((type, index) => ({
            label: type,
            data: dashboardData.jobsCompletedByDay.map(day => day.jobTypesCounts[type] || 0),
            backgroundColor: colors[index % colors.length]
        }));

        const labels = dashboardData.jobsCompletedByDay.map(day => {
            const parts = day.date.split('T')[0].split('-');
            return `${parseInt(parts[1])}/${parseInt(parts[2])}`;
        });

        jobsChartInstanceRef.current = new Chart(jobsChartRef.current, {
            type: 'bar',
            data: { labels, datasets },
            options: {
                maintainAspectRatio: false,
                scales: {
                    x: { stacked: true },
                    y: { 
                        stacked: true, 
                        beginAtZero: true,
                        ticks: {
                            stepSize: 1,
                            precision: 0
                        }
                    }
                },
                plugins: {
                    legend: { display: false }
                }
            }
        });

        return () => {
            if (jobsChartInstanceRef.current) jobsChartInstanceRef.current.destroy();
        };
    }, [dashboardData, jobTypes]);

    // Pie chart for job type distribution
    useEffect(() => {
        if (!dashboardData || !pieChartRef.current || dashboardData.jobTypeDistribution.length === 0) return;

        if (pieChartInstanceRef.current) pieChartInstanceRef.current.destroy();

        pieChartInstanceRef.current = new Chart(pieChartRef.current, {
            type: 'pie',
            data: {
                labels: dashboardData.jobTypeDistribution.map(jt => jt.jobTypeName),
                datasets: [{
                    data: dashboardData.jobTypeDistribution.map(jt => jt.count),
                    backgroundColor: dashboardData.jobTypeDistribution.map((_, index) => colors[index % colors.length])
                }]
            },
            options: {
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false }
                }
            }
        });

        return () => {
            if (pieChartInstanceRef.current) pieChartInstanceRef.current.destroy();
        };
    }, [dashboardData]);

    if (loading) {
        return <main className="main"><div className="tile">Loading...</div></main>;
    }

    if (!dashboardData) {
        return <main className="main"><div className="tile">Failed to load dashboard data</div></main>;
    }

    return (
        <main className="main dashboard">
            <section className="grid">
                <div className="tile count-tile glow job-status-all" onClick={() => navigate('/orders')}>
                    <h3>All orders</h3>
                    <p className="count">{dashboardData.orderStats.allOrders}</p>
                </div>
                <div className="tile count-tile glow job-status-pending" onClick={() => navigate('/orders?status=pending')}>
                    <h3>Pending jobs</h3>
                    <p className="count">{dashboardData.orderStats.pendingJobs}</p>
                </div>
                <div className="tile count-tile glow job-status-inprogress" onClick={() => navigate('/orders?status=inprogress')}>
                    <h3>Jobs in progress</h3>
                    <p className="count">{dashboardData.orderStats.inProgressJobs}</p>
                </div>

                <div className="tile chart-card">
                    <h3 className="tile-header">Jobs completed — last 30 days</h3>
                    
                    <div className="custom-chart-legend">
                        {jobTypes.map((type, index) => (
                            <div key={type} className="legend-item">
                                <span className="legend-marker" style={{ backgroundColor: colors[index % colors.length] }}></span>
                                <span className="legend-label">{type}</span>
                            </div>
                        ))}
                    </div>

                    <div className="scrollable-content-wrapper grow">
                        <div className="min-width-content" style={{ height: '100%' }}>
                            <canvas ref={jobsChartRef} />
                        </div>
                    </div>
                </div>

                <div className="tile small-card">
                    <div className="tile-header">
                        <h3>Parts low on stock</h3>
                        <a className="btn" onClick={() => navigate('/parts')}>Open</a>
                    </div>
                    <div className="parts-list">
                        {dashboardData.lowStockParts.length === 0 ? (
                            <div className="list-empty">No parts low on stock</div>
                        ) : (
                            dashboardData.lowStockParts.map(p => (
                                <div key={p.id} className="part-row" onClick={() => navigate(`/parts?id=${p.id}`)}>
                                    <span>{p.name}</span>
                                    <span className={p.deficitStatus === 2 ? 'status-higher-deficit' : (p.deficitStatus === 1 ? 'status-lower-deficit' : '')}>
                                        {p.currentQuantity}/{p.minimumQuantity}
                                    </span>
                                </div>
                            ))
                        )}
                    </div>
                </div>

                <div className="tile chart-card worker-performance">
                    <div className="tile-header">
                        <h3>Worker performance — finished jobs</h3>
                    </div>
                    <div className="parts-list">
                        {dashboardData.workerPerformance.length === 0 ? (
                            <div className="list-empty">No worker data available</div>
                        ) : (
                            <div className="worker-grid">
                                {dashboardData.workerPerformance.map(w => (
                                    <div key={w.workerId} className="worker-row">
                                        <div className="worker-name">{w.workerName}</div>
                                        <div className="worker-stats">
                                            {Object.entries(w.jobTypesCounts).map(([type, count]) => (
                                                <span key={type} className="job-type-badge">
                                                    {type}: {count}
                                                </span>
                                            ))}
                                        </div>
                                        <div className="worker-hours">
                                            {w.totalHoursWorked.toFixed(1)}h worked
                                        </div>
                                    </div>
                                ))}
                            </div>
                        )}
                    </div>
                </div>

                <div className="tile small-card ">
                    <h3 className="tile-header">Finished jobs' types — last month</h3>
                    
                    <div className="custom-chart-legend">
                        {dashboardData.jobTypeDistribution.map((jt, index) => (
                            <div key={jt.jobTypeName} className="legend-item">
                                <span className="legend-marker" style={{ backgroundColor: colors[index % colors.length] }}></span>
                                <span className="legend-label">{jt.jobTypeName} ({jt.count})</span>
                            </div>
                        ))}
                    </div>

                    <div style={{ height: '300px', width: '100%' }}>
                        <canvas ref={pieChartRef} />
                    </div>
                </div>
            </section>
            <footer>GarageFlow — Internal Service Dashboard</footer>
        </main>
    );
};

export default Dashboard;
