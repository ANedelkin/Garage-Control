import React, { useState, useEffect } from 'react';
import { modelApi } from '../../services/modelApi';
import Dropdown from '../common/Dropdown.jsx';
import FieldError from '../common/FieldError.jsx';

const CarPopup = ({ onClose, onSave, car, makes, errors = {} }) => {
    const [currentCar, setCurrentCar] = useState({
        id: null,
        makeId: "",
        modelId: "",
        registrationNumber: "",
        vin: "",
        kilometers: 0
    });
    const [popupModels, setPopupModels] = useState([]);

    useEffect(() => {
        if (car) {
            setCurrentCar({ ...car });
        } else {
            setCurrentCar({ id: null, makeId: "", modelId: "", registrationNumber: "", vin: "", kilometers: 0 });
        }
    }, [car]);

    useEffect(() => {
        if (currentCar.makeId) {
            const fetchPopupModels = async () => {
                try {
                    const mRes = await modelApi.getAll(currentCar.makeId);
                    setPopupModels(mRes);
                } catch (error) {
                    console.error("Failed to fetch models", error);
                }
            };
            fetchPopupModels();
        } else {
            setPopupModels([]);
        }
    }, [currentCar.makeId]);

    const handleSave = () => {
        onSave(currentCar);
    };

    return (
        <div className="car-form">
            <div className="form-body">
                <div className="form-section">
                    <label>Make</label>
                    <Dropdown
                        value={currentCar.makeId}
                        onChange={e => setCurrentCar({ ...currentCar, makeId: e.target.value, modelId: "" })}
                    >
                        <option value="">Select Make</option>
                        {makes.map(m => <option key={m.id} value={m.id}>{m.name}</option>)}
                    </Dropdown>
                    <FieldError name="MakeId" errors={errors} />
                </div>

                <div className="form-section">
                    <label>Model</label>
                    <Dropdown
                        value={currentCar.modelId}
                        onChange={e => setCurrentCar({ ...currentCar, modelId: e.target.value })}
                        disabled={!currentCar.makeId}
                    >
                        <option value="">Select Model</option>
                        {popupModels.map(m => <option key={m.id} value={m.id}>{m.name}</option>)}
                    </Dropdown>
                    <FieldError name="ModelId" errors={errors} />
                </div>

                <div className="form-section">
                    <label>Registration Number (Plate)</label>
                    <input
                        type="text"
                        name="RegistrationNumber"
                        value={currentCar.registrationNumber}
                        onChange={e => setCurrentCar({ ...currentCar, registrationNumber: e.target.value })}
                    />
                    <FieldError name="RegistrationNumber" errors={errors} />
                </div>

                <div className="form-section">
                    <label>VIN (Optional)</label>
                    <input
                        type="text"
                        name="Vin"
                        value={currentCar.vin || ''}
                        onChange={e => setCurrentCar({ ...currentCar, vin: e.target.value })}
                    />
                    <FieldError name="Vin" errors={errors} />
                </div>

                <div className="form-section">
                    <label>Kilometers</label>
                    <input
                        type="number"
                        name="Kilometers"
                        min="0"
                        value={currentCar.kilometers}
                        onChange={e => setCurrentCar({ ...currentCar, kilometers: parseInt(e.target.value) || 0 })}
                    />
                    <FieldError name="Kilometers" errors={errors} />
                </div>
            </div>

            <div className="form-footer">
                {errors.general && <p className="form-error">{errors.general}</p>}
                <button type="button" className="btn" onClick={handleSave}>Save</button>
                <button type="button" className="btn" onClick={onClose}>Cancel</button>
            </div>
        </div>
    );
};

export default CarPopup;
