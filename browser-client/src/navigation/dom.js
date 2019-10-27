import { Vector } from "../../lib/geometry.js";

const pos = document.getElementById('position');
const xInput = document.getElementById('xInput');
const yInput = document.getElementById('yInput');

const canvas = document.getElementById('canvas');
canvas.width = 800;
canvas.height = 800;
canvas.onclick = (event) => {
    const rect = event.target.getBoundingClientRect();
    const p = new Vector(event.clientX - rect.left, event.clientY - rect.top);
    const worldP = p.add(new Vector(-400, -400)).scale(1 / 200).invertY();
    const mapP = new Vector(Math.floor(worldP.x / 0.15) + 10, Math.floor(-worldP.y / 0.15) + 10);
    console.log("click", event, p, mapP);
    model.onGoTo(mapP.x, mapP.y);
}

export const ctx = canvas.getContext('2d');
window.ctx = ctx;

export const model = {
    goTo: () => {
        console.log("going to", xInput.value, yInput.value);
        model.onGoTo(parseInt(xInput.value), parseInt(yInput.value));
    },
    updatePosition: (x, y, phi) => {
        pos.innerText = `${x} / ${y} @ ${phi}`;
    },
    onGoTo: (x, y) => {},
};
window.model = model;
