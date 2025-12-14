import React, { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import "../../assets/css/common.css";
import "../../assets/css/clients.css";
import "../../assets/css/edit-client.css";
import { clientApi } from "../../services/clientApi";

const EditClient = () => {
    const { id } = useParams();
    const navigate = useNavigate();
    const isNew = !id || id === 'new';

    const [client, setClient] = useState({
        name: "",
        phoneNumber: "",
        email: "",
        address: "",
        registrationNumber: ""
    });

    const [loading, setLoading] = useState(true);

    // Sample cars data for display only
    const [cars] = useState([
        { id: 1, make: "Toyota", model: "Corolla", year: 2020, plate: "ABC-123" },
        { id: 2, make: "Honda", model: "Civic", year: 2018, plate: "XYZ-789" }
    ]);

    useEffect(() => {
        const fetchData = async () => {
            if (!isNew) {
                try {
                    const data = await clientApi.getDetails(id);
                    if (data) setClient(data);
                } catch (error) {
                    console.error("Error loading client", error);
                    alert("Failed to load client details.");
                }
            }
            setLoading(false);
        };
        fetchData();
    }, [id, isNew]);

    const handleSave = async (e) => {
        e.preventDefault();
        try {
            if (isNew) {
                await clientApi.create(client);
            } else {
                await clientApi.edit(client);
            }
            navigate('/clients');
        } catch (error) {
            console.error("Error saving client", error);
            alert("Failed to save client.");
        }
    };

    if (loading) return <div>Loading...</div>;

    return (
        <main className="main">
            <div className="tile">
                <h3 className="tile-header">{isNew ? "New Client" : "Edit Client"}</h3>
                <form onSubmit={handleSave}>
                    <div className="edit-client-container">
                        <div className="form-column">
                            <div className="form-section">
                                <label>Name</label>
                                <input
                                    type="text"
                                    value={client.name}
                                    onChange={e => setClient({ ...client, name: e.target.value })}
                                    required
                                />
                            </div>
                            <div className="form-section">
                                <label>Phone Number</label>
                                <input
                                    type="text"
                                    value={client.phoneNumber}
                                    onChange={e => setClient({ ...client, phoneNumber: e.target.value })}
                                    required
                                />
                            </div>
                            <div className="form-section">
                                <label>Email</label>
                                <input
                                    type="email"
                                    value={client.email}
                                    onChange={e => setClient({ ...client, email: e.target.value })}
                                />
                            </div>
                            <div className="form-section">
                                <label>Address</label>
                                <input
                                    type="text"
                                    value={client.address}
                                    onChange={e => setClient({ ...client, address: e.target.value })}
                                />
                            </div>
                            <div className="form-section">
                                <label>Registration Number</label>
                                <input
                                    type="text"
                                    value={client.registrationNumber}
                                    onChange={e => setClient({ ...client, registrationNumber: e.target.value })}
                                />
                            </div>
                        </div>

                        {!isNew && (
                            <div className="form-column">
                                <div className="car-list-section">
                                    <div className="car-list-header">
                                        <h4>Cars</h4>
                                        <button type="button" className="btn" onClick={() => { }}>+ Add Car</button>
                                    </div>
                                    {cars.map(car => (
                                        <div key={car.id} className="car-item">
                                            <div>
                                                <strong>{car.make} {car.model}</strong> ({car.year}) - {car.plate}
                                            </div>
                                            <div>
                                                <button type="button" className="icon-btn" style={{ marginRight: '10px' }} onClick={() => { }}>
                                                    <i className="fa-solid fa-pen"></i>
                                                </button>
                                                <button type="button" className="icon-btn delete" onClick={() => { }}>
                                                    <i className="fa-solid fa-trash"></i>
                                                </button>
                                            </div>
                                        </div>
                                    ))}
                                    <p style={{ marginTop: '10px', fontSize: '0.9em', color: '#888' }}>
                                        * Car functionality is currently disabled (UI demo only).
                                    </p>
                                </div>
                            </div>
                        )}
                    </div>

                    <div className="form-footer" style={{ marginTop: '20px' }}>
                        <button type="submit" className="btn">Save Client</button>
                        <button type="button" className="btn secondary" onClick={() => navigate('/clients')}>Cancel</button>
                    </div>
                </form>
            </div>
        </main>
    );
};

export default EditClient;
