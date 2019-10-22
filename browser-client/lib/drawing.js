import { Vector } from "./geometry.js";

const identityTransform = x => x;

export class MapRenderer {
    constructor(map, tileSize, transform) {
        this.map = map;
        this.tileSize = tileSize;
        this.transform = transform;
    }

    render(ctx) {
        this.map.each((v, x, y) => {
            const hue = 120 - 120 * (v / 100.0);
            ctx.fillStyle = 'hsl(' + hue + ', 100%, 75%)';
            ctx.fillRect(
                x * this.tileSize + 1,
                y * this.tileSize + 1,
                this.tileSize - 2,
                this.tileSize - 2
            );
        });
    }
}

export class PointCloud {
    constructor(color, transform = identityTransform) {
        this.color = color;
        this.transform = transform;
        this.origin = new Vector(0, 0);
    }

    render(ctx, points) {
        ctx.fillStyle = this.color;
        let count = 0;
        for (let point of points) {
            const p = this.transform(point);
            // if (count % 10 === 0) {
            //     this.drawRadial(ctx, p);
            // }
            ctx.fillRect(p.x - 1, p.y - 1, 3, 3);
            count++;
        }
    }

    drawRadial(ctx, dest) {
        const o = this.transform(this.origin);
        ctx.strokeStyle = 'hsl(180, 100%, 85%)';
        ctx.moveTo(o.x, o.y);
        ctx.lineTo(dest.x, dest.y);
        ctx.stroke();
    }
}

export class Robot {
    constructor(pose, transform = identityTransform) {
        this.pose = pose;
        this.transform = transform;
    }

    render(ctx) {
        ctx.fillStyle = 'blue';

        const p = this.transform(this.pose.position);
        const p1 = p.add(new Vector(10, 0).rotate(-this.pose.orientation));
        const p2 = p.add(new Vector(-10, -7).rotate(-this.pose.orientation));
        const p3 = p.add(new Vector(-10, 7).rotate(-this.pose.orientation));
        console.log(p, this.pose.orientation, p1);
        ctx.beginPath();
        ctx.moveTo(p1.x, p1.y);
        ctx.lineTo(p2.x, p2.y);
        ctx.lineTo(p3.x, p3.y);
        ctx.fill();
    }
}