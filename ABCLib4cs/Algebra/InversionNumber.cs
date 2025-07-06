using ABCLib4cs.Data.Struct;

namespace ABCLib4cs.Algebra;

public class InversionNumber
{
    public static long Calc(IReadOnlyCollection<int> a){
        var max = a.Max() + 1;
        var ft = new FenwickTree(max);
        long sum = 0;
        foreach (var x in a)
        {
            sum += ft.Sum(x, max);
            ft.Add(x, 1);
        }
        return sum;
    }
}