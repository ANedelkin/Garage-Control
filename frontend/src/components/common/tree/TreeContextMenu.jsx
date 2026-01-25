import React from 'react';

const TreeContextMenu = ({ handleRename, handleAddGroup, handleAddItem, handleDelete, type, menuPos, menuRef, labels = {} }) => {
    return (
        <div
            className="context-menu tile"
            style={{ top: menuPos.y, left: menuPos.x, position: 'fixed', zIndex: 1000 }} // Fixed needed because of recursion
            ref={menuRef}
            onClick={e => e.stopPropagation()}
        >
            {type === 'group' && (
                <>
                    {handleRename && (
                        <div className="context-menu-item" onClick={handleRename}>
                            <i className="fa-solid fa-pen"></i> {labels.rename || 'Rename'}
                        </div>
                    )}
                    {handleAddGroup && (
                        <div className="context-menu-item" onClick={handleAddGroup}>
                            <i className="fa-solid fa-folder-plus"></i> {labels.addGroup || 'Add Group'}
                        </div>
                    )}
                    {handleAddItem && (
                        <div className="context-menu-item" onClick={handleAddItem}>
                            <i className="fa-solid fa-plus"></i> {labels.addItem || 'Add Item'}
                        </div>
                    )}
                </>
            )}

            {type === 'item' && handleRename && (
                <div className="context-menu-item" onClick={handleRename}>
                    <i className="fa-solid fa-pen"></i> {labels.rename || 'Rename'}
                </div>
            )}

            {handleDelete && (
                <div className="context-menu-item" onClick={handleDelete}>
                    <i className="fa-solid fa-trash"></i> {labels.delete || 'Delete'}
                </div>
            )}
        </div>
    )
}

export default TreeContextMenu;
