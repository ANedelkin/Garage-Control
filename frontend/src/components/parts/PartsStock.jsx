import React, { useState, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import '../../assets/css/parts-stock.css';

import { partApi } from '../../services/partApi';
import { handleAddFolder, handleAddPart } from './helpers';

import PartsTree from './PartsTree';
import PartDetails from './PartDetails';

const PartsStock = () => {
    const [searchParams] = useSearchParams();
    const [selectedPart, setSelectedPart] = useState(null);
    const [selectedPath, setSelectedPath] = useState([]);
    const [refreshTree, setRefreshTree] = useState(0);
    const [rootFolders, setRootFolders] = useState([]);
    const [rootParts, setRootParts] = useState([]);

    const handlePartSelect = (part, path) => {
        setSelectedPart(part);
        setSelectedPath(path);
    };

    const handleRefresh = () => {
        setRefreshTree(prev => prev + 1);
        if (selectedPart) {
            // TODO: Implement
        }
    };

    useEffect(() => {
        const loadLinkedPart = async () => {
            const partId = searchParams.get('partId') || searchParams.get('id');
            if (partId) {
                try {
                    const part = await partApi.getPart(partId);
                    if (part) {
                        setSelectedPart(part);
                        setSelectedPath(part.path || []);
                    }
                } catch (error) {
                    console.error("Error loading linked part", error);
                }
            }
        };
        loadLinkedPart();
    }, [searchParams]);

    useEffect(() => {
        fetchFolderContent(null);
    }, [refreshTree]);

    const fetchFolderContent = async (folderId) => {
        try {
            const data = await partApi.getFolderContent(folderId);
            // Sort alphabetically
            if (data.subFolders) data.subFolders.sort((a, b) => a.name.localeCompare(b.name));
            if (data.parts) data.parts.sort((a, b) => a.name.localeCompare(b.name));

            if (!folderId) {
                setRootFolders(data.subFolders);
                setRootParts(data.parts);
            }
            return data;
        } catch (error) {
            console.error("Error fetching parts", error);
            return { subFolders: [], parts: [] };
        }
    };

    return (
        <main className="main parts-stock">
            <div className="tile">
                <div className="horizontal grow">
                    <div className="form-left">
                        <div className="section-header">
                            <h3>Parts Stock</h3>
                        </div>
                        <div className="list-container grow">
                            <div className="parts-tree">
                                <div className="section-header-small">
                                    <button className="btn icon-btn" title="Refresh" onClick={() => handleRefresh()}>
                                        <i className="fa-solid fa-sync"></i>
                                    </button>
                                    <button className="btn icon-btn" title="Add Folder" onClick={() => handleAddFolder(null, handleRefresh)}>
                                        <i className="fa-solid fa-folder-plus"></i>
                                    </button>
                                    <button className="btn icon-btn" title="Add Part" onClick={async () => {
                                        const newPart = await handleAddPart(null, handleRefresh);
                                        if (newPart) handlePartSelect(newPart, [newPart.id]);
                                    }}>
                                        <i className="fa-solid fa-plus"></i>
                                    </button>
                                </div>

                                <PartsTree
                                    folders={rootFolders}
                                    parts={rootParts}
                                    onSelectPart={handlePartSelect}
                                    fetchContent={fetchFolderContent}
                                    onRefresh={handleRefresh}
                                    refreshTrigger={refreshTree}
                                    selectedPartId={selectedPart?.id}
                                    selectedPath={selectedPath}
                                />
                            </div>
                        </div>
                    </div>

                    <div className="vertical-divider"></div>

                    <div className="form-right">
                        <PartDetails
                            part={selectedPart}
                            onUpdate={handleRefresh}
                            onDelete={() => { setSelectedPart(null); handleRefresh(); }}
                        />
                    </div>
                </div>
            </div>
        </main>
    );
};

export default PartsStock;
