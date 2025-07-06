using System.Numerics;
using ABCLib4cs.Algebra;

namespace ABCLib4cs.Data.Struct;

public class LazySegmentTree<S, F>
{
    public int Size { get; }

    private readonly IMonoid<S> _m;
    private readonly ILazyOperation<S, F> _op;
    private readonly S[] _d;
    private readonly F[] _lz;

    public LazySegmentTree(IReadOnlyList<S> list, IMonoid<S> m, ILazyOperation<S, F> op)
    {
        Size = list.Count;
        _m = m;
        _op = op;
        _d = new S[Size * 2];
        _lz = new F[Size * 2];
        
        for (int i = 0; i < list.Count; i++) 
            _d[i + Size] = list[i];
        for (int i = Size - 1; i > 0; i--)
            _d[i] = _m.Op(_d[i << 1], _d[i << 1 | 1]);
    }

    private S EvalAt(int i) => _op.Map(_lz[i], _d[i]);

    private void PropagateAt(int i)
    {
        _d[i] = EvalAt(i);
        if (i < Size) // Don't propagate to children of leaf nodes
        {
            _lz[i << 1] = _op.Composite(_lz[i], _lz[i << 1]);
            _lz[i << 1 | 1] = _op.Composite(_lz[i], _lz[i << 1 | 1]);
        }

        _lz[i] = _op.Identity;
    }

    private void PropagateAbove(int i)
    {
        if (i == 0) return;
        int h = BitOperations.Log2((uint)i);
        for (int shift = h; shift > 0; shift--)
            PropagateAt(i >> shift);
    }

    private void RecalcAbove(int i)
    {
        while (i > 1)
        {
            i >>= 1;
            _d[i] = _m.Op(EvalAt(i << 1), EvalAt(i << 1 | 1));
        }
    }

    /// <summary>
    /// Sets the value at a specific index.
    /// </summary>
    public void SetValue(int i, S x)
    {
        i += Size;
        PropagateAbove(i);
        _d[i] = x;
        _lz[i] = _op.Identity;
        RecalcAbove(i);
    }

    /// <summary>
    /// Queries the aggregated value over the range [l, r).
    /// </summary>
    public S Query(int l, int r)
    {
        l += Size;
        r += Size;
        PropagateAbove(l / (l & -l));
        PropagateAbove(r / (r & -r) - 1);

        S vL = _m.E;
        S vR = _m.E;
        while (l < r)
        {
            if ((l & 1) > 0) vL = _m.Op(vL, EvalAt(l++));
            if ((r & 1) > 0) vR = _m.Op(EvalAt(--r), vR);
            l >>= 1;
            r >>= 1;
        }

        return _m.Op(vL, vR);
    }

    /// <summary>
    /// Applies an operation to all elements in the range [l, r).
    /// </summary>
    public void UpdateRange(int l, int r, F f)
    {
        l += Size;
        r += Size;
        int l0 = l / (l & -l);
        int r0 = r / (r & -r) - 1;

        PropagateAbove(l0);
        PropagateAbove(r0);

        while (l < r)
        {
            if ((l & 1) > 0) _lz[l] = _op.Composite(f, _lz[l++]);
            if ((r & 1) > 0) _lz[--r] = _op.Composite(f, _lz[r]);
            l >>= 1;
            r >>= 1;
        }

        RecalcAbove(l0);
        RecalcAbove(r0);
    }
}