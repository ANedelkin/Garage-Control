import React from "react";
import ReactDOM from "react-dom";
import { usePopup } from "../../context/PopupContext";
import "../../assets/css/popup.css";
import Popup from "./Popup";

const modalRoot = document.getElementById("modal-root");

const PopupPortal = () => {
  const { stack, removeLastPopup } = usePopup();

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
            onClick={isTop ? removeLastPopup : undefined}
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
