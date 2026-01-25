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
    currentPath = [],
    actions = {},
    labels = {},
    renderIcon,
    renderActions
}) => {
    const [expanded, setExpanded] = useState(false);
    const [children, setChildren] = useState({ groups: [], items: [] });
    const [loaded, setLoaded] = useState(false);

    const [showMenu, setShowMenu] = useState(false);
    const [menuPos, setMenuPos] = useState({ x: 0, y: 0 });
    const menuRef = useRef(null);

    useEffect(() => {
        if (type === 'group' && selectedPath.includes(node.id) && !expanded) {
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

    // Default Icon Logic if not provided
    const getIcon = () => {
        if (renderIcon) return renderIcon(node, type, expanded);
        if (type === 'group') return <i className={`fa-solid ${expanded ? 'fa-folder-open' : 'fa-folder'}`}></i>;
        return <i className="fa-solid fa-gear"></i>;
    };

    return (
        <>
            <div
                className={`list-item ${isActive ? 'active' : ''} ${node.className || ''}`}
                onClick={handleExpand}
                onContextMenu={handleContextMenu}
            >
                <div className="item-label">
                    {getIcon()}
                    <span>{node.name} {node.count !== undefined && <span className='text-muted'>({node.count})</span>}</span>
                </div>

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

            {/* Children */}
            {expanded && type === 'group' && (
                <div className="parts-tree-children">
                    <ItemsTree
                        groups={children.groups || children.subFolders} // Backward compatibility check
                        items={children.items || children.parts}
                        onSelectItem={onSelectItem}
                        fetchChildren={fetchChildren}
                        onRefresh={onRefresh}
                        refreshTrigger={refreshTrigger}
                        selectedItemId={selectedItemId}
                        selectedPath={selectedPath}
                        currentPath={currentPath}
                        actions={actions}
                        labels={labels}
                        renderIcon={renderIcon}
                        renderActions={renderActions}
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
                />
            )}
        </>
    );
};

export default ItemsTreeNode;
