
export class Vector {
    constructor(x, y) {
        this._x = x;
        this._y = y;
    }

    get x() {
        return this._x;
    }

    get y() {
        return this._y;
    }

    add(other) {
        return new Vector(this.x + other.x, this.y + other.y);
    }

    scale(factor) {
        return new Vector(factor * this.x, factor * this.y);
    }

    rotate(phi) {
        return new Vector(
            this.x * Math.cos(phi) - this.y * Math.sin(phi),
            this.x * Math.sin(phi) + this.y * Math.cos(phi)
        );
    }

    invertX() {
        return new Vector(-this.x, this.y);
    }

    invertY() {
        return new Vector(this.x, -this.y);
    }
    invert() {
        return new Vector(-this.x, -this.y);
    }

    distance(other) {
        return Math.sqrt((other.x - this.x) ** 2 + (other.y - this.y) ** 2);
    }
}

Vector.fromAngle = (phi, magnitude) => {
    return new Vector(magnitude * Math.cos(phi), magnitude * Math.sin(phi));
}


export class Pose {
    constructor(position, orientation) {
        this.position = position;
        this.orientation = orientation;
    }
    apply(vector) {
        return vector.rotate(this.orientation).add(this.position);
    }
    unapply(vector) {
        return vector.rotate(-this.orientation).add(this.position.invert());
    }
}


export class Map {
    constructor(width, height, initial) {
        this._width = width;
        this._height = height;

        if (initial !== null && initial.length) {
            this._data = initial;
        } else {
            this._data = [];
            for (let y = 0; y < this._height; y++) {
                for (let x = 0; x < this._width; x++) {
                    this._data.push(initial);
                }
            }
        }
    }

    get width() {
        return this._width;
    }

    get height() {
        return this._height;
    }

    at(x, y) {
        if (x < 0 || x >= this._width || y < 0 || y >= this._height) {
            throw new Error('Map access out of bounds');
        }

        return this._data[y * this._width + x];
    }

    set(x, y, value) {
        if (x >= this._width || y >= this._height) {
            throw new Error('Map access out of bounds');
        }

        this._data[y * this._width + x] = value;
    }

    each(action) {
        for (let y = 0; y < this._height; y++) {
            for (let x = 0; x < this._width; x++) {
                action(this.at(x,y), x, y);
            }
        }
    }
}