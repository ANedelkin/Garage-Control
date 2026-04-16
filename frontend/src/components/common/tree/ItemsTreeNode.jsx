import React, { useState, useEffect, useRef } from 'react';
import TreeContextMenu from './TreeContextMenu';
import ItemsTree from './ItemsTree';

const ItemsTreeNode = ({
    node,
    type,
    onSelectItem,
    fetchChildren,
    onRefresh,
    refreshTrigger,
    selectedItemId,
    selectedPath = [],
    autoExpandPath = [],
    onAutoExpand,
    currentPath = [],
    actions = {},
    labels = {},
    renderIcon,
    renderItemLabel,
    renderActions,
    allowDrag,
    parentId
}) => {
    const [expanded, setExpanded] = useState(false);
    const [children, setChildren] = useState({ groups: [], items: [] });
    const [loaded, setLoaded] = useState(false);

    const [showMenu, setShowMenu] = useState(false);
    const [menuPos, setMenuPos] = useState({ x: 0, y: 0 });
    const menuRef = useRef(null);

    useEffect(() => {
        if (type === 'group' && autoExpandPath && autoExpandPath.includes(node.id) && !expanded) {
            handleExpand().then(() => {
                if (onAutoExpand) onAutoExpand(node.id);
            });
        }
    }, [autoExpandPath, node.id, type]);

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
        if (expanded && type === 'group') {
            refreshNodeContent();
        }
    }, [refreshTrigger]);

    const refreshNodeContent = async () => {
        if (fetchChildren) {
            const data = await fetchChildren(node.id);
            // Map data to expected format { groups: [], items: [] } if needed
            // Assuming fetchChildren returns { subFolders, parts } or { groups, items }
            // Adapter logic might be needed if API returns specific names
            // Let's assume the passed fetchChildren returns normalized { groups, items }
            setChildren(data);
        }
    };

    const handleExpand = async (e) => {
        e && e.stopPropagation && e.stopPropagation();
        if (type === 'item') {
            if (onSelectItem) onSelectItem(node, currentPath);
            return;
        }

        if (!expanded && !loaded) {
            await refreshNodeContent();
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

    // Wrapper Actions
    const handleRenameWrapper = () => {
        if (actions.onRename) actions.onRename(node, type, () => onRefresh());
        setShowMenu(false);
    };

    const handleDeleteWrapper = () => {
        if (actions.onDelete) actions.onDelete(node, type, () => onRefresh());
        setShowMenu(false);
    };

    const handleAddGroupWrapper = () => {
        if (actions.onAddGroup) {
            actions.onAddGroup(node, () => {
                setLoaded(false);
                setExpanded(true);
                refreshNodeContent().then(() => setLoaded(true));
            });
        }
        setShowMenu(false);
    };

    const handleAddItemWrapper = () => {
        if (actions.onAddItem) {
            actions.onAddItem(node, (newItem) => {
                refreshNodeContent().then(() => {
                    setLoaded(true);
                    setExpanded(true);
                    if (newItem && onSelectItem) onSelectItem(newItem, [...currentPath, newItem.id]);
                });
            });
        }
        setShowMenu(false);
    };

    const isActive = ((type === 'item' && node.id === selectedItemId) || (type === 'group' && selectedPath.includes(node.id) && !expanded));

    // Drag and Drop Handlers
    const [dragOver, setDragOver] = useState(false);
    const dragCounter = useRef(0); // Use counter to handle enter/leave on children

    const handleDragStart = (e) => {
        if (!allowDrag) return;
        e.dataTransfer.setData("application/json", JSON.stringify({ id: node.id, type: type }));
        e.stopPropagation();
    };

    const handleDragOver = (e) => {
        if (!allowDrag) return;
        e.preventDefault();
        e.stopPropagation();
    };

    const handleDragEnter = (e) => {
        if (!allowDrag) return;
        e.preventDefault();
        e.stopPropagation();
        dragCounter.current++;
        setDragOver(true);
    };

    const handleDragLeave = (e) => {
        if (!allowDrag) return;
        e.preventDefault();
        e.stopPropagation();
        dragCounter.current--;
        if (dragCounter.current === 0) {
            setDragOver(false);
        }
    };

    const handleDrop = (e) => {
        if (!allowDrag) return;
        e.preventDefault();
        e.stopPropagation();
        setDragOver(false);
        dragCounter.current = 0;

        try {
            const data = JSON.parse(e.dataTransfer.getData("application/json"));
            if (data.id === node.id) return; // Cannot drop on itself

            // Determine target: self (if group) or parent (if item)
            let targetId = null;
            if (type === 'group') {
                targetId = node.id;
            } else {
                targetId = parentId || null;
            }

            if (actions.onMoveItem) {
                actions.onMoveItem(data, { id: targetId }, () => {
                    // If target was this group, refresh it
                    if (type === 'group') {
                        setLoaded(false);
                        refreshNodeContent().then(() => setLoaded(true));
                    }
                });
            }
        } catch (err) {
            console.error("Drop error", err);
        }
    };

    const content = (
        <>
            <div
                className={`list-item ${isActive ? 'active' : ''} ${node.className || ''}`}
                onClick={handleExpand}
                onContextMenu={handleContextMenu}
                draggable={allowDrag}
                onDragStart={handleDragStart}
            >
                {renderItemLabel ? (
                    renderItemLabel(node, type, expanded)
                ) : (
                    <div className="item-label">
                        {renderIcon ? renderIcon(node, type, expanded) : (
                            type === 'group' ? <i className={`fa-solid ${expanded ? 'fa-folder-open' : 'fa-folder'}`}></i> : <i className="fa-solid fa-gear"></i>
                        )}
                        <span>{node.name} {node.count !== undefined && <span style={{ color: 'var(--text-clr2)' }}>({node.count})</span>}</span>
                    </div>
                )}

                <div className="item-actions">
                    {renderActions && renderActions(node, type, () => refreshNodeContent())}
                </div>

                {/* 3-dot menu button - Only show if actions exist */}
                {(actions.onRename || actions.onDelete || (type === 'group' && (actions.onAddGroup || actions.onAddItem))) && (
                    <button
                        className="icon-btn btn"
                        onClick={(e) => { e.stopPropagation(); handleContextMenu(e); }}
                    >
                        <i className="fa-solid fa-ellipsis-vertical"></i>
                    </button>
                )}
            </div>

            {/* Children container inside the wrapper if it's a group */}
            {expanded && type === 'group' && (
                <div className="parts-tree-children">
                    <ItemsTree
                        groups={children.groups || children.subFolders}
                        items={children.items || children.parts}
                        onSelectItem={onSelectItem}
                        fetchChildren={fetchChildren}
                        onRefresh={onRefresh}
                        refreshTrigger={refreshTrigger}
                        selectedItemId={selectedItemId}
                        selectedPath={selectedPath}
                        autoExpandPath={autoExpandPath}
                        onAutoExpand={onAutoExpand}
                        currentPath={currentPath}
                        actions={actions}
                        labels={labels}
                        renderIcon={renderIcon}
                        renderItemLabel={renderItemLabel}
                        renderActions={renderActions}
                        allowDrag={allowDrag}
                        parentId={node.id}
                    />
                </div>
            )}

            {/* Context Menu */}
            {showMenu && (
                <TreeContextMenu
                    handleRename={actions.onRename ? handleRenameWrapper : null}
                    handleAddGroup={actions.onAddGroup ? handleAddGroupWrapper : null}
                    handleAddItem={actions.onAddItem ? handleAddItemWrapper : null}
                    handleDelete={actions.onDelete ? handleDeleteWrapper : null}
                    type={type}
                    menuPos={menuPos}
                    menuRef={menuRef}
                    labels={labels}
                    onClose={() => setShowMenu(false)}
                />
            )}
        </>
    );

    if (type === 'group') {
        return (
            <div
                className={`tree-node-wrapper ${dragOver ? 'drag-over' : ''}`}
                onDragOver={handleDragOver}
                onDragEnter={handleDragEnter}
                onDragLeave={handleDragLeave}
                onDrop={handleDrop}
            >
                {content}
            </div>
        );
    }

    return content;
};

export default ItemsTreeNode;
