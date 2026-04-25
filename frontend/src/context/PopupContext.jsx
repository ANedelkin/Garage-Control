import { createContext, useContext, useState, useCallback } from "react";

const PopupContext = createContext();

export const usePopup = () => useContext(PopupContext);

export const PopupProvider = ({ children }) => {
  const [stack, setStack] = useState([]);

  const addPopup = useCallback((title, children, isRaw = false, onCloseCallback = null) => {
    setStack(prev => [...prev, { title, children, isRaw, onCloseCallback }]);
  }, []);

  const removeLastPopup = useCallback(() => {
    setStack(prev => {
        const last = prev[prev.length - 1];
        if (last && last.onCloseCallback) {
            last.onCloseCallback();
        }
        return prev.slice(0, -1);
    });
  }, []);

  // Updates the children of the topmost popup without triggering onCloseCallback
  const updateLastPopup = useCallback((newChildren) => {
    setStack(prev => {
        if (prev.length === 0) return prev;
        const updated = [...prev];
        updated[updated.length - 1] = { ...updated[updated.length - 1], children: newChildren };
        return updated;
    });
  }, []);

  return (
    <PopupContext.Provider value={{ stack, addPopup, removeLastPopup, updateLastPopup }}>
      {children}
    </PopupContext.Provider>
  );
};

