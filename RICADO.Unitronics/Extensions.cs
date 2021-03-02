using System;

namespace RICADO.Unitronics
{
    internal static class Extensions
    {
        internal static bool TryConvertValue<T>(this object @object, out T value) where T : struct
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
