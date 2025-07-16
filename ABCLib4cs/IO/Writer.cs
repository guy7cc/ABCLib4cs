namespace ABCLib4cs.IO;

public static class Writer
{
    private static readonly StreamWriter _writer = new(Console.OpenStandardOutput()) { AutoFlush = false };

    public static void Setup()
    {
        Console.SetOut(_writer);
    }

    public static void W(object o)
    {
        _writer.Write(o);
    }

    public static void W<T>(ICollection<T> collection)
    {
        var first = true;
        foreach (var item in collection)
        {
            if (!first) _writer.Write(' ');
            _writer.Write(item);
        }
    }
    
    public static void WL()
    {
        _writer.WriteLine();
    }

    public static void WL(object o)
    {
        Console.WriteLine(o);
    }

    public static void WL<T>(ICollection<T> collection)
    {
        var first = true;
        foreach (var item in collection)
        {
            if (!first) _writer.Write(' ');
            _writer.Write(item);
            first = false;
        }

        _writer.WriteLine();
    }

    public static void Dispose()
    {
        _writer.Flush();
        _writer.Dispose();
    }
}