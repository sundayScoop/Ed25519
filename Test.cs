using Tide.Math.Ed;
using System.Numerics;
using System.Security.Cryptography;

class Test
{
    static void Main(string[] args)
    {
        ref readonly var G = ref Curve.G;
        int i = 0;
        var watch = System.Diagnostics.Stopwatch.StartNew();
        while (i < 1000)
        {
            // random bytes for h, priv and r
            byte[] priv1_b = RandomNumberGenerator.GetBytes(32);
            byte[] priv2_b = RandomNumberGenerator.GetBytes(64);

            // bytes to numbers
            BigInteger priv1 = Mod(new BigInteger(priv1_b)); // Mod N to ensure size constraint
            BigInteger priv2 = Mod(new BigInteger(priv2_b));

            // Alice creates her public key
            var pub1 = Curve.G * priv1;

            // Bob creates his public key + sends it
            var pub2 = Curve.G * priv2;

            // Alice creates shared priv key from pub2 and her private
            var shared1 = pub2 * priv1;

            // Bob does the same
            //var shared2 = pub1 * priv2;

            // Check they have the same shared key
            //bool valid = shared1.GetX().Equals(shared2.GetX());

            i += 1;
        }
        watch.Stop();
        Console.WriteLine($"Time: {watch.ElapsedMilliseconds}");
    }
    private static BigInteger Mod(BigInteger a)
    {
        BigInteger res = a % Curve.N;
        return res >= BigInteger.Zero ? res : Curve.N + res;
    }

}
