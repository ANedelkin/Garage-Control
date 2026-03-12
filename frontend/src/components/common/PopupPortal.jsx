import React, { useState } from "react";
import ReactDOM from "react-dom";
import { usePopup } from "../../context/PopupContext";
import "../../assets/css/popup.css";
import Popup from "./Popup";

const modalRoot = document.getElementById("modal-root");

const PopupPortal = () => {
  const { stack, removeLastPopup } = usePopup();
  const [isMouseDown, setIsMouseDown] = useState(false);

  if (!modalRoot) return null;

  return ReactDOM.createPortal(
    <>
      {console.log(stack)}
      {stack.map((params, index) => {
        const isTop = index === stack.length - 1;

        return (
          <div
            key={index}
            className={`popup-overlay ${isTop ? "top" : ""}`}
            style={{ zIndex: 1000 + index }}
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
            <Popup title={params.title}>
              {params.children}
            </Popup>
          </div>
        );
      })}
    </>,
    modalRoot
  );
};

export default PopupPortal;
