using System.Numerics;
using System.Security.Cryptography;

namespace Tide.Math.Ed
{
    /// <summary>
    /// To represent a point on the Ed25519 Curve.
    /// Coordinates are stored as X, Y, Z, T where the actual x = X/Z, y = Y/Z
    /// </summary>
    readonly struct Point
    {
        readonly private BigInteger X { get; }
        readonly private BigInteger Y { get; }
        readonly private BigInteger Z { get; }
        readonly private BigInteger T { get; }

        /// <summary>
        /// Create a point from extended coordinates. Consider passing only x and y for simpler use.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="t"></param>
        public Point(BigInteger x, BigInteger y, BigInteger z, BigInteger t)
        {
            X = x;
            Y = y;
            Z = z;
            T = t;
        }
        /// <summary>
        /// Create a point from normal coordinates.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Point(BigInteger x, BigInteger y)
        {
            X = x;
            Y = y;
            Z = 1;
            T = Mod(x * y);
        }
        /// <summary>
        /// Performs ( X * modular_inverse(Z) ) % M to get the actual x coordinate.
        /// </summary>
        /// <returns>Returns the actual x coordinate of this point.</returns>
        public BigInteger GetX() => Mod((X * BigInteger.ModPow(Z, Curve.M - 2, Curve.M)));
        /// <summary>
        /// Performs ( Y * modular_inverse(Z) ) % M to get the actual y coordinate.
        /// </summary>
        /// <returns>Returns the actual y coordinate of this point.</returns>
        public BigInteger GetY() => Mod((Y * BigInteger.ModPow(Z, Curve.M - 2, Curve.M)));
        /// <summary>
        /// Multiplies a point by a scalar using double and add algorithm on the Ed25519 Curve.
        /// Does not perform safety checks on scalar or the point, yet.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="num"></param>
        /// <returns>A new point on the Ed25519 Curve.</returns>
        public static Point operator *(Point point, BigInteger num)
        {
            // TODO: Check to see if 0 < num < N
            Point newPoint = new Point(BigInteger.Zero, BigInteger.One, BigInteger.One, BigInteger.Zero);
            while (num > BigInteger.Zero)
            {
                if ((num & BigInteger.One).Equals(BigInteger.One)) newPoint = newPoint + point;
                point = Double(point);
                num = num >> 1;
            }
            return newPoint;
        }
        /// <summary>
        /// Add a point by itself ("double") on the Ed25519 Curve. Currently, does not check if point is on curve or prime order group.
        /// </summary>
        /// <param name="point"></param>
        /// <returns>A new point on the Ed25519 Curve.</returns>
        public static Point Double(in Point point)
        {
            // TODO: check to see if point is on curve and on the prime order subgroup
            // Algorithm taken from https://www.hyperelliptic.org/EFD/g1p/auto-twisted-extended-1.html.

            BigInteger A = Mod(point.X * point.X);
            BigInteger B = Mod(point.Y * point.Y);
            BigInteger C = Mod(Curve.Two * Mod(point.Z * point.Z));
            BigInteger D = Mod(Curve.A * A);
            BigInteger x1y1 = point.X + point.Y;
            BigInteger E = Mod(Mod(x1y1 * x1y1) - A - B);
            BigInteger G = D + B;
            BigInteger F = G - C;
            BigInteger H = D - B;
            BigInteger X3 = Mod(E * F);
            BigInteger Y3 = Mod(G * H);
            BigInteger T3 = Mod(E * H);
            BigInteger Z3 = Mod(F * G);
            return new Point(X3, Y3, Z3, T3);
        }
        /// <summary>
        /// Adds two points on the Ed25519 Curve. Currently, does not check if point is on curve or prime order group.
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns>A new point on the Ed25519 Curve.</returns>
        public static Point operator +(in Point point1, in Point point2)
        {
            // TODO: check to see if point is on curve and on the prime order subgroup
            // Algorithm taken from https://www.hyperelliptic.org/EFD/g1p/auto-twisted-extended-1.html.

            BigInteger A = Mod((point1.Y - point1.X) * (point2.Y + point2.X));
            BigInteger B = Mod((point1.Y + point1.X) * (point2.Y - point2.X));
            BigInteger F = Mod(B - A);
            if (F.Equals(BigInteger.Zero)) return Double(point1);
            BigInteger C = Mod(point1.Z * Curve.Two * point2.T);
            BigInteger D = Mod(point1.T * Curve.Two * point2.Z);
            BigInteger E = D + C;
            BigInteger G = B + A;
            BigInteger H = D - C;
            BigInteger X3 = Mod(E * F);
            BigInteger Y3 = Mod(G * H);
            BigInteger T3 = Mod(E * H);
            BigInteger Z3 = Mod(F * G);
            return new Point(X3, Y3, Z3, T3);
        }
        private static BigInteger Mod(BigInteger a)
        {
            BigInteger res = a % Curve.M;
            return res >= BigInteger.Zero ? res : Curve.M + res;
        }
    }
    readonly ref struct Curve
    {
        private readonly static BigInteger m = BigInteger.Parse("57896044618658097711785492504343953926634992332820282019728792003956564819949");
        private readonly static BigInteger n = BigInteger.Parse("7237005577332262213973186563042994240857116359379907606001950938285454250989");
        private readonly static BigInteger d = BigInteger.Parse("37095705934669439343138083508754565189542113879843219016388785533085940283555");
        private readonly static BigInteger two = BigInteger.Parse("2");
        private readonly static BigInteger minusOne = BigInteger.MinusOne;
        public static ref readonly BigInteger M => ref m;
        public static ref readonly BigInteger N => ref n;
        public static ref readonly BigInteger D => ref d;
        public static ref readonly BigInteger MinusOne => ref minusOne;
        public static ref readonly BigInteger A => ref minusOne;
        public static ref readonly BigInteger Two => ref two;

        private readonly static BigInteger gx = BigInteger.Parse("15112221349535400772501151409588531511454012693041857206046113283949847762202");
        private readonly static BigInteger gy = BigInteger.Parse("46316835694926478169428394003475163141307993866256225615783033603165251855960");
        private readonly static BigInteger gt = BigInteger.Parse("46827403850823179245072216630277197565144205554125654976674165829533817101731");
        private readonly static Point g = new Point(gx, gy, BigInteger.One, gt);
        public static ref readonly Point G => ref g;
    }
}
