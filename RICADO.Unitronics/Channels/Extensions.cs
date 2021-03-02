using System;
using System.Collections.Generic;
using System.Linq;

namespace RICADO.Unitronics.Channels
{
    internal static class Extensions
    {
        internal static bool ContainsPattern(this List<byte> list, ReadOnlySpan<byte> pattern)
        {
            if (list.Count() == 0 || pattern.Length == 0 || list.Count() < pattern.Length)
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
