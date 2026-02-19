// const ContextMenu = ({ handleRename, handleAddSubFolder, handleAddSubPart, handleDelete, type, menuPos, menuRef }) => {
//     return (
//         <div
//             className="context-menu tile"
//             style={{ top: menuPos.y, left: menuPos.x, position: 'fixed' }} // Fixed needed because of recursion
//             ref={menuRef}
//             onClick={e => e.stopPropagation()}
//         >
//             {type === 'folder' && (
//                 <>
//                     <div className="list-item" onClick={handleRename}>
//                         <div className="item-label">
//                             <i className="fa-solid fa-pen"></i> Rename
//                         </div>
//                     </div>
//                     <div className="list-item" onClick={handleAddSubFolder}>
//                         <div className="item-label">
//                             <i className="fa-solid fa-folder-plus"></i> Add Folder
//                         </div>
//                     </div>
//                     <div className="list-item" onClick={handleAddSubPart}>
//                         <div className="item-label">
//                             <i className="fa-solid fa-plus"></i> Add Part
//                         </div>
//                     </div>
//                 </>
//             )}
//             <div className="context-menu-item" onClick={handleDelete}>
//                 <i className="fa-solid fa-trash"></i> Delete
//             </div>
//         </div>
//     )
// }

// export default ContextMenu;