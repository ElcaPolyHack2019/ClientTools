import {
    Vector,
} from '../../lib/geometry.js';
import {
    PointCloud,
    Robot,
} from '../../lib/drawing.js';
import {
    observableTopic,
    RosMaster
} from '../../lib/ros.js';

const config = window.config;

const ros = new RosMaster(config.hostname, config.port);

const lidarData = ros.observable({
    name : '/scan',
    messageType : 'sensor_msgs/LaserScan',
    throttle_rate: 500,
    queue_size: 1,
    queue_length: 1,
});

const botData = rxjs.interval(1000).pipe(
    rxjs.operators.flatMap(_ => {
        return rxjs.from(fetch('http://192.168.1.164:5000/api/elcaduck', {
            mode: 'cors',
        }).then(response => {
            return response.json();
        }));
    })
);

const pos = document.getElementById('position');
const canvas = document.getElementById('canvas');
canvas.width = 1920;
canvas.height = 1080;
const ctx = canvas.getContext('2d');
window.ctx = ctx;

const robot = new Robot(config.bot.pose, config.canvas.transform);

const cloud = new PointCloud('red', x => {
    return config.canvas.transform(robot.pose.apply(x));
});

botData.pipe(/*rxjs.operators.take(10)*/).subscribe(data => {
    pos.innerText = `${data.gps_x} / ${data.gps_y} @ ${data.gps_orientation}`;

    robot.pose.position = new Vector(data.gps_x, data.gps_y);
    robot.pose.orientation = data.gps_orientation_rad;
});

lidarData/*.pipe(rxjs.operators.take(100))*/.subscribe((message) => {
    ctx.clearRect(0, 0, ctx.canvas.width, ctx.canvas.height);

    const points = message.ranges.map((r, i) => {
        const phi = message.angle_min + i * message.angle_increment;
        return Vector.fromAngle(phi, r);
    });

    cloud.render(ctx, points);
    robot.render(ctx);
});
