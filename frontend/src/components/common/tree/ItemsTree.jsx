import React from 'react';
import ItemsTreeNode from './ItemsTreeNode';

import '../../../assets/css/context-menu.css';

const ItemsTree = ({
    groups,
    items,
    onSelectItem,
    fetchChildren,
    onRefresh,
    refreshTrigger,
    selectedItemId,
    selectedPath = [],
    autoExpandPath = [],
    onAutoExpand,
    currentPath = [],
    actions,
    labels,
    renderIcon,
    renderItemLabel,
    renderActions,
    allowDrag,
    parentId
}) => {
    return (
        <>
            {groups && groups.map(group => (
                <ItemsTreeNode
                    key={group.id}
                    node={group}
                    type="group"
                    onSelectItem={onSelectItem}
                    fetchChildren={fetchChildren}
                    onRefresh={onRefresh}
                    refreshTrigger={refreshTrigger}
                    selectedItemId={selectedItemId}
                    selectedPath={selectedPath}
                    autoExpandPath={autoExpandPath}
                    onAutoExpand={onAutoExpand}
                    currentPath={[...currentPath, group.id]}
                    actions={actions}
                    labels={labels}
                    renderIcon={renderIcon}
                    renderItemLabel={renderItemLabel}
                    renderActions={renderActions}
                    allowDrag={allowDrag}
                    parentId={parentId}
                />
            ))}
            {items && items.map(item => (
                <ItemsTreeNode
                    key={item.id}
                    node={item}
                    type="item"
                    onSelectItem={onSelectItem}
                    fetchChildren={fetchChildren}
                    onRefresh={onRefresh}
                    refreshTrigger={refreshTrigger}
                    selectedItemId={selectedItemId}
                    selectedPath={selectedPath}
                    autoExpandPath={autoExpandPath}
                    onAutoExpand={onAutoExpand}
                    currentPath={[...currentPath, item.id]}
                    actions={actions}
                    labels={labels}
                    renderIcon={renderIcon}
                    renderItemLabel={renderItemLabel}
                    renderActions={renderActions}
                    allowDrag={allowDrag}
                    parentId={parentId}
                />
            ))}
        </>
    );
};

export default ItemsTree;
