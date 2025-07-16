namespace ABCLib4cs.Enumerator;

public class Bit
{
    public int Radix { get; }

    public Bit(int radix)
    {
        Radix = radix;
    }
    
    public IEnumerable<(long number, int[] digits)> EnumerateNaryDigitsFixedLength(long start, long end)
    {
        long r = end - 1;
        int length = 0;
        while(r > 0)
        {
            length++;
            r /= Radix;
        }
        
        for (long i = start; i < end; i++)
        {
            var digits = new int[length];
            long ii = i;
            
            for (int j = length - 1; j >= 0; j--)
            {
                if (ii == 0) break;
                
                digits[j] = (int)(ii % Radix);
                ii /= Radix;
            }

            yield return (i, digits);
        }
    }
}