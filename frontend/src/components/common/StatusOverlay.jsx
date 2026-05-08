import React from 'react';
import { useStatus } from '../../context/StatusContext';
import '../../assets/css/common/status-overlay.css';

const StatusOverlay = () => {
    const { status } = useStatus();

    if (!status) return null;

    const getIcon = () => {
        switch (status.type) {
            case 'loading':
                return <i className="fa-solid fa-circle-notch fa-spin"></i>;
            case 'success':
                return <i className="fa-solid fa-circle-check"></i>;
            case 'error':
                return <i className="fa-solid fa-circle-exclamation"></i>;
            default:
                return null;
        }
    };

    return (
        <div className={`status-overlay-container ${status.type}`}>
            <div className="status-tile">
                <span className="status-icon">{getIcon()}</span>
                <span className="status-message">{status.message}</span>
            </div>
        </div>
    );
};

export default StatusOverlay;
