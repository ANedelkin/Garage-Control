import React, { useState, useEffect, useRef } from 'react';
import { partApi } from '../../services/partApi';
import ContextMenu from './ContextMenu';
import PartsTree from './PartsTree';
import { handleAddFolder, handleAddPart } from './helpers';

const PartsTreeNode = ({ node, type, onSelectPart, fetchContent, onRefresh, refreshTrigger, selectedPartId, selectedPath = [], currentPath = [] }) => {
    const [expanded, setExpanded] = useState(false);
    const [children, setChildren] = useState({ subFolders: [], parts: [] });
    const [loaded, setLoaded] = useState(false);

    const [showMenu, setShowMenu] = useState(false);
    const [menuPos, setMenuPos] = useState({ x: 0, y: 0 });
    const menuRef = useRef(null);

    useEffect(() => {
        if (type === 'folder' && selectedPath.includes(node.id) && !expanded) {
            handleExpand({ stopPropagation: () => { } });
        }
    }, [selectedPath]);

    useEffect(() => {
        const handleClickOutside = (event) => {
            if (menuRef.current && !menuRef.current.contains(event.target)) {
                setShowMenu(false);
            }
        };
        document.addEventListener('mousedown', handleClickOutside);
        return () => document.removeEventListener('mousedown', handleClickOutside);
    }, []);

    // Listen to global refresh if expanded
    useEffect(() => {
        if (expanded && type === 'folder') {
            fetchContent(node.id).then(data => setChildren(data));
        }
    }, [refreshTrigger]);

    const handleExpand = async (e) => {
        e.stopPropagation();
        if (type === 'part') {
            onSelectPart(node, currentPath);
            return;
        }

        if (!expanded && !loaded) {
            const data = await fetchContent(node.id);
            setChildren(data);
            setLoaded(true);
        }
        setExpanded(!expanded);
    };

    const handleContextMenu = (e) => {
        e.preventDefault();
        e.stopPropagation();
        setMenuPos({ x: e.pageX, y: e.pageY });
        setShowMenu(true);
    };

    // Actions
    const handleRename = async () => {
        const newName = prompt("Enter new name:", node.name);
        if (newName) {
            try {
                await partApi.renameFolder(node.id, newName);
                onRefresh();
            } catch (error) {
                alert("Failed to rename");
            }
        }
        setShowMenu(false);
    };

    const handleDelete = async () => {
        if (!window.confirm(`Delete ${type} ${node.name}?`)) return;
        try {
            if (type === 'folder') {
                await partApi.deleteFolder(node.id);
            } else {
                await partApi.deletePart(node.id);
            }
            onRefresh();
        } catch (error) {
            alert("Failed to delete");
        }
        setShowMenu(false);
    };

    const handleAddSubFolder = async () => {
        handleAddFolder(node.id, () => {
            // Force reload children
            setLoaded(false);
            setExpanded(true);
            // Verify if we can just re-fetch this node's content?
            // Simplest is to trigger global refresh or specifically refresh this node
            // Passed from parent is global refresh, which is heavy. 
            // Better: reload this node specific
            fetchContent(node.id).then(data => {
                setChildren(data);
                setLoaded(true);
                setExpanded(true);
            });
        });
        setShowMenu(false);
    };

    const handleAddSubPart = async () => {
        const newPart = await handleAddPart(node.id, () => {
            fetchContent(node.id).then(data => {
                setChildren(data);
                setLoaded(true);
                setExpanded(true);
            });
        });
        if (newPart) {
            onSelectPart(newPart, [...currentPath, newPart.id]);
        }
        setShowMenu(false);
    };

    return (
        <>
            <div
                className={`list-item ${((type === 'part' && node.id === selectedPartId) || (type === 'folder' && selectedPath.includes(node.id) && !expanded)) ? 'active' : ''} ${type === 'part' && node.quantity === 0 ? 'out-of-stock' : (type === 'part' && node.quantity < node.minimumQuantity ? 'low-stock' : '')}`}
                onClick={handleExpand}
                onContextMenu={handleContextMenu} // Right click on item itself? Request says "On the right of each folder list-item there will be a 3-dot button"
            >
                <div className="item-label">
                    {type === 'folder' && (
                        <i className={`fa-solid ${expanded ? 'fa-folder-open' : 'fa-folder'}`}></i>
                    )}
                    {type === 'part' && <i className="fa-solid fa-gear"></i>}
                    <span>{node.name}</span>
                </div>

                {/* 3-dot menu button */}
                <button
                    className="icon-btn btn"
                    onClick={(e) => { e.stopPropagation(); handleContextMenu(e); }}
                >
                    <i className="fa-solid fa-ellipsis-vertical"></i>
                </button>
            </div>

            {/* Children */}
            {expanded && type === 'folder' && (
                <div className="parts-tree-children">
                    <PartsTree
                        folders={children.subFolders}
                        parts={children.parts}
                        onSelectPart={onSelectPart}
                        fetchContent={fetchContent}
                        onRefresh={onRefresh}
                        refreshTrigger={refreshTrigger}
                        selectedPartId={selectedPartId}
                        selectedPath={selectedPath}
                        currentPath={currentPath}
                    />
                </div>
            )}

            {/* Context Menu */}
            {showMenu && (
                <ContextMenu
                    handleRename={handleRename}
                    handleAddSubFolder={handleAddSubFolder}
                    handleAddSubPart={handleAddSubPart}
                    handleDelete={handleDelete}
                    type={type}
                    menuPos={menuPos}
                    menuRef={menuRef}
                />
            )}
        </>
    );
};


export default PartsTreeNode;