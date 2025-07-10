namespace ABCLib4cs.Algorithm;

public class Doubling
{
    private IReadOnlyList<int> _dest;
    private int[][] _dp;
    
    public Doubling(IReadOnlyList<int> destinations, long maxMove)
    {
        _dest = destinations;
        int logK = 1;
        while (maxMove > 1)
        {
            maxMove /= 2;
            logK++;
        }

        _dp = new int[logK][];
        for (int i = 0; i < logK; i++) _dp[i] = new int[_dest.Count];

        for (int j = 0; j < _dest.Count; j++)
        {
            _dp[0][j] = _dest[j];
        }

        for (int i = 1; i < logK; i++)
        {
            for (int j = 0; j < _dest.Count; j++)
            {
                _dp[i][j] = _dp[i - 1][_dp[i - 1][j]];
            }
        }
    }

    public int GetDestination(int from, long K)
    {
        for (int i = 0; K > 0; i++)
        {
            if ((K & 1) > 0) from = _dp[i][from];
            K >>= 1;
        }
        return from;
    }
}