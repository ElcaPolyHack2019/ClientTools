import {
    RosMaster,
} from '../../lib/ros.js';

const config = window.config;

const ros = new RosMaster(config.hostname, config.port);

const imageData = ros.observable({
    name : config.topic,
    messageType : 'sensor_msgs/CompressedImage'
});

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

window.model = {
    update: () => {
        imageData.pipe(rxjs.operators.take(1)).subscribe((message) => {
            renderImage(message);
        });
    }
};

window.model.update();
