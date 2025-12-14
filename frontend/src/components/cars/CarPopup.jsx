import React, { useState, useEffect } from 'react';
import { modelApi } from '../../services/modelApi';
import '../../assets/css/common.css';

const CarPopup = ({ isOpen, onClose, onSave, car, makes }) => {
    const [currentCar, setCurrentCar] = useState({
        id: null,
        makeId: "",
        modelId: "",
        registrationNumber: "",
        vin: ""
    });
    const [popupModels, setPopupModels] = useState([]);

    useEffect(() => {
        if (isOpen && car) {
            setCurrentCar({ ...car });
        } else if (isOpen) {
            setCurrentCar({ id: null, makeId: "", modelId: "", registrationNumber: "", vin: "" });
        }
    }, [isOpen, car]);

    useEffect(() => {
        if (isOpen && currentCar.makeId) {
            const fetchPopupModels = async () => {
                try {
                    const mRes = await modelApi.getModels(currentCar.makeId);
                    setPopupModels(mRes);
                } catch (error) {
                    console.error("Failed to fetch models", error);
                }
            };
            fetchPopupModels();
        } else {
            setPopupModels([]);
        }
    }, [isOpen, currentCar.makeId]);

    const handleSave = () => {
        onSave(currentCar);
    };

    if (!isOpen) return null;

    return (
        <div className="popup-overlay" onClick={onClose}>
            <div className="popup" onClick={e => e.stopPropagation()} style={{ width: '400px' }}>
                <h3>{currentCar.id ? "Edit Car" : "Add Car"}</h3>

                <div className="form-section">
                    <label>Make</label>
                    <div className="select-wrapper">
                        <select
                            value={currentCar.makeId}
                            onChange={e => setCurrentCar({ ...currentCar, makeId: e.target.value, modelId: "" })}
                            style={{ width: '100%' }}
                        >
                            <option value="">Select Make</option>
                            {makes.map(m => <option key={m.id} value={m.id}>{m.name}</option>)}
                        </select>
                        <i className="fa-solid fa-chevron-down"></i>
                    </div>
                </div>

                <div className="form-section">
                    <label>Model</label>
                    <div className="select-wrapper">
                        <select
                            value={currentCar.modelId}
                            onChange={e => setCurrentCar({ ...currentCar, modelId: e.target.value })}
                            disabled={!currentCar.makeId}
                            style={{ width: '100%' }}
                        >
                            <option value="">Select Model</option>
                            {popupModels.map(m => <option key={m.id} value={m.id}>{m.name}</option>)}
                        </select>
                        <i className="fa-solid fa-chevron-down"></i>
                    </div>
                </div>

                <div className="form-section">
                    <label>Registration Number (Plate)</label>
                    <input
                        type="text"
                        value={currentCar.registrationNumber}
                        onChange={e => setCurrentCar({ ...currentCar, registrationNumber: e.target.value })}
                    />
                </div>

                <div className="form-section">
                    <label>VIN (Optional)</label>
                    <input
                        type="text"
                        value={currentCar.vin || ''}
                        onChange={e => setCurrentCar({ ...currentCar, vin: e.target.value })}
                    />
                </div>

                <div className="popup-actions">
                    <button type="button" className="btn secondary" onClick={onClose}>Cancel</button>
                    <button type="button" className="btn" onClick={handleSave}>Save</button>
                </div>
            </div>
        </div>
    );
};

export default CarPopup;
