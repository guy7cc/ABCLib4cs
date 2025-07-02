namespace ABCLib4cs.Graph;

public class UnionFind
{
    private readonly int[] _parent;
    private readonly int[] _rank;
    
    public UnionFind(int size)
    {
        _parent = new int[size];
        _rank = new int[size];
        for (var i = 0; i < size; i++)
        {
            _parent[i] = i;
            _rank[i] = 0;
        }
    }
    
    public int Find(int x)
    {
        if (_parent[x] == x) return x;
        return _parent[x] = Find(_parent[x]);
    }
    
    public bool Connected(int x, int y)
    {
        return Find(x) == Find(y);
    }
    
    public void Union(int x, int y)
    {
        x = Find(x);
        y = Find(y);
        
        if (x == y) return;
        
        if (_rank[x] < _rank[y])
            _parent[x] = y;
        else
        {
            _parent[y] = x;
            if(_rank[x] == _rank[y]) _rank[x]++;
        }
    }
}