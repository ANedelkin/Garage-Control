import React, { useState, useEffect } from 'react';
import '../../assets/css/orders.css';

const OrderDetailsPopup = ({ order, cars, onClose, onSave }) => {
    const [carSearch, setCarSearch] = useState(`${order.carRegistrationNumber} - ${order.carName}`);
    const [selectedCarId, setSelectedCarId] = useState(order.carId);
    const [kilometers, setKilometers] = useState(order.kilometers);
    const [suggestions, setSuggestions] = useState([]);
    const [isDone, setIsDone] = useState(order.isDone);

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
        <div className="popup-overlay">
            <div className="popup-content" style={{ maxWidth: '450px' }}>
                <div className="popup-header">
                    <h3>Order Details</h3>
                    <button className="btn-close" onClick={onClose}>&times;</button>
                </div>
                <div className="popup-body">
                    <div className="form-group" style={{ position: 'relative' }}>
                        <label>Car:</label>
                        <input
                            type="text"
                            placeholder="Search car..."
                            value={carSearch}
                            onChange={(e) => handleCarSearch(e.target.value)}
                        />
                        {suggestions.length > 0 && (
                            <ul className="car-suggestions" style={{ position: 'absolute', top: '100%', left: 0, right: 0, zIndex: 100, background: 'var(--card-bg)', border: '1px solid var(--border-color)', borderRadius: '8px', maxHeight: '150px', overflowY: 'auto' }}>
                                {suggestions.map(c => (
                                    <li key={c.id} onClick={() => selectCar(c)} style={{ padding: '8px', cursor: 'pointer', borderBottom: '1px solid var(--border-color)' }}>
                                        {c.registrationNumber} - {c.model.make.name} {c.model.name}
                                    </li>
                                ))}
                            </ul>
                        )}
                    </div>
                    <div className="form-group">
                        <label>Kilometers:</label>
                        <input
                            type="number"
                            value={kilometers}
                            onChange={(e) => setKilometers(e.target.value)}
                        />
                    </div>

                    <div style={{ display: 'flex', flexWrap: 'wrap', gap: '10px', marginTop: '10px' }}>
                        <button className="btn secondary" style={{ flex: 1 }} onClick={() => setIsDone(!isDone)}>
                            {isDone ? 'Mark as Not Done' : 'Mark as Done'}
                        </button>
                        <button className="btn secondary" style={{ flex: 1 }} onClick={() => alert("Print Invoice (Placeholder)")}>
                            Print Invoice (Placeholder)
                        </button>
                    </div>
                </div>
                <div className="popup-footer">
                    <button className="btn primary" onClick={handleSave}>Save Changes</button>
                    <button className="btn" onClick={onClose}>Cancel</button>
                </div>
            </div>
        </div>
    );
};

export default OrderDetailsPopup;