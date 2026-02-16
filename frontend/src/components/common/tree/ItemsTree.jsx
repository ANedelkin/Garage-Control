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
