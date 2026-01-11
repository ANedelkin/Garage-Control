import PartsTreeNode from './PartsTreeNode';

const PartsTree = ({ folders, parts, onSelectPart, fetchContent, onRefresh, refreshTrigger, selectedPartId, selectedPath = [], currentPath = [] }) => {
    return (
        <>
            {folders.map(folder => (
                <PartsTreeNode
                    key={folder.id}
                    node={folder}
                    type="folder"
                    onSelectPart={onSelectPart}
                    fetchContent={fetchContent}
                    onRefresh={onRefresh}
                    refreshTrigger={refreshTrigger}
                    selectedPartId={selectedPartId}
                    selectedPath={selectedPath}
                    currentPath={[...currentPath, folder.id]}
                />
            ))}
            {parts.map(part => (
                <PartsTreeNode
                    key={part.id}
                    node={part}
                    type="part"
                    onSelectPart={onSelectPart}
                    fetchContent={fetchContent}
                    onRefresh={onRefresh}
                    refreshTrigger={refreshTrigger}
                    selectedPartId={selectedPartId}
                    selectedPath={selectedPath}
                    currentPath={[...currentPath, part.id]}
                />
            ))}
        </>
    );
};

export default PartsTree;