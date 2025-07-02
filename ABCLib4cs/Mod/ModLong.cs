using ABCLib4cs.Util;

namespace ABCLib4cs.Mod;

/// <summary>
/// Represents a modular integer (residue class) for a given modulus.
/// This struct is immutable.
/// </summary>
public readonly struct ModLong : IEquatable<ModLong>, IComparable<ModLong>, IComparable
{
    /// <summary>
    /// The modulus used for calculations. Defaults to 998244353.
    /// </summary>
    public static long Mod { get; set; } = Const.Mod998244353;

    /// <summary>
    /// The integer value, which is between 0 and Mod-1.
    /// </summary>
    public readonly long Value;

    private ModLong(long value)
    {
        Value = ModValue(value);
    }

    /// <summary>
    /// Creates an instance of ModLong from a long value.
    /// </summary>
    /// <param name="value">The original value.</param>
    /// <returns>The created ModLong instance.</returns>
    public static ModLong Of(long value)
    {
        return new ModLong(value);
    }

    /// <summary>
    /// Converts a value to its positive remainder modulo Mod.
    /// </summary>
    private static long ModValue(long value)
    {
        long r = value % Mod;
        return r < 0 ? r + Mod : r;
    }

    #region Arithmetic Operators

    public static ModLong operator +(ModLong a, ModLong b) => new(a.Value + b.Value);
    public static ModLong operator +(ModLong a, long b) => new(a.Value + b);
    public static ModLong operator +(long a, ModLong b) => new(a + b.Value);

    public static ModLong operator -(ModLong a, ModLong b) => new(a.Value - b.Value);
    public static ModLong operator -(ModLong a, long b) => new(a.Value - b);
    public static ModLong operator -(long a, ModLong b) => new(a - b.Value);

    public static ModLong operator *(ModLong a, ModLong b) => new(a.Value * b.Value);
    public static ModLong operator *(ModLong a, long b) => new(a.Value * b);
    public static ModLong operator *(long a, ModLong b) => new(a * b.Value);

    public static ModLong operator /(ModLong a, ModLong b) => new(a.Value * Inv(b.Value));
    public static ModLong operator /(ModLong a, long b) => new(a.Value * Inv(b));
    public static ModLong operator /(long a, ModLong b) => new(a * Inv(b.Value));

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

    /// <summary>
    /// Compares this instance to another ModLong object and returns an integer that indicates their relative values.
    /// </summary>
    public int CompareTo(ModLong other)
    {
        return Value.CompareTo(other.Value);
    }

    /// <summary>
    /// Compares this instance to a specified object and returns an integer that indicates their relative values.
    /// </summary>
    public int CompareTo(object obj)
    {
        if (obj is ModLong other)
        {
            return CompareTo(other);
        }
        // Throw an exception because it cannot be compared with other types.
        throw new ArgumentException($"Object must be of type {nameof(ModLong)}");
    }

    public static bool operator <(ModLong left, ModLong right) => left.CompareTo(right) < 0;
    public static bool operator <=(ModLong left, ModLong right) => left.CompareTo(right) <= 0;
    public static bool operator >(ModLong left, ModLong right) => left.CompareTo(right) > 0;
    public static bool operator >=(ModLong left, ModLong right) => left.CompareTo(right) >= 0;

    #endregion
    
    /// <summary>
    /// Calculates the modular multiplicative inverse using the extended Euclidean algorithm.
    /// </summary>
    public static long Inv(long a)
    {
        long b = Mod, u = 1, v = 0;
        long tmp;
        a = ModValue(a);

        while (b > 0)
        {
            long t = a / b;
            a -= t * b;
            tmp = a; a = b; b = tmp;
            u -= t * v;
            tmp = u; u = v; v = tmp;
        }
        
        u %= Mod;
        if (u < 0) u += Mod;
        return u;
    }

    public override string ToString() => Value.ToString();
}