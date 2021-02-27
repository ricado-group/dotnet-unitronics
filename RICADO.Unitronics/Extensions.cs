using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RICADO.Unitronics
{
    internal static class Extensions
    {
        internal static ushort CalculateChecksum(this IEnumerable<byte> enumerable)
        {
            int sum = enumerable.Sum(@byte => @byte);

            int checksum = ~(sum % 0x10000) + 1;

            if(checksum < ushort.MinValue || checksum > ushort.MaxValue)
            {
                return 0;
            }

            return (ushort)checksum;
        }

        internal static void AppendChecksum(this StringBuilder stringBuilder)
        {
            if(stringBuilder == null || stringBuilder.Length == 0)
            {
                return;
            }

            stringBuilder.Append(stringBuilder.ToString().CalculateChecksum());
        }

        internal static string CalculateChecksum(this string @string)
        {
            if(@string == null || @string.Length == 0)
            {
                return "00";
            }
            
            int sum = @string.Sum(@char => (int)@char);

            sum %= 256;

            return sum.ToString("X").PadLeft(2, '0');
        }

        internal static bool ContainsPattern(this List<byte> list, ReadOnlySpan<byte> pattern)
        {
            if(list.Count() == 0 || pattern.Length == 0 || list.Count() < pattern.Length)
            {
                return false;
            }

            ReadOnlySpan<byte> bytes = list.ToArray();

            return bytes.IndexOf(pattern) >= 0;
        }

        internal static int IndexOf(this List<byte> list, ReadOnlySpan<byte> pattern)
        {
            if (list.Count() == 0 || pattern.Length == 0 || list.Count() < pattern.Length)
            {
                return -1;
            }

            ReadOnlySpan<byte> bytes = list.ToArray();

            return bytes.IndexOf(pattern);
        }
    }
}
