import React, { useState, useEffect } from 'react';
import '../../assets/css/clients.css';
import { clientApi } from '../../services/clientApi';
import { vehicleApi } from '../../services/vehicleApi';
import { makeApi } from '../../services/makeApi';
import { modelApi } from '../../services/modelApi';
import { usePopup } from '../../context/PopupContext';
import Popup from '../common/Popup';
import CarPopup from '../cars/CarPopup';
import ConfirmationPopup from '../common/ConfirmationPopup';
import FieldError from '../common/FieldError.jsx';
import { parseValidationErrors } from '../../Utilities/formErrors.js';

const ClientPopup = ({ onClose, onSave, clientId }) => {
    const isNew = !clientId;

    const [client, setClient] = useState({
        name: "",
        phoneNumber: "",
        email: "",
        address: "",
        registrationNumber: ""
    });

    const [errors, setErrors] = useState({});

    const [loading, setLoading] = useState(false);

    // Cars Logic
    const [cars, setCars] = useState([]);
    const [makes, setMakes] = useState([]);
    const [modelsMap, setModelsMap] = useState({});

    const { addPopup, removeLastPopup, updateLastPopup } = usePopup();
    const [activeTab, setActiveTab] = useState("info");
    const [isMobile, setIsMobile] = useState(window.innerWidth < 800);

    useEffect(() => {
        const handleResize = () => setIsMobile(window.innerWidth < 800);
        window.addEventListener("resize", handleResize);
        return () => window.removeEventListener("resize", handleResize);
    }, []);

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

    // Load client data when component mounts or clientId changes
    useEffect(() => {
        const fetchData = async () => {
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
    }, [clientId, isNew]);

    const handleSave = async (e) => {
        e.preventDefault();
        try {
            let savedClientId = clientId;
            if (isNew) {
                await clientApi.create({ ...client, cars });
            } else {
                await clientApi.edit(clientId, client);
            }

            onSave();
            onClose();
        } catch (error) {
            console.error('Error saving client info:', error);
            setErrors(parseValidationErrors(error));
        }
    };

    const handleSaveCar = async (carData) => {
        try {
            if (isNew) {
                // Local mode (isNew client)
                if (carData.id && carData.id.toString().startsWith('temp-')) {
                    // Updating car in local list
                    setCars(cars.map(c => c.id === carData.id ? { ...carData } : c));
                } else {
                    // Adding new car to local list
                    const tempId = 'temp-' + Date.now();
                    setCars([...cars, { ...carData, id: tempId }]);
                }

                // Update modelsMap for display
                if (!modelsMap[carData.modelId]) {
                    const mRes = await modelApi.getAll(carData.makeId);
                    const updMap = { ...modelsMap };
                    mRes.forEach(m => updMap[m.id] = m.name);
                    setModelsMap(updMap);
                }

                removeLastPopup();
                return;
            }

            // Remote mode (existing client)
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

            removeLastPopup();
        } catch (error) {
            console.error("Error saving car", error);
            const errors = parseValidationErrors(error);
            updateLastPopup(
                <CarPopup
                    onClose={removeLastPopup}
                    onSave={handleSaveCar}
                    car={carData}
                    makes={makes}
                    errors={errors}
                />
            );
        }
    };

    const handleDeleteCar = async (carId) => {
        const car = cars.find(c => c.id === carId);
        const carTitle = car ?
            `${makes.find(m => m.id === car.makeId)?.name || ''} ${modelsMap[car.modelId] || ''} (${car.registrationNumber})` :
            'this car';

        addPopup(
            'Delete Car',
            <ConfirmationPopup
                message={`Are you sure you want to delete ${carTitle}?`}
                confirmText="Delete"
                isDanger={true}
                onConfirm={async () => {
                    try {
                        if (isNew || carId.toString().startsWith('temp-')) {
                            setCars(cars.filter(c => c.id !== carId));
                            removeLastPopup();
                            return;
                        }
                        await vehicleApi.delete(carId);
                        setCars(cars.filter(c => c.id !== carId));
                        removeLastPopup();
                    } catch (error) {
                        console.error("Error deleting car", error);
                        alert("Failed to delete car.");
                    }
                }}
                onClose={removeLastPopup}
            />
        );
    };

    const openCarPopup = (car = null) => {
        addPopup(
            car ? 'Edit Car' : 'Add Car',
            <CarPopup
                onClose={removeLastPopup}
                onSave={handleSaveCar}
                car={car}
                makes={makes}
            />
        );
    };

    return (
        <>
            <form className="client-form" onSubmit={handleSave}>
                {isMobile && (
                    <div className="popup-tabs">
                        <button
                            type="button"
                            className={`tab-btn ${activeTab === 'info' ? 'active' : ''}`}
                            onClick={() => setActiveTab('info')}
                        >
                            <i className="fa-solid fa-user"></i> Info
                        </button>
                        <button
                            type="button"
                            className={`tab-btn ${activeTab === 'cars' ? 'active' : ''}`}
                            onClick={() => setActiveTab('cars')}
                        >
                            <i className="fa-solid fa-car"></i> Cars
                        </button>
                    </div>
                )}
                <div className="tab-content horizontal">
                    {(!isMobile || activeTab === 'info') && (
                        <div className="form-column">
                            <div className="form-section">
                                <label>Name</label>
                                <input
                                    type="text"
                                    name="Name"
                                    value={client.name}
                                    onChange={e => setClient({ ...client, name: e.target.value })}
                                    required
                                />
                                <FieldError name="Name" errors={errors} />
                            </div>
                            <div className="form-section">
                                <label>Phone Number</label>
                                <input
                                    type="text"
                                    name="PhoneNumber"
                                    value={client.phoneNumber}
                                    onChange={e => setClient({ ...client, phoneNumber: e.target.value })}
                                    required
                                />
                                <FieldError name="PhoneNumber" errors={errors} />
                            </div>
                            <div className="form-section">
                                <label>Email</label>
                                <input
                                    type="email"
                                    name="Email"
                                    value={client.email}
                                    onChange={e => setClient({ ...client, email: e.target.value })}
                                />
                                <FieldError name="Email" errors={errors} />
                            </div>
                            <div className="form-section">
                                <label>Address</label>
                                <input
                                    type="text"
                                    name="Address"
                                    value={client.address}
                                    onChange={e => setClient({ ...client, address: e.target.value })}
                                />
                                <FieldError name="Address" errors={errors} />
                            </div>
                            <div className="form-section">
                                <label>Registration Number (Personal)</label>
                                <input
                                    type="text"
                                    name="RegistrationNumber"
                                    value={client.registrationNumber}
                                    onChange={e => setClient({ ...client, registrationNumber: e.target.value })}
                                />
                                <FieldError name="RegistrationNumber" errors={errors} />
                            </div>
                        </div>
                    )}

                    {(!isMobile || activeTab === 'cars') && (
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
                                                    <strong className="car-title">{makeName} {modelName}</strong>
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
                    )}
                </div>

                <div className="form-footer">
                    {errors.general && <p className="form-error">{errors.general}</p>}
                    <button type="submit" className="btn">Save Client</button>
                    <button type="button" className="btn" onClick={onClose}>Cancel</button>
                </div>
            </form>
        </>
    );
};

export default ClientPopup;
