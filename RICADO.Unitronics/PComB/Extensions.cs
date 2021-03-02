using System;
using System.Collections.Generic;
using System.Linq;

namespace RICADO.Unitronics.PComB
{
    internal static class Extensions
    {
        internal static ushort CalculateChecksum(this IEnumerable<byte> enumerable)
        {
            int sum = enumerable.Sum(@byte => @byte);

            return (ushort)(~(sum % 0x10000) + 1);
        }
    }
}
