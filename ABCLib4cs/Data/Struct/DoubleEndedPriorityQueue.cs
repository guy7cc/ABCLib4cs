// Inspired by: https://natsugiri.hatenablog.com/entry/2016/10/10/035445, to whom I am very grateful.
//
// I waive all copyrights to this code and release it into the public domain.
// Use, modify, and distribute freely.

namespace ABCLib4cs.Data.Struct;

public class DoubleEndedPriorityQueue<T> where T : IComparable<T>
{
    private readonly List<T> _data;

    public DoubleEndedPriorityQueue()
    {
        _data = new List<T>();
    }
    
    public DoubleEndedPriorityQueue(IEnumerable<T> collection)
    {
        _data = collection.ToList();
        MakeHeap();
    }
    
    public int Count => _data.Count;
    
    public bool IsEmpty => _data.Count == 0;
    
    public T Min => _data.Count < 2 ? _data[0] : _data[1];
    
    public T Max => _data[0];
    
    public void MakeHeap()
    {
        for (int i = _data.Count - 1; i >= 0; i--)
        {
            if ((i & 1) > 0 && i > 0 && _data[i - 1].CompareTo(_data[i]) < 0)
            {
                Swap(i - 1, i);
            }

            int k = Down(i);
            Up(k, i);
        }
    }
    
    public void Push(T item)
    {
        int k = _data.Count;
        _data.Add(item);
        Up(k, 1);
    }
    
    public T PopMin()
    {
        if (_data.Count < 3)
        {
            T item = _data[_data.Count - 1];
            _data.RemoveAt(_data.Count - 1);
            return item;
        }
        else
        {
            T res = Min;
            Swap(1, _data.Count - 1);
            _data.RemoveAt(_data.Count - 1);
            int k = Down(1);
            Up(k, 1);
            return res;
        }
    }
    
    public T PopMax()
    {
        if (_data.Count < 2)
        {
            T item = _data[_data.Count - 1];
            _data.RemoveAt(_data.Count - 1);
            return item;
        }
        else
        {
            T res = Max;
            Swap(0, _data.Count - 1);
            _data.RemoveAt(_data.Count - 1);
            int k = Down(0);
            Up(k, 1);
            return res;
        }
    }

    private int Down(int k)
    {
        int n = _data.Count;
        if ((k & 1) > 0)
        {
            while (2 * k + 1 < n)
            {
                int c = 2 * k + 3;
                if (n <= c || _data[c - 2].CompareTo(_data[c]) < 0)
                {
                    c -= 2;
                }

                if (c < n && _data[c].CompareTo(_data[k]) < 0)
                {
                    Swap(k, c);
                    k = c;
                }
                else
                {
                    break;
                }
            }
        }
        else
        {
            while (2 * k + 2 < n)
            {
                int c = 2 * k + 4;
                if (n <= c || _data[c].CompareTo(_data[c - 2]) < 0)
                {
                    c -= 2;
                }

                if (c < n && _data[k].CompareTo(_data[c]) < 0)
                {
                    Swap(k, c);
                    k = c;
                }
                else
                {
                    break;
                }
            }
        }

        return k;
    }

    private int Up(int k, int root)
    {
        if ((k | 1) < _data.Count && _data[k & ~1].CompareTo(_data[k | 1]) < 0)
        {
            Swap(k & ~1, k | 1);
            k ^= 1;
        }

        int p;
        while (root < k && _data[(p = Parent(k))].CompareTo(_data[k]) < 0)
        {
            Swap(p, k);
            k = p;
        }
        
        while (root < k && _data[k].CompareTo(_data[p = (Parent(k) | 1)]) < 0)
        {
            Swap(p, k);
            k = p;
        }

        return k;
    }

    private int Parent(int k)
    {
        return ((k >> 1) - 1) & ~1;
    }

    private void Swap(int i, int j)
    {
        (_data[i], _data[j]) = (_data[j], _data[i]);
    }
}