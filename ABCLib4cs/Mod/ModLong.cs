using ABCLib4cs.Util;

namespace ABCLib4cs.Mod;

public readonly struct ModLong : IEquatable<ModLong>, IComparable<ModLong>, IComparable
{
    public static long Mod { get; set; } = Const.Mod998244353;
    
    public readonly long Value;

    private ModLong(long value)
    {
        Value = ModValue(value);
    }
    
    public static ModLong Of(long value)
    {
        return new ModLong(value);
    }
    
    private static long ModValue(long value)
    {
        long r = value % Mod;
        return r < 0 ? r + Mod : r;
    }

    private static long ModValue(long value, long mod)
    {
        long r = value % mod;
        return r < 0 ? r + mod : r;
    }

    #region Arithmetic Operators

    public static ModLong operator +(ModLong a, ModLong b) => new(a.Value + b.Value);
    public static ModLong operator +(ModLong a, long b) => new(a.Value + ModValue(b));
    public static ModLong operator +(long a, ModLong b) => new(ModValue(a) + b.Value);

    public static ModLong operator -(ModLong a, ModLong b) => new(a.Value - b.Value);
    public static ModLong operator -(ModLong a, long b) => new(a.Value - ModValue(b));
    public static ModLong operator -(long a, ModLong b) => new(ModValue(a) - b.Value);

    public static ModLong operator *(ModLong a, ModLong b) => new(a.Value * b.Value);
    public static ModLong operator *(ModLong a, long b) => new(a.Value * ModValue(b));
    public static ModLong operator *(long a, ModLong b) => new(ModValue(a) * b.Value);

    public static ModLong operator /(ModLong a, ModLong b) => new(a.Value * Inv(b.Value, Mod));
    public static ModLong operator /(ModLong a, long b) => new(a.Value * Inv(b, Mod));
    public static ModLong operator /(long a, ModLong b) => new(ModValue(a) * Inv(b.Value, Mod));

    public static ModLong operator +(ModLong a) => a;
    public static ModLong operator -(ModLong a) => new(-a.Value);

    #endregion

    #region Type Conversions

    public static implicit operator ModLong(long value) => new(value);
    public static explicit operator long(ModLong modLong) => modLong.Value;

    #endregion

    #region Equality

    public override bool Equals(object obj) => obj is ModLong other && Equals(other);
    public bool Equals(ModLong other) => Value == other.Value;
    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(ModLong left, ModLong right) => left.Equals(right);
    public static bool operator !=(ModLong left, ModLong right) => !(left == right);

    #endregion
    
    #region Comparison
    
    public int CompareTo(ModLong other)
    {
        return Value.CompareTo(other.Value);
    }
    
    public int CompareTo(object obj)
    {
        if (obj is ModLong other)
        {
            return CompareTo(other);
        }
        throw new ArgumentException($"Object must be of type {nameof(ModLong)}");
    }

    public static bool operator <(ModLong left, ModLong right) => left.CompareTo(right) < 0;
    public static bool operator <=(ModLong left, ModLong right) => left.CompareTo(right) <= 0;
    public static bool operator >(ModLong left, ModLong right) => left.CompareTo(right) > 0;
    public static bool operator >=(ModLong left, ModLong right) => left.CompareTo(right) >= 0;

    #endregion
    
    public static long Inv(long a, long mod)
    {
        long b = mod, u = 1, v = 0;
        long tmp;
        a = ModValue(a, mod);

        while (b > 0)
        {
            long t = a / b;
            a -= t * b;
            tmp = a; a = b; b = tmp;
            u -= t * v;
            tmp = u; u = v; v = tmp;
        }
        
        u %= mod;
        if (u < 0) u += mod;
        return u;
    }

    public ModLong Exp(long exp)
    {
        var result = Of(1);
        var baseValue = this;
        while (exp > 0)
        {
            if ((exp & 1) == 1)
            {
                result *= baseValue;
            }
            baseValue *= baseValue;
            exp >>= 1;
        }
        return result;
    }

    public static long Log(ModLong a, ModLong b)
    {
        var q = (long) Mazz.SqrtCeiling((ulong)Mod);
        var babySteps = new SortedDictionary<ModLong, long>();
        var right = b;
        for (long i = 0; i < q; i++)
        {
            babySteps.TryAdd(right, i);
            right /= a;
        }

        var giantStep = a.Exp(q);
        var left = ModLong.Of(1);
        for (int i = 0; i < q; i++)
        {
            if (babySteps.TryGetValue(left, out long j))
            {
                return i * q + j;
            }

            left *= giantStep;
        }

        return -1;
    }

    public override string ToString() => Value.ToString();
}