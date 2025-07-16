namespace ABCLib4cs.Util;

public static class CollectionUtils
{
    public static int GetMaxNotLessThanBound<T>(this List<T> list, T bound) where T : IComparable<T>
    {
        int i = list.BinarySearch(bound);
        return i >= 0 ? i : ~i;
    }
    
    public static int GetMaxNotLessThanBound<T>(this List<T> list, T bound, Comparer<T> comparer)
    {
        int i = list.BinarySearch(bound, comparer);
        return i >= 0 ? i : ~i;
    }
    
    public static int GetMinNotMoreThanBound<T>(this List<T> list, T bound) where T : IComparable<T>
    {
        int i = list.BinarySearch(bound);
        return i >= 0 ? i : ~i - 1;
    }
    
    public static int GetMinNotMoreThanBound<T>(this List<T> list, T bound, Comparer<T> comparer)
    {
        int i = list.BinarySearch(bound, comparer);
        return i >= 0 ? i : ~i - 1;
    }
    
    public static List<int> CompressCoord<T>(this IReadOnlyList<T> list) where T : IComparable<T>
    {
        var sorted = new List<T>(list);
        sorted.Sort();
        var ans = new List<int>(list.Count);
        foreach (var t in sorted)
            ans.Add(sorted.BinarySearch(t));
        return ans;
    }
}