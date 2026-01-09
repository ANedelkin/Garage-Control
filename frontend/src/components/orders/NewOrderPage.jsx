import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { orderApi } from '../../services/orderApi';
import { partApi } from '../../services/partApi';
import { request } from '../../Utilities/request';
import ServiceForm from './ServiceForm';
import '../../assets/css/orders.css';

const NewOrderPage = () => {
    const { id } = useParams();
    const navigate = useNavigate();
    const isEdit = !!id;

    const [cars, setCars] = useState([]);
    const [carSearch, setCarSearch] = useState('');
    const [selectedCar, setSelectedCar] = useState(null);
    const [suggestions, setSuggestions] = useState([]);

    const [services, setServices] = useState([]);

    // Config Data
    const [jobTypes, setJobTypes] = useState([]);
    const [workers, setWorkers] = useState([]);
    const [allParts, setAllParts] = useState([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const loadInitialData = async () => {
            try {
                // Fetch essential config and order data concurrently
                const [jtData, workerData, orderData] = await Promise.all([
                    (await request('GET', 'jobtype/all')).json(),
                    (await request('GET', 'worker/all')).json(),
                    isEdit ? orderApi.getOrder(id) : Promise.resolve(null)
                ]);

                setJobTypes(jtData);
                setWorkers(workerData);

                if (isEdit && orderData) {
                    setSelectedCar({
                        id: orderData.carId,
                        registrationNumber: orderData.carRegistrationNumber,
                        model: { name: orderData.carName.split(' ').slice(1).join(' '), make: { name: orderData.carName.split(' ')[0] } }
                    });
                    setCarSearch(`${orderData.carRegistrationNumber} - ${orderData.carName}`);

                    setServices(orderData.jobs.map(j => ({
                        id: j.id,
                        jobTypeId: j.jobTypeId,
                        workerId: j.workerId,
                        laborCost: j.laborCost,
                        startTime: j.startTime,
                        endTime: j.endTime,
                        description: j.description,
                        status: j.status,
                        parts: j.parts.map(p => ({
                            partId: p.partId,
                            name: p.partName,
                            quantity: p.quantity,
                            price: p.price
                        }))
                    })));
                } else if (!isEdit) {
                    addService();
                }
            } catch (e) {
                console.error("Failed to load initial data", e);
            } finally {
                setLoading(false);
            }

            // Background load the heavy datasets for suggestions
            try {
                const [carsData, partsData] = await Promise.all([
                    (await request('GET', 'vehicle/all')).json(),
                    partApi.getAllParts()
                ]);
                setCars(carsData);
                setAllParts(partsData);
            } catch (e) {
                console.error("Failed to load search data", e);
            }
        };
        loadInitialData();
    }, [id, isEdit]);

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
        setServices(prev => [...prev, {
            id: 'temp-' + Date.now(),
            jobTypeId: '',
            workerId: '',
            laborCost: 0,
            startTime: '',
            endTime: '',
            description: '',
            parts: [],
            status: 0
        }]);
    };

    const updateService = (sid, field, value) => {
        setServices(prev => prev.map(s => s.id === sid ? { ...s, [field]: value } : s));
    };

    const removeService = (sid) => {
        setServices(prev => prev.filter(s => s.id !== sid));
    };

    const handleSave = async () => {
        if (!selectedCar) {
            alert("Please select a car");
            return;
        }

        const model = {
            carId: selectedCar.id,
            jobs: services.map(s => ({
                id: s.id.toString().startsWith('temp-') ? null : s.id,
                jobTypeId: s.jobTypeId,
                workerId: s.workerId,
                laborCost: s.laborCost,
                startTime: s.startTime || new Date().toISOString(),
                endTime: s.endTime || new Date().toISOString(),
                description: s.description,
                status: s.status,
                parts: s.parts.map(p => ({
                    partId: p.partId,
                    quantity: p.quantity,
                    price: p.price
                }))
            }))
        };

        try {
            if (isEdit) {
                await orderApi.updateOrder(id, model);
                alert("Order updated!");
            } else {
                await orderApi.createOrder(model);
                alert("Order created!");
            }
            navigate('/orders');
        } catch (e) {
            console.error(e);
            alert(`Failed to ${isEdit ? 'update' : 'create'} order`);
        }
    };

    if (loading) return <main className="main"><p>Loading...</p></main>;

    return (
        <main className="main new-order">
            <div className="orders-header">
                <h3>{isEdit ? "Edit Order" : "New Order"}</h3>
                <button className="btn primary" onClick={handleSave}>
                    {isEdit ? "Update & Save" : "Create & Save"}
                </button>
            </div>

            <div className="tile" style={{ overflow: 'visible' }}>
                <div style={{ position: 'relative' }}>
                    <label>Select Car</label>
                    <input
                        type="text"
                        placeholder="Search by Reg Number or Model..."
                        value={carSearch}
                        onChange={e => handleCarSearch(e.target.value)}
                        disabled={isEdit} // Optional: usually don't change car on existing order
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

                {services.map((s, i) => (
                    <div key={s.id}>
                        {isEdit && (
                            <div className="form-group" style={{ maxWidth: '200px', marginBottom: '0.5rem' }}>
                                <label>Job Status</label>
                                <select
                                    className="status-selector"
                                    value={s.status}
                                    onChange={e => updateService(s.id, 'status', parseInt(e.target.value))}
                                >
                                    <option value={0}>Pending</option>
                                    <option value={1}>In Progress</option>
                                    <option value={2}>Finished</option>
                                </select>
                            </div>
                        )}
                        <ServiceForm
                            index={i}
                            service={s}
                            updateService={updateService}
                            removeService={removeService}
                            jobTypes={jobTypes}
                            workers={workers}
                            allParts={allParts}
                        />
                    </div>
                ))}

                <button className="btn" onClick={addService} style={{ width: 'fit-content', marginTop: '20px' }}>
                    + Add Service (Job)
                </button>
            </div>
        </main>
    );
};

export default NewOrderPage;

