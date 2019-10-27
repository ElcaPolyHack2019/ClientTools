import {
    RosMaster,
} from '../../lib/ros.js';

const config = window.config;

const ros = new RosMaster(config.hostname, config.port);

const imageData = ros.observable({
    name : config.topic,
    messageType : 'sensor_msgs/CompressedImage',
    queue_size: 1,
    queue_length: 1,
    throttle_rate: 2000,
}).pipe(rxjs.operators.tap(() => console.log("img")));

const image = document.getElementById("image");

function renderImage(message) {
    console.log('Received message', message);

    const byteCharacters = atob(message.data);
    const byteNumbers = new Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
    }

    const byteArray = new Uint8Array(byteNumbers);
    const blob = new Blob([byteArray], {type: "image/jpeg"});
    
    const uri = URL.createObjectURL(blob);
    image.src = uri;
};

let stopSignal = new rxjs.Subject();

window.model = {
    update: () => {
        imageData.pipe(rxjs.operators.take(1)).subscribe((message) => {
            renderImage(message);
        });
    },
    start: () => {
        imageData.pipe(rxjs.operators.takeUntil(stopSignal)).subscribe((message) => {
            renderImage(message);
        });
    },
    stop: () => {
        stopSignal.next();
    }
};
