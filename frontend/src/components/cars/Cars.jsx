import React, { useState, useEffect, useRef } from 'react';
import { useNavigate, useParams, useSearchParams } from 'react-router-dom';
import { vehicleApi } from '../../services/vehicleApi';
import { makeApi } from '../../services/makeApi';
import { modelApi } from '../../services/modelApi';
import { usePopup } from '../../context/PopupContext';
import CarPopup from './CarPopup';
import '../../assets/css/clients.css';

const Cars = () => {
    const navigate = useNavigate();
    const { carId } = useParams();
    const [cars, setCars] = useState([]);
    const [loading, setLoading] = useState(true);
    const [search, setSearch] = useState('');
    const [makes, setMakes] = useState({}); // id -> name map
    const [makesList, setMakesList] = useState([]); // array for popup prop
    const [models, setModels] = useState({}); // id -> name map
    const rowRefs = useRef({});
    const [searchParams] = useSearchParams();
    const highlight = searchParams.get('highlight') === 'true';

    const { addPopup, removeLastPopup } = usePopup();

    useEffect(() => {
        const fetchData = async () => {
            try {
                const [carsData, makesData] = await Promise.all([
                    vehicleApi.getAll(),
                    makeApi.getAll()
                ]);

                setCars(carsData);
                setMakesList(makesData);

                // create map for makes
                const makesMap = {};
                makesData.forEach(m => makesMap[m.id] = m.name);
                setMakes(makesMap);

                const uniqueMakeIds = [...new Set(carsData.map(c => c.makeId))];

                const modelsMap = {};
                await Promise.all(uniqueMakeIds.map(async (makeId) => {
                    if (makeId) {
                        try {
                            const modelsForMake = await modelApi.getAll(makeId);
                            modelsForMake.forEach(m => modelsMap[m.id] = m.name);
                        } catch (e) {
                            console.error(`Failed to load models for make ${makeId}`, e);
                        }
                    }
                }));
                setModels(modelsMap);

            } catch (error) {
                console.error("Failed to load cars data", error);
            } finally {
                setLoading(false);
            }
        };
        fetchData();
    }, []);


    const filteredCars = cars.filter(c =>
        (c.registrationNumber && c.registrationNumber.toLowerCase().includes(search.toLowerCase())) ||
        (c.ownerName && c.ownerName.toLowerCase().includes(search.toLowerCase())) ||
        (c.vin && c.vin.toLowerCase().includes(search.toLowerCase()))
    );

    const handleDelete = async (e, id) => {
        e.stopPropagation(); // prevent row click
        if (window.confirm("Are you sure you want to delete this car?")) {
            try {
                await vehicleApi.delete(id);
                setCars(cars.filter(c => c.id !== id));
            } catch (error) {
                console.error("Failed to delete car", error);
                alert("Failed to delete car");
            }
        }
    };

    const handleRowClick = (car) => {
        addPopup(
            car.id ? 'Edit Car' : 'Add Car',
            <CarPopup
                onClose={() => { removeLastPopup(); navigate('/cars'); }}
                onSave={handleSaveCar}
                car={car}
                makes={makesList}
            />,
            false,
            () => navigate('/cars')
        );
    };

    useEffect(() => {
        if (!loading && carId && carId !== 'new') {
            const row = rowRefs.current[carId];
            if (row) {
                row.scrollIntoView({ behavior: 'smooth', block: 'center' });
            }
            if (!highlight) {
                const car = cars.find(c => c.id === carId);
                if (car) handleRowClick(car);
            }
        }
    }, [loading, carId, cars, highlight]);

    useEffect(() => {
        if (carId === 'new') {
            handleRowClick({});
        }
    }, [carId]);

    const handleSaveCar = async (carData) => {
        try {
            await vehicleApi.edit(carData.id, carData);

            // update local list
            const updatedCars = cars.map(c => c.id === carData.id ? { ...c, ...carData } : c);
            setCars(updatedCars);

            // if model changed, ensure we failover to ID or fetch name (simpler to just show ID until refresh or we could fetch name here)
            // But realistically, user might just close popup. 
            // Ideally we should update the models hashmap if a new model is introduced that we haven't seen.
            // But since we can't easily fetch just one model name without API call...
            // Let's just create a quick fetch to update the map if it's missing.
            if (carData.modelId && !models[carData.modelId]) {
                const mRes = await modelApi.getModels(carData.makeId);
                const newModel = mRes.find(m => m.id === carData.modelId);
                if (newModel) {
                    setModels(prev => ({ ...prev, [newModel.id]: newModel.name }));
                }
            }

            removeLastPopup();
            navigate('/cars');
        } catch (error) {
            console.error("Failed to save car", error);
            alert("Failed to save changes");
        }
    };

    const handleContainerClick = () => {
        if (carId) {
            navigate('/cars', { replace: true });
        }
    };

    return (
        <main className="main" onClick={handleContainerClick}>
            <div className="header">
                <input
                    type="text"
                    placeholder="Search by Plate, VIN or Owner..."
                    value={search}
                    onChange={e => setSearch(e.target.value)}
                />
            </div>

            <div className="tile">
                <h3>All Cars</h3>
                <div style={{ overflowX: 'auto' }}>
                    <table className="clients-table">
                        <thead>
                            <tr>
                                <th>Make</th>
                                <th>Model</th>
                                <th>Plate</th>
                                <th>VIN</th>
                                <th>KM</th>
                                <th>Owner</th>
                                <th></th>
                            </tr>
                        </thead>
                        <tbody>
                            {loading ? <tr><td colSpan="7">Loading...</td></tr> : filteredCars.map(c => (
                                <tr 
                                    key={c.id} 
                                    ref={el => rowRefs.current[c.id] = el}
                                    onClick={(e) => { e.stopPropagation(); handleRowClick(c); }} 
                                    style={{ cursor: 'pointer' }} 
                                    className={`clickable-row ${carId === c.id ? 'highlight-outline' : ''}`}
                                >
                                    <td>{makes[c.makeId] || c.makeId}</td>
                                    <td>{models[c.modelId] || c.modelId}</td>
                                    <td>{c.registrationNumber}</td>
                                    <td>{c.vin || '-'}</td>
                                    <td>{c.kilometers}</td>
                                    <td>{c.ownerName}</td>
                                    <td>
                                        <button className="btn delete icon-btn" onClick={(e) => handleDelete(e, c.id)}>
                                            <i className="fa-solid fa-trash"></i>
                                        </button>
                                    </td>
                                </tr>
                            ))}
                            {!loading && filteredCars.length === 0 && (
                                <tr><td colSpan="6" style={{ textAlign: 'center' }}>No cars found.</td></tr>
                            )}
                        </tbody>
                    </table>
                </div>
            </div>


            <footer>GarageFlow — Cars Management</footer>
        </main>
    );
};

export default Cars;
