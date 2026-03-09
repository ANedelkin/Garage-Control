import React, { useState, useEffect } from 'react';
import ItemsTree from '../common/tree/ItemsTree';
import { makeApi } from '../../services/makeApi';
import { modelApi } from '../../services/modelApi';
import SuggestedModelPopup from './SuggestedModelPopup';
import '../../assets/css/admin-makes-models.css';
import '../../assets/css/popup.css';
import { parseValidationErrors } from '../../Utilities/formErrors.js';

const AdminMakesModels = () => {
    const [existing, setExisting] = useState([]);
    const [suggestions, setSuggestions] = useState([]);
    const [popupNode, setPopupNode] = useState(null);
    const [errors, setErrors] = useState({});

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

            setSuggestions(suggestionsData.map((s, index) => ({
                id: s.name,
                name: s.name,
                count: s.count,
                isExisting: s.isExisting,
                type: 'group',
                className: s.isExisting ? 'suggestion-node existing' : 'suggestion-node'
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
                id: `SUGG_MOD_${m.name}`, //TODO: names are already unique, no need  for prefix
                type: 'item',
                makeName: makeName,
                className: 'suggestion-model-node'
            })),
            groups: []
        };
    };

    const existingActions = {
        onAddItem: async (node, onSuccess) => {
            const name = prompt("Enter model name:");
            if (!name) return;
            try {
                await modelApi.createModel({ name, makeId: node.id });
                onSuccess();
            } catch (e) {
                alert("Failed to create model");
            }
        },
        onRename: async (node, type, onSuccess) => {
            const newName = prompt("Enter new name:", node.name);
            if (!newName) return;
            try {
                if (type === 'group') {
                    await makeApi.editMake(node.id, { name: newName });
                } else {
                    await modelApi.editModel(node.id, { name: newName, makeId: node.carMakeId });
                }
                onSuccess();
            } catch (e) {
                alert("Failed to rename");
            }
        },
        onDelete: async (node, type, onSuccess) => {
            if (!window.confirm(`Delete coverage for ${node.name}?`)) return;
            try {
                if (type === 'group') {
                    await makeApi.deleteMake(node.id);
                } else {
                    await modelApi.deleteModel(node.id);
                }
                onSuccess();
            } catch (e) {
                alert("Failed to delete");
            }
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

        setPopupNode({
            ...node,
            makeName: node.makeName,
            isMakeExisting: !!parentMake
        });
    };

    const handlePromote = async (node) => {
        const newName = prompt("Enter name for the new Make:", node.name);
        if (!newName) return;

        const normalized = newName.trim().toUpperCase();
        const duplicate = existing.find(m => m.name.toUpperCase() === normalized);

        if (duplicate) {
            const confirmed = window.confirm(`Make "${duplicate.name}" already exists. Do you want to use the existing one instead? (Cancels creation)`);
            if (confirmed) return;
        }

        try {
            await makeApi.promote({ name: node.name, newName });
            loadData();
        } catch (e) {
            alert("Failed to promote");
        }
    };

    const handleConfirmModelAdd = async (makeName, modelName) => {
        try {
            await makeApi.promoteModel({
                makeName: popupNode.makeName,
                newMakeName: makeName !== popupNode.makeName ? makeName : null,
                modelName: popupNode.name,
                newModelName: modelName !== popupNode.name ? modelName : null
            });

            setPopupNode(null);
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
                className="icon-btn btn success-text"
                title="Add to system"
                onClick={(e) => { e.stopPropagation(); handleOpenPopup(node); }}
            >
                <i className="fa-solid fa-plus"></i>
            </button>
        );
    };

    const handleAddMake = async () => {
        const name = prompt("Enter new make name:");
        if (!name) return;

        const normalized = name.trim().toUpperCase();
        const duplicate = existing.find(m => m.name.toUpperCase() === normalized);
        if (duplicate) {
            if (window.confirm(`Make "${duplicate.name}" already exists. Continue?`)) return;
        }

        try {
            await makeApi.createMake({ name });
            loadData();
        } catch (e) {
            alert("Failed to create make");
        }
    };

    return (
        <div className="main admin-makes-models container">

            <div className="tile">
                <div className="horizontal grow"></div>
                <div className={`form-left`}>
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
                        />
                        {suggestions.length === 0 && <div className="list-empty">No suggestions</div>}
                    </div>
                </div>

                <div className="vertical-divider"></div>

                <div className={`form-right`}>
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
                        />
                    </div>
                </div>
            </div>

            {popupNode && (
                <SuggestedModelPopup
                    node={popupNode}
                    onClose={() => setPopupNode(null)}
                    onConfirm={handleConfirmModelAdd}
                    errors={errors}
                />
            )}
        </div>
    );
};

export default AdminMakesModels;
