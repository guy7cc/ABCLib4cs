namespace ABCLib4cs.Data.Struct;

public static class LazyOperations
{
    /// <summary>
    ///  区間加算・区間最大or最小
    /// </summary>
    public class MA : ILazyOperation<long, long>
    {
        public long Map(long f, long x) => f + x;

        public long Composite(long f, long g) => f + g;
        
        public long Identity => 0;
    }

    /// <summary>
    ///  区間加算・区間和
    /// </summary>
    public class SA : ILazyOperation<Segment<long>, long>
    {
        public Segment<long> Map(long f, Segment<long> x) => new(x.Value + f * x.Size, x.Size);
        
        public long Composite(long f, long g) => f + g;
        
        public long Identity => 0;
    }
    
    /// <summary>
    ///  区間更新・区間最大or最小
    /// </summary>
    public class MU : ILazyOperation<long, LazyOperationData<long>>
    {
        public long Map(LazyOperationData<long> f, long x) => f.IsIdentity ? x : f.Value;

        public LazyOperationData<long> Composite(LazyOperationData<long> f, LazyOperationData<long> g) => f.IsIdentity ? g : f;
        
        public LazyOperationData<long> Identity => LazyOperationData<long>.Identity;
    }
    
    /// <summary>
    ///  区間更新・区間和
    /// </summary>
    public class SU : ILazyOperation<Segment<long>, LazyOperationData<long>>
    {
        public Segment<long> Map(LazyOperationData<long> f, Segment<long> x) => f.IsIdentity ? x : new Segment<long>(f.Value * x.Size, x.Size);
        
        public LazyOperationData<long> Composite(LazyOperationData<long> f, LazyOperationData<long> g) => f.IsIdentity ? g : f;
        
        public LazyOperationData<long> Identity => LazyOperationData<long>.Identity;
    }
}