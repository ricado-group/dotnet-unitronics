using System;
using System.Linq;
using System.Text;

namespace RICADO.Unitronics.PComA
{
    internal static class Extensions
    {
        internal static void AppendChecksum(this StringBuilder stringBuilder)
        {
            if (stringBuilder == null || stringBuilder.Length == 0)
            {
                return;
            }

            stringBuilder.Append(stringBuilder.ToString().CalculateChecksum());
        }

        internal static string CalculateChecksum(this string @string)
        {
            if (@string == null || @string.Length == 0)
            {
                return "00";
            }

            int sum = @string.Sum(@char => (int)@char);

            sum %= 256;

            return sum.ToString("X").PadLeft(2, '0');
        }

        internal static void AppendHexValue(this StringBuilder stringBuilder, byte value)
        {
            stringBuilder.Append(value.ToString("X").PadLeft(2, '0'));
        }

        internal static void AppendHexValue(this StringBuilder stringBuilder, short value)
        {
            stringBuilder.Append(value.ToString("X").PadLeft(4, '0'));
        }

        internal static void AppendHexValue(this StringBuilder stringBuilder, ushort value)
        {
            stringBuilder.Append(value.ToString("X").PadLeft(4, '0'));
        }

        internal static void AppendHexValue(this StringBuilder stringBuilder, int value)
        {
            stringBuilder.Append(value.ToString("X").PadLeft(8, '0'));
        }

        internal static void AppendHexValue(this StringBuilder stringBuilder, uint value)
        {
            stringBuilder.Append(value.ToString("X").PadLeft(8, '0'));
        }

        internal static void AppendHexValue(this StringBuilder stringBuilder, float value)
        {
            ReadOnlySpan<byte> bytes = BitConverter.GetBytes(value);

            stringBuilder.AppendHexValue(bytes[1]);
            stringBuilder.AppendHexValue(bytes[0]);
            stringBuilder.AppendHexValue(bytes[3]);
            stringBuilder.AppendHexValue(bytes[2]);
        }

        internal static void AppendHexValue(this StringBuilder stringBuilder, int value, int length, char padCharacter = '0')
        {
            stringBuilder.Append(value.ToString("X").PadLeft(length, padCharacter));
        }
    }
}
