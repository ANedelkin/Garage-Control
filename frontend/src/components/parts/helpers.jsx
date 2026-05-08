import { partApi } from "../../services/partApi";
import GenericInputPopup from "../common/GenericInputPopup";

export const handleAddFolder = async (parentId, addPopup, removeLastPopup, onSuccess, showStatus) => {
    addPopup('Add Folder', (
        <GenericInputPopup 
            label="Folder Name"
            confirmText="Add"
            onConfirm={async (name) => {
                showStatus?.('Creating folder...', 'loading');
                try {
                    await partApi.createFolder({ name, parentId });
                    onSuccess();
                    removeLastPopup();
                    showStatus?.('Folder created successfully', 'success');
                } catch (e) {
                    console.error(e);
                    showStatus?.('Failed to create folder', 'error');
                }
            }}
            onClose={removeLastPopup}
        />
    ));
};

export const handleAddPart = async (parentId, onSuccess, showStatus) => {
    showStatus?.('Creating part...', 'loading');
    try {
        const newPart = await partApi.createPart({
            name: "Unnamed Part",
            partNumber: "000",
            price: 0,
            quantity: 0,
            parentId
        });
        onSuccess(newPart);
        showStatus?.('Part created successfully', 'success');
        return newPart;
    } catch (e) {
        console.error(e);
        showStatus?.('Failed to create part', 'error');
    }
};

// export { handleAddFolder, handleAddPart };