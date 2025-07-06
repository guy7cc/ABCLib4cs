using ABCLib4cs.Algebra;

namespace ABCLib4cs.Data.Struct;

public static class LazySegmentTreeFactory
{
    // 区間加算・区間最小値
    public static LazySegmentTree<long, long> RMinQRAQ(int size)
        => new(new long[size], new Monoids.MinMonoid(), new LazyOperations.MA());
    
    // 区間加算・区間最大値
    public static LazySegmentTree<long, long> RMaxQRAQ(int size)
        => new(new long[size], new Monoids.MaxMonoid(), new LazyOperations.MA());

    // 区間加算・区間和
    public static LazySegmentTree<Segment<long>, long> RSQRAQ(int size)
    {
        var arr = new Segment<long>[size];
        for (int i = 0; i < size; i++)
            arr[i] = new Segment<long>(0, 1);
        return new(arr, new Monoids.SumMonoidSegment(), new LazyOperations.SA());
    }
    
    // 区間更新・区間最小値
    public static LazySegmentTree<long, LazyOperationData<long>> RMinQRUQ(int size)
        => new(new long[size], new Monoids.MinMonoid(), new LazyOperations.MU());
    
    // 区間更新・区間最大値
    public static LazySegmentTree<long, LazyOperationData<long>> RMaxQRUQ(int size)
        => new(new long[size], new Monoids.MaxMonoid(), new LazyOperations.MU());
    
    // 区間更新・区間和
    public static LazySegmentTree<Segment<long>, LazyOperationData<long>> RSQRUQ(int size)
    {
        var arr = new Segment<long>[size];
        for (int i = 0; i < size; i++)
            arr[i] = new Segment<long>(0, 1);
        return new(arr, new Monoids.SumMonoidSegment(), new LazyOperations.SU());
    }
}