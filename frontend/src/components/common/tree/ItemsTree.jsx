import React from 'react';
import ItemsTreeNode from './ItemsTreeNode';

const ItemsTree = ({
    groups,
    items,
    onSelectItem,
    fetchChildren,
    onRefresh,
    refreshTrigger,
    selectedItemId,
    selectedPath = [],
    currentPath = [],
    actions,
    labels,
    renderIcon,
    renderActions,
    allowDrag,
    parentId,
    onStatusChange
}) => {
    return (
        <>
            {groups && groups.map(group => (
                <ItemsTreeNode
                    key={group.id}
                    node={group}
                    type="group" // 'group' equivalent to 'folder'
                    onSelectItem={onSelectItem}
                    fetchChildren={fetchChildren}
                    onRefresh={onRefresh}
                    refreshTrigger={refreshTrigger}
                    selectedItemId={selectedItemId}
                    selectedPath={selectedPath}
                    currentPath={[...currentPath, group.id]}
                    actions={actions}
                    labels={labels}
                    renderIcon={renderIcon}
                    renderActions={renderActions}
                    allowDrag={allowDrag}
                    parentId={parentId}
                    onStatusChange={onStatusChange}
                />
            ))}
            {items && items.map(item => (
                <ItemsTreeNode
                    key={item.id}
                    node={item}
                    type="item" // 'item' equivalent to 'part'
                    onSelectItem={onSelectItem}
                    fetchChildren={fetchChildren}
                    onRefresh={onRefresh}
                    refreshTrigger={refreshTrigger}
                    selectedItemId={selectedItemId}
                    selectedPath={selectedPath}
                    currentPath={[...currentPath, item.id]}
                    actions={actions}
                    labels={labels}
                    renderIcon={renderIcon}
                    renderActions={renderActions}
                    allowDrag={allowDrag}
                    parentId={parentId}
                    onStatusChange={onStatusChange}
                />
            ))}
        </>
    );
};

export default ItemsTree;
