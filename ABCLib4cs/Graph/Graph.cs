namespace ABCLib4cs.Graph;

public class Graph<VT, ET> where VT : Vertex where ET : Edge
{
    public int Size { get; }

    public VT[] V { get; private set; }

    public List<ET>[] E { get; private set; }

    public List<ET> this[int index] => E[index];
    
    public Graph(int size)
    {
        Size = size;
        V = new VT[size];
        E = new List<ET>[size];
        for (var i = 0; i < size; i++)
        {
            E[i] = new List<ET>();
        }
    }
    
    public static Graph<Vertex, Edge> Simple(int size)
    {
        return new Graph<Vertex, Edge>(size);
    }
    
    public void AddV(VT vertex)
    {
        int index = vertex.Index;
        V[index] = vertex;
    }
    
    public void AddE(ET edge)
    {
        int from = edge.From;
        int to = edge.To;
        E[from].Add(edge);
    }
}