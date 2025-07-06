using ABCLib4cs.Data;

namespace ABCLib4cs.Algebra;

public static class Monoids
{
    public class MinMonoid : IMonoid<long>
    {
        public long Op(long a, long b) => Math.Min(a, b);

        public long E => long.MaxValue;
    }

    public class MaxMonoid : IMonoid<long>
    {
        public long Op(long a, long b) => Math.Max(a, b);

        public long E => long.MinValue;
    }

    public class SumMonoidSegment : IMonoid<Segment<long>>
    {
        public Segment<long> Op(Segment<long> a, Segment<long> b) => new(a.Value + b.Value, a.Size + b.Size);
        
        public Segment<long> E => new(0, 0);
    }
}