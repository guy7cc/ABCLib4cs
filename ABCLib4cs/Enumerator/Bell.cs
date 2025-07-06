namespace ABCLib4cs.Enumerator;

/// <summary>
///  ベル数を計算し、集合 {0, 1, ..., N-1} のすべての分割を列挙するクラス。
/// </summary>
public class Bell
{
    public int N { get; }

    private long[][]? _bellNumbers;
    
    public Bell(int n)
    {
        N = n;
    }
    
    public long Size()
    {
        if (N == 0)
        {
            return 1;
        }

        if (_bellNumbers == null)
        {
            _bellNumbers = new long[N][];
            _bellNumbers[0] = new long[1];
            _bellNumbers[0][0] = 1;

            for (int i = 1; i < N; i++)
            {
                _bellNumbers[i] = new long[i + 1];
                _bellNumbers[i][0] = _bellNumbers[i - 1][i - 1];
                for (int j = 1; j <= i; j++)
                {
                    _bellNumbers[i][j] = _bellNumbers[i][j - 1] + _bellNumbers[i - 1][j - 1];
                }
            }
        }
        return _bellNumbers[N - 1][N - 1];
    }
    
    public IEnumerable<List<SortedSet<int>>> Enumerate()
    {
        if (N == 0)
        {
            yield return new List<SortedSet<int>>();
            yield break;
        }

        foreach (var partition in EnumerateRecursive(0, new List<SortedSet<int>>()))
        {
            yield return partition;
        }
    }
    
    private IEnumerable<List<SortedSet<int>>> EnumerateRecursive(int index, List<SortedSet<int>> currentPartition)
    {
        if (index == N)
        {
            yield return currentPartition;
        }
        else
        {
            for (int i = 0; i < currentPartition.Count; i++)
            {
                currentPartition[i].Add(index);
                foreach (var result in EnumerateRecursive(index + 1, currentPartition))
                {
                    yield return result;
                }
                currentPartition[i].Remove(index);
            }
            
            currentPartition.Add(new SortedSet<int> { index });
            foreach (var result in EnumerateRecursive(index + 1, currentPartition))
            {
                yield return result;
            }
            currentPartition.RemoveAt(currentPartition.Count - 1);
        }
    }
}