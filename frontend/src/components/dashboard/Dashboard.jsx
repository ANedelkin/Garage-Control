// src/components/dashboard/Dashboard.jsx
import React, { useState, useEffect, useRef, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import Chart from 'chart.js/auto';

import '../../assets/css/dashboard.css';
import '../../assets/css/common/glow.css';
import '../../assets/css/common/status.css';
import { dashboardApi } from '../../services/dashboardApi';
import usePageTitle from '../../hooks/usePageTitle';

const TOP_COLORS = [
    'rgb(255, 122, 24)',   // Orange
    'rgb(54, 162, 235)',   // Blue
    'rgb(255, 205, 86)',   // Yellow
    'rgb(75, 192, 192)',   // Teal
    'rgb(153, 102, 255)',  // Purple
    'rgb(255, 99, 132)',   // Pink
    'rgb(76, 175, 80)',    // Green
    'rgb(63, 81, 181)',    // Indigo
    'rgb(0, 188, 212)'     // Cyan
];
const OTHERS_COLOR = 'rgb(158, 158, 158)'; // Gray

const Dashboard = () => {
    usePageTitle('Dashboard');
    const navigate = useNavigate();
    const jobsChartRef = useRef(null);
    const jobsChartInstanceRef = useRef(null);
    const pieChartRef = useRef(null);
    const pieChartInstanceRef = useRef(null);

    const [dashboardData, setDashboardData] = useState(null);
    const [loading, setLoading] = useState(true);

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

    // 1. Process distribution to get top 9 + Others
    const processedDistribution = useMemo(() => {
        if (!dashboardData || !dashboardData.jobTypeDistribution) return [];
        
        const sorted = [...dashboardData.jobTypeDistribution].sort((a, b) => b.count - a.count);
        const top9 = sorted.slice(0, 9);
        const others = sorted.slice(9);
        
        const result = top9.map((item, index) => ({
            name: item.jobTypeName,
            count: item.count,
            color: TOP_COLORS[index]
        }));

        if (others.length > 0) {
            result.push({
                name: 'Others',
                count: others.reduce((sum, item) => sum + item.count, 0),
                color: OTHERS_COLOR
            });
        }
        return result;
    }, [dashboardData]);

    // 2. Create color mapping for lookup
    const colorMap = useMemo(() => {
        const map = {};
        processedDistribution.forEach(item => {
            map[item.name] = item.color;
        });
        return map;
    }, [processedDistribution]);

    // 3. Process day-by-day data to group Others
    const processedDays = useMemo(() => {
        if (!dashboardData || processedDistribution.length === 0) return [];
        
        const topNames = new Set(processedDistribution.map(d => d.name));
        const hasOthers = topNames.has('Others');

        return dashboardData.jobsCompletedByDay.map(day => {
            const counts = {};
            let othersCount = 0;

            Object.entries(day.jobTypesCounts).forEach(([type, count]) => {
                if (topNames.has(type)) {
                    counts[type] = (counts[type] || 0) + count;
                } else if (hasOthers) {
                    othersCount += count;
                }
            });

            if (hasOthers && othersCount > 0) {
                counts['Others'] = (counts['Others'] || 0) + othersCount;
            }

            return {
                date: day.date,
                counts
            };
        });
    }, [dashboardData, processedDistribution]);

    // Jobs completed chart
    useEffect(() => {
        if (!dashboardData || !jobsChartRef.current || processedDistribution.length === 0) return;

        if (jobsChartInstanceRef.current) jobsChartInstanceRef.current.destroy();

        const datasets = processedDistribution.map((item) => ({
            label: item.name,
            data: processedDays.map(day => day.counts[item.name] || 0),
            backgroundColor: item.color
        }));

        const labels = processedDays.map(day => {
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
    }, [dashboardData, processedDistribution, processedDays]);

    // Pie chart for job type distribution
    useEffect(() => {
        if (!dashboardData || !pieChartRef.current || processedDistribution.length === 0) return;

        if (pieChartInstanceRef.current) pieChartInstanceRef.current.destroy();

        pieChartInstanceRef.current = new Chart(pieChartRef.current, {
            type: 'pie',
            data: {
                labels: processedDistribution.map(jt => jt.name),
                datasets: [{
                    data: processedDistribution.map(jt => jt.count),
                    backgroundColor: processedDistribution.map(jt => jt.color)
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
    }, [dashboardData, processedDistribution]);

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
                        {processedDistribution.map((item) => (
                            <div key={item.name} className="legend-item">
                                <span className="legend-marker" style={{ backgroundColor: item.color }}></span>
                                <span className="legend-label">{item.name} ({item.count})</span>
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
                    
                    <div style={{ height: '300px', width: '100%', marginTop: 'auto' }}>
                        <canvas ref={pieChartRef} />
                    </div>
                </div>
            </section>
            <footer>GarageFlow — Internal Service Dashboard</footer>
        </main>
    );
};

export default Dashboard;

