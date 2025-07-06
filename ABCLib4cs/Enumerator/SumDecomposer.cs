namespace ABCLib4cs.Enumerator;

/// <summary>
///  正の数に対し、正の数の足し算による表現を列挙するクラス
/// </summary>
public class SumDecomposer
{
    public int Size { get; }

    private long[]? _dp;

    public SumDecomposer(int size)
    {
        Size = size;
    }
    
    public long Num()
    {
        if (_dp != null) return _dp[Size];
        _dp = new long[Size + 1];
        _dp[0] = 1;

        for (int m = 1; m <= Size; m++) {
            long total = 0;
            for (int k = 1; ; k++) {
                int g1 = (3 * k * k - k) / 2;
                int g2 = (3 * k * k + k) / 2;
                if (g1 > m && g2 > m) break;

                int sign = (k % 2 == 0) ? -1 : 1;

                if (g1 <= m) total += sign * _dp[m - g1];
                if (g2 <= m) total += sign * _dp[m - g2];
            }
            _dp[m] = total;
        }
        
        return _dp[Size];
    }
    
    public IEnumerable<(int, int[])> Enumerate()
    {
        int[] a = new int[Size];
        int i = 0;
        int k = 0;
        a[k] = Size;
        while (true) {
            yield return (i++, a);
            
            int rem = 0;
            while (k >= 0 && a[k] == 1) {
                rem += a[k];
                k--;
            }

            if (k < 0) break;
            a[k]--;
            rem++;
            while (rem > a[k]) {
                a[k+1] = a[k];
                rem -= a[k];
                k++;
            }
            a[k+1] = rem;
            k++;
        }
    }
}