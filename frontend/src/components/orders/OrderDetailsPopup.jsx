import React, { useState, useEffect, useRef } from 'react';
import { request } from '../../Utilities/request';
import Suggestions from '../common/Suggestions';
import '../../assets/css/orders.css';
import FieldError from '../common/FieldError.jsx';

const OrderDetailsPopup = ({ order, cars, onClose, onSave, errors = {} }) => {
    const [carSearch, setCarSearch] = useState(`${order.carRegistrationNumber} - ${order.carName}`);
    const [selectedCarId, setSelectedCarId] = useState(order.carId);
    const [kilometers, setKilometers] = useState(order.kilometers);
    const [suggestions, setSuggestions] = useState([]);
    const [isDone, setIsDone] = useState(order.isDone);
    const suggestionsRef = useRef(null);

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
        setSelectedCarId(car.id);
        setCarSearch(`${car.registrationNumber} - ${car.model.make.name} ${car.model.name}`);
        setSuggestions([]);
    };

    const handleSave = () => {
        onSave({
            carId: selectedCarId,
            kilometers: parseInt(kilometers) || 0,
            isDone: isDone
        });
    };

    return (
        <div className="order-details-form">
            <div className="form-section">
                <label>Car:</label>
                <input
                    type="text"
                    name="CarId"
                    placeholder="Search car..."
                    value={carSearch}
                    onChange={(e) => handleCarSearch(e.target.value)}
                    onKeyDown={(e) => suggestionsRef.current?.handleKeyDown(e)}
                    onBlur={() => setTimeout(() => setSuggestions([]), 200)}
                    style={{ position: 'relative' }}
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
                            {car.registrationNumber} - {car.model.make.name} {car.model.name}
                        </>
                    )}
                    maxHeight="150px"
                    style={{ position: 'absolute', top: '100%', left: 0, right: 0 }}
                />
            </div>
            <div className="form-section">
                <label>Kilometers:</label>
                <input
                    type="number"
                    name="Kilometers"
                    value={kilometers}
                    onChange={(e) => setKilometers(e.target.value)}
                />
                <FieldError name="Kilometers" errors={errors} />
            </div>
            <div style={{ display: 'flex', flexWrap: 'wrap', gap: '10px', marginTop: '10px' }}>
                <button className="btn secondary" style={{ flex: 1 }} onClick={() => setIsDone(!isDone)}>
                    {isDone ? 'Mark as Not Done' : 'Mark as Done'}
                </button>
                <button
                    className="btn secondary"
                    style={{ flex: 1 }}
                    onClick={async () => {
                        try {
                            const response = await request('get',
                                `order/${order.id}/invoice`,
                                null,
                                { cache: 'no-store' });

                            if (!response.ok) throw new Error('Failed to fetch invoice');
                            const blob = await response.blob();
                            const blobUrl = URL.createObjectURL(blob);
                            const printWindow = window.open(blobUrl, '_blank');
                            if (printWindow) {
                                printWindow.addEventListener('load', () => {
                                    printWindow.focus();
                                    printWindow.print();
                                });
                            }
                        } catch (err) {
                            console.error(err);
                            alert('Failed to load invoice for printing.');
                        }
                    }}
                >
                    Print Invoice
                </button>
            </div>
            <div className="divider"></div>
            <div className="form-footer">
                {errors.general && <p className="form-error">{errors.general}</p>}
                <button className="btn" onClick={handleSave}>Save Changes</button>
                <button className="btn" onClick={onClose}>Cancel</button>
            </div>
        </div>
    );
};

export default OrderDetailsPopup;