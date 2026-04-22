import React, { useState, useEffect } from 'react';
import ItemsTree from '../common/tree/ItemsTree';
import { makeApi } from '../../services/makeApi';
import { modelApi } from '../../services/modelApi';
import SuggestedModelPopup from './SuggestedModelPopup';
import PromoteMakePopup from './PromoteMakePopup';
import SimpleInputPopup from './SimpleInputPopup';
import RenamePopup from './RenamePopup';
import ConfirmationPopup from '../common/ConfirmationPopup';
import { usePopup } from '../../context/PopupContext';
import '../../assets/css/admin-makes-models.css';
import '../../assets/css/popup.css';
import { parseValidationErrors } from '../../Utilities/formErrors.js';
import usePageTitle from '../../hooks/usePageTitle.js';

const AdminMakesModels = () => {
    usePageTitle('Admin Makes & Models');
    const [existing, setExisting] = useState([]);
    const [suggestions, setSuggestions] = useState([]);
    const [errors, setErrors] = useState({});
    const [activeTab, setActiveTab] = useState('existing');

    const { addPopup, removeLastPopup } = usePopup();

    const loadData = async () => {
        try {
            const [makesData, suggestionsData] = await Promise.all([
                makeApi.getAll(),
                makeApi.getSuggestions()
            ]);

            setExisting(makesData.map(m => ({
                id: m.id,
                name: m.name,
                type: 'group',
                className: 'make-node'
            })));

            setSuggestions(suggestionsData.map((s) => ({
                id: s.name,
                name: s.name,
                count: s.count,
                isExisting: s.isExisting,
                type: 'group'
            })));

        } catch (error) {
            console.error("Failed to load makes/models", error);
            alert("Error loading data");
        }
    };

    useEffect(() => {
        loadData();
    }, []);

    const fetchModels = async (makeId) => {
        const models = await modelApi.getAll(makeId);
        return {
            items: models.map(m => ({ ...m, type: 'item' })),
            groups: []
        };
    };

    const fetchSuggestedModels = async (makeName) => {
        const models = await makeApi.getSuggestedModels(makeName);
        return {
            items: models.map(m => ({
                ...m,
                id: m.name,
                type: 'item',
                makeName: makeName,
                className: 'suggestion-model-node'
            })),
            groups: []
        };
    };

    const existingActions = {
        onAddItem: async (node, onSuccess) => {
            addPopup('Add Model', (
                <SimpleInputPopup 
                    label="Model Name" 
                    onConfirm={async (name) => {
                        try {
                            await modelApi.createModel({ name, makeId: node.id });
                            removeLastPopup();
                            onSuccess();
                        } catch (e) {
                            alert("Failed to create model");
                        }
                    }}
                    onClose={removeLastPopup}
                />
            ));
        },
        onRename: async (node, type, onSuccess) => {
            addPopup('Rename', (
                <RenamePopup 
                    node={node}
                    onConfirm={async (newName) => {
                        try {
                            if (type === 'group') {
                                await makeApi.editMake(node.id, { name: newName });
                            } else {
                                await modelApi.editModel(node.id, { name: newName, makeId: node.carMakeId });
                            }
                            removeLastPopup();
                            onSuccess();
                        } catch (e) {
                            alert("Failed to rename");
                        }
                    }}
                    onClose={removeLastPopup}
                />
            ));
        },
        onDelete: async (node, type, onSuccess) => {
            addPopup(
                'Delete Coverage',
                <ConfirmationPopup 
                    message={`Are you sure you want to delete coverage for ${node.name}?`}
                    confirmText="Delete"
                    isDanger={true}
                    onConfirm={async () => {
                        try {
                            if (type === 'group') {
                                await makeApi.deleteMake(node.id);
                            } else {
                                await modelApi.deleteModel(node.id);
                            }
                            removeLastPopup();
                            onSuccess();
                        } catch (e) {
                            alert("Failed to delete");
                        }
                    }}
                    onClose={removeLastPopup}
                />
            );
        }
    };

    const existingLabels = {
        addItem: "Add Model",
        delete: "Delete",
        rename: "Rename"
    };

    const handleOpenPopup = (node) => {
        if (node.type === 'group') {
            if (!node.isExisting) handlePromote(node);
            return;
        }

        const parentMake = existing.find(m => m.name.toUpperCase() === node.makeName?.toUpperCase());

        addPopup('Add Suggested Model', 
            <SuggestedModelPopup
                node={{ ...node, isMakeExisting: !!parentMake }}
                onClose={removeLastPopup}
                onConfirm={(makeName, modelName) => handleConfirmModelAdd(node, makeName, modelName)}
                errors={errors}
            />
        );
    };

    const handlePromote = (node) => {
        addPopup('Promote Make', <PromoteMakePopup node={node} onClose={removeLastPopup} onConfirm={(newName) => handlePromoteConfirm(node, newName)} />);
    };

    const handlePromoteConfirm = async (node, newName) => {
        const normalized = newName.trim().toUpperCase();
        const duplicate = existing.find(m => m.name.toUpperCase() === normalized);

        if (duplicate) {
            addPopup(
                'Duplicate Make',
                <ConfirmationPopup 
                    message={`Make "${duplicate.name}" already exists. Do you want to use the existing one instead? (Cancels creation)`}
                    confirmText="Use Existing"
                    onConfirm={() => {
                        removeLastPopup(); // Close duplicate popup
                        removeLastPopup(); // Close promotion popup
                    }}
                    onClose={removeLastPopup}
                />
            );
            return;
        }

        try {
            await makeApi.promote({ name: node.name, newName });
            removeLastPopup();
            loadData();
        } catch (e) {
            alert("Failed to promote");
        }
    };

    const handleConfirmModelAdd = async (originalNode, makeName, modelName) => {
        try {
            await makeApi.promoteModel({
                makeName: originalNode.makeName,
                newMakeName: makeName !== originalNode.makeName ? makeName : null,
                modelName: originalNode.name,
                newModelName: modelName !== originalNode.name ? modelName : null
            });

            removeLastPopup();
            setErrors({});
            loadData();
        } catch (e) {
            console.error("Error promoting model", e);
            setErrors(parseValidationErrors(e));
        }
    };

    const renderSuggestionActions = (node, type) => {
        if (type === 'group' && node.isExisting) return null;

        return (
            <button
                className="icon-btn btn"
                title="Add to system"
                onClick={(e) => { e.stopPropagation(); handleOpenPopup(node); }}
            >
                <i className="fa-solid fa-plus"></i>
            </button>
        );
    };

    const handleAddMake = () => {
        addPopup('Add Make', (
            <SimpleInputPopup 
                label="Make Name"
                onConfirm={async (name) => {
                    const normalized = name.trim().toUpperCase();
                    const duplicate = existing.find(m => m.name.toUpperCase() === normalized);
                    if (duplicate) {
                        addPopup(
                            'Make Already Exists',
                            <ConfirmationPopup 
                                message={`Make "${duplicate.name}" already exists. Continue anyway?`}
                                confirmText="Continue"
                                onConfirm={async () => {
                                    removeLastPopup(); // Close duplicate warning
                                    try {
                                        await makeApi.createMake({ name });
                                        removeLastPopup(); // Close Add Make popup
                                        loadData();
                                    } catch (e) {
                                        alert("Failed to create make");
                                    }
                                }}
                                onClose={removeLastPopup}
                            />
                        );
                        return;
                    }

                    try {
                        await makeApi.createMake({ name });
                        removeLastPopup();
                        loadData();
                    } catch (e) {
                        alert("Failed to create make");
                    }
                }}
                onClose={removeLastPopup}
            />
        ));
    };

    return (
        <div className="main admin-makes-models">
            <div className="header">
                <h1>Makes & Models</h1>
            </div>

            <div className="popup-tabs mobile-only">
                <button
                    type="button"
                    className={`tab-btn ${activeTab === 'suggested' ? 'active' : ''}`}
                    onClick={() => setActiveTab('suggested')}
                >
                    <i className="fa-solid fa-lightbulb"></i> Suggested
                </button>
                <button
                    type="button"
                    className={`tab-btn ${activeTab === 'existing' ? 'active' : ''}`}
                    onClick={() => setActiveTab('existing')}
                >
                    <i className="fa-solid fa-list-check"></i> Existing
                </button>
            </div>

            <div className={`tile ${activeTab === 'suggested' ? 'mobile-show-suggested' : 'mobile-show-existing'}`}>
                <div className="horizontal grow">
                    <div className="form-left">
                        <div className="section-header">
                            <h3>Suggested</h3>
                            <button className="btn icon-btn" onClick={loadData} title="Refresh">
                                <i className="fa-solid fa-rotate-right"></i>
                            </button>
                        </div>
                        <div className="list-container grow">
                            <ItemsTree
                                groups={suggestions}
                                fetchChildren={fetchSuggestedModels}
                                actions={{}}
                                renderActions={renderSuggestionActions}
                                onRefresh={loadData}
                            />
                            {suggestions.length === 0 && <div className="list-empty">No suggestions</div>}
                        </div>
                    </div>

                    <div className="vertical-divider"></div>

                    <div className="form-right">
                        <div className="section-header">
                            <h3>Existing</h3>
                            <button className="btn" onClick={handleAddMake}>+ Add Make</button>
                        </div>
                        <div className="list-container grow">
                            <ItemsTree
                                groups={existing}
                                fetchChildren={fetchModels}
                                actions={existingActions}
                                labels={existingLabels}
                                onRefresh={loadData}
                            />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default AdminMakesModels;
