import React from 'react';
import ItemsTree from '../common/tree/ItemsTree';
import { partApi } from '../../services/partApi';
import { handleAddFolder, handleAddPart } from './helpers';

// Custom label renderer for parts tree with deficit status visualization
const PartItemLabel = ({ node, type, expanded }) => {
    // Calculate deficit status class based on node data
    const getDeficitStatusClass = () => {
        if (type === 'group') {
            // For folders: check aggregate deficit counts
            const higherDeficitCount = node.higherDeficitSeverityCount || 0;
            const lowerDeficitCount = node.lowerDeficitSeverityCount || 0;

            if (higherDeficitCount > 0) {
                return 'status-higher-deficit'; // Red
            } else if (lowerDeficitCount > 0) {
                return 'status-lower-deficit'; // Yellow
            }
            return '';
        } else {
            // For parts: check individual deficit status
            // DeficitStatus enum: 0=NoDeficit, 1=LowerSeverity, 2=HigherSeverity
            if (node.deficitStatus === 2) {
                return 'status-higher-deficit'; // Red
            } else if (node.deficitStatus === 1) {
                return 'status-lower-deficit'; // Yellow
            }
            return '';
        }
    };

    const statusClass = getDeficitStatusClass();

    // Get the appropriate icon
    const getIcon = () => {
        if (type === 'group') {
            return <i className={`fa-solid ${expanded ? 'fa-folder-open' : 'fa-folder'}`}></i>;
        }
        return <i className="fa-solid fa-gear"></i>;
    };

    return (
        <div className={`item-label ${statusClass}`}>
            {getIcon()}
            <span>{node.name} {node.count !== undefined && <span className='text-muted'>({node.count})</span>}</span>
        </div>
    );
};

const PartsTree = ({ folders, parts, onSelectPart, fetchContent, onRefresh, refreshTrigger, selectedPartId, selectedPath = [], currentPath = [] }) => {

    // Define Actions for the Parts Tree
    const actions = {
        onRename: async (node, type, onSuccess) => {
            const newName = prompt("Enter new name:", node.name);
            if (newName) {
                try {
                    if (type === 'group') {
                        await partApi.renameFolder(node.id, newName);
                    } else {
                        await partApi.renamePart(node.id, newName);
                    }
                    onSuccess();
                } catch (error) {
                    alert("Failed to rename");
                }
            }
        },
        onDelete: async (node, type, onSuccess) => {
            if (!window.confirm(`Delete ${type === 'group' ? 'folder' : 'part'} ${node.name}?`)) return;
            try {
                if (type === 'group') {
                    await partApi.deleteFolder(node.id);
                } else {
                    await partApi.deletePart(node.id);
                }
                onSuccess();
            } catch (error) {
                alert("Failed to delete");
            }
        },
        onAddGroup: async (node, onSuccess) => {
            handleAddFolder(node.id, onSuccess);
        },
        onAddItem: async (node, onSuccess) => {
            const newPart = await handleAddPart(node.id, onSuccess);
            return newPart;
        },
        onMoveItem: async (draggedItem, targetFolder, onSuccess) => {
            // draggedItem: { id, type }
            // targetFolder: node (the folder we dropped onto)
            try {
                if (draggedItem.type === 'item') {
                    await partApi.movePart(draggedItem.id, targetFolder.id);
                } else if (draggedItem.type === 'group') {
                    await partApi.moveFolder(draggedItem.id, targetFolder.id);
                }
                onSuccess(); // Refreshes the target folder
                onRefresh(); // Refreshes the whole tree (or source folder ideally, but full refresh is safer for now)
            } catch (error) {
                alert("Failed to move item");
                console.error(error);
            }
        }
    };

    const labels = {
        addGroup: "Add Folder",
        addItem: "Add Part"
    };

    // Custom Icon Renderer (Optional, to match exact previous style if needed, but defaults are close)
    // Previous: Folder -> fa-folder/fa-folder-open, Part -> fa-gear. 
    // Defaults in ItemsTreeNode match this.

    return (
        <div
            style={{ minHeight: '100px', height: '100%', display: 'flex', flexDirection: 'column', flex: 1 }}
            onDragOver={(e) => { e.preventDefault(); }}
            onDrop={(e) => {
                e.preventDefault();
                try {
                    const data = JSON.parse(e.dataTransfer.getData("application/json"));
                    // Dropped on empty space -> Move to root
                    actions.onMoveItem(data, { id: null }, () => { });
                } catch (err) { console.error(err); }
            }}
        >
            <ItemsTree
                groups={folders}
                items={parts}
                onSelectItem={onSelectPart}
                fetchChildren={fetchContent}
                onRefresh={onRefresh}
                refreshTrigger={refreshTrigger}
                selectedItemId={selectedPartId}
                selectedPath={selectedPath}
                currentPath={currentPath}
                actions={actions}
                labels={labels}
                renderItemLabel={(node, type, expanded) => <PartItemLabel node={node} type={type} expanded={expanded} />}
                allowDrag={true}
                parentId={null}
            />
        </div>
    );
};

export default PartsTree;