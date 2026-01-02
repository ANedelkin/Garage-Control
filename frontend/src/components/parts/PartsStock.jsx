import React, { useState, useEffect } from 'react';
import '../../assets/css/parts-stock.css';
import PartsTree from './PartsTree';
import PartDetails from './PartDetails';

const PartsStock = () => {
    const [selectedPart, setSelectedPart] = useState(null);
    const [refreshTree, setRefreshTree] = useState(0);

    const handlePartSelect = (part) => {
        setSelectedPart(part);
    };

    const handleRefresh = () => {
        setRefreshTree(prev => prev + 1);
        if (selectedPart) {
            // Deselect if verified deleted? For now keep simple
            // setSelectedPart(null); 
        }
    };

    return (
        <main className="main parts-stock container">
            <div className="tile">
                <div className="horizontal grow">
                    {/* Left Panel: Tree */}
                    <div className="form-left">
                        <div className="section-header">
                            <h3>Parts Stock</h3>
                        </div>
                        <div className="list-container grow">
                            <PartsTree
                                onSelectPart={handlePartSelect}
                                refreshTrigger={refreshTree}
                                onRefresh={handleRefresh}
                            />
                        </div>
                    </div>

                    <div className="vertical-divider"></div>

                    {/* Right Panel: Details */}
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
