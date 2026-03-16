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

  return (
    <PopupContext.Provider value={{ stack, addPopup, removeLastPopup }}>
      {children}
    </PopupContext.Provider>
  );
};
