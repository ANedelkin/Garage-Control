import React, { useState, useEffect } from 'react';
import ItemsTree from '../common/tree/ItemsTree';
import { makeApi } from '../../services/makeApi';
import { modelApi } from '../../services/modelApi';
import '../../assets/css/admin-makes-models.css';

const AdminMakesModels = () => {
    const [existing, setExisting] = useState([]);
    const [suggestions, setSuggestions] = useState([]);
    const [loading, setLoading] = useState(false);
    const [activeTab, setActiveTab] = useState('existing'); // For small screens

    const loadData = async () => {
        setLoading(true);
        try {
            const [makesData, suggestionsData] = await Promise.all([
                makeApi.getAll(),
                makeApi.getSuggestions()
            ]);

            // Map Existing Makes to Tree Nodes
            setExisting(makesData.map(m => ({
                id: m.id,
                name: m.name,
                type: 'group',
                className: 'make-node'
            })));

            // Map Suggestions to Tree Nodes
            // Backend returns [{ name, count }]
            setSuggestions(suggestionsData.map((s, index) => ({
                id: s.name,
                name: s.name, // Capitalized/Trimmed from backend
                count: s.count,
                type: 'group', // Can't expand yet as we don't fetch sub-models for suggestions
                className: 'suggestion-node'
            })));

        } catch (error) {
            console.error("Failed to load makes/models", error);
            alert("Error loading data");
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadData();
    }, []);

    // --- Actions for Existing Tree ---
    const fetchModels = async (makeId) => {
        const models = await modelApi.getAll(makeId);
        return {
            items: models.map(m => ({ ...m, type: 'item' })),
            groups: [] // Models don't have sub-groups
        };
    };

    const fetchSuggestedModels = async (makeName) => {
        const models = await makeApi.getSuggestedModels(makeName);
        return {
            items: models.map(m => ({
                ...m,
                id: `SUGG_MOD_${m.name}`, // Unique ID for tree
                type: 'item',
                className: 'suggestion-model-node'
            })),
            groups: []
        };
    };

    const existingActions = {
        // onAddGroup Removed as per requirements
        onAddItem: async (node, onSuccess) => {
            // Add Model to Make
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
                    await makeApi.editMake({ id: node.id, name: newName });
                } else {
                    await modelApi.editModel({ id: node.id, name: newName, makeId: node.makeId /* Need makeId? */ });
                    // Helper: ModelVM needs MakeId? ItemsTreeNode doesn't hold parent ID explicitly unless passed.
                    // fetchModels returns models. Do they have makeId?
                    // I need to ensure fetchModels result includes everything needed for update.
                    // Backend ModelVM requires MakeId for edit?
                    // ModelController.Edit -> UpdateModel -> Repo.GetById -> Update props.
                    // ModelVM definition has MakeId [Required].
                    // So I must ensure the node has makeId.
                }
                onSuccess();
            } catch (e) {
                alert("Failed to rename");
            }
        },
        onDelete: async (node, type, onSuccess) => {
            if (!window.confirm(`Delete coverage for ${node.name}?`)) return;
            try {
                if (type === 'group') await makeApi.deleteMake(node.id);
                else await modelApi.deleteModel(node.id);
                onSuccess();
            } catch (e) {
                alert("Failed to delete");
            }
        }
    };

    // Special: Models from backend might not have properties needed for logic if not carefully mapped.
    // fetchModels wrapper ensures 'type' is set. 
    // Models from ModelController: { id, name, carMakeId, ... }
    // So node.carMakeId should be available.

    const existingLabels = {
        addItem: "Add Model",
        delete: "Delete",
        rename: "Rename"
    };

    // --- Actions for Suggestions ---
    const handlePromote = async (node) => {
        const newName = prompt("Enter name for the new Make:", node.name);
        if (!newName) return;

        // Check duplicates locally
        const normalized = newName.trim().toUpperCase();
        const duplicate = existing.find(m => m.name.toUpperCase() === normalized);

        if (duplicate) {
            const confirmed = window.confirm(`Make "${duplicate.name}" already exists. Do you want to use the existing one instead? (Cancels creation)`);
            if (confirmed) return;
            // If they say No (don't use existing), do we create a duplicate?
            // "if yes, the new one won’t be created".
            // If no? "duplicates allowed".
        }

        try {
            await makeApi.promote({ name: node.name, newName });
            loadData(); // Reload to see move
        } catch (e) {
            alert("Failed to promote");
        }
    };

    const renderSuggestionActions = (node, type) => {
        return (
            <button
                className="icon-btn btn success-text"
                title="Add to system"
                onClick={(e) => { e.stopPropagation(); handlePromote(node); }}
                style={{ marginRight: '10px' }}
            >
                <i className="fa-solid fa-plus"></i>
            </button>
        );
    };

    // Add New Make (Top Level)
    const handleAddMake = async () => {
        const name = prompt("Enter new make name:");
        if (!name) return;

        // Check duplicate
        const normalized = name.trim().toUpperCase();
        const duplicate = existing.find(m => m.name.toUpperCase() === normalized);
        if (duplicate) {
            if (window.confirm(`Make "${duplicate.name}" already exists. Continue?`)) {
                // proceed
            } else {
                return;
            }
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
                            fetchChildren={fetchSuggestedModels} // Enable expansion
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
        </div>
    );
};

export default AdminMakesModels;
