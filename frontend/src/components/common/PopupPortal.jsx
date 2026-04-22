import React, { useState, useEffect, useRef } from "react";
import ReactDOM from "react-dom";
import { usePopup } from "../../context/PopupContext";
import "../../assets/css/popup.css";
import Popup from "./Popup";

const modalRoot = document.getElementById("modal-root");

const PopupPortal = () => {
  const { stack, removeLastPopup } = usePopup();
  const [isMouseDown, setIsMouseDown] = useState(false);
  const popupRefs = useRef([]);

  useEffect(() => {
    if (stack.length > 0) {
      const topIdx = stack.length - 1;
      const topPopup = popupRefs.current[topIdx];
      if (topPopup) {
        topPopup.focus();
      }
    }
  }, [stack.length]);

  if (!modalRoot) return null;

  return ReactDOM.createPortal(
    <>
      {stack.map((params, index) => {
        const isTop = index === stack.length - 1;

        return (
          <div
            key={index}
            ref={(el) => (popupRefs.current[index] = el)}
            tabIndex="-1"
            className={`popup-overlay ${isTop ? "top" : ""}`}
            style={{ zIndex: 1000 + index, outline: 'none' }}
            onMouseDown={(e) => {
              if (isTop && e.target.classList.contains("popup-overlay")) {
                setIsMouseDown(true);
              }
            }}
            onMouseUp={(e) => {
              if (isTop && isMouseDown && e.target.classList.contains("popup-overlay")) {
                removeLastPopup();
              }
              setIsMouseDown(false);
            }}
          >
            {params.isRaw ? (
              params.children
            ) : (
              <Popup title={params.title}>
                {params.children}
              </Popup>
            )}
          </div>
        );
      })}
    </>,
    modalRoot
  );
};

export default PopupPortal;
