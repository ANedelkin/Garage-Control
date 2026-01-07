import { partApi } from "../../services/partApi";

export const handleAddFolder = async (parentId, onSuccess) => {
    const name = prompt("Enter folder name:");
    if (!name) return;
    try {
        await partApi.createFolder({ name, parentId });
        onSuccess();
    } catch (e) {
        console.error(e);
        alert("Failed to create folder");
    }
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
        onSuccess();
        return newPart;
    } catch (e) {
        console.error(e);
        alert("Failed to create part");
    }
};

// export { handleAddFolder, handleAddPart };