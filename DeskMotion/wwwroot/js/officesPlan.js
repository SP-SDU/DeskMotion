document.addEventListener("DOMContentLoaded", () => {
    const fgCanvas = document.getElementById("foregroundCanvas");
    const fgCtx = fgCanvas.getContext("2d");
    const bgCanvas = document.getElementById("backgroundCanvas");
    const bgCtx = bgCanvas.getContext("2d");
    const deskIcon = document.getElementById("deskIcon");
    const dialog = document.getElementById("dialog");
    const macAddressSelect = document.getElementById("macAddressSelect");
    const officeNameDisplay = document.getElementById("OfficeNameDisplay");
    const saveForm = document.getElementById("saveForm");
    const removeAllImgBtn = document.getElementById("removeAllImg");

    let objects = [];
    let selectedObject = null;

    // Initialize background canvas
    bgCtx.fillStyle = "lightgray";
    bgCtx.fillRect(0, 0, bgCanvas.width, bgCanvas.height);

    const blueRect1 = { x: 20, y: 50, width: 350, height: 400 };
    const blueRect2 = { x: 400, y: 150, width: 380, height: 240 };
    bgCtx.fillStyle = "#255ECF";
    bgCtx.fillRect(blueRect1.x, blueRect1.y, blueRect1.width, blueRect1.height);
    bgCtx.fillRect(blueRect2.x, blueRect2.y, blueRect2.width, blueRect2.height);

    // Drag and drop functionality
    deskIcon.addEventListener("dragstart", (e) => {
        if (!isAdmin) return;
        e.dataTransfer.setData("text/plain", e.target.id);
    });

    fgCanvas.addEventListener("dragover", (e) => e.preventDefault());

    fgCanvas.addEventListener("drop", (e) => {
        if (!isAdmin) return;
        e.preventDefault();
        const id = e.dataTransfer.getData("text/plain");
        const img = document.getElementById(id);
        const x = e.offsetX - 40;
        const y = e.offsetY - 40;

        const newRect = { x, y, width: 80, height: 80, img: img.src, macAddress: "", angle: 0 };

        if (isValidPlacement(newRect)) {
            objects.push(newRect);
            redrawCanvas();
        } else {
            alert("Cannot place here!");
        }
    });

    function isValidPlacement(rect) {
        return (
            rect.x >= 0 &&
            rect.x + rect.width <= fgCanvas.width &&
            rect.y >= 0 &&
            rect.y + rect.height <= fgCanvas.height &&
            (isWithinBlueRect(rect, blueRect1) || isWithinBlueRect(rect, blueRect2))
        );
    }

    function isWithinBlueRect(rect, blueRect) {
        return (
            rect.x >= blueRect.x &&
            rect.x + rect.width <= blueRect.x + blueRect.width &&
            rect.y >= blueRect.y &&
            rect.y + rect.height <= blueRect.y + blueRect.height
        );
    }

    function redrawCanvas() {
        fgCtx.clearRect(0, 0, fgCanvas.width, fgCanvas.height);
        objects.forEach((obj) => {
            const img = new Image();
            img.onload = () => {
                fgCtx.save();
                fgCtx.translate(obj.x + obj.width / 2, obj.y + obj.height / 2);
                fgCtx.rotate((obj.angle || 0) * Math.PI / 180);
                fgCtx.drawImage(img, -obj.width / 2, -obj.height / 2, obj.width, obj.height);
                fgCtx.restore();
                if (obj.macAddress) {
                    fgCtx.fillStyle = "white";
                    fgCtx.font = "bold 13px Arial";
                    fgCtx.fillText(obj.macAddress, obj.x, obj.y + obj.height + 8);
                }
            };
            img.src = obj.img;
        });
        updateTotalDesks();
    }

    function updateTotalDesks() {
        const totalDesksLabel = document.getElementById("totalDesksLabel");
        totalDesksLabel.textContent = `Total Desks: ${objects.length}`;
    }

    removeAllImgBtn.addEventListener("click", () => {
        if (!isAdmin) return;
        objects = [];
        redrawCanvas();
    });

    saveForm.addEventListener("submit", handleFormSubmit);

    function handleFormSubmit(event) {
        event.preventDefault();
        document.getElementById("BgCanvasDataInput").value = bgCanvas.toDataURL();
        document.getElementById("FgCanvasDataInput").value = JSON.stringify(objects);
        document.getElementById("OfficeNameInput").value = officeNameDisplay.textContent.trim();

        const formData = new FormData(saveForm);

        fetch(saveForm.action, {
            method: "POST",
            body: formData,
        })
            .then((response) => {
                if (response.ok) {
                    window.location.reload();
                } else {
                    console.error("Form submission failed:", response.statusText);
                }
            })
            .catch((error) => {
                console.error("Form submission error:", error);
            });
    }

    // Initialize canvas on load
    if (bgCanvasData) {
        const img = new Image();
        img.onload = () => bgCtx.drawImage(img, 0, 0);
        img.src = bgCanvasData;
    }

    if (fgCanvasData) {
        try {
            objects = JSON.parse(fgCanvasData);
            redrawCanvas();
        } catch (err) {
            console.error("Error parsing FgCanvasData:", err);
        }
    }
    updateTotalDesks();
});
