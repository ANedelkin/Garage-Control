import { createContext, useContext, useState, useCallback, useRef } from "react";

const StatusContext = createContext();

export const useStatus = () => useContext(StatusContext);

export const StatusProvider = ({ children }) => {
    const [status, setStatus] = useState(null); // { message, type: 'loading' | 'success' | 'error' }
    const timeoutRef = useRef(null);

    const showStatus = useCallback((message, type = 'loading') => {
        if (timeoutRef.current) {
            clearTimeout(timeoutRef.current);
            timeoutRef.current = null;
        }

        setStatus({ message, type });

        if (type === 'success' || type === 'error') {
            timeoutRef.current = setTimeout(() => {
                setStatus(null);
                timeoutRef.current = null;
            }, 3000);
        }
    }, []);

    const hideStatus = useCallback(() => {
        setStatus(null);
        if (timeoutRef.current) {
            clearTimeout(timeoutRef.current);
            timeoutRef.current = null;
        }
    }, []);

    return (
        <StatusContext.Provider value={{ status, showStatus, hideStatus }}>
            {children}
        </StatusContext.Provider>
    );
};
