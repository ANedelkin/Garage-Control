import React, { useState, useEffect, useRef } from 'react';
import { useNavigate, useParams, useSearchParams, useLocation } from 'react-router-dom';
import '../../assets/css/common/list.css';
import '../../assets/css/clients.css';
import { clientApi } from '../../services/clientApi';
import { usePopup } from '../../context/PopupContext';
import ClientPopup from './ClientPopup';
import usePageTitle from '../../hooks/usePageTitle';

const Clients = () => {
    usePageTitle('Clients');
    const navigate = useNavigate();
    const { clientId } = useParams();
    const [search, setSearch] = useState('');
    const [loading, setLoading] = useState(true);
    const [clients, setClients] = useState([]);
    const { addPopup, removeLastPopup } = usePopup();
    const rowRefs = useRef({});
    const [searchParams] = useSearchParams();
    const highlight = searchParams.get('highlight') === 'true';
    const location = useLocation();

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
        addPopup(null, <ClientPopup
            onClose={() => { removeLastPopup(); navigate('/clients'); }}
            onSave={handlePopupSave}
            clientId={id === 'new' ? null : id}
        />, false, () => navigate('/clients'));
    };

    useEffect(() => {
        if (!loading && clientId && clientId !== 'new') {
            const row = rowRefs.current[clientId];
            if (row) {
                row.scrollIntoView({ behavior: 'smooth', block: 'center' });
            }
            if (!highlight) {
                openEditPopup(clientId);
            }
        }
    }, [loading, clientId, clients, highlight]);

    useEffect(() => {
        if (location.pathname.endsWith('/new')) {
            openEditPopup('new');
        }
    }, [location.pathname]);

    const handlePopupSave = (id) => {
        fetchClients();
        removeLastPopup();
        navigate('/clients');
    };

    const handleContainerClick = () => {
        if (clientId) {
            navigate('/clients', { replace: true });
        }
    };

    return (
        <main className="main" onClick={handleContainerClick}>
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
                                <th className="hide-sm">Phone</th>
                                <th className="hide-md">Email</th>
                                <th className="hide-lg">Address</th>
                                <th></th>
                            </tr>
                        </thead>
                        <tbody>
                            {loading ? <tr><td colSpan="5">Loading...</td></tr> : filteredClients.map((c) => (
                                <tr
                                    key={c.id}
                                    ref={el => rowRefs.current[c.id] = el}
                                    onClick={(e) => { e.stopPropagation(); openEditPopup(c.id); }}
                                    style={{ cursor: 'pointer' }}
                                    className={`clickable-row ${clientId === c.id ? 'highlight-outline' : ''}`}
                                >
                                    <td>{c.name}</td>
                                    <td className="hide-sm">{c.phoneNumber}</td>
                                    <td className="hide-md">{c.email}</td>
                                    <td className="hide-lg">{c.address}</td>
                                    <td onClick={e => e.stopPropagation()}>
                                        <button className="btn icon-btn" title="Edit client" onClick={() => openEditPopup(c.id)}>
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
