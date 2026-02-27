import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import Popup from '../common/Popup';
import { orderApi } from '../../services/orderApi';
import { request } from '../../Utilities/request';
import '../../assets/css/orders.css';

const NewOrderSetup = ({ onClose, onSuccess }) => {
    const navigate = useNavigate();
    const [cars, setCars] = useState([]);
    const [carSearch, setCarSearch] = useState('');
    const [selectedCar, setSelectedCar] = useState(null);
    const [suggestions, setSuggestions] = useState([]);
    const [kilometers, setKilometers] = useState(0);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const loadCars = async () => {
            try {
                const carsData = await request('GET', 'vehicle/all');
                setCars(carsData);
            } catch (e) {
                console.error("Failed to load cars", e);
            } finally {
                setLoading(false);
            }
        };
        loadCars();
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

    const handleCreateOrder = async () => {
        if (!selectedCar) {
            alert("Please select a car");
            return;
        }

        try {
            const result = await orderApi.createOrder({
                carId: selectedCar.id,
                kilometers: parseInt(kilometers) || 0,
                jobs: [] // Start with empty jobs
            });

            if (onSuccess) {
                onSuccess();
            } else if (onClose) {
                onClose();
            }
        } catch (e) {
            console.error(e);
            alert("Failed to create order");
        }
    };

    if (loading) return <Popup isOpen={true} onClose={onClose}><p>Loading...</p></Popup>;

    return (
        <Popup isOpen={true} onClose={onClose} title="New Order" maxWidth="600px">
            <div className="form-section" style={{ position: 'relative' }}>
                <label>Select Car:</label>
                <input
                    type="text"
                    placeholder="Search by Reg Number or Model..."
                    value={carSearch}
                    onChange={e => handleCarSearch(e.target.value)}
                />
                {suggestions.length > 0 && (
                    <ul className="car-suggestions" style={{ position: 'absolute', top: '100%', left: 0, right: 0, zIndex: 100, background: 'var(--card-bg)', border: '1px solid var(--border-color)', borderRadius: '8px', maxHeight: '200px', overflowY: 'auto' }}>
                        {suggestions.map(c => (
                            <li key={c.id} onClick={() => selectCar(c)} style={{ padding: '10px', cursor: 'pointer', borderBottom: '1px solid var(--border-color)' }}>
                                <b>{c.registrationNumber}</b> - {c.model.make.name} {c.model.name}
                            </li>
                        ))}
                    </ul>
                )}
            </div>

            <div className="form-section">
                <label>Current Kilometers:</label>
                <input
                    type="number"
                    value={kilometers}
                    onChange={e => setKilometers(e.target.value)}
                />
            </div>

            <div className="form-footer" style={{ marginTop: '20px' }}>
                <button className="btn primary" onClick={handleCreateOrder} disabled={!selectedCar}>
                    Create Order
                </button>
                <button className="btn" onClick={onClose}>
                    Cancel
                </button>
            </div>
        </Popup>
    );
};

export default NewOrderSetup;
