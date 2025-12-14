import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import '../../assets/css/common.css';
import '../../assets/css/clients.css';
import { clientApi } from '../../services/clientApi';

const Clients = () => {
    const [search, setSearch] = useState('');
    const [loading, setLoading] = useState(true);
    const [clients, setClients] = useState([]);

    useEffect(() => {
        const fetchClients = async () => {
            try {
                const data = await clientApi.getAll();
                setClients(data);
            } catch (error) {
                console.error("Failed to fetch clients", error);
            } finally {
                setLoading(false);
            }
        };
        fetchClients();
    }, []);

    const filteredClients = clients.filter(c =>
        c.name.toLowerCase().includes(search.toLowerCase()) ||
        c.phoneNumber.includes(search)
    );

    const handleDelete = async (id) => {
        if (window.confirm('Delete client?')) {
            try {
                await clientApi.delete(id);
                setClients(clients.filter(c => c.id !== id));
            } catch (error) {
                console.error("Failed to delete client", error);
                alert("Failed to delete client.");
            }
        }
    };

    return (
        <main className="main">
            <div className="header">
                <input
                    type="text"
                    placeholder="Search clients..."
                    value={search}
                    onChange={e => setSearch(e.target.value)}
                />
                <Link className="btn" to="/clients/new">+ New Client</Link>
            </div>

            <div className="tile">
                <h3>Clients</h3>
                <div style={{ overflowX: 'auto' }}>
                    <table className="clients-table">
                        <thead>
                            <tr>
                                <th>Name</th>
                                <th>Phone</th>
                                <th>Email</th>
                                <th>Address</th>
                                <th></th>
                            </tr>
                        </thead>
                        <tbody>
                            {loading ? <tr><td colSpan="5">Loading...</td></tr> : filteredClients.map((c) => (
                                <tr
                                    key={c.id}
                                    onClick={() => window.location.href = `/clients/${c.id}`}
                                    style={{ cursor: 'pointer' }}
                                    className="clickable-row"
                                >
                                    <td>{c.name}</td>
                                    <td>{c.phoneNumber}</td>
                                    <td>{c.email}</td>
                                    <td>{c.address}</td>
                                    <td onClick={e => e.stopPropagation()}>
                                        <button className="btn delete" onClick={() => handleDelete(c.id)}>
                                            <i className="fa-solid fa-trash"></i>
                                        </button>
                                    </td>
                                </tr>
                            ))}
                            {!loading && filteredClients.length === 0 && (
                                <tr><td colSpan="5" style={{ textAlign: 'center' }}>No clients found.</td></tr>
                            )}
                        </tbody>
                    </table>
                </div>
            </div>
            <footer>GarageFlow — Clients Management</footer>
        </main>
    );
};

export default Clients;
