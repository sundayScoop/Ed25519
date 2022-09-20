import { randomBytes } from 'crypto';
import BI from 'big-integer';



const _0n = BigInt(0);
const _1n = BigInt(1);
const _2n = BigInt(2);

class Point {

    /**
     * 
     * @param {BigInt.BigInteger} x 
     * @param {BigInt.BigInteger} y 
     * @param {BigInt.BigInteger} z 
     * @param {BigInt.BigInteger} t 
     */
    constructor(x, y, z=null, t=null) {
        this.x = x;
        this.y = y;

        if(z === null) this.z = _1n;
        else this.z = z;

        if(t === null) this.t = mod(x*y);
        else this.t = t;
    }
    

    static a = BigInt(-1);
    static d = BigInt("37095705934669439343138083508754565189542113879843219016388785533085940283555");
    static m =  BigInt("57896044618658097711785492504343953926634992332820282019728792003956564819949");
    static n = BigInt("7237005577332262213973186563042994240857116359379907606001950938285454250989");
    static g = new Point(
        BigInt("15112221349535400772501151409588531511454012693041857206046113283949847762202"),
        BigInt("46316835694926478169428394003475163141307993866256225615783033603165251855960"), 
        _1n, 
        BigInt("46827403850823179245072216630277197565144205554125654976674165829533817101731")
    );
    static infinity = new Point(_0n, _1n, _1n, _0n); //infinity also known as identity for ed25519

    isInfinity(){
        return this.isEqual(Point.infinity);  // TODO: Check if  t needs check too.
    }

    isEqual(other){
        const X1Z2 = mod(this.x * other.z);
        const X2Z1 = mod(other.x * this.z);
        const Y1Z2 = mod(this.y * other.z);
        const Y2Z1 = mod(other.y * this.z);
        return X1Z2 === X2Z1 && Y1Z2 === Y2Z1;
    }

    negate(){
        return new Point(mod(-this.x), this.y, this.z, mod(-this.t)); //// not checked if works
    }

    

    // Using double and add algorithm.
    multiply(num) {
        var point = this;
        let newPoint = new Point(_0n, _1n, _1n, _0n); // identity aka infinity
        while (num > _0n) {
            if ((num & _1n) === (_1n)) {
                newPoint = newPoint.add(point);
            }
            point = point.double();
            num = num >> _1n;
        }
        return newPoint;
    }

    double() {
        let A = mod(this.x * this.x);
        let B = mod(this.y * this.y);
        let C = mod(_2n * mod(this.z * this.z));
        let D = mod(Point.a * A);
        let x1y1 = this.x + this.y;
        let E = mod(mod(x1y1 * x1y1) - A - B);
        let G = D + B;
        let F = G - C;
        let H = D - B;
        let X3 = mod(E * F);
        let Y3 = mod(G * H);
        let T3 = mod(E * H);
        let Z3 = mod(F * G);
        return new Point(X3, Y3, Z3, T3);
    }

    add(other) {
        let A = mod((this.y - this.x) * (other.y + other.x));
        let B = mod((this.y + this.x) * (other.y - other.x));
        let F = mod(B - A);
        if (F == _0n) return this.double();
        let C = mod(this.z * _2n * other.t);
        let D = mod(this.t * _2n * other.z);
        let E = D + C;
        let G = B + A;
        let H = D - C;
        let X3 = mod(E * F);
        let Y3 = mod(G * H);
        let T3 = mod(E * H);
        let Z3 = mod(F * G);
        return new Point(X3, Y3, Z3, T3);
    }

    // Notes on getX and getY for next dev who runs by this.
    // From what I found, using the native BigInt lib is MUCH easier (and faster apparently)
    // to perform all the maths above compared to big-integer lib.
    // Unfortunately, most of the signing operations in cryptide 
    // use big-integer as it's easier to convert to bytes etc.
    // So its important to stay consistent and use big-integer everhwere.
    // This is why the getX returns a big-integer object, converted
    // from a BigInt object.
    getX(){
        return BI(mod(this.x * mod_inv(this.z)).toString());
    }
    getY(){
        return BI(mod(this.y * mod_inv(this.z)).toString());
    }
    /**
     * @param {Uint8Array} data
     */
    static from(data){ 
        var x = BigInt(BI.fromArray(Array.from(data.slice(0, 32)), 256, false).toString()); 
        var y = BigInt(BI.fromArray(Array.from(data.slice(32, 64)), 256, false).toString());  
        return new Point(x, y);
    }

    /** @returns {Uint8Array} */
    toArray(){
        var r_xBuff = this.getX().toArray(256).value;
        while(r_xBuff.length < 32){r_xBuff.unshift(0);} // pad if array < 32
    
        var r_yBuff = this.getY().toArray(256).value;
        while(r_yBuff.length < 32){r_yBuff.unshift(0);} // pad if array < 32
        var a =  new Uint8Array(r_xBuff.concat(r_yBuff));
        console.log(a);
        return new Uint8Array(r_xBuff.concat(r_yBuff));
    }

    static fromString(message){
        var x = BigInt(BI.fromArray(Array.from(Hash.shaBuffer(message)), 256, false).toString()); 

    }


}

function mod(a, b = Point.m) {
    var res = a % b;
    return res >= _0n ? res : b + res;
}

function mod_inv(number, modulo = Point.m) {
    if (number === _0n || modulo <= _0n) {
        throw new Error(`invert: expected positive integers, got n=${number} mod=${modulo}`);
    }
    let a = mod(number, modulo);
    let b = modulo;
    // prettier-ignore
    let x = _0n, y = _1n, u = _1n, v = _0n;
    while (a !== _0n) {
        const q = b / a;
        const r = b % a;
        const m = x - u * q;
        const n = y - v * q;
        // prettier-ignore
        b = a, a = r, x = u, y = v, u = m, v = n;
    }
    const gcd = b;
    if (gcd !== _1n) throw new Error('invert: does not exist');
    return mod(x, modulo);
}

let i = 0;
let start = Date.now();
while (i < 10) {
    let r_b = randomBytes(32);
    let h_b = randomBytes(64);
    let priv_b = randomBytes(32);

    // Bytes to numbers.
    let priv = mod(BigInt(BI.fromArray(Array.from(priv_b), 256, false)));
    let r = mod(BigInt(BI.fromArray(Array.from(r_b), 256, false)));
    let h = mod(BigInt(BI.fromArray(Array.from(h_b), 256, false)));

    let R = Point.g.multiply(r);
    let s = mod(r + (h * priv), Point.n);
    let pub = Point.g.multiply(priv);

    //Verifying
    let point1 = Point.g.multiply(s);
    let point2 = R.add(pub.multiply(h));

    console.log(point1.isEqual(point2));
    i++;
}
let end = Date.now();
console.log(`Execution time: ${end - start} ms`);

var p = Point.g.multiply(BigInt(5));
var data = p.toArray();
var new_p = Point.from(data);

console.log(new_p.isEqual(p));
console.log(new_p.getX().toString());
console.log(p.getX().toString());
//console.log(p.toArray());


/// TODO, figure out x from y ed25519
