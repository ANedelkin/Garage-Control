import React, { useState, useEffect } from 'react';
import '../../assets/css/clients.css';
import { clientApi } from '../../services/clientApi';
import { vehicleApi } from '../../services/vehicleApi';
import { makeApi } from '../../services/makeApi';
import { modelApi } from '../../services/modelApi';
import Popup from '../common/Popup';
import CarPopup from '../cars/CarPopup';

const ClientPopup = ({ isOpen, onClose, onSave, clientId }) => {
    const isNew = !clientId;

    const [client, setClient] = useState({
        name: "",
        phoneNumber: "",
        email: "",
        address: "",
        registrationNumber: ""
    });

    const [loading, setLoading] = useState(false);

    // Cars Logic
    const [cars, setCars] = useState([]);
    const [makes, setMakes] = useState([]);
    const [modelsMap, setModelsMap] = useState({});

    const [showCarPopup, setShowCarPopup] = useState(false);
    const [currentCar, setCurrentCar] = useState(null);

    // Load makes once
    useEffect(() => {
        const fetchMakes = async () => {
            try {
                const makesRes = await makeApi.getAll();
                setMakes(makesRes);
            } catch (error) {
                console.error("Error loading makes", error);
            }
        };
        fetchMakes();
    }, []);

    // Load client data when popup opens
    useEffect(() => {
        const fetchData = async () => {
            if (!isOpen) return;

            setLoading(true);
            try {
                if (!isNew) {
                    const clientRes = await clientApi.getDetails(clientId);
                    if (clientRes) setClient(clientRes);

                    const carsRes = await vehicleApi.getByClient(clientId);
                    setCars(carsRes);

                    // Fetch models for display names
                    const uniqueMakeIds = [...new Set(carsRes.map(c => c.makeId))];
                    const newModelsMap = {};

                    await Promise.all(uniqueMakeIds.map(async (mkId) => {
                        if (mkId) {
                            const mRes = await modelApi.getAll(mkId);
                            mRes.forEach(m => newModelsMap[m.id] = m.name);
                        }
                    }));
                    setModelsMap(newModelsMap);
                } else {
                    setClient({
                        name: "",
                        phoneNumber: "",
                        email: "",
                        address: "",
                        registrationNumber: ""
                    });
                    setCars([]);
                    setModelsMap({});
                }
            } catch (error) {
                console.error("Error loading data", error);
                alert("Failed to load data.");
            } finally {
                setLoading(false);
            }
        };
        fetchData();
    }, [isOpen, clientId, isNew]);

    const handleSave = async (e) => {
        e.preventDefault();
        try {
            if (isNew) {
                await clientApi.create(client);
            } else {
                await clientApi.edit(clientId, client);
            }
            onSave();
            onClose();
        } catch (error) {
            console.error('Error saving client info:', error);
            alert(error.message || 'Error occurred while saving client details.');
        }
    };

    const handleSaveCar = async (carData) => {
        try {
            const carDto = {
                ...carData,
                ownerId: clientId
            };

            if (carData.id) {
                await vehicleApi.edit(carData.id, carDto);
            } else {
                await vehicleApi.create(carDto);
            }

            // Refresh cars list
            const carsRes = await vehicleApi.getByClient(clientId);
            setCars(carsRes);

            // Re-fetch models map if needed
            if (!modelsMap[carData.modelId]) {
                const mRes = await modelApi.getAll(carData.makeId);
                const updMap = { ...modelsMap };
                mRes.forEach(m => updMap[m.id] = m.name);
                setModelsMap(updMap);
            }

            setShowCarPopup(false);
        } catch (error) {
            console.error("Error saving car", error);
            alert("Failed to save car.");
        }
    };

    const handleDeleteCar = async (carId) => {
        if (!window.confirm("Delete this car?")) return;
        try {
            await vehicleApi.delete(carId);
            setCars(cars.filter(c => c.id !== carId));
        } catch (error) {
            console.error("Error deleting car", error);
            alert("Failed to delete car.");
        }
    };

    const openCarPopup = (car = null) => {
        setCurrentCar(car);
        setShowCarPopup(true);
    };

    return (
        <>
            {!loading && (
                <Popup
                    isOpen={isOpen}
                    onClose={onClose}
                    title={isNew ? "New Client" : "Edit Client"}
                >
                    <form className="client-form" onSubmit={handleSave}>
                        <div className="horizontal">
                            <div className="form-column" style={{ flex: 1 }}>
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
                                    <label>Registration Number (Personal)</label>
                                    <input
                                        type="text"
                                        value={client.registrationNumber}
                                        onChange={e => setClient({ ...client, registrationNumber: e.target.value })}
                                    />
                                </div>
                            </div>

                            <div className="form-column cars-section" style={{ flex: 1 }}>
                                <div className="form-section max-height">
                                    <div className="section-header">
                                        <label>Cars</label>
                                        {!isNew && (
                                            <button type="button" className="btn" onClick={() => openCarPopup()}>+ Add Car</button>
                                        )}
                                    </div>
                                    <div className="list-container max-width max-height">
                                        {cars.length === 0 && <p className="list-empty">No cars added.</p>}
                                        {cars.map(car => {
                                            const makeName = makes.find(m => m.id === car.makeId)?.name || "Unknown";
                                            const modelName = modelsMap[car.modelId] || "Unknown";

                                            return (
                                                <div key={car.id} className="list-item">
                                                    <div>
                                                        <strong>{makeName} {modelName}</strong> <br />
                                                        <span style={{ fontSize: '0.9em' }}>{car.registrationNumber}</span>
                                                    </div>
                                                    <div>
                                                        <button type="button" className="icon-btn btn" style={{ marginRight: '10px' }} onClick={() => openCarPopup(car)}>
                                                            <i className="fa-solid fa-pen"></i>
                                                        </button>
                                                        <button type="button" className="icon-btn delete btn" onClick={() => handleDeleteCar(car.id)}>
                                                            <i className="fa-solid fa-trash"></i>
                                                        </button>
                                                    </div>
                                                </div>
                                            )
                                        })}
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div className="form-footer">
                            <button type="submit" className="btn">Save Client</button>
                            <button type="button" className="btn" onClick={onClose}>Cancel</button>
                        </div>
                    </form>

                    <CarPopup
                        isOpen={showCarPopup}
                        onClose={() => setShowCarPopup(false)}
                        onSave={handleSaveCar}
                        car={currentCar}
                        makes={makes}
                    />
                </Popup>
            )}
        </>
    );
};

export default ClientPopup;
