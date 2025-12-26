// src/components/dashboard/Dashboard.jsx
import React, { useState, useEffect, useRef } from 'react';
import Chart from 'chart.js/auto';

import '../../assets/css/dashboard.css';

const Dashboard = () => {
    const chartRef = useRef(null);
    const chartInstanceRef = useRef(null);

    const [ordersData] = useState({ all: 124, pending: 36, inProgress: 48 });

    const [partsNeeded] = useState([
        { part: 'Brake pads - front', qty: 12 },
        { part: 'Oil filter - 4 cyl', qty: 6 },
        { part: 'Alternator (reman)', qty: 1 },
        { part: 'Timing belt - VW', qty: 100 },
        { part: 'Air filter - SUV', qty: 3 }
    ]);

    useEffect(() => {
        const canvas = chartRef.current;
        if (!canvas) return;

        if (chartInstanceRef.current) chartInstanceRef.current.destroy();

        const labels = Array.from({ length: 30 }, (_, i) => {
            const d = new Date();
            d.setDate(d.getDate() - (29 - i));
            return `${d.getMonth() + 1}/${d.getDate()}`;
        });

        const data = Array.from({ length: 30 }, () => Math.max(0, Math.round(3 + Math.random() * 5)));

        chartInstanceRef.current = new Chart(canvas, {
            type: 'bar',
            data: {
                labels,
                datasets: [{ label: 'Orders completed', data, backgroundColor: 'rgb(255,122,24)' }]
            },
            options: {
                maintainAspectRatio: false,
                scales: { y: { beginAtZero: true, max: Math.max(...data) + 2 } },
                plugins: { legend: { display: false } }
            }
        });

        return () => {
            if (chartInstanceRef.current) chartInstanceRef.current.destroy();
        };
    }, []);

    return (
        <main className="main">
            <section className="grid">
                <div className="tile all-orders">
                    <h3>All orders <span className="count">{ordersData.all}</span></h3>
                </div>
                <div className="tile pending-orders">
                    <h3>Pending orders <span className="count">{ordersData.pending}</span></h3>
                </div>
                <div className="tile inprogress-orders">
                    <h3>Orders in progress <span className="count">{ordersData.inProgress}</span></h3>
                </div>

                <div className="tile chart-card">
                    <h3 classname="tile-header">Orders completed — last 30 days</h3>
                    <canvas ref={chartRef} />
                </div>

                <div className="tile small-card">
                    <div className="tile-header">
                        <h3>Parts needed (out of stock)</h3>
                        <a className="btn" href="#">Open</a>
                    </div>
                    <div className="parts-list">
                        {partsNeeded.map(p => (
                            <div key={p.part} className="part-row" href="#">
                                <span>{p.part}</span>
                                <span style={{ color: 'inherit' }}>
                                    {`${p.qty} needed`}
                                </span>
                            </div>
                        ))}
                    </div>

                </div>
            </section>
            <footer>GarageFlow — Internal Service Dashboard</footer>
        </main>
    );
};

export default Dashboard;
