import {
    RosMaster
} from '../../lib/ros.js';

const config = window.config;

const ros = new RosMaster(config.hostname, config.port);

const lidarData = ros.observable({
    name : '/scan',
    messageType : 'sensor_msgs/LaserScan'
});

let samples = 0;
let data = [];
lidarData.subscribe((message) => {
    if (samples % 10 === 0) {
        console.log('Recording... ', samples);
    }

    data.push(message);
    samples++;
});

window.model = {
    save: () => {
        window.lidarData = JSON.stringify(data);
        console.log(window.lidarData);
    }
}
