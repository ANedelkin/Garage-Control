import React, { useState, useEffect } from 'react';
import '../../assets/css/makes-models.css';
import { makeApi } from '../../services/makeApi';
import { modelApi } from '../../services/modelApi';

const MakesAndModels = () => {
    const [makes, setMakes] = useState([]);
    const [models, setModels] = useState([]);
    const [selectedMake, setSelectedMake] = useState(null);
    const [loadingMakes, setLoadingMakes] = useState(true);
    const [loadingModels, setLoadingModels] = useState(false);

    // Modal State
    const [showModal, setShowModal] = useState(false);
    const [modalType, setModalType] = useState('make'); // 'make' or 'model'
    const [editingItem, setEditingItem] = useState(null); // null for create, object for edit
    const [itemName, setItemName] = useState('');

    useEffect(() => {
        fetchMakes();
    }, []);

    useEffect(() => {
        if (selectedMake) {
            fetchModels(selectedMake.id);
        } else {
            setModels([]);
        }
    }, [selectedMake]);

    const fetchMakes = async () => {
        setLoadingMakes(true);
        try {
            const data = await makeApi.getAll();
            setMakes(data);
        } catch (error) {
            console.error("Error fetching makes", error);
        } finally {
            setLoadingMakes(false);
        }
    };

    const fetchModels = async (makeId) => {
        setLoadingModels(true);
        try {
            const data = await modelApi.getAll(makeId);
            setModels(data);
        } catch (error) {
            console.error("Error fetching models", error);
        } finally {
            setLoadingModels(false);
        }
    };

    const handleOpenModal = (type, item = null) => {
        setModalType(type);
        setEditingItem(item);
        setItemName(item ? item.name : '');
        setShowModal(true);
    };

    const handleSave = async (e) => {
        e.preventDefault();
        try {
            if (modalType === 'make') {
                if (editingItem) {
                    await makeApi.editMake({ id: editingItem.id, name: itemName });
                } else {
                    await makeApi.createMake({ name: itemName });
                }
                fetchMakes();
            } else {
                if (editingItem) {
                    await modelApi.editModel({ id: editingItem.id, name: itemName, makeId: selectedMake.id });
                } else {
                    await modelApi.createModel({ name: itemName, makeId: selectedMake.id });
                }
                fetchModels(selectedMake.id);
            }
            setShowModal(false);
        } catch (error) {
            console.error("Error saving", error);
            alert("Failed to save. check console.");
        }
    };

    const handleDelete = async (type, id) => {
        if (!window.confirm("Are you sure?")) return;
        try {
            if (type === 'make') {
                await makeApi.deleteMake(id);
                if (selectedMake && selectedMake.id === id) setSelectedMake(null);
                fetchMakes();
            } else {
                await modelApi.deleteModel(id);
                fetchModels(selectedMake.id);
            }
        } catch (error) {
            console.error("Error deleting", error);
            alert("Failed to delete. It might be in use.");
        }
    };

    return (
        <main className="main">
            <div className="makes-models-container">
                {/* Makes Pane */}
                <div className="pane">
                    <div className="pane-header">
                        <h3>Makes</h3>
                        <button className="btn" onClick={() => handleOpenModal('make')}>+ Add Make</button>
                    </div>
                    <div className="list-container">
                        {loadingMakes ? <p>Loading...</p> : (
                            makes.map(make => (
                                <div
                                    key={make.id}
                                    className={`list-item ${selectedMake?.id === make.id ? 'selected' : ''}`}
                                    onClick={() => setSelectedMake(make)}
                                >
                                    <span>{make.name}</span>
                                    <div className="actions">
                                        <button onClick={(e) => { e.stopPropagation(); handleOpenModal('make', make); }}>
                                            <i className="fa-solid fa-pen"></i>
                                        </button>
                                        <button onClick={(e) => { e.stopPropagation(); handleDelete('make', make.id); }}>
                                            <i className="fa-solid fa-trash"></i>
                                        </button>
                                    </div>
                                </div>
                            ))
                        )}
                    </div>
                </div>

                {/* Models Pane */}
                <div className="pane">
                    <div className="pane-header">
                        <h3>Models {selectedMake ? `for ${selectedMake.name}` : ''}</h3>
                        {selectedMake && (
                            <button className="btn" onClick={() => handleOpenModal('model')}>+ Add Model</button>
                        )}
                    </div>
                    <div className="list-container">
                        {!selectedMake ? (
                            <p className="text-muted">Select a make to view models</p>
                        ) : loadingModels ? (
                            <p>Loading...</p>
                        ) : (
                            models.map(model => (
                                <div key={model.id} className="list-item">
                                    <span>{model.name}</span>
                                    <div className="actions">
                                        <button onClick={() => handleOpenModal('model', model)}>
                                            <i className="fa-solid fa-pen"></i>
                                        </button>
                                        <button onClick={() => handleDelete('model', model.id)}>
                                            <i className="fa-solid fa-trash"></i>
                                        </button>
                                    </div>
                                </div>
                            ))
                        )}
                    </div>
                </div>
            </div>

            {/* Modal */}
            {showModal && (
                <div className="popup-overlay" onClick={() => setShowModal(false)}>
                    <div className="popup" onClick={e => e.stopPropagation()}>
                        <h3>{editingItem ? 'Edit' : 'Add'} {modalType === 'make' ? 'Make' : 'Model'}</h3>
                        <form onSubmit={handleSave}>
                            <div className="form-section">
                                <label>Name</label>
                                <input
                                    type="text"
                                    value={itemName}
                                    onChange={e => setItemName(e.target.value)}
                                    required
                                    autoFocus
                                />
                            </div>
                            <div className="popup-actions">
                                <button type="button" className="btn secondary" onClick={() => setShowModal(false)}>Cancel</button>
                                <button type="submit" className="btn">Save</button>
                            </div>
                        </form>
                    </div>
                </div>
            )}
        </main>
    );
};

export default MakesAndModels;
