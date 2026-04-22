import { partApi } from "../../services/partApi";

import GenericInputPopup from "../common/GenericInputPopup";

export const handleAddFolder = async (parentId, addPopup, removeLastPopup, onSuccess) => {
    addPopup('Add Folder', (
        <GenericInputPopup 
            label="Folder Name"
            confirmText="Add"
            onConfirm={async (name) => {
                try {
                    await partApi.createFolder({ name, parentId });
                    onSuccess();
                } catch (e) {
                    console.error(e);
                    alert("Failed to create folder");
                }
            }}
            onClose={removeLastPopup}
        />
    ));
};

export const handleAddPart = async (parentId, onSuccess) => {
    try {
        const newPart = await partApi.createPart({
            name: "Unnamed Part",
            partNumber: "000",
            price: 0,
            quantity: 0,
            parentId
        });
        onSuccess(newPart);
        return newPart;
    } catch (e) {
        console.error(e);
        alert("Failed to create part");
    }
};

// export { handleAddFolder, handleAddPart };