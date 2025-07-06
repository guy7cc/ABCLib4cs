using ABCLib4cs.Algebra;

namespace ABCLib4cs.Data.Struct;

/// <summary>
    /// Represents a segment tree, a data structure for storing information about intervals or segments.
    /// </summary>
    /// <typeparam name="T">The type of the values in the segment tree.</typeparam>
    public class SegmentTree<T>
    {
        /// <summary>
        /// Gets the number of elements that the SegmentTree can contain.
        /// </summary>
        public int Size { get; }

        private readonly IMonoid<T> _monoid;
        public int N0 { get; }
        public readonly T[] _data;

        /// <summary>
        /// Initializes a new instance of the SegmentTree class.
        /// </summary>
        /// <param name="size">The number of elements of the segment tree.</param>
        /// <param name="monoid">The monoid that defines the binary operation and identity element.</param>
        public SegmentTree(IReadOnlyList<T> list, IMonoid<T> monoid)
        {
            Size = list.Count;
            _monoid = monoid;
            
            int n0 = 1;
            while (n0 < Size)
            {
                n0 *= 2;
            }
            N0 = n0;

            _data = new T[2 * N0];
            for (int i = 0; i < list.Count; i++)
            {
                _data[i + N0 - 1] = list[i];
            }
            for (int i = N0 - 2; i >= 0; i--)
            {
                _data[i] = _monoid.Op(_data[2 * i + 1], _data[2 * i + 2]);
            }
        }

        /// <summary>
        /// Updates the value at the specified index.
        /// </summary>
        /// <param name="k">The zero-based index of the element to update.</param>
        /// <param name="x">The new value for the element.</param>
        public void Update(int k, T x)
        {
            k += N0 - 1;
            _data[k] = x;
            while (k > 0) // Loop until the root
            {
                k = (k - 1) / 2;
                _data[k] = _monoid.Op(_data[2 * k + 1], _data[2 * k + 2]);
            }
        }

        /// <summary>
        /// Queries the range [l, r).
        /// </summary>
        /// <param name="l">The start of the range (inclusive).</param>
        /// <param name="r">The end of the range (exclusive).</param>
        /// <returns>The result of the monoid operation over the specified range.</returns>
        public T Query(int l, int r)
        {
            int L = l + N0;
            int R = r + N0;
            T s = _monoid.E;

            while (L < R)
            {
                if ((R & 1) == 1)
                {
                    R--;
                    s = _monoid.Op(s, _data[R - 1]);
                }
                if ((L & 1) == 1)
                {
                    s = _monoid.Op(s, _data[L - 1]);
                    L++;
                }
                L >>= 1;
                R >>= 1;
            }
            return s;
        }
    }