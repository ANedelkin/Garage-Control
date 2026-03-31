import React from 'react';

const TreeContextMenu = ({ handleRename, handleAddGroup, handleAddItem, handleDelete, type, menuPos, menuRef, labels = {}, onClose }) => {
    const [adjustedPos, setAdjustedPos] = React.useState({ x: menuPos.x, y: menuPos.y });

    React.useLayoutEffect(() => {
        if (!menuRef.current || window.innerWidth <= 500) return;

        const rect = menuRef.current.getBoundingClientRect();
        const viewportWidth = window.innerWidth;
        const viewportHeight = window.innerHeight;

        let { x, y } = menuPos;

        // Bounds checking
        if (x + rect.width > viewportWidth) {
            x = viewportWidth - rect.width - 10;
        }
        if (y + rect.height > viewportHeight) {
            y = viewportHeight - rect.height - 10;
        }

        setAdjustedPos({ x, y });
    }, [menuPos, menuRef]);

    const isMobile = window.innerWidth <= 500;

    return (
        <>
            {isMobile && <div className="context-menu-overlay" onClick={onClose}></div>}
            <div
                className="context-menu tile"
                style={!isMobile ? { top: adjustedPos.y, left: adjustedPos.x, position: 'fixed', zIndex: 1000 } : { position: 'fixed', zIndex: 1000 }}
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
                        <div className="item-label" style={{ color: 'var(--danger)' }}>
                            <i className="fa-solid fa-trash"></i> {labels.delete || 'Delete'}
                        </div>
                    </div>
                )}
            </div>
        </>
    )
}

export default TreeContextMenu;
