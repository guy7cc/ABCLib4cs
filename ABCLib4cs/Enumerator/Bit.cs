namespace ABCLib4cs.Enumerator;

public class Bit
{
    public int Radix { get; }

    public Bit(int radix)
    {
        Radix = radix;
    }
    
    public IEnumerable<(int number, int[] digits)> EnumerateNaryDigitsFixedLength(int limit)
    {
        int l = limit - 1;
        int length = 0;
        while(l > 0)
        {
            length++;
            l /= Radix;
        }
        
        for (int i = 0; i < limit; i++)
        {
            var digits = new int[length];
            int ii = i;
            
            for (int j = length - 1; j >= 0; j--)
            {
                if (ii == 0) break;
                
                digits[j] = ii % Radix;
                ii /= Radix;
            }

            yield return (i, digits);
        }
    }
}