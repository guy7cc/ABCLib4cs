using ABCLib4cs.Util;

namespace ABCLib4cs.Data.Struct;

/// <summary>
/// Provides methods to solve rectangle sum problems using a plane sweep algorithm.
/// </summary>
public static class StaticRectangleSum
{
    #region Data Classes

    public class AddPoint
    {
        public long X { get; set; }
        public long Y { get; set; }
        public long W { get; set; }

        public AddPoint(long x, long y, long w)
        {
            this.X = x;
            this.Y = y;
            this.W = w;
        }
    }

    public class AddRectangle
    {
        public long L { get; set; }
        public long D { get; set; }
        public long R { get; set; }
        public long U { get; set; }
        public long W { get; set; }

        public AddRectangle(long l, long d, long r, long u, long w)
        {
            L = l;
            D = d;
            R = r;
            U = u;
            W = w;
        }
    }

    public class Query
    {
        public long L { get; set; }
        public long D { get; set; }
        public long R { get; set; }
        public long U { get; set; }

        public Query(long l, long d, long r, long u)
        {
            L = l;
            D = d;
            R = r;
            U = u;
        }
    }

    #endregion

    /// <summary>
    /// Calculates the sum of weights of points within each query rectangle.
    /// </summary>
    public static long[] AddPointQuery(List<AddPoint> points, List<Query> queries)
    {
        int n = points.Count;
        int q = queries.Count;

        // Coordinate Compression for Y
        points.Sort((a, b) => a.Y.CompareTo(b.Y));
        var yCoords = new List<long>();
        foreach (var p in points)
        {
            if (yCoords.Count == 0 || yCoords[^1] != p.Y)
            {
                yCoords.Add(p.Y);
            }

            p.Y = yCoords.Count - 1; // Re-use Y as compressed index
        }

        var queryEvents = new List<(long X, int Index)>(2 * q);
        for (int i = 0; i < q; i++)
        {
            var currentQuery = queries[i];
            currentQuery.D = CollectionUtils.LowerBound(yCoords, currentQuery.D);
            currentQuery.U = CollectionUtils.LowerBound(yCoords, currentQuery.U);
            queryEvents.Add((currentQuery.L, i));
            queryEvents.Add((currentQuery.R, i + q));
        }

        queryEvents.Sort((a, b) => a.X.CompareTo(b.X));
        points.Sort((a, b) => a.X.CompareTo(b.X));

        var results = new long[q];
        var ft = new FenwickTree(yCoords.Count);
        int pointIndex = 0;

        foreach (var (x, queryIndex) in queryEvents)
        {
            while (pointIndex < n && points[pointIndex].X < x)
            {
                ft.Add((int)points[pointIndex].Y, points[pointIndex].W);
                pointIndex++;
            }

            int i = queryIndex;
            if (i < q)
            {
                results[i] -= ft.Sum((int)queries[i].D, (int)queries[i].U);
            }
            else
            {
                i -= q;
                results[i] += ft.Sum((int)queries[i].D, (int)queries[i].U);
            }
        }

        return results;
    }

    public static long[] AddRectangleQuery(List<AddRectangle> rectangles, List<Query> qs)
    {
        int rectCount = rectangles.Count;
        int queryCount = qs.Count;

        var yCoordsTemp = new List<long>(2 * rectCount);
        foreach (var rect in rectangles)
        {
            yCoordsTemp.Add(rect.D);
            yCoordsTemp.Add(rect.U);
        }

        var ys = yCoordsTemp.Distinct().OrderBy(y => y).ToList();

        var rectangleEvents = new List<(long X, int Index)>(2 * rectCount);
        for (int i = 0; i < rectCount; i++)
        {
            var rect = rectangles[i];
            rect.D = CollectionUtils.LowerBound(ys, rect.D);
            rect.U = CollectionUtils.LowerBound(ys, rect.U);
            rectangleEvents.Add((rect.L, i));
            rectangleEvents.Add((rect.R, i + rectCount));
        }

        var queryEvents = new List<(long X, int Index)>(2 * queryCount);
        var qdis = new int[queryCount];
        var quis = new int[queryCount];

        for (int i = 0; i < queryCount; i++)
        {
            var q = qs[i];
            qdis[i] = CollectionUtils.LowerBound(ys, q.D);
            quis[i] = CollectionUtils.LowerBound(ys, q.U);
            queryEvents.Add((q.L, i));
            queryEvents.Add((q.R, i + queryCount));
        }

        rectangleEvents.Sort((a, b) => a.X.CompareTo(b.X));
        queryEvents.Sort((a, b) => a.X.CompareTo(b.X));

        var results = new long[queryCount];
        var fts = new FenwickTree[4];
        for (int i = 0; i < 4; i++)
        {
            fts[i] = new FenwickTree(ys.Count + 1);
        }

        int rectEventIndex = 0;
        foreach (var (queryX, queryEventIndex) in queryEvents)
        {
            while (rectEventIndex < 2 * rectCount && rectangleEvents[rectEventIndex].X < queryX)
            {
                int rectIdx = rectangleEvents[rectEventIndex].Index;
                long x, w;
                int y1, y2;

                if (rectIdx < rectCount)
                {
                    var r = rectangles[rectIdx];
                    x = r.L;
                    w = r.W;
                    y1 = (int)r.D;
                    y2 = (int)r.U;

                    fts[0].Add(y1, w * x * ys[y1]);
                    fts[0].Add(y2, -w * x * ys[y2]);
                    fts[1].Add(y1, -w * x);
                    fts[1].Add(y2, w * x);
                    fts[2].Add(y1, -w * ys[y1]);
                    fts[2].Add(y2, w * ys[y2]);
                    fts[3].Add(y1, w);
                    fts[3].Add(y2, -w);
                }
                else
                {
                    var r = rectangles[rectIdx - rectCount];
                    x = r.R;
                    w = r.W;
                    y1 = (int)r.D;
                    y2 = (int)r.U;

                    fts[0].Add(y1, -w * x * ys[y1]);
                    fts[0].Add(y2, w * x * ys[y2]);
                    fts[1].Add(y1, w * x);
                    fts[1].Add(y2, -w * x);
                    fts[2].Add(y1, w * ys[y1]);
                    fts[2].Add(y2, -w * ys[y2]);
                    fts[3].Add(y1, -w);
                    fts[3].Add(y2, w);
                }

                rectEventIndex++;
            }

            int i = queryEventIndex;
            long sign = 1;

            if (i < queryCount) sign = -1;
            else i -= queryCount;

            long qd = qs[i].D;
            long qu = qs[i].U;
            int qdi = qdis[i];
            int qui = quis[i];

            long sumU = fts[0].Sum(0, qui) +
                        fts[1].Sum(0, qui) * qu +
                        fts[2].Sum(0, qui) * queryX +
                        fts[3].Sum(0, qui) * queryX * qu;

            long sumD = fts[0].Sum(0, qdi) +
                        fts[1].Sum(0, qdi) * qd +
                        fts[2].Sum(0, qdi) * queryX +
                        fts[3].Sum(0, qdi) * queryX * qd;

            results[i] += sign * (sumU - sumD);
        }

        return results;
    }
}