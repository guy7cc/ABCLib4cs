namespace ABCLib4cs.Data;

public struct Segment<T>
{
    public T Value { get; }
    public int Size { get; }
    
    public Segment(T value, int size)
    {
        Value = value;
        Size = size;
    }

    public override string ToString()
    {
        return $"Segment(Value: {Value}, Size: {Size})";
    }
}