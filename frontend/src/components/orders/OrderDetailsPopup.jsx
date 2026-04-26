import React, { useState, useEffect, useRef } from 'react';
import { request } from '../../Utilities/request';
import Suggestions from '../common/Suggestions';
import '../../assets/css/orders.css';
import FieldError from '../common/FieldError.jsx';
import { usePopup } from '../../context/PopupContext';
import ConfirmationPopup from '../common/ConfirmationPopup';

const OrderDetailsPopup = ({ order, cars, onClose, onSave, errors = {} }) => {
    const { addPopup, removeLastPopup } = usePopup();
    const [carSearch, setCarSearch] = useState(order.carRegistrationNumber);
    const [selectedCarId, setSelectedCarId] = useState(order.carId);
    const [kilometers, setKilometers] = useState(order.kilometers);
    const [minKilometers, setMinKilometers] = useState(order.kilometers);
    const [suggestions, setSuggestions] = useState([]);
    const suggestionsRef = useRef(null);

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
        setSelectedCarId(car.id);
        setCarSearch(car.registrationNumber);
        setKilometers(car.kilometers || 0);
        setMinKilometers(car.kilometers || 0);
        setSuggestions([]);
    };

    const handleSave = (markDone = false) => {
        onSave({
            carId: selectedCarId,
            kilometers: parseInt(kilometers) || 0,
            isDone: markDone
        });
    };

    return (
        <div className="order-details-form">
            <div className="form-section" style={{ position: 'relative' }}>
                <label>Car:</label>
                <input
                    type="text"
                    name="CarId"
                    placeholder="Search car..."
                    value={carSearch}
                    onInput={(e) => handleCarSearch(e.target.value)}
                    onFocus={() => handleCarSearch(carSearch)}
                    onKeyDown={(e) => suggestionsRef.current?.handleKeyDown(e)}
                    onBlur={() => {
                        setTimeout(() => setSuggestions([]), 200);
                    }}
                    disabled={order.isDone}
                />
                <FieldError name="CarId" errors={errors} />
                <Suggestions
                    ref={suggestionsRef}
                    suggestions={suggestions}
                    isOpen={suggestions.length > 0 && !order.isDone}
                    onSelect={selectCar}
                    onClose={() => setSuggestions([])}
                    renderItem={(car) => (
                        <>
                            {car.registrationNumber} - {car.model?.make?.name} {car.model?.name}
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
                    min={minKilometers}
                    value={kilometers}
                    onChange={(e) => setKilometers(e.target.value)}
                    disabled={order.isDone}
                />
                <FieldError name="Kilometers" errors={errors} />
                {errors.general && errors.general.toLowerCase().includes('kilometers') && (
                    <p className="field-error">{errors.general}</p>
                )}
            </div>
            <div style={{ display: 'flex', flexWrap: 'wrap', gap: '10px', marginTop: '10px' }}>
                {!order.isDone && (
                    <button
                        className="btn"
                        onClick={() => {
                            addPopup(
                                'Confirm Completion',
                                <ConfirmationPopup
                                    message="Are you sure you want to mark this order as complete? This action is permanent and will archive the order."
                                    confirmText="Mark as Done"
                                    isDanger={false}
                                    onConfirm={() => {
                                        removeLastPopup();
                                        handleSave(true);
                                    }}
                                    onClose={removeLastPopup}
                                />
                            );
                        }}
                    >
                        Save & Mark as Done
                    </button>
                )}
                <button
                    className="btn"
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
                {errors.general && !errors.general.toLowerCase().includes('kilometers') && <p className="form-error">{errors.general}</p>}
                {!order.isDone && (
                    <button className="btn" onClick={() => handleSave(false)}>Save Changes</button>
                )}
                <button className="btn" onClick={onClose}>{order.isDone ? 'Close' : 'Cancel'}</button>
            </div>
        </div>
    );
};

export default OrderDetailsPopup;