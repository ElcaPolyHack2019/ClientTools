import { Vector, Map } from "../../lib/geometry.js";

const neighbors = [
    new Vector(-1, -1),
    new Vector(0, -1),
    new Vector(1, -1),
    new Vector(-1, 0),
    new Vector(1, 0),
    new Vector(-1, 1),
    new Vector(0, 1),
    new Vector(1, 1),
];

export class AStar {
    find(map, current, target) {
        this._map = new Map(map.width, map.height, null);

        const node = {
            position: current,
            visited: false,
            previous: null,
            cost: 0,
            distance: current.distance(target),
        }
        this._map.set(current.x, current.y, node);
        this._openSet = [node];

        let path = null;
        const safety = map.width * map.height;
        while (path === null && this._openSet.length > 0) {
            path = this.iterate(map, target);
            if (this._openSet.length > safety) {
                console.log(this._map);
                throw new Error("nope");
            }
        }
        return path;
    }

    iterate(map, target) {
        this._openSet.sort((a, b) => (a.cost + a.distance) - (b.cost + b.distance));
        const current = this._openSet.shift();
        current.visited = true;
        current.open = false;

        if (current.position.x === target.x && current.position.y === target.y) {
            return this.traceback(current);
        }

        for (let n of neighbors) {
            const p = current.position.add(n);
            if (p.x < 0 || p.x >= map.width || p.y < 0 || p.y >= map.height) {
                continue;
            }
            const node = {
                position: p,
                visited: false,
                previous: current,
                cost: current.cost + current.position.distance(p) + map.at(p.x, p.y) / 10.0,
                distance: p.distance(target),
            };
            let oldNode = this._map.at(p.x, p.y);
            if (oldNode === null) {
                this._map.set(p.x, p.y, node);
                oldNode = node;
            } else if (!oldNode.visited && oldNode.cost > node.cost) {
                Object.assign(oldNode, node);
            }

            if (!(oldNode.open || oldNode.visited)) {
                oldNode.open = true;
                this._openSet.push(oldNode);
            }
        }

        return null;
    }

    traceback(node) {
        console.log("traceback", node, this._map);
        const path = [];
        while (node) {
            path.unshift(node.position);
            node = node.previous;
        }
        return path;
    }
}