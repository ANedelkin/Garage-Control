import { partApi } from "../../services/partApi";

import SimpleInputPopup from "../common/SimpleInputPopup";

export const handleAddFolder = async (parentId, addPopup, removeLastPopup, onSuccess) => {
    addPopup('Add Folder', (
        <SimpleInputPopup 
            label="Folder Name"
            onConfirm={async (name) => {
                try {
                    await partApi.createFolder({ name, parentId });
                    removeLastPopup();
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