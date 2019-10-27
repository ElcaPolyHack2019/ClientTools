import {
    Vector,
    Map,
    Pose,
} from '../../lib/geometry.js';
import {
    PointCloud,
    Robot,
    MapRenderer,
} from '../../lib/drawing.js';
import {
    RosMaster
} from '../../lib/ros.js';
import {
    model,
    ctx,
} from './dom.js';
import { AStar } from './algorithm.js';

const config = window.config;

const ros = new RosMaster(config.hostname, config.port);
const map = new Map(20, 20, 0);
const mapOffset = new Vector(100, 100);
const mapResolution = 0.15;
const mapRenderer = new MapRenderer(map, 30, v => v.add(mapOffset));

const lidarData = ros.observable({
    name : '/scan',
    messageType : 'sensor_msgs/LaserScan',
    throttle_rate: 500,
    queue_size: 1,
    queue_length: 1,
});

const botData = rxjs.interval(1000).pipe(
    rxjs.operators.flatMap(_ => {
        return rxjs.from(fetch(`${config.gps}/api/${config.rover}`, {
            mode: 'cors',
        }).then(response => {
            return response.json();
        }));
    })
);

const robot = new Robot(config.bot.pose, config.canvas.transform);

const cloud = new PointCloud('red', config.canvas.transform);
const path = new PointCloud('blue', v => {
    return config.canvas.transform(v.invertY().scale(0.15).add(new Vector(-1.5 + 0.075, 1.5 - 0.075)));
});

const botPose = botData.pipe(
    rxjs.operators.map(raw => {
        model.updatePosition(raw.gps_x, raw.gps_y, raw.gps_orientation);
        return new Pose(new Vector(raw.gps_x, raw.gps_y), raw.gps_orientation_rad);
    })
);

botPose.subscribe(data => {
    robot.pose.position = data.position;
    robot.pose.orientation = data.orientation;
});
/*
const posBuffer = [null, null, null];
let posBufferIndex = 0;
const stable = botPose.pipe(
    rxjs.operators.map(data => {
        posBuffer[posBufferIndex % posBuffer.length] = data;
        posBufferIndex++;
        return posBuffer;
    }),
    rxjs.operators.map(win => {
        let avg = { x: 0, y: 0, phi: 0 };
        win.forEach(p => {
            if (p) {
                avg.x = avg.x + p.position.x;
                avg.y = avg.y + p.position.y;
                avg.phi = avg.phi + p.orientation;
            } else {
                avg.x = NaN;
                avg.y = NaN;
                avg.phi = NaN;
            }
        });
        avg.x = avg.x / win.length;
        avg.y = avg.y / win.length;
        avg.phi = avg.phi / win.length;
        //console.log(`avg: ${avg.x} / ${avg.y} @ ${avg.phi}`);
        if (isNaN(avg.x) || isNaN(avg.y) || isNaN(avg.phi) || (avg.x === 0 && avg.y === 0 && avg.phi === 0)) {
            return false;
        }
        let err = { x: 0, y: 0, phi: 0 };
        win.forEach(p => {
            err.x += ((p.position.x - avg.x) * 100) ** 2;
            err.y += ((p.position.y - avg.y) * 100) ** 2;
            err.phi += (p.orientation - avg.phi) ** 2;
        });
        //console.log(`err: ${err.x} / ${err.y} @ ${err.phi}`);
        if (err.x > 0.1 || err.y > 0.1 || err.y > 5) {
            return false;
        }

        return true;
    })
);

stable.subscribe(stable => {
    console.log("stable", stable);
});*/
let pathPoints = [];
lidarData.subscribe(message => {
    ctx.clearRect(0, 0, ctx.canvas.width, ctx.canvas.height);

    mapRenderer.render(ctx);

    const points = message.ranges.map((r, i) => {
        if (r === null) {
            return null;
        }
        const phi = message.angle_min + i * message.angle_increment;
        const obstacle = robot.pose.apply(Vector.fromAngle(phi, r));

        if (true) {
            const mapX = Math.floor(obstacle.x / mapResolution) + map.width / 2;
            const mapY = Math.floor(-obstacle.y / mapResolution) + map.width / 2;
            if (mapX >= 0 && mapX < 20 && mapY >= 0 && mapY < 20) {
                const v = map.at(mapX, mapY);
                if (v < 100) {
                    map.set(mapX, mapY, v + 1);
                }
            }
        }

        return obstacle;
    }).filter(p => p !== null);

    cloud.render(ctx, points);
    path.render(ctx, pathPoints)
    robot.render(ctx);
});

model.onGoTo = (x, y) => {
    const alg = new AStar();
    const mapX = Math.floor(robot.pose.position.x / mapResolution) + map.width / 2;
    const mapY = Math.floor(-robot.pose.position.y / mapResolution) + map.width / 2;
    
    pathPoints = alg.find(map, new Vector(mapX, mapY), new Vector(x, y));
    console.log("path points", pathPoints);
};
