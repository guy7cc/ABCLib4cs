using ABCLib4cs.Data;

namespace ABCLib4cs.Algebra;

public interface IMonoid<T>
{
    T Op(T a, T b);
    
    T E { get; }
}