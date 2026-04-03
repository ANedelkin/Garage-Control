import React, { useState, useEffect, useRef } from 'react';
import { useSearchParams, useParams, useNavigate, useLocation } from 'react-router-dom';
import '../../assets/css/makes-models.css';
import { makeApi } from '../../services/makeApi';
import { modelApi } from '../../services/modelApi';
import { usePopup } from '../../context/PopupContext';
import MergePopup from './MergePopup';
import AddEditItemModal from './AddEditItemModal';
import { parseValidationErrors } from '../../Utilities/formErrors.js';
import usePageTitle from '../../hooks/usePageTitle.js';

const MakesAndModels = () => {
    usePageTitle('Makes & Models');
    const { addPopup, removeLastPopup } = usePopup();
    const navigate = useNavigate();
    const { makeId, modelId } = useParams();
    const [searchParams, setSearchParams] = useSearchParams();
    const [makes, setMakes] = useState([]);
    const [models, setModels] = useState([]);
    const [selectedMake, setSelectedMake] = useState(null);
    const [loadingMakes, setLoadingMakes] = useState(true);
    const [loadingModels, setLoadingModels] = useState(false);
    
    const makeRefs = useRef({});
    const modelRefs = useRef({});
    const location = useLocation();
    const highlight = searchParams.get('highlight') === 'true';

    // Modal state (only for controlled variables, actual modal is in PopupContext)
    const [modalType, setModalType] = useState('make'); // 'make' or 'model'
    const [editingItem, setEditingItem] = useState(null); // null for create, object for edit
    const [itemName, setItemName] = useState('');
    const [errors, setErrors] = useState({});

    useEffect(() => {
        fetchMakes();
    }, []);

    useEffect(() => {
        if (selectedMake) {
            fetchModels(selectedMake.id);
            // Don't auto-navigate to /makes-and-models on make selection unless we intentionally want it in the URL
            // Actually the requirement is mainly linking the popup.
        } else {
            setModels([]);
        }
    }, [selectedMake]);

    useEffect(() => {
        if (makes.length > 0 && makeId) {
            const make = makes.find(m => m.id === makeId);
            if (make && (!selectedMake || selectedMake.id !== makeId)) {
                setSelectedMake(make);
            }
            if (makeRefs.current[makeId]) {
                makeRefs.current[makeId].scrollIntoView({ behavior: 'smooth', block: 'center' });
            }
        }
    }, [makeId, makes, highlight, modelId]);

    useEffect(() => {
        if (selectedMake && selectedMake.id === makeId && modelId && models.length > 0) {
            const model = models.find(m => m.id === modelId);
            // Instead of opening a modal, we just scroll to it
            if (modelRefs.current[modelId]) {
                modelRefs.current[modelId].scrollIntoView({ behavior: 'smooth', block: 'center' });
            }
            if (!highlight && model && (!editingItem || editingItem.id !== modelId)) {
                handleOpenModal('model', model);
            }
        }
    }, [modelId, models, selectedMake, makeId, highlight]);

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
                            handleOpenMerge('make', custom, global);
                        }
                    } else if (merge === 'model') {
                        // Need to find the make first
                        const allMakes = await makeApi.getAll();
                        for (const make of allMakes) {
                            const makeModels = await modelApi.getAll(make.id);
                            const custom = makeModels.find(m => m.id === customId);
                            const global = makeModels.find(m => m.id === globalId);

                            if (custom && global) {
                                handleOpenMerge('model', custom, global);
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

        addPopup(
            `${item ? 'Edit' : 'Add'} ${type === 'make' ? 'Make' : 'Model'}`,
            <AddEditItemModal
                itemType={type}
                currentName={item ? item.name : ''}
                onClose={() => { removeLastPopup(); navigate('/makes-and-models'); }}
                onConfirm={(name) => handleSaveModal(type, item, name)}
                errors={errors}
            />,
            false,
            () => navigate('/makes-and-models')
        );
    };

    const handleSaveModal = async (type, editingItem, itemName) => {
        try {
            if (type === 'make') {
                if (editingItem) {
                    await makeApi.editMake(editingItem.id, { name: itemName });
                } else {
                    await makeApi.createMake({ name: itemName });
                }
                fetchMakes();
            } else {
                if (editingItem) {
                    await modelApi.editModel(editingItem.id, { name: itemName, makeId: selectedMake.id });
                } else {
                    await modelApi.createModel({ name: itemName, makeId: selectedMake.id });
                }
                fetchModels(selectedMake.id);
            }
            removeLastPopup();
            navigate('/makes-and-models');
            setErrors({});
        } catch (error) {
            console.error("Error saving item", error);
            setErrors(parseValidationErrors(error));
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
        const handleMergeAction = async () => {
            try {
                if (type === 'make') {
                    await makeApi.mergeMakeWithGlobal(custom.id, global.id);
                    if (selectedMake && selectedMake.id === custom.id) setSelectedMake(null);
                    fetchMakes();
                } else {
                    await modelApi.mergeModelWithGlobal(custom.id, global.id);
                    fetchModels(selectedMake.id);
                }
                removeLastPopup();
                alert('Merge completed successfully!');
            } catch (error) {
                console.error('Error merging:', error);
                alert('Failed to merge. Check console.');
            }
        };

        addPopup(
            `Merge ${type === 'make' ? 'Make' : 'Model'}`,
            <MergePopup
                mergeType={type}
                customItem={custom}
                globalItem={global}
                onMerge={handleMergeAction}
                onClose={removeLastPopup}
            />
        );
    };

    // Helper to find matching global make/model
    const findGlobalMatch = (items, item) => {
        return items.find(i =>
            i.id !== item.id &&
            i.name.trim().toUpperCase() === item.name.trim().toUpperCase()
        );
    };


    const handleContainerClick = () => {
        if (makeId || modelId) {
            navigate('/makes-and-models', { replace: true });
        }
        setSelectedMake(null);
    };

    return (
        <main className="main makes-models" onClick={handleContainerClick}>
            <div className={`tile ${selectedMake ? 'mobile-show-models' : 'mobile-show-makes'}`} onClick={(e) => e.stopPropagation()}>
                <div className="horizontal grow align-stretch">
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
                                    ref={el => makeRefs.current[make.id] = el}
                                    className={`list-item ${selectedMake?.id === make.id ? 'active' : ''}`}
                                    onClick={(e) => { e.stopPropagation(); setSelectedMake(make); navigate(`/makes-and-models/${make.id}`); }}
                                >
                                    <span className="item-label">{make.name}</span>
                                    <div>
                                        {make.globalId && (
                                            <button
                                                className="btn icon-btn"
                                                onClick={(e) => {
                                                    e.stopPropagation();
                                                    const globalMake = makes.find(m => m.id === make.globalId);
                                                    if (globalMake) {
                                                        handleOpenMerge('make', make, globalMake);
                                                    }
                                                }}
                                                title="Merge with global"
                                            >
                                                <i className="fa-solid fa-arrows-to-circle"></i>
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
                            <div className="horizontal align-center gap-10">
                                <button className="btn icon-btn mobile-only" onClick={() => setSelectedMake(null)} title="Back to Makes">
                                    <i className="fa-solid fa-arrow-left"></i>
                                </button>
                                <h3>Models {selectedMake ? `for ${selectedMake.name}` : ''}</h3>
                            </div>
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
                                    <div 
                                        key={model.id} 
                                        ref={el => modelRefs.current[model.id] = el}
                                        className="list-item"
                                        onClick={(e) => { e.stopPropagation(); navigate(`/makes-and-models/${selectedMake.id}/model/${model.id}`); }}
                                    >
                                        <span className="item-label">{model.name}</span>
                                        <div>
                                            {model.globalId && (
                                                <button
                                                    className="btn icon-btn"
                                                    onClick={async () => {
                                                        try {
                                                            const globalModel = await modelApi.getModel(model.globalId);
                                                            if (globalModel) {
                                                                 handleOpenMerge('model', model, globalModel);
                                                            }
                                                        } catch (error) {
                                                            console.error('Error fetching global model:', error);
                                                            alert('Failed to load global model');
                                                        }
                                                    }}
                                                    title="Merge with global"
                                                >
                                                    <i className="fa-solid fa-arrows-to-circle"></i>
                                                </button>
                                            )}
                                            <button className="btn icon-btn" onClick={(e) => { e.stopPropagation(); handleOpenModal('model', model); }}>
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
        </main>
    );
};

export default MakesAndModels;
