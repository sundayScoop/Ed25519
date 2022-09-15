using System;
using System.Numerics;
using System.Security.Cryptography;
using Tide.Math.Ed;

namespace Tide.Encryption.Ed
{
	readonly struct Key
	{
		readonly public BigInteger Priv { get; }
		readonly public Point Pub { get; }

		public Key()
		{
			Priv = Mod(new BigInteger(RandomNumberGenerator.GetBytes(32)));
			Pub = Curve.G * Priv;
        }
		public Key(BigInteger priv, Point pub)
		{
			Priv = priv;
			Pub = pub;
		}
		public Key(Point pub)
		{
			Priv = BigInteger.MinusOne;  // When key is initialized without private key
			Pub = pub;
		}

		public static Key Private(BigInteger priv, bool noPublic = false) =>
			new Key(priv, noPublic ? Curve.G * Curve.N : Curve.G * priv);

        private static BigInteger Mod(BigInteger a)
        {
            BigInteger res = a % Curve.N;
            return res >= BigInteger.Zero ? res : Curve.N + res;
        }
    }
}
