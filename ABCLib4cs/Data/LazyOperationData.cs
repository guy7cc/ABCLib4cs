namespace ABCLib4cs.Data;

public struct LazyOperationData<T>
{
    public T? Value { get; }
    public bool IsIdentity { get; }

    private LazyOperationData(T? value, bool isIdentity)
    {
        Value = value;
        IsIdentity = isIdentity;
    }
    
    public static LazyOperationData<T> Of(T value) => new(value, false);
    
    public static LazyOperationData<T> Identity => new(default, true);
}