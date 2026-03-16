import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import '../../assets/css/common/list.css';
import '../../assets/css/clients.css';
import { clientApi } from '../../services/clientApi';
import { usePopup } from '../../context/PopupContext';
import ClientPopup from './ClientPopup';

const Clients = () => {
    const navigate = useNavigate();
    const { clientId } = useParams();
    const [search, setSearch] = useState('');
    const [loading, setLoading] = useState(true);
    const [clients, setClients] = useState([]);
    const { addPopup, removeLastPopup } = usePopup();

    useEffect(() => {
        fetchClients();
    }, []);

    const fetchClients = async () => {
        setLoading(true);
        try {
            const data = await clientApi.getAll();
            setClients(data);
        } catch (error) {
            console.error("Failed to fetch clients", error);
        } finally {
            setLoading(false);
        }
    };

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

    const openEditPopup = (id) => {
        addPopup("Edit Client", <ClientPopup
            onClose={() => { removeLastPopup(); navigate('/clients'); }}
            onSave={handlePopupSave}
            clientId={id === 'new' ? null : id}
        />, false, () => navigate('/clients'));
    };

    useEffect(() => {
        if (clientId) {
            openEditPopup(clientId);
        }
    }, [clientId]);

    const handlePopupSave = (id) => {
        fetchClients();
        removeLastPopup();
        navigate('/clients');
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
                <button className="btn" onClick={() => navigate("/clients/new")}>+ New Client</button>
            </div>

            <div className="tile">
                <h3>Clients</h3>
                <div style={{ overflowX: 'auto' }}>
                    <table>
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
                                    onClick={() => navigate(`/clients/${c.id}`)}
                                    style={{ cursor: 'pointer' }}
                                    className="clickable-row"
                                >
                                    <td>{c.name}</td>
                                    <td>{c.phoneNumber}</td>
                                    <td>{c.email}</td>
                                    <td>{c.address}</td>
                                    <td onClick={e => e.stopPropagation()}>
                                        <button className="btn icon-btn" title="Edit client" onClick={() => navigate(`/clients/${c.id}`)}>
                                            <i className="fa-solid fa-pen"></i>
                                        </button>
                                        <button className="btn delete icon-btn" title="Delete client" onClick={() => handleDelete(c.id)}>
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

            {/* <ClientPopup
                isOpen={showPopup}
                onClose={handlePopupClose}
                onSave={handlePopupSave}
                clientId={selectedClientId}
            /> */}

            <footer>GarageFlow — Clients Management</footer>
        </main>
    );
};

export default Clients;
