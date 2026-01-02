import React, { useState, useEffect, useRef } from 'react';
import { partApi } from '../../services/partApi';

const PartsTree = ({ onSelectPart, refreshTrigger, onRefresh }) => {
    const [rootFolders, setRootFolders] = useState([]);
    const [rootParts, setRootParts] = useState([]);
    const [loading, setLoading] = useState(false);

    // Initial fetch of root content
    useEffect(() => {
        fetchFolderContent(null);
    }, [refreshTrigger]);

    const fetchFolderContent = async (folderId) => {
        setLoading(true);
        try {
            const data = await partApi.getFolderContent(folderId);
            if (!folderId) {
                setRootFolders(data.subFolders);
                setRootParts(data.parts);
            }
            return data;
        } catch (error) {
            console.error("Error fetching parts", error);
            return { subFolders: [], parts: [] };
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="parts-tree">
            <div className="section-header-small">
                <button className="btn icon-btn" title="Add Root Folder" onClick={() => onRefresh()}>
                    <i className="fa-solid fa-sync"></i>
                </button>
                <button className="btn icon-btn" title="Add Root Folder" onClick={() => handleAddFolder(null, onRefresh)}>
                    <i className="fa-solid fa-folder-plus"></i>
                </button>
                <button className="btn icon-btn" title="Add Root Part" onClick={() => handleAddPart(null, onRefresh)}>
                    <i className="fa-solid fa-plus"></i> Part
                </button>
            </div>

            {rootFolders.map(folder => (
                <TreeNode
                    key={folder.id}
                    node={folder}
                    type="folder"
                    onSelectPart={onSelectPart}
                    fetchContent={fetchFolderContent}
                    onRefresh={onRefresh}
                    refreshTrigger={refreshTrigger}
                />
            ))}
            {rootParts.map(part => (
                <TreeNode
                    key={part.id}
                    node={part}
                    type="part"
                    onSelectPart={onSelectPart}
                    onRefresh={onRefresh}
                    refreshTrigger={refreshTrigger}
                />
            ))}
        </div>
    );
};

const TreeNode = ({ node, type, onSelectPart, fetchContent, onRefresh, refreshTrigger }) => {
    const [expanded, setExpanded] = useState(false);
    const [children, setChildren] = useState({ subFolders: [], parts: [] });
    const [loaded, setLoaded] = useState(false);

    // Context Menu State
    const [showMenu, setShowMenu] = useState(false);
    const [menuPos, setMenuPos] = useState({ x: 0, y: 0 });
    const menuRef = useRef(null);

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
            onSelectPart(node);
            return;
        }

        if (!expanded && !loaded) {
            // Lazy load
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
        handleAddPart(node.id, () => {
            fetchContent(node.id).then(data => {
                setChildren(data);
                setLoaded(true);
                setExpanded(true);
            });
        });
        setShowMenu(false);
    };

    return (
        <div className="tree-node-wrapper">
            <div
                className={`parts-tree-item ${type}`}
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
                    className="context-menu-btn"
                    onClick={(e) => { e.stopPropagation(); handleContextMenu(e); }}
                >
                    <i className="fa-solid fa-ellipsis-vertical"></i>
                </button>
            </div>

            {/* Context Menu */}
            {showMenu && (
                <div
                    className="context-menu"
                    style={{ top: menuPos.y, left: menuPos.x, position: 'fixed' }} // Fixed needed because of recursion
                    ref={menuRef}
                    onClick={e => e.stopPropagation()}
                >
                    {type === 'folder' && (
                        <>
                            <div className="context-menu-item" onClick={handleRename}>
                                <i className="fa-solid fa-pen"></i> Rename
                            </div>
                            <div className="context-menu-item" onClick={handleAddSubFolder}>
                                <i className="fa-solid fa-folder-plus"></i> Add Folder
                            </div>
                            <div className="context-menu-item" onClick={handleAddSubPart}>
                                <i className="fa-solid fa-plus"></i> Add Part
                            </div>
                        </>
                    )}
                    <div className="context-menu-item" onClick={handleDelete}>
                        <i className="fa-solid fa-trash"></i> Delete
                    </div>
                </div>
            )}

            {/* Children */}
            {expanded && type === 'folder' && (
                <div className="parts-tree-children">
                    {children.subFolders.map(child => (
                        <TreeNode
                            key={child.id}
                            node={child}
                            type="folder"
                            onSelectPart={onSelectPart}
                            fetchContent={fetchContent}
                            onRefresh={onRefresh} // This triggers full refresh, maybe suboptimal but safe
                            refreshTrigger={refreshTrigger}
                        />
                    ))}
                    {children.parts.map(child => (
                        <TreeNode
                            key={child.id}
                            node={child}
                            type="part"
                            onSelectPart={onSelectPart}
                            fetchContent={fetchContent}
                            onRefresh={onRefresh} // Recursive prop passing
                            refreshTrigger={refreshTrigger}
                        />
                    ))}
                    {children.subFolders.length === 0 && children.parts.length === 0 && <div style={{ paddingLeft: '1rem', fontStyle: 'italic', color: '#888' }}>Empty</div>}
                </div>
            )}
        </div>
    );
};

// Helpers for Add (should ideally be modals in parent, but using prompts for speed/simplicity initially or reusing modal logic? 
// The user requested "On the right of each folder list-item there will be a 3-dot button for a context menu"
// Just implementing the API calls here for now.
const handleAddFolder = async (parentId, onSuccess) => {
    const name = prompt("Enter folder name:");
    if (!name) return;
    try {
        await partApi.createFolder({ name, parentId });
        onSuccess();
    } catch (e) {
        console.error(e);
        alert("Failed to create folder");
    }
};

const handleAddPart = async (parentId, onSuccess) => {
    // This ideally opens the form on the right or a modal. 
    // Wait, "Clicking on a part will show its details on the right where you will be able to edit them."
    // So "Add Part" should probably create a dummy part or open a "New Part" form on the right? 
    // Let's create a dummy part with name "New Part" and let user edit it on the right.
    try {
        await partApi.createPart({
            name: "New Part",
            partNumber: "000",
            price: 0,
            quantity: 0,
            parentId
        });
        onSuccess();
    } catch (e) {
        console.error(e);
        alert("Failed to create part");
    }
};

export default PartsTree;
