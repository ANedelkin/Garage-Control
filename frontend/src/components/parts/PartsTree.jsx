import React from 'react';
import ItemsTree from '../common/tree/ItemsTree';
import { partApi } from '../../services/partApi';
import { handleAddFolder, handleAddPart } from './helpers';

const PartsTree = ({ folders, parts, onSelectPart, fetchContent, onRefresh, refreshTrigger, selectedPartId, selectedPath = [], currentPath = [] }) => {

    // Define Actions for the Parts Tree
    const actions = {
        onRename: async (node, type, onSuccess) => {
            const newName = prompt("Enter new name:", node.name);
            if (newName) {
                try {
                    await partApi.renameFolder(node.id, newName);
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
        />
    );
};

export default PartsTree;