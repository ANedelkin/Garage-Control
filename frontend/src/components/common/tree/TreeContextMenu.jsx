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
                        <div className="list-item" onClick={handleRename}>
                            <div className="item-label">
                                <i className="fa-solid fa-pen"></i> {labels.rename || 'Rename'}
                            </div>
                        </div>
                    )}
                    {handleAddGroup && (
                        <div className="list-item" onClick={handleAddGroup}>
                            <div className="item-label">
                                <i className="fa-solid fa-folder-plus"></i> {labels.addGroup || 'Add Group'}
                            </div>
                        </div>
                    )}
                    {handleAddItem && (
                        <div className="list-item" onClick={handleAddItem}>
                            <div className="item-label">
                                <i className="fa-solid fa-plus"></i> {labels.addItem || 'Add Item'}
                            </div>
                        </div>
                    )}
                </>
            )}

            {type === 'item' && handleRename && (
                <div className="list-item" onClick={handleRename}>
                    <div className="item-label">
                        <i className="fa-solid fa-pen"></i> {labels.rename || 'Rename'}
                    </div>
                </div>
            )}

            {handleDelete && (
                <div className="list-item" onClick={handleDelete}>
                    <div className="item-label">
                        <i className="fa-solid fa-trash"></i> {labels.delete || 'Delete'}
                    </div>
                </div>
            )}
        </div>
    )
}

export default TreeContextMenu;
