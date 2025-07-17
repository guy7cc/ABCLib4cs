using ABCLib4cs.Mod;

namespace ABCLib4cs.Util;

public static class Mazz
{
    public static long Pow(this long x, long n)
    {
        long result = 1;
        while (n > 0)
        {
            if ((n & 1) == 1) result *= x;
            x *= x;
            n >>= 1;
        }
        return result;
    }
    
    public static ulong SqrtFloor(ulong n)
    {
        if (n == 0) return 0; 
        ulong t = (ulong)Math.Sqrt(n);
        return (t - 1) * (t + 1) < n ? t : t - 1;
    }

    public static ulong SqrtCeiling(ulong n)
    {
        if (n == 0) return 0;
        ulong t = (ulong)Math.Sqrt(n);
        return (t - 1) * (t + 1) < n - 1 ? t + 1 : t;
    }
    
    public static long GCD(long a, long b)
    {
        if (b == 0) return a;
        return GCD(b, a % b);
    }
    
    /// <summary>
    ///  拡張ユークリッドの互除法
    ///  ap + bq = gcd(a, b)
    /// </summary>
    public static long ExtendedGCD(long a, long b, out long p, out long q)
    {
        if (b == 0)
        {
            p = 1;
            q = 0;
            return a;
        }

        long gcd = ExtendedGCD(b, a % b, out q, out p);
        q -= (a / b) * p;
        return gcd;
    }
    
    /// <summary>
    ///  LinearFloorSum(n, m, a, b) = Σ_{i=0}^{n-1} ⌊(ai + b) / m⌋
    /// </summary>
    public static long LinearFloorSum(long n, long m, long a, long b)
    {
        if (n == 0)
        {
            return 0;
        }

        long aDivM = Math.Sign(a) * Math.Abs(a) / m;
        long aModM = Math.Sign(a) * Math.Abs(a) % m;
        if (a < 0 && aModM != 0) {
            aDivM--;
            aModM += m;
        }
        
        long bDivM = Math.Sign(b) * Math.Abs(b) / m;
        long bModM = Math.Sign(b) * Math.Abs(b) % m;
        if (b < 0 && bModM != 0) {
            bDivM--;
            bModM += m;
        }

        long s = n * (n - 1) / 2 * aDivM;

        if (aModM == 0)
        {
            return s + bDivM * n;
        }

        long k = (aModM * (n - 1) + bModM) / m;
        
        return s + n * (k + bDivM) - LinearFloorSum(k, aModM, m, m + aModM - bModM - 1);
    }

    /// <summary>
    ///  FractionFloorSum(N) = Σ_{i=1}^{N-1} ⌊N/i⌋
    /// </summary>
    public static long FractionFloorSum(long N)
    {
        long sum = 0;
        for (long i = 1; i < N;)
        {
            long k = N / i;
            long j = Math.Min(N / k, N - 1);
            var count = j - i + 1;
            sum += count * k;
            i = j + 1;
        }
        return sum;
    }
    
    /// <summary>
    ///  FractionFloorSumMod(N) = Σ_{i=1}^{N-1} ⌊N/i⌋ (mod ModLong.Mod)
    /// </summary>
    public static ModLong FractionFloorSumMod(long N)
    {
        ModLong sum = 0;
        for (long b = 1; b < N;)
        {
            long k = N / b;
            long j = Math.Min(N / k, N - 1);
            var count = ModLong.Of(j) - b + 1;
            sum += count * k;
            b = j + 1;
        }
        return sum;
    }
}