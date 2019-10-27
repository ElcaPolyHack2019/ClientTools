import { Vector, Pose } from "../../lib/geometry.js"

const canvasCenter = new Vector(2, 2);

const canvasTransform = (point) => {
    return point.invertY().add(canvasCenter).scale(200);
}

const rover = "elcaduck";

window.config = {
    rover: rover,
    gps: "http://192.168.1.164:5000",
    hostname: `${rover}.local`,
    port: 9001,
    map: {
        tileSize: 40,
        transform: (point) => point.scale(window.config.map.tileSize),
    },
    canvas: {
        transform: canvasTransform,
    },
    bot: {
        pose: new Pose(
            new Vector(0, 0),
            0 // radians - 0 points to positive X
        ),
    },
};
