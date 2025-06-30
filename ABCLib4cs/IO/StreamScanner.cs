// Inspired by https://qiita.com/Camypaper/items/de6d576fe5513743a50e, to whom I am very grateful.
//
// I waive all copyrights to this code and release it into the public domain.
// Use, modify, and distribute freely.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABCLib4cs.IO
{
    public static class StreamScanner
    {
        private static readonly Stream str = Console.OpenStandardInput();
        private static readonly byte[] buf = new byte[1024];
        private static int len, ptr;
        public static bool IsEndOfStream { get; private set; } = false;
        private static byte read()
        {
            if (IsEndOfStream) throw new EndOfStreamException();
            if (ptr >= len)
            {
                ptr = 0;
                if ((len = str.Read(buf, 0, 1024)) <= 0)
                {
                    IsEndOfStream = true;
                    return 0;
                }
            }
            return buf[ptr++];
        }
        public static char C()
        {
            byte b = 0;
            do b = read();
            while (b < 33 || 126 < b);
            return (char)b;
        }
        public static string S()
        {
            var sb = new StringBuilder();
            for (var b = C(); b >= 33 && b <= 126; b = (char)read())
                sb.Append(b);
            return sb.ToString();
        }

        public static int I() => (int)L();
        public static long L()
        {
            long ret = 0; byte b = 0; var ng = false;
            do b = read();
            while (b != '-' && (b < '0' || '9' < b));
            if (b == '-') { ng = true; b = read(); }
            for (; true; b = read())
            {
                if (b < '0' || '9' < b)
                    return ng ? -ret : ret;
                else ret = ret * 10 + b - '0';
            }
        }

        public static float F() => float.Parse(S(), CultureInfo.InvariantCulture);

        public static double D() => double.Parse(S(), CultureInfo.InvariantCulture);

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

        public static T ByType<T>()
        {
            var type = typeof(T);
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Char: return (T)(object)C();
                case TypeCode.String: return (T)(object)S();
                case TypeCode.Boolean: return (T)(object)B();
                case TypeCode.Int32: return (T)(object)I();
                case TypeCode.Int64: return (T)(object)L();
                case TypeCode.Single: return (T)(object)F();
                case TypeCode.Double: return (T)(object)D();
                case TypeCode.Decimal: return (T)(object)Dec();
                default:
                    throw new NotSupportedException($"The type {type.Name} cannot be read directly. Use T<T0, T1>(...) for tuples.");
            }
        }

        public static (T0, T1) T<T0, T1>()
        {
            return (ByType<T0>(), ByType<T1>());
        }

        public static (T0, T1, T2) T<T0, T1, T2>()
        {
            return (ByType<T0>(), ByType<T1>(), ByType<T2>());
        }

        public static (T0, T1, T2, T3) T<T0, T1, T2, T3>()
        {
            return (ByType<T0>(), ByType<T1>(), ByType<T2>(), ByType<T3>());
        }

        public static T[] Arr<T>(int size)
        {
            var arr = new T[size];
            for (int i = 0; i < size; i++)
            {
                arr[i] = ByType<T>();
            }
            return arr;
        }



        public static void Collect<T0>(int size, ICollection<T0> t0s)
        {
            for (int i = 0; i < size; i++)
            {
                t0s.Add(ByType<T0>());
            }
        }

        public static void Collect<T0, T1>(int size, ICollection<T0> t0s, ICollection<T1> t1s)
        {
            for (int i = 0; i < size; i++)
            {
                t0s.Add(ByType<T0>());
                t1s.Add(ByType<T1>());
            }
        }

        public static void Collect<T0, T1, T2>(int size, ICollection<T0> t0s, ICollection<T1> t1s, ICollection<T2> t2s)
        {
            for (int i = 0; i < size; i++)
            {
                t0s.Add(ByType<T0>());
                t1s.Add(ByType<T1>());
                t2s.Add(ByType<T2>());
            }
        }

        public static void Collect<T0, T1, T2, T3>(int size, ICollection<T0> t0s, ICollection<T1> t1s, ICollection<T2> t2s, ICollection<T3> t3s)
        {
            for (int i = 0; i < size; i++)
            {
                t0s.Add(ByType<T0>());
                t1s.Add(ByType<T1>());
                t2s.Add(ByType<T2>());
                t3s.Add(ByType<T3>());
            }
        }
    }
}
