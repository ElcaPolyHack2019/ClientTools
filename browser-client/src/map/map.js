import {
    Map,
} from '../../lib/geometry.js';
import {
    MapRenderer,
} from '../../lib/drawing.js';

const config = window.config;

var ros = new ROSLIB.Ros({
    url : `ws://${config.hostname}:${config.port}`
});

ros.on('connection', function() {
    console.log('Connected to websocket server.');
});

ros.on('error', function(error) {
    console.log('Error connecting to websocket server: ', error);
});

ros.on('close', function() {
    console.log('Connection to websocket server closed.');
});

const mapTopic = new ROSLIB.Topic({
    ros : ros,
    name : '/mapstate',
    // http://docs.ros.org/api/nav_msgs/html/msg/OccupancyGrid.html
    messageType : 'nav_msgs/OccupancyGrid'
});

const mapData = rxjs.Observable.create(function(observer) {
    mapTopic.subscribe(data => {
        observer.next(data);
    });
});

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
