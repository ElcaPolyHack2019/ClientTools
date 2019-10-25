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
    messageType : 'sensor_msgs/LaserScan'
});

const botData = rxjs.interval(1000).pipe(
    rxjs.operators.flatMap(_ => {
        return rxjs.from(fetch('http://localhost:5000/api/elcaduck', {
            mode: 'cors',
        }).then(response => {
            return response.json();
        }));
    })
);

const canvas = document.getElementById('canvas');
canvas.width = 1920;
canvas.height = 1080;
const ctx = canvas.getContext('2d');
window.ctx = ctx;

const robot = new Robot(config.bot.pose, config.bot.transform);

const cloud = new PointCloud('red', x => {
    return config.bot.transform(x.rotate(robot.pose.orientation));
});

botData.pipe(/*rxjs.operators.take(10)*/).subscribe(data => {
    console.log('Robot data', data);

    robot.pose.position = new Vector(data.gps_x, data.gps_y);
    robot.pose.orientation = data.gps_orientation_rad;
});

lidarData/*.pipe(rxjs.operators.take(100))*/.subscribe((message) => {
    //console.log('Received message on ' + message.header.seq, message);

    ctx.clearRect(0, 0, ctx.canvas.width, ctx.canvas.height);

    const points = message.ranges.map((r, i) => {
        const phi = i * message.angle_increment + message.angle_min;
        return Vector.fromAngle(phi, r).add(config.bot.pose.position);
    });

    cloud.render(ctx, points);
    robot.render(ctx);
});
