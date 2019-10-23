export class RosMaster {
    constructor(host, port) {
        this.ros = new ROSLIB.Ros({
            url : `ws://${host}:${port}`
        });

        this.ros.on('connection', () => {
            console.log('Connected to websocket server.');
        });
        
        this.ros.on('error', (error) => {
            console.log('Error connecting to websocket server: ', error);
        });
        
        this.ros.on('close', () => {
            console.log('Connection to websocket server closed.');
        });
    }

    topic(topicConfig) {
        const config = Object.assign({ ros: this.ros }, topicConfig);
        return new ROSLIB.Topic(config);
    }

    observable(topicConfig) {
        return rxjs.Observable.create((observer) => {
            this.topic(topicConfig).subscribe(data => {
                observer.next(data);
            });
        });
    }
}
