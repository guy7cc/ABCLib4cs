// Inspired by https://qiita.com/Camypaper/items/de6d576fe5513743a50e, to whom I am very grateful.
//
// I waive all copyrights to this code and release it into the public domain.
// Use, modify, and distribute freely.

using System.Globalization;
using System.Text;
using ABCLib4cs.Util;

namespace ABCLib4cs.IO;

public static class Scanner
{
    private static readonly Stream Str = Console.OpenStandardInput();
    private static readonly byte[] Buf = new byte[1024];
    private static int _len, _ptr;
    public static bool IsEndOfStream { get; private set; }

    private static byte Read()
    {
        if (IsEndOfStream) throw new EndOfStreamException();
        if (_ptr >= _len)
        {
            _ptr = 0;
            if ((_len = Str.Read(Buf, 0, 1024)) <= 0)
            {
                IsEndOfStream = true;
                return 0;
            }
        }

        return Buf[_ptr++];
    }

    public static char C()
    {
        byte b = 0;
        do
        {
            b = Read();
        } while (b < 33 || 126 < b);

        return (char)b;
    }

    public static string S()
    {
        var sb = new StringBuilder();
        for (var b = C(); b >= 33 && b <= 126; b = (char)Read())
            sb.Append(b);
        return sb.ToString();
    }

    public static int I()
    {
        return (int)L();
    }

    public static long L()
    {
        long ret = 0;
        byte b = 0;
        var ng = false;
        do
        {
            b = Read();
        } while (b != '-' && (b < '0' || '9' < b));

        if (b == '-')
        {
            ng = true;
            b = Read();
        }

        while (true)
        {
            if (b < '0' || '9' < b)
                return ng ? -ret : ret;
            ret = ret * 10 + b - '0';
            b = Read();
        }
    }

    public static float F()
    {
        return float.Parse(S(), CultureInfo.InvariantCulture);
    }

    public static double D()
    {
        return double.Parse(S(), CultureInfo.InvariantCulture);
    }

    public static decimal Dec()
    {
        return decimal.Parse(S(), CultureInfo.InvariantCulture);
    }

    public static bool B()
    {
        var s = S();
        if (s == "true" || s == "1") return true;
        if (s == "false" || s == "0") return false;
        throw new FormatException("Invalid boolean value: " + s);
    }

    public static (T0, T1) T<T0, T1>(Supplier<T0> sup0, Supplier<T1> sup1)
    {
        return (sup0(), sup1());
    }
    
    public static (T0, T1, T2) T<T0, T1, T2>(Supplier<T0> sup0, Supplier<T1> sup1, Supplier<T2> sup2)
    {
        return (sup0(), sup1(), sup2());
    }
    
    public static (T0, T1, T2, T3) T<T0, T1, T2, T3>(Supplier<T0> sup0, Supplier<T1> sup1, Supplier<T2> sup2, Supplier<T3> sup3)
    {
        return (sup0(), sup1(), sup2(), sup3());
    }

    public static T0[] A<T0>(int size, Supplier<T0> sup0)
    {
        var arr = new T0[size];
        for (var i = 0; i < size; i++) arr[i] = sup0();

        return arr;
    }
    
    public static (T0, T1)[] A<T0, T1>(int size, Supplier<T0> sup0, Supplier<T1> sup1)
    {
        var arr = new (T0, T1)[size];
        for (var i = 0; i < size; i++) arr[i] = (sup0(), sup1());

        return arr;
    }
    
    public static (T0, T1, T2)[] A<T0, T1, T2>(int size, Supplier<T0> sup0, Supplier<T1> sup1, Supplier<T2> sup2)
    {
        var arr = new (T0, T1, T2)[size];
        for (var i = 0; i < size; i++) arr[i] = (sup0(), sup1(), sup2());

        return arr;
    }
    
    public static (T0, T1, T2, T3)[] A<T0, T1, T2, T3>(int size, Supplier<T0> sup0, Supplier<T1> sup1, Supplier<T2> sup2, Supplier<T3> sup3)
    {
        var arr = new (T0, T1, T2, T3)[size];
        for (var i = 0; i < size; i++) arr[i] = (sup0(), sup1(), sup2(), sup3());

        return arr;
    }
    
    public static List<T> L<T>(int size, Supplier<T> sup0)
    {
        var list = new List<T>(size);
        for (var i = 0; i < size; i++) list.Add(sup0());

        return list;
    }
    
    public static List<(T0, T1)> L<T0, T1>(int size, Supplier<T0> sup0, Supplier<T1> sup1)
    {
        var list = new List<(T0, T1)>(size);
        for (var i = 0; i < size; i++) list.Add((sup0(), sup1()));

        return list;
    }
    
    public static List<(T0, T1, T2)> L<T0, T1, T2>(int size, Supplier<T0> sup0, Supplier<T1> sup1, Supplier<T2> sup2)
    {
        var list = new List<(T0, T1, T2)>(size);
        for (var i = 0; i < size; i++) list.Add((sup0(), sup1(), sup2()));

        return list;
    }
    
    public static List<(T0, T1, T2, T3)> L<T0, T1, T2, T3>(int size, Supplier<T0> sup0, Supplier<T1> sup1, Supplier<T2> sup2, Supplier<T3> sup3)
    {
        var list = new List<(T0, T1, T2, T3)>(size);
        for (var i = 0; i < size; i++) list.Add((sup0(), sup1(), sup2(), sup3()));

        return list;
    }
}