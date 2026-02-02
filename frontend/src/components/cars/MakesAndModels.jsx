import React, { useState, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import '../../assets/css/makes-models.css';
import { makeApi } from '../../services/makeApi';
import { modelApi } from '../../services/modelApi';
import MergePopup from './MergePopup';

const MakesAndModels = () => {
    const [searchParams, setSearchParams] = useSearchParams();
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

    // Merge State
    const [showMergePopup, setShowMergePopup] = useState(false);
    const [mergeType, setMergeType] = useState('make');
    const [customItem, setCustomItem] = useState(null);
    const [globalItem, setGlobalItem] = useState(null);

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

    // Handle query params for merge from notification
    useEffect(() => {
        const handleMergeFromParams = async () => {
            const merge = searchParams.get('merge');
            const customId = searchParams.get('customId');
            const globalId = searchParams.get('globalId');

            if (merge && customId && globalId) {
                try {
                    if (merge === 'make') {
                        // Fetch both makes
                        const allMakes = await makeApi.getAll();
                        const custom = allMakes.find(m => m.id === customId);
                        const global = allMakes.find(m => m.id === globalId);

                        if (custom && global) {
                            setMergeType('make');
                            setCustomItem(custom);
                            setGlobalItem(global);
                            setShowMergePopup(true);
                        }
                    } else if (merge === 'model') {
                        // Need to find the make first
                        const allMakes = await makeApi.getAll();
                        for (const make of allMakes) {
                            const makeModels = await modelApi.getAll(make.id);
                            const custom = makeModels.find(m => m.id === customId);
                            const global = makeModels.find(m => m.id === globalId);

                            if (custom && global) {
                                setMergeType('model');
                                setCustomItem(custom);
                                setGlobalItem(global);
                                setShowMergePopup(true);
                                break;
                            }
                        }
                    }
                } catch (error) {
                    console.error('Error loading merge items:', error);
                }

                // Clear query params
                setSearchParams({});
            }
        };

        handleMergeFromParams();
    }, [searchParams, setSearchParams]);

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

    const handleOpenMerge = (type, custom, global) => {
        setMergeType(type);
        setCustomItem(custom);
        setGlobalItem(global);
        setShowMergePopup(true);
    };

    const handleMerge = async () => {
        try {
            if (mergeType === 'make') {
                await makeApi.mergeMakeWithGlobal(customItem.id, globalItem.id);
                if (selectedMake && selectedMake.id === customItem.id) setSelectedMake(null);
                fetchMakes();
            } else {
                await modelApi.mergeModelWithGlobal(customItem.id, globalItem.id);
                fetchModels(selectedMake.id);
            }
            setShowMergePopup(false);
            alert('Merge completed successfully!');
        } catch (error) {
            console.error('Error merging:', error);
            alert('Failed to merge. Check console.');
        }
    };

    // Helper to find matching global make/model
    const findGlobalMatch = (items, item) => {
        return items.find(i =>
            i.id !== item.id &&
            i.name.trim().toUpperCase() === item.name.trim().toUpperCase()
        );
    };


    return (
        <main className="main makes-models container">
            <div className="tile">
                <div className="horizontal grow">
                    {/* Makes Pane */}
                    <div className="form-left">
                        <div className="section-header">
                            <h3>Makes</h3>
                            <button className="btn" onClick={() => handleOpenModal('make')}>+ Add Make</button>
                        </div>
                        <div className="list-container grow">
                            {makes.map(make => (
                                <div
                                    key={make.id}
                                    className={`list-item ${selectedMake?.id === make.id ? 'active' : ''}`}
                                    onClick={() => setSelectedMake(make)}
                                >
                                    <span>{make.name}</span>
                                    <div>
                                        {make.globalId && (
                                            <button
                                                className="btn icon-btn merge"
                                                onClick={(e) => {
                                                    e.stopPropagation();
                                                    handleOpenMerge('make', make, { id: make.globalId, name: make.name });
                                                }}
                                                title="Merge with global"
                                            >
                                                <i className="fa-solid fa-code-merge"></i>
                                            </button>
                                        )}
                                        <button className="btn icon-btn" onClick={(e) => { e.stopPropagation(); handleOpenModal('make', make); }}>
                                            <i className="fa-solid fa-pen"></i>
                                        </button>
                                        <button className="btn icon-btn delete" onClick={(e) => { e.stopPropagation(); handleDelete('make', make.id); }}>
                                            <i className="fa-solid fa-trash"></i>
                                        </button>
                                    </div>
                                </div>
                            ))}
                        </div>
                    </div>

                    <div className="vertical-divider"></div>

                    {/* Models Pane */}
                    <div className="form-right">
                        <div className="section-header">
                            <h3>Models {selectedMake ? `for ${selectedMake.name}` : ''}</h3>
                            {selectedMake && (
                                <button className="btn" onClick={() => handleOpenModal('model')}>+ Add Model</button>
                            )}
                        </div>
                        <div className="list-container grow">
                            {!selectedMake ? (
                                <p className="list-empty">Select a make to view models</p>
                            ) : loadingModels ? (
                                <p className="list-empty">Loading...</p>
                            ) : (
                                models.map(model => (
                                    <div key={model.id} className="list-item">
                                        <span>{model.name}</span>
                                        <div>
                                            {model.globalId && (
                                                <button
                                                    className="btn icon-btn merge"
                                                    onClick={() => handleOpenMerge('model', model, { id: model.globalId, name: model.name })}
                                                    title="Merge with global"
                                                >
                                                    <i className="fa-solid fa-code-merge"></i>
                                                </button>
                                            )}
                                            <button className="btn icon-btn" onClick={() => handleOpenModal('model', model)}>
                                                <i className="fa-solid fa-pen"></i>
                                            </button>
                                            <button className="btn icon-btn delete" onClick={() => handleDelete('model', model.id)}>
                                                <i className="fa-solid fa-trash"></i>
                                            </button>
                                        </div>
                                    </div>
                                ))
                            )}
                        </div>
                    </div>
                </div>
            </div>

            {/* Modal */}
            {showModal && (
                <div className="popup-overlay" onClick={() => setShowModal(false)}>
                    <div className="popup tile" onClick={e => e.stopPropagation()}>
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
                            <div className="form-footer">
                                <button type="submit" className="btn">Save</button>
                                <button type="button" className="btn" onClick={() => setShowModal(false)}>Cancel</button>
                            </div>
                        </form>
                    </div>
                </div>
            )}

            {/* Merge Popup */}
            <MergePopup
                isOpen={showMergePopup}
                onClose={() => setShowMergePopup(false)}
                mergeType={mergeType}
                customItem={customItem}
                globalItem={globalItem}
                onMerge={handleMerge}
            />
        </main>
    );
};

export default MakesAndModels;
