namespace ABCLib4cs.Graph;

public class CostEdge : Edge, IWeighted<long>
{
    public long Weight { get; set; }
    
    public CostEdge(int from, int to, long cost) : base(from, to)
    {
        Weight = cost;
    }
}