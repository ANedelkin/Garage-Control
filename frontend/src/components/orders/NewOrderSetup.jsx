import React, { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { orderApi } from '../../services/orderApi';
import { request } from '../../Utilities/request';
import Suggestions from '../common/Suggestions';
import '../../assets/css/orders.css';
import { parseValidationErrors } from '../../Utilities/formErrors.js';
import FieldError from '../common/FieldError.jsx';

const NewOrderSetup = ({ onClose, onSuccess }) => {
    const navigate = useNavigate();
    const [cars, setCars] = useState([]);
    const [carSearch, setCarSearch] = useState('');
    const [selectedCar, setSelectedCar] = useState(null);
    const [suggestions, setSuggestions] = useState([]);
    const [kilometers, setKilometers] = useState(0);
    const [loading, setLoading] = useState(true);
    const [errors, setErrors] = useState({});
    const suggestionsRef = useRef(null);

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
            c.registrationNumber?.toLowerCase().includes(val.toLowerCase()) ||
            (c.model?.name?.toLowerCase().includes(val.toLowerCase()))
        );
        setSuggestions(filtered);
    };

    const selectCar = (car) => {
        setSelectedCar(car);
        setCarSearch(car.registrationNumber);
        setKilometers(car.kilometers || 0);
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
            setErrors(parseValidationErrors(e));
        }
    };

    if (loading) return <div>Loading...</div>;

    return (
        <>
            <div className="form-section" style={{ position: 'relative' }}>
                <label>Select Car:</label>
                <input
                    type="text"
                    name="CarId"
                    placeholder="Search by Reg Number or Model..."
                    value={carSearch}
                    onInput={e => handleCarSearch(e.target.value)}
                    onFocus={() => handleCarSearch(carSearch)}
                    onKeyDown={(e) => suggestionsRef.current?.handleKeyDown(e)}
                    onBlur={() => {
                        setTimeout(() => setSuggestions([]), 200);
                    }}
                />
                <FieldError name="CarId" errors={errors} />
                <Suggestions
                    ref={suggestionsRef}
                    suggestions={suggestions}
                    isOpen={suggestions.length > 0}
                    onSelect={selectCar}
                    onClose={() => setSuggestions([])}
                    renderItem={(car) => (
                        <>
                            <b>{car.registrationNumber}</b> - {car.model?.make?.name} {car.model?.name}
                        </>
                    )}
                    maxHeight="200px"
                    style={{ position: 'absolute', top: '100%', left: 0, right: 0 }}
                />
            </div>

            <div className="form-section">
                <label>Current Kilometers:</label>
                <input
                    type="number"
                    name="Kilometers"
                    value={kilometers}
                    onChange={e => setKilometers(e.target.value)}
                />
                <FieldError name="Kilometers" errors={errors} />
            </div>

            <div className="form-footer" style={{ marginTop: '20px' }}>
                {errors.general && <p className="form-error">{errors.general}</p>}
                <button className="btn primary" onClick={handleCreateOrder} disabled={!selectedCar}>
                    Create Order
                </button>
                <button className="btn" onClick={onClose}>
                    Cancel
                </button>
            </div>
        </>
    );
};

export default NewOrderSetup;
