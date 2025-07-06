namespace ABCLib4cs.Data.Struct;

public interface ILazyOperation<S, F>
{
    /// <summary>
    /// Maps a lazy value to a data value.
    /// </summary>
    S Map(F f, S x);

    /// <summary>
    /// Composes two lazy values.
    /// </summary>
    F Composite(F f, F g);

    /// <summary>
    /// Gets the identity lazy value.
    /// </summary>
    F Identity { get; }
}