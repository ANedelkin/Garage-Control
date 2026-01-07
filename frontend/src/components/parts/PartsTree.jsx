import PartsTreeNode from './PartsTreeNode';

const PartsTree = ({ folders, parts, onSelectPart, fetchContent, onRefresh, refreshTrigger, selectedPartId }) => {
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
                />
            ))}
        </>
    );
};

export default PartsTree;