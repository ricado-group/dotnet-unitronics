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

            return (ushort)(~(sum % 0x10000) + 1);
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
            stringBuilder.Append(value.ToString("X").PadLeft(length, '0'));
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

        internal static bool IsNumeric(this object @object)
        {
            if(@object == null)
            {
                return false;
            }

            switch(Type.GetTypeCode(@object.GetType()))
            {
                case TypeCode.Boolean:
                case TypeCode.Byte:
                case TypeCode.Char:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
            }

            return false;
        }

        internal static bool TryGetValue<T>(this object @object, out T value) where T : struct
        {
            value = default(T);
            
            if(@object == null)
            {
                return false;
            }
            
            Type objectType = @object.GetType();

            Type valueType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

            if(objectType == valueType || objectType.IsSubclassOf(valueType))
            {
                value = (T)@object;
                return true;
            }

            try
            {
                value = (T)Convert.ChangeType(@object, valueType);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
