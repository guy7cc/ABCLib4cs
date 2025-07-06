namespace ABCLib4cs.Util;

public static class CollectionUtils
{
    public static int LowerBound<T>(List<T> list, T key) where T : IComparable<T>
    {
        int i = list.BinarySearch(key);
        return i >= 0 ? i : ~i;
    }

    public static int LowerBound<T>(List<T> list, T key, Comparer<T> comparer)
    {
        int i = list.BinarySearch(key, comparer);
        return i >= 0 ? i : ~i;
    }
    
    public static int UpperBound<T>(List<T> list, T key) where T : IComparable<T>
    {
        int i = list.BinarySearch(key);
        return i >= 0 ? i + 1 : ~i;
    }
    
    public static int UpperBound<T>(List<T> list, T key, Comparer<T> comparer)
    {
        int i = list.BinarySearch(key, comparer);
        return i >= 0 ? i + 1 : ~i;
    }
    
    public static List<int> CompressCoord<T>(IReadOnlyList<T> list) where T : IComparable<T>
    {
        var sorted = new List<T>(list);
        sorted.Sort();
        var ans = new List<int>(list.Count);
        foreach (var t in sorted)
            ans.Add(sorted.BinarySearch(t));
        return ans;
    }
}