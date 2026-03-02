import { createContext, useContext, useState, useCallback } from "react";

const PopupContext = createContext();

export const usePopup = () => useContext(PopupContext);

export const PopupProvider = ({ children }) => {
  const [stack, setStack] = useState([]);

  const addPopup = useCallback((title, children) => {
    console.log({title, children});
    setStack(prev => [...prev, { title, children }]);
  }, []);

  const removeLastPopup = useCallback(() => {
    setStack(prev => prev.slice(0, -1));
  }, []);

  return (
    <PopupContext.Provider value={{ stack, addPopup, removeLastPopup }}>
      {children}
    </PopupContext.Provider>
  );
};
