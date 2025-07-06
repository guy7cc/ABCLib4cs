namespace ABCLib4cs.Enumerator;

public class Permutation
{
    public int Size { get; }

    private readonly int[] _arr;

    public Permutation(int size)
    {
        Size = size;
        _arr = new int[size];
        for (int i = 0; i < size; i++)
        {
            _arr[i] = i;
        }
    }
    
    public IEnumerable<(int, int[])> Enumerate()
    {
        int i = 0;
        do
        {
            yield return (i++, _arr);
        } while (Next());
    }
    
    public bool Next() {
        for (int i = Size - 2; i >= 0; i--) {
            if (_arr[i] < _arr[i + 1]) {
                int j = Size - 1;
                while (_arr[i] >= _arr[j]) {
                    j--;
                }
                Swap(i, j);
                Reverse(i + 1, Size - 1);
                return true;
            }
        }

        Reverse(0, Size - 1);
        return false;
    }
    
    private void Swap(int i, int j) {
        (_arr[i], _arr[j]) = (_arr[j], _arr[i]);
    }

    private void Reverse(int from, int to) {
        while (from < to) {
            Swap(from++, to--);
        }
    }
}