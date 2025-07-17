using ABCLib4cs.Util;
using Microsoft.VisualBasic;

namespace ABCLib4cs.Mod;

/// <summary>
/// 剰余乗算における組み合わせの計算を提供します。
/// </summary>
/// <remarks>
/// 階乗とその逆元を前計算することで、O(1)の時間計算量で組み合わせの数を計算します。
/// インスタンスの生成時に、計算に必要なテーブルを構築します。
/// </remarks>
public class ModCombination
{
    /// <summary>
    /// 計算に使用する法。
    /// </summary>
    public static long Mod { get; set; } = Const.Mod998244353;

    /// <summary>
    /// 前計算された階乗の最大範囲を取得します。
    /// </summary>
    public int Max { get; }

    /// <summary>
    /// 階乗のテーブルを取得します。Fac[i] = i! % Mod
    /// </summary>
    public long[] Fac { get; }

    /// <summary>
    /// 階乗の逆元のテーブルを取得します。Finv[i] = (i!)^(-1) % Mod
    /// </summary>
    public long[] Finv { get; }

    /// <summary>
    /// 逆元のテーブルを取得します。Inv[i] = i^(-1) % Mod
    /// </summary>
    public long[] Inv { get; }

    /// <summary>
    /// <see cref="ModCombination"/> クラスの新しいインスタンスを初期化し、組み合わせ計算のためのテーブルを生成します。
    /// </summary>
    /// <param name="max">前計算する数の最大値。使用するnの最大値より大きい値を指定してください。</param>
    public ModCombination(int max)
    {
        Max = max;
        Fac = new long[max];
        Finv = new long[max];
        Inv = new long[max];

        Fac[0] = Fac[1] = 1;
        Finv[0] = Finv[1] = 1;
        Inv[1] = 1;

        for (int i = 2; i < max; i++)
        {
            Fac[i] = Fac[i - 1] * i % Mod;
            Inv[i] = Mod - Inv[Mod % i] * (Mod / i) % Mod;
            Finv[i] = Finv[i - 1] * Inv[i] % Mod;
        }
    }

    /// <summary>
    /// 多項係数 C(n, k₁, k₂, ..., kₘ) = n! / (k₁! * k₂! * ... * kₘ! * (n - Σk)!) を計算します。
    /// </summary>
    /// <param name="n">全体の要素数。</param>
    /// <param name="k">各グループの要素数の配列。</param>
    /// <returns>計算された多項係数。引数が無効な場合は0。</returns>
    public long Get(int n, params int[] k)
    {
        if (n < 0 || k.Any(val => val < 0)) return 0;

        long kSum = k.Sum(x => (long)x);
        if (n < kSum) return 0;

        // 引数が前計算の範囲外の場合は IndexOutOfRangeException が発生します。

        long result = Fac[n];
        foreach (int val in k)
        {
            result = result * Finv[val] % Mod;
        }

        return result * Finv[n - (int)kSum] % Mod;
    }

    /// <summary>
    /// 多項係数を計算します。Getメソッドのエイリアスです。
    /// </summary>
    /// <param name="n">全体の要素数。</param>
    /// <param name="k">各グループの要素数の配列。</param>
    /// <returns>計算された多項係数。</returns>
    public long C(int n, params int[] k)
    {
        return Get(n, k);
    }

    /// <summary>
    /// 二項係数 C(n, k) = n! / (k! * (n-k)!) を高速に計算します。
    /// </summary>
    /// <param name="n">全体の要素数。</param>
    /// <param name="k">選ぶ要素数。</param>
    /// <returns>計算された二項係数。引数が無効な場合は0。</returns>
    public long SimpleC(int n, int k)
    {
        if (n < 0 || k < 0 || k > n) return 0;

        // 引数が前計算の範囲外の場合は IndexOutOfRangeException が発生します。

        return Fac[n] * (Finv[k] * Finv[n - k] % Mod) % Mod;
    }

    /// <summary>
    /// 重複組み合わせ H(n, k) = C(n + k - 1, k) を計算します。
    /// </summary>
    /// <param name="n">選ぶ対象の種類数。</param>
    /// <param name="k">選ぶ総数。</param>
    /// <returns>計算された重複組み合わせの数。</returns>
    public long H(int n, int k)
    {
        if (n < 0 || k < 0) return 0;
        if (n == 0 && k == 0) return 1;
        if (n == 0 || k == 0) return (n == 0) ? 0 : 1;

        return C(n + k - 1, k);
    }
}