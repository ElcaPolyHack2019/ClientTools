import {
    RosMaster
} from '../../lib/ros.js';

const config = window.config;
const ros = new RosMaster('localhost', 9090);

const gpsData = ros.observable({
    name : '/tag_detections',
    messageType : 'apriltag_ros/AprilTagDetectionArray'
});

gpsData.pipe(rxjs.operators.take(100)).subscribe((message) => {
    console.log('Received message on ', message);
});
