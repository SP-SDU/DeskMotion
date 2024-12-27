const fgCanvas = document.getElementById('foregroundCanvas');
const fgCtx = fgCanvas.getContext('2d');
const bgCanvas = document.getElementById("backgroundCanvas");
const bgCtx = bgCanvas.getContext("2d");
const deskIcon = document.getElementById('deskIcon');
const dialog = document.getElementById('dialog');
const macAddressSelect = document.getElementById('macAddressSelect');
let objects = [];
let selectedObject = null;

// Initialize canvas background
bgCtx.fillStyle = "lightgray";
bgCtx.fillRect(0, 0, bgCanvas.width, bgCanvas.height);

const blueRect1 = { x: 20, y: 50, width: 350, height: 400 };
const blueRect2 = { x: 400, y: 150, width: 380, height: 240 };
bgCtx.fillStyle = "#255ECF";
bgCtx.fillRect(blueRect1.x, blueRect1.y, blueRect1.width, blueRect1.height);
bgCtx.fillRect(blueRect2.x, blueRect2.y, blueRect2.width, blueRect2.height);

deskIcon.addEventListener('dragstart', (e) => {
	e.dataTransfer.setData('text/plain', e.target.id);
});

fgCanvas.addEventListener('dragover', (e) => e.preventDefault());

fgCanvas.addEventListener('drop', (e) => {
	e.preventDefault();
	const id = e.dataTransfer.getData('text/plain');
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
	return rect.x >= 0 && rect.x + rect.width <= fgCanvas.width &&
		rect.y >= 0 && rect.y + rect.height <= fgCanvas.height &&
		(isWithinBlueRect(rect, blueRect1) || isWithinBlueRect(rect, blueRect2));
}

function isWithinBlueRect(rect, blueRect) {
	return rect.x >= blueRect.x && rect.x + rect.width <= blueRect.x + blueRect.width &&
		rect.y >= blueRect.y && rect.y + rect.height <= blueRect.y + blueRect.height;
}

// Open dialog for selected desk
fgCanvas.addEventListener('click', (e) => {
	const x = e.offsetX, y = e.offsetY;
	selectedObject = objects.find(obj => x >= obj.x && x <= obj.x + obj.width && y >= obj.y && y <= obj.y + obj.height);

	if (selectedObject) {
		dialog.style.display = 'block';
		dialog.style.left = `${e.pageX}px`;
		dialog.style.top = `${e.pageY}px`;

		macAddressSelect.value = selectedObject.macAddress || "";
	} else {
		dialog.style.display = 'none';
	}
});

// Assign MacAddress to selected desk
document.getElementById('assignBtn').addEventListener('click', () => {
	if (selectedObject) {
		const selectedMac = macAddressSelect.value;

		if (!selectedMac) {
			alert("Please select a MAC address.");
			return;
		}

		// Remove assigned MacAddress from dropdown list
		macAddressSelect.querySelector(`option[value="${selectedMac}"]`).remove();

		selectedObject.macAddress = selectedMac;
		redrawCanvas();
		dialog.style.display = "none";
	}
});

// Delete selected desk
document.getElementById('deleteBtn').addEventListener('click', () => {
	if (selectedObject) {
		if (selectedObject.macAddress) {
			const option = document.createElement("option");
			option.value = selectedObject.macAddress;
			option.textContent = selectedObject.macAddress;
			macAddressSelect.appendChild(option);
		}

		objects = objects.filter(obj => obj !== selectedObject);
		redrawCanvas();
		dialog.style.display = 'none';
	}
});

// Rotate selected desk
document.getElementById('rotateBtn').addEventListener('click', () => {
	if (selectedObject) {
		selectedObject.angle = (selectedObject.angle + 90) % 360;
		redrawCanvas();
		dialog.style.display = 'none';
	}
});

// Remove all desks from offices
document.getElementById('removeAllImg').addEventListener('click', () => {
	objects.forEach(obj => {
		if (obj.macAddress) {
			const option = document.createElement("option");
			option.value = obj.macAddress;
			option.textContent = obj.macAddress;
			macAddressSelect.appendChild(option);
		}
	});

	objects = [];
	redrawCanvas();
});

function redrawCanvas() {
	fgCtx.clearRect(0, 0, fgCanvas.width, fgCanvas.height);
	objects.forEach(obj => {
		const img = new Image();
		img.onload = () => {
			fgCtx.save();
			fgCtx.translate(obj.x + obj.width / 2, obj.y + obj.height / 2);
			fgCtx.rotate(obj.angle * Math.PI / 180);
			fgCtx.drawImage(img, -obj.width / 2, -obj.height / 2, obj.width, obj.height);
			fgCtx.restore();
			if (obj.macAddress) {
				fgCtx.fillStyle = 'black';
				fgCtx.font = 'bold 12px Arial';
				fgCtx.fillText(obj.macAddress, obj.x + -8, obj.y + obj.height + 8);
			}
		};
		img.src = obj.img;
	});
}

// Populate data on page load
window.onload = () => {
	const bgCanvasData = document.getElementById('BgCanvasDataInput').value;
	const fgCanvasData = document.getElementById('FgCanvasDataInput').value;

	if (bgCanvasData) {
		const img = new Image();
		img.onload = () => bgCtx.drawImage(img, 0, 0);
		img.src = bgCanvasData;
	}

	if (fgCanvasData) {
		try {
			objects = JSON.parse(fgCanvasData);
			const usedMacs = objects.map(obj => obj.macAddress).filter(mac => mac);
			usedMacs.forEach(mac => {
				macAddressSelect.querySelector(`option[value="${mac}"]`)?.remove();
			});
			redrawCanvas();
		} catch (err) {
			console.error("Error parsing FgCanvasData:", err);
		}
	}
};

// Save form data
document.getElementById('saveForm').addEventListener('submit', () => {
	document.getElementById('BgCanvasDataInput').value = bgCanvas.toDataURL();
	document.getElementById('FgCanvasDataInput').value = JSON.stringify(objects);
});
