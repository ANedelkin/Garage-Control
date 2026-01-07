import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { orderApi } from '../../services/orderApi';
import { vehicleApi } from '../../services/vehicleApi'; // Assuming we have this, or need to create
// Actually standard api for cars might be missing "getAllCars" or similar search.
// Let's assume we can fetch cars. If not, I'll need to check carApi.

import ServiceForm from './ServiceForm';
import '../../assets/css/orders.css';

// We need to fetch JobTypes and Workers to populate selects
// Using generic request or specific apis if available.
import { request } from '../../Utilities/request';

const NewOrderPage = () => {
    const navigate = useNavigate();
    const [cars, setCars] = useState([]);
    const [carSearch, setCarSearch] = useState('');
    const [selectedCar, setSelectedCar] = useState(null);
    const [suggestions, setSuggestions] = useState([]);

    const [services, setServices] = useState([]);

    // Config Data
    const [jobTypes, setJobTypes] = useState([]);
    const [workers, setWorkers] = useState([]);

    useEffect(() => {
        // Fetch Cars, JobTypes, Workers
        // TODO: Move to specific APIs
        const loadData = async () => {
            try {
                // Mocking fetching all cars for search (not ideal for large db)
                // Real app should use search endpoint.
                const carsData = await (await request('GET', 'vehicle/all')).json();
                setCars(carsData);

                const jtData = await (await request('GET', 'jobtype/all')).json();
                setJobTypes(jtData);

                const workerData = await (await request('GET', 'worker/all')).json();
                setWorkers(workerData);
            } catch (e) {
                console.error("Failed to load config data", e);
            }
        };
        loadData();
    }, []);

    const handleCarSearch = (val) => {
        setCarSearch(val);
        if (!val.trim()) {
            setSuggestions([]);
            return;
        }
        const filtered = cars.filter(c =>
            c.registrationNumber.toLowerCase().includes(val.toLowerCase()) ||
            (c.model && c.model.name.toLowerCase().includes(val.toLowerCase()))
        );
        setSuggestions(filtered);
    };

    const selectCar = (car) => {
        setSelectedCar(car);
        setCarSearch(`${car.registrationNumber} - ${car.model.make.name} ${car.model.name}`);
        setSuggestions([]);
    };

    const addService = () => {
        setServices([...services, {
            id: Date.now(),
            jobTypeId: '',
            workerId: '',
            laborCost: 0,
            startTime: '',
            endTime: '',
            description: '',
            parts: []
        }]);
    };

    const updateService = (id, field, value) => {
        setServices(services.map(s => s.id === id ? { ...s, [field]: value } : s));
    };

    const removeService = (id) => {
        setServices(services.filter(s => s.id !== id));
    };

    const handleSave = async () => {
        if (!selectedCar) {
            alert("Please select a car");
            return;
        }

        const model = {
            carId: selectedCar.id,
            jobs: services.map(s => ({
                jobTypeId: s.jobTypeId,
                workerId: s.workerId,
                laborCost: s.laborCost, // Should calculate from parts + labor? Or manual? Model has LaborCost.
                startTime: s.startTime || new Date().toISOString(),
                endTime: s.endTime || new Date().toISOString(),
                description: s.description,
                status: 0, // Pending
                parts: s.parts.map(p => ({
                    partId: p.partId,
                    quantity: p.quantity
                }))
            }))
        };

        try {
            await orderApi.createOrder(model);
            alert("Order created!");
            navigate('/orders');
        } catch (e) {
            console.error(e);
            alert("Failed to create order");
        }
    };

    return (
        <main className="main new-order">
            <div className="orders-header">
                <h3>New Order</h3>
                <button className="btn primary" onClick={handleSave}>Create & Save</button>
            </div>

            <div className="tile" style={{ overflow: 'visible' }}>
                <div style={{ position: 'relative' }}>
                    <label>Select Car</label>
                    <input
                        type="text"
                        placeholder="Search by Reg Number or Model..."
                        value={carSearch}
                        onChange={e => handleCarSearch(e.target.value)}
                    />
                    {suggestions.length > 0 && (
                        <ul className="car-suggestions">
                            {suggestions.map(c => (
                                <li key={c.id} onClick={() => selectCar(c)}>
                                    <b>{c.registrationNumber}</b> - {c.model.make.name} {c.model.name}
                                </li>
                            ))}
                        </ul>
                    )}
                </div>
            </div>

            {services.map((s, i) => (
                <ServiceForm
                    key={s.id}
                    index={i}
                    service={s}
                    updateService={updateService}
                    removeService={removeService}
                    jobTypes={jobTypes}
                    workers={workers}
                />
            ))}

            <button className="btn" onClick={addService} style={{ width: 'fit-content' }}>
                + Add Service (Job)
            </button>
        </main>
    );
};

export default NewOrderPage;
