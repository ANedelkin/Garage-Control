import React, { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import "../../assets/css/clients.css";
import { clientApi } from "../../services/clientApi";
import { vehicleApi } from "../../services/vehicleApi";
import { makeApi } from "../../services/makeApi";
import { modelApi } from "../../services/modelApi";
import CarPopup from "../cars/CarPopup";

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

    // Cars Logic
    const [cars, setCars] = useState([]);
    const [makes, setMakes] = useState([]);
    const [modelsMap, setModelsMap] = useState({}); // map[modelId] -> modelName

    const [showCarPopup, setShowCarPopup] = useState(false);
    const [currentCar, setCurrentCar] = useState(null);

    // Initial Load
    useEffect(() => {
        const fetchData = async () => {
            try {
                const makesRes = await makeApi.getAll();
                setMakes(makesRes);

                if (!isNew) {
                    const clientRes = await clientApi.getDetails(id);
                    if (clientRes) setClient(clientRes);

                    const carsRes = await vehicleApi.getByClient(id);
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
                }
            } catch (error) {
                console.error("Error loading data", error);
                alert("Failed to load data.");
            } finally {
                setLoading(false);
            }
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

    const handleSaveCar = async (carData) => {
        try {
            const carDto = {
                ...carData,
                ownerId: id // Ensure owner is set
            };

            if (carData.id) {
                await vehicleApi.edit(carDto);
            } else {
                await vehicleApi.create(carDto);
            }

            // Refresh cars list
            const carsRes = await vehicleApi.getByClient(id);
            setCars(carsRes);

            // Re-fetch models map if needed
            if (!modelsMap[carData.modelId]) {
                const mRes = await modelApi.getModels(carData.makeId);
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

    if (loading) return <div>Loading...</div>;

    return (
        <main className="main container edit-client">
            <div className="tile">
                <h3 className="tile-header">{isNew ? "New Client" : "Edit Client"}</h3>
                <form onSubmit={handleSave}>
                    <div className="horizontal">
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
                                <label>Registration Number (Personal)</label>
                                <input
                                    type="text"
                                    value={client.registrationNumber}
                                    onChange={e => setClient({ ...client, registrationNumber: e.target.value })}
                                />
                            </div>
                        </div>

                        <div className="form-column cars-section">
                            <div className="form-section max-height">
                                <div className="section-header">
                                    <label>Cars</label>
                                    <button type="button" className="btn" onClick={() => openCarPopup()}>+ Add Car</button>
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

                    <div className="form-footer" style={{ marginTop: '20px' }}>
                        <button type="submit" className="btn">Save Client</button>
                    </div>
                </form>
            </div>

            <CarPopup
                isOpen={showCarPopup}
                onClose={() => setShowCarPopup(false)}
                onSave={handleSaveCar}
                car={currentCar}
                makes={makes}
            />
        </main>
    );
};

export default EditClient;
