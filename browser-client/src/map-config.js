import { Vector, Pose } from "../lib/geometry.js"

window.config = {
    hostname: "elcaduck.local",
    port: 9001,
    map: {
        tileSize: 40,
        transform: (point) => point.scale(window.config.map.tileSize),
    },
};
