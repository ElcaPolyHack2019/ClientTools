import {
    Map,
} from '../../lib/geometry.js';
import {
    MapRenderer,
} from '../../lib/drawing.js';
import {
    observableTopic,
    RosMaster
} from '../../lib/ros.js';

const config = window.config;

const ros = new RosMaster(config.hostname, config.port);

const mapTopic = ros.topic({
    name : '/mapstate',
    // http://docs.ros.org/api/nav_msgs/html/msg/OccupancyGrid.html
    messageType : 'nav_msgs/OccupancyGrid'
});

const mapData = observableTopic(mapTopic);

const canvas = document.getElementById('canvas');
canvas.width = 1920;
canvas.height = 1080;
const ctx = canvas.getContext('2d');
window.ctx = ctx;

mapData.subscribe((message) => {
    console.log('Received message on ' + message.header.seq, message);

    ctx.clearRect(0, 0, ctx.canvas.width, ctx.canvas.height);

    const map = new Map(message.info.width, message.info.height, message.data);
    const mapRenderer = new MapRenderer(map, config.map.tileSize, config.map.transform);

    mapRenderer.render(ctx);
});

window.model = {
    loadDummyMap: () => {
        var msg = new ROSLIB.Message({
            info: {
                width: 2,
                height: 2,
            },
            data: [0, 25, 50, 75],
        });
        mapTopic.publish(msg);
    }
};
