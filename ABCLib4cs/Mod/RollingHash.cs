namespace ABCLib4cs.Mod;

/// <summary>
    /// 文字列のローリングハッシュを計算し、部分文字列の比較などを効率的に行います。
    /// </summary>
    public class RollingHash
    {
        // 定数
        public const long Mask30 = (1L << 30) - 1;
        public const long Mask31 = (1L << 31) - 1;
        public const long Mask61 = (1L << 61) - 1;
        public const long Mod = Mask61;
        public static readonly long Base = 10007;
        
        public string Str { get; }

        private readonly long[] _hashed;
        private readonly long[] _power;
        
        public RollingHash(string s)
        {
            Str = s;
            int n = s.Length;
            _hashed = new long[n + 1];
            _power = new long[n + 1];
            _power[0] = 1;

            for (int i = 0; i < n; i++)
            {
                _power[i + 1] = Mul(_power[i], Base);
                long h = Mul(_hashed[i], Base) + s[i];
                _hashed[i + 1] = (h >= Mod ? h - Mod : h);
            }
        }

        /// <summary>
        /// 61ビットの法の下で2つの64ビット整数の乗算を安全に行います。
        /// </summary>
        public static long Mul(long a, long b)
        {
            long au = a >> 31;
            long ad = a & Mask31;
            long bu = b >> 31;
            long bd = b & Mask31;
            long mid = ad * bu + au * bd;
            long midu = mid >> 30;
            long midd = mid & Mask30;
            return ModValue(au * bu * 2 + midu + (midd << 31) + ad * bd);
        }

        /// <summary>
        /// 61ビットの法の下で剰余を計算します。
        /// </summary>
        public static long ModValue(long x)
        {
            long xu = x >> 61;
            long xd = x & Mask61;
            long res = xu + xd;
            if (res >= Mod) res -= Mod;
            return res;
        }

        /// <summary>
        /// 部分文字列 s[l..r) のハッシュ値を取得します。
        /// </summary>
        /// <param name="l">開始インデックス（含む）。</param>
        /// <param name="r">終了インデックス（含まない）。</param>
        /// <returns>部分文字列のハッシュ値。</returns>
        public long Get(int l, int r)
        {
            long x = Mul(_hashed[l], _power[r - l]);
            long ret = _hashed[r] - x;
            if (ret < 0) ret += Mod;
            return ret;
        }

        /// <summary>
        /// 2つのハッシュ値を連結します。
        /// </summary>
        /// <param name="h1">最初のハッシュ値。</param>
        /// <param name="h2">2番目のハッシュ値。</param>
        /// <param name="h2len">2番目のハッシュ値の元の文字列長。</param>
        /// <returns>連結された新しいハッシュ値。</returns>
        public long Connect(long h1, long h2, int h2len)
        {
            long x = Mul(h1, _power[h2len]) + h2;
            return x >= Mod ? x - Mod : x;
        }

        /// <summary>
        /// 2つの文字列（またはその部分文字列）の最長共通接頭辞（LCP）の長さを計算します。
        /// </summary>
        /// <param name="other">比較対象のRollingHashオブジェクト。</param>
        /// <param name="l1">このインスタンスの文字列の比較開始インデックス。</param>
        /// <param name="r1">このインスタンスの文字列の比較終了インデックス。</param>
        /// <param name="l2">比較対象の文字列の比較開始インデックス。</param>
        /// <param name="r2">比較対象の文字列の比較終了インデックス。</param>
        /// <returns>最長共通接頭辞の長さ。</returns>
        public int Lcp(RollingHash other, int l1, int r1, int l2, int r2)
        {
            int max = Math.Min(r1 - l1, r2 - l2);
            int low = 0;
            int high = max + 1;

            while (high - low > 1)
            {
                int mid = low + (high - low) / 2;
                if (this.Get(l1, l1 + mid) == other.Get(l2, l2 + mid))
                {
                    low = mid;
                }
                else
                {
                    high = mid;
                }
            }
            return low;
        }
    }