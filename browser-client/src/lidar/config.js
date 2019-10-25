import { Vector, Pose } from "../../lib/geometry.js"

const canvasCenter = new Vector(3, 3);

const canvasTransform = (point) => {
    return point.invertY().add(canvasCenter).scale(200);
}

window.config = {
    hostname: "elcaduck.local",
    port: 9001,
    map: {
        tileSize: 40,
        transform: (point) => point.scale(window.config.map.tileSize),
    },
    bot: {
        pose: new Pose(
            new Vector(0, 0),
            0 // radians - 0 points to positive X
        ),
        transform: canvasTransform,
    },
};
