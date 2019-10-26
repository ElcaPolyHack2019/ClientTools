export function observableTopic(topic) {
    return rxjs.Observable.create((observer) => {
        topic.subscribe(data => {
            observer.next(data);
        });
    });
}

export class RosMaster {
    constructor(host, port) {
        this.ros = new ROSLIB.Ros({
            url : `ws://${host}:${port}`
        });
        this.topics = [];

        this.ros.on('connection', () => {
            console.log('Connected to websocket server.');
        });
        
        this.ros.on('error', (error) => {
            console.log('Error connecting to websocket server: ', error);
        });
        
        this.ros.on('close', () => {
            console.log('Connection to websocket server closed.');
        });

        window.onunload = () => {
            console.log('Unloading...');
            for (let t of this.topics) {
                console.log(t.name);
                t.unsubscribe();
                t.unadvertise();
            }
            this.ros.close();
            console.log('done');
        }
    }

    topic(topicConfig) {
        const config = Object.assign({ ros: this.ros }, topicConfig);
        const topic = new ROSLIB.Topic(config);
        this.topics.push(topic);
        return topic;
    }

    observable(topicConfig) {
        return observableTopic(this.topic(topicConfig));
    }
}
