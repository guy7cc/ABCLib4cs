namespace ABCLib4cs.Data.Struct;

public class FenwickTree
{
    private readonly int _size;
    public int Size => _size;
    private readonly long[] _tree;
    
    public FenwickTree(int size)
    {
        _size = size;
        _tree = new long[size];
    }

    public void Add(int p, long x)
    {
        for (p++; p <= _size; p += p & -p){
            _tree[p - 1] += x;
        }
    }
    
    /// <summary>
    ///  Returns the sum of the range [l, r).
    /// </summary>
    public long Sum(int l, int r){
        return Sum(r) - Sum(l);
    }

    private long Sum(int r){
        long sum = 0;
        for (; r > 0; r -= r & -r){
            sum += _tree[r - 1];
        }
        return sum;
    }
}