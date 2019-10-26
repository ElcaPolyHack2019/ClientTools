import {
    RosMaster
} from '../../lib/ros.js';

const config = window.config;
const host = window.location.search.match(/bot=([A-Za-z0-9]+)/)[1];
console.log("Bot:", host);

const ros = new RosMaster(host.toLowerCase(), config.port);

const joyTopic = ros.topic({
    name : `/${host}/joy`,
    messageType : 'sensor_msgs/Joy'
});

window.onkeypress = (event) => {
    let axes = [0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0];

    switch (event.key) {
        case "w":
            axes = [0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0];
            break;
        case "s":
            axes = [0.0, -1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0];
            break;
        case "a":
            axes = [0.0, 0.0, 0.0, 0.8, 0.0, 0.0, 0.0, 0.0];
            break;
        case "d":
            axes = [0.0, 0.0, 0.0, -0.8, 0.0, 0.0, 0.0, 0.0];
            break;
        case "Space":            
            break;
    }

    console.log("sending", event.key, axes);
    var msg = new ROSLIB.Message({
        buttons: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
        axes: axes,
    });
    joyTopic.publish(msg);
}
